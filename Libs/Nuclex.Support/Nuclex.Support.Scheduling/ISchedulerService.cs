using System;
using System.Threading;

namespace Nuclex.Support.Scheduling;

/// <summary>Service that allows the scheduled invocation of tasks</summary>
public interface ISchedulerService
{
	/// <summary>Time source being used by the scheduler</summary>
	ITimeSource TimeSource { get; }

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
	object NotifyAt(DateTime notificationTime, WaitCallback callback);

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
	object NotifyEach(int delayMilliseconds, int intervalMilliseconds, WaitCallback callback);

	/// <summary>
	///   Schedules a recurring notification after the specified time span
	/// </summary>
	/// <param name="delay">Delay after which the first notification will occur</param>
	/// <param name="interval">Interval at which the notification will be repeated</param>
	/// <param name="callback">
	///   Callback that will be invoked when the notification is due
	/// </param>
	/// <returns>A handle that can be used to cancel the notification</returns>
	object NotifyEach(TimeSpan delay, TimeSpan interval, WaitCallback callback);

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
	object NotifyIn(int delayMilliseconds, WaitCallback callback);

	/// <summary>Schedules a notification after the specified time span</summary>
	/// <param name="delay">Delay after which the notification will occur</param>
	/// <param name="callback">
	///   Callback that will be invoked when the notification is due
	/// </param>
	/// <returns>A handle that can be used to cancel the notification</returns>
	object NotifyIn(TimeSpan delay, WaitCallback callback);

	/// <summary>Cancels a scheduled notification</summary>
	/// <param name="notificationHandle">
	///   Handle of the notification that will be cancelled
	/// </param>
	void Cancel(object notificationHandle);
}
