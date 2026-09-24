#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Nuclex.Support.Collections;

namespace Nuclex.Support.Scheduling;

/// <summary>Schedules actions for execution at a future point in time</summary>
/// <summary>Schedules actions for execution at a future point in time</summary>
public class Scheduler : ISchedulerService, IDisposable
{
	/// <summary>Scheduled notification</summary>
	private class Notification
	{
		/// <summary>
		///   Ticks specifying the interval in which the notification will be re-executed
		/// </summary>
		public long IntervalTicks;

		/// <summary>Next due time for this notification</summary>
		public long NextDueTicks;

		/// <summary>Absolute time in UTC at which the notification is due</summary>
		/// <remarks>
		///   Only stored for notifications scheduled in absolute time, meaning they
		///   have to be adjusted if the system date/time changes
		/// </remarks>
		public DateTime AbsoluteUtcTime;

		/// <summary>Callback that will be invoked when the notification is due</summary>
		public WaitCallback Callback;

		/// <summary>Whether the notification has been cancelled</summary>
		public bool Cancelled;

		/// <summary>Initializes a new notification</summary>
		/// <param name="intervalTicks">
		///   Interval in which the notification will re-executed
		/// </param>
		/// <param name="nextDueTicks">
		///   Time source ticks the notification is next due at
		/// </param>
		/// <param name="absoluteUtcTime">
		///   Absolute time in UTC at which the notification is due
		/// </param>
		/// <param name="callback">
		///   Callback to be invoked when the notification is due
		/// </param>
		public Notification(long intervalTicks, long nextDueTicks, DateTime absoluteUtcTime, WaitCallback callback)
		{
			IntervalTicks = intervalTicks;
			NextDueTicks = nextDueTicks;
			AbsoluteUtcTime = absoluteUtcTime;
			Callback = callback;
			Cancelled = false;
		}
	}

	/// <summary>Compares two notifications to each other</summary>
	private class NotificationComparer : IComparer<Notification>
	{
		/// <summary>The default instance of the notification comparer</summary>
		public static readonly NotificationComparer Default = new NotificationComparer();

		/// <summary>Compares two notifications to each other based on their time</summary>
		/// <param name="left">Notification that will be compared on the left side</param>
		/// <param name="right">Notification that will be comapred on the right side</param>
		/// <returns>The relation of the two notification's times to each other</returns>
		public int Compare(Notification left, Notification right)
		{
			if (left.NextDueTicks > right.NextDueTicks)
			{
				return -1;
			}
			if (left.NextDueTicks < right.NextDueTicks)
			{
				return 1;
			}
			return 0;
		}
	}

	/// <summary>
	///   Manages the singleton instance of the scheduler's default time source
	/// </summary>
	private class TimeSourceSingleton
	{
		/// <summary>The singleton instance of the default time source</summary>
		internal static readonly ITimeSource Instance;

		/// <summary>
		///   Explicit static constructor to guarantee the singleton is initialized only
		///   when a static member of this class is accessed.
		/// </summary>
		static TimeSourceSingleton()
		{
			Instance = CreateDefaultTimeSource();
		}
	}

	/// <summary>One tick is 100 ns, meaning 10000 ticks equal 1 ms</summary>
	private const long TicksPerMillisecond = 10000L;

	/// <summary>Time source used by the scheduler</summary>
	private ITimeSource timeSource;

	/// <summary>Thread that will wait for the next scheduled event</summary>
	private Thread timerThread;

	/// <summary>Notifications in the scheduler's queue</summary>
	private PriorityQueue<Notification> notifications;

	/// <summary>Event used by the timer thread to wait for the next notification</summary>
	private AutoResetEvent notificationWaitEvent;

	/// <summary>Whether the timer thread should end</summary>
	private volatile bool endRequested;

	/// <summary>Delegate for the dateTimeAdjusted() method</summary>
	private EventHandler dateTimeAdjustedDelegate;

	/// <summary>Time source being used by the scheduler</summary>
	public ITimeSource TimeSource => timeSource;

	/// <summary>Returns the default time source for the scheduler</summary>
	public static ITimeSource DefaultTimeSource => TimeSourceSingleton.Instance;

	/// <summary>Initializes a new scheduler using the default time source</summary>
	public Scheduler()
		: this(DefaultTimeSource)
	{
	}

	/// <summary>Initializes a new scheduler using the specified time source</summary>
	/// <param name="timeSource">Time source the scheduler will use</param>
	public Scheduler(ITimeSource timeSource)
	{
		dateTimeAdjustedDelegate = dateTimeAdjusted;
		this.timeSource = timeSource;
		this.timeSource.DateTimeAdjusted += dateTimeAdjustedDelegate;
		notifications = new PriorityQueue<Notification>(NotificationComparer.Default);
		notificationWaitEvent = new AutoResetEvent(initialState: false);
		timerThread = new Thread(runTimerThread);
		timerThread.Name = "Nuclex.Support.Scheduling.Scheduler";
		timerThread.Priority = ThreadPriority.Highest;
		timerThread.IsBackground = true;
		timerThread.Start();
	}

	/// <summary>Immediately releases all resources owned by the instance</summary>
	public void Dispose()
	{
		if (timerThread != null)
		{
			endRequested = true;
			notificationWaitEvent.Set();
			bool condition = timerThread.Join(2500);
			Trace.Assert(condition, "Scheduler timer thread did not exit in time");
			if (timeSource != null)
			{
				timeSource.DateTimeAdjusted -= dateTimeAdjustedDelegate;
				timeSource = null;
			}
			notificationWaitEvent.Close();
			notificationWaitEvent = null;
			notifications = null;
			timerThread = null;
		}
	}

	/// <summary>Schedules a notification at the specified absolute time</summary>
	/// <param name="notificationTime">
	///   Absolute time at which the notification will occur
	/// </param>
	/// <param name="callback">
	///   Callback that will be invoked when the notification is due
	/// </param>
	/// <returns>A handle that can be used to cancel the notification</returns>
	/// <remarks>
	///   The notification is scheduled for the indicated absolute time. If the system
	///   enters/leaves daylight saving time or the date/time is changed (for example
	///   when the system synchronizes with an NTP server), this will affect
	///   the notification. So if you need to be notified after a fixed time, use
	///   the NotifyIn() method instead.
	/// </remarks>
	public object NotifyAt(DateTime notificationTime, WaitCallback callback)
	{
		if (notificationTime.Kind == DateTimeKind.Unspecified)
		{
			throw new ArgumentException("Notification time is neither UTC or local", "notificationTime");
		}
		DateTime absoluteUtcTime = notificationTime.ToUniversalTime();
		DateTime currentUtcTime = timeSource.CurrentUtcTime;
		long num = absoluteUtcTime.Ticks - currentUtcTime.Ticks;
		long nextDueTicks = timeSource.Ticks + num;
		return scheduleNotification(new Notification(0L, nextDueTicks, absoluteUtcTime, callback));
	}

	/// <summary>Schedules a notification after the specified time span</summary>
	/// <param name="delay">Delay after which the notification will occur</param>
	/// <param name="callback">
	///   Callback that will be invoked when the notification is due
	/// </param>
	/// <returns>A handle that can be used to cancel the notification</returns>
	public object NotifyIn(TimeSpan delay, WaitCallback callback)
	{
		return scheduleNotification(new Notification(0L, timeSource.Ticks + delay.Ticks, DateTime.MinValue, callback));
	}

	/// <summary>
	///   Schedules a notification after the specified amount of milliseconds
	/// </summary>
	/// <param name="delayMilliseconds">
	///   Number of milliseconds after which the notification will occur
	/// </param>
	/// <param name="callback">
	///   Callback that will be invoked when the notification is due
	/// </param>
	/// <returns>A handle that can be used to cancel the notification</returns>
	public object NotifyIn(int delayMilliseconds, WaitCallback callback)
	{
		return scheduleNotification(new Notification(0L, timeSource.Ticks + (long)delayMilliseconds * 10000L, DateTime.MinValue, callback));
	}

	/// <summary>
	///   Schedules a recurring notification after the specified time span
	/// </summary>
	/// <param name="delay">Delay after which the first notification will occur</param>
	/// <param name="interval">Interval at which the notification will be repeated</param>
	/// <param name="callback">
	///   Callback that will be invoked when the notification is due
	/// </param>
	/// <returns>A handle that can be used to cancel the notification</returns>
	public object NotifyEach(TimeSpan delay, TimeSpan interval, WaitCallback callback)
	{
		return scheduleNotification(new Notification(interval.Ticks, timeSource.Ticks + delay.Ticks, DateTime.MinValue, callback));
	}

	/// <summary>
	///   Schedules a recurring notification after the specified amount of milliseconds
	/// </summary>
	/// <param name="delayMilliseconds">
	///   Milliseconds after which the first notification will occur
	/// </param>
	/// <param name="intervalMilliseconds">
	///   Interval in milliseconds at which the notification will be repeated
	/// </param>
	/// <param name="callback">
	///   Callback that will be invoked when the notification is due
	/// </param>
	/// <returns>A handle that can be used to cancel the notification</returns>
	public object NotifyEach(int delayMilliseconds, int intervalMilliseconds, WaitCallback callback)
	{
		return scheduleNotification(new Notification((long)intervalMilliseconds * 10000L, timeSource.Ticks + (long)delayMilliseconds * 10000L, DateTime.MinValue, callback));
	}

	/// <summary>Cancels a scheduled notification</summary>
	/// <param name="notificationHandle">
	///   Handle of the notification that will be cancelled
	/// </param>
	public void Cancel(object notificationHandle)
	{
		if (notificationHandle is Notification notification)
		{
			notification.Cancelled = true;
		}
	}

	/// <summary>Called when the system date/time have been adjusted</summary>
	/// <param name="sender">Time source which detected the adjustment</param>
	/// <param name="arguments">Not used</param>
	private void dateTimeAdjusted(object sender, EventArgs arguments)
	{
		lock (timerThread)
		{
			long ticks = timeSource.Ticks;
			DateTime currentUtcTime = timeSource.CurrentUtcTime;
			PriorityQueue<Notification> priorityQueue = new PriorityQueue<Notification>(NotificationComparer.Default);
			while (notifications.Count > 0)
			{
				Notification notification = notifications.Dequeue();
				if (!notification.Cancelled)
				{
					if (notification.AbsoluteUtcTime != DateTime.MinValue)
					{
						long ticks2 = (notification.AbsoluteUtcTime - currentUtcTime).Ticks;
						notification.NextDueTicks = ticks + ticks2;
					}
					priorityQueue.Enqueue(notification);
				}
			}
			notifications = priorityQueue;
		}
		notificationWaitEvent.Set();
	}

	/// <summary>Schedules a notification for processing by the timer thread</summary>
	/// <param name="notification">Notification that will be scheduled</param>
	/// <returns>The scheduled notification</returns>
	private object scheduleNotification(Notification notification)
	{
		lock (timerThread)
		{
			notifications.Enqueue(notification);
			if (object.ReferenceEquals(notifications.Peek(), notification))
			{
				notificationWaitEvent.Set();
			}
		}
		return notification;
	}

	/// <summary>Executes the timer thread</summary>
	private void runTimerThread()
	{
		Notification nextDueNotification;
		lock (timerThread)
		{
			nextDueNotification = getNextDueNotification();
		}
		while (true)
		{
			if (nextDueNotification == null)
			{
				notificationWaitEvent.WaitOne();
			}
			else
			{
				long num = nextDueNotification.NextDueTicks - timeSource.Ticks;
				if (num > 0)
				{
					timeSource.WaitOne(notificationWaitEvent, num);
				}
			}
			if (endRequested)
			{
				break;
			}
			long ticks = timeSource.Ticks;
			lock (timerThread)
			{
				while (true)
				{
					nextDueNotification = getNextDueNotification();
					if (nextDueNotification != null)
					{
						long num2 = nextDueNotification.NextDueTicks - ticks;
						if (num2 < 10000)
						{
							if (!nextDueNotification.Cancelled)
							{
								ThreadPool.QueueUserWorkItem(nextDueNotification.Callback);
							}
							notifications.Dequeue();
							if (nextDueNotification.IntervalTicks != 0)
							{
								nextDueNotification.NextDueTicks += nextDueNotification.IntervalTicks;
								notifications.Enqueue(nextDueNotification);
							}
							continue;
						}
						break;
					}
					break;
				}
			}
		}
	}

	/// <summary>Retrieves the notification that is due next</summary>
	/// <returns>The notification that is due next</returns>
	private Notification getNextDueNotification()
	{
		while (notifications.Count > 0)
		{
			Notification notification = notifications.Peek();
			if (notification.Cancelled)
			{
				notifications.Dequeue();
				continue;
			}
			return notification;
		}
		return null;
	}

	/// <summary>Creates a new default time source for the scheduler</summary>
	/// <param name="useWindowsTimeSource">
	///   Whether the specialized windows time source should be used
	/// </param>
	/// <returns>The newly created time source</returns>
	internal static ITimeSource CreateTimeSource(bool useWindowsTimeSource)
	{
		// The Windows-only time source (SystemEvents) was dropped in the port;
		// the generic one works everywhere.
		return new GenericTimeSource();
	}

	/// <summary>Creates a new default time source for the scheduler</summary>
	/// <returns>The newly created time source</returns>
	internal static ITimeSource CreateDefaultTimeSource()
	{
		return CreateTimeSource(useWindowsTimeSource: false);
	}
}
