using System;
using System.Diagnostics;
using System.Threading;

namespace Nuclex.Support.Scheduling;

/// <summary>
///   Generic time source implementation using the Stopwatch or Environment.TickCount
/// </summary>
public class GenericTimeSource : ITimeSource
{
	/// <summary>Number of ticks (100 ns intervals) in a millisecond</summary>
	private const long TicksPerMillisecond = 10000L;

	/// <summary>Tolerance for the detection of a date/time adjustment</summary>
	/// <remarks>
	///   If the current system date/time jumps by more than this tolerance into any
	///   direction, the default time source will trigger the DateTimeAdjusted event.
	/// </remarks>
	private const long TimeAdjustmentToleranceTicks = 750000L;

	/// <summary>Last local time we checked for a date/time adjustment</summary>
	private long lastCheckedDateTimeTicks;

	/// <summary>Timer ticks at which we last checked the local time</summary>
	private long lastCheckedStopwatchTicks;

	/// <summary>Number of ticks per Stopwatch time unit</summary>
	private static double tickFrequency;

	/// <summary>Whether ot use the Stopwatch class for measuring time</summary>
	private bool useStopwatch;

	/// <summary>Current system time in UTC format</summary>
	public DateTime CurrentUtcTime => DateTime.UtcNow;

	/// <summary>How long the time source has been running</summary>
	/// <remarks>
	///   There is no guarantee this value starts at zero (or anywhere near it) when
	///   the time source is created. The only requirement for this value is that it
	///   keeps increasing with the passing of time and that it stays unaffected
	///   (eg. doesn't skip or jump back) when the system date/time are changed.
	/// </remarks>
	public long Ticks
	{
		get
		{
			if (useStopwatch)
			{
				double num = Stopwatch.GetTimestamp();
				return (long)(num * tickFrequency);
			}
			return (long)Environment.TickCount * 10000L;
		}
	}

	/// <summary>Called when the system date/time are adjusted</summary>
	/// <remarks>
	///   An adjustment is a change out of the ordinary, eg. when a time synchronization
	///   alters the current system time, when daylight saving time takes effect or
	///   when the user manually adjusts the system date/time.
	/// </remarks>
	public event EventHandler DateTimeAdjusted;

	/// <summary>Initializes the static fields of the default time source</summary>
	static GenericTimeSource()
	{
		tickFrequency = 10000000.0;
		tickFrequency /= Stopwatch.Frequency;
	}

	/// <summary>Initializes the default time source</summary>
	public GenericTimeSource()
		: this(Stopwatch.IsHighResolution)
	{
	}

	/// <summary>Initializes the default time source</summary>
	/// <param name="useStopwatch">
	///   Whether to use the Stopwatch class for measuring time
	/// </param>
	/// <remarks>
	///   <para>
	///     Normally it's a good idea to use the default constructor. If the Stopwatch
	///     is unable to use the high-resolution timer, it will fall back to
	///     DateTime.Now (as stated on MSDN). This is bad because then the tick count
	///     will jump whenever the system time changes (eg. when the system synchronizes
	///     its time with a time server).
	///   </para>
	///   <para>
	///     Your can safely use this constructor if you always set its arugment to 'false',
	///     but then your won't profit from the high-resolution timer if one is available.
	///   </para>
	/// </remarks>
	public GenericTimeSource(bool useStopwatch)
	{
		this.useStopwatch = useStopwatch;
		checkForTimeAdjustment();
	}

	/// <summary>Waits for an AutoResetEvent to become signalled</summary>
	/// <param name="waitHandle">WaitHandle the method will wait for</param>
	/// <param name="ticks">Number of ticks to wait</param>
	/// <returns>
	///   True if the WaitHandle was signalled, false if the timeout was reached
	/// </returns>
	public virtual bool WaitOne(AutoResetEvent waitHandle, long ticks)
	{
		int val = (int)(ticks / 10000);
		bool result = waitHandle.WaitOne(Math.Min(1000, val), exitContext: false);
		checkForTimeAdjustment();
		return result;
	}

	/// <summary>Called when the system time is changed</summary>
	/// <param name="sender">Not used</param>
	/// <param name="arguments">Not used</param>
	protected virtual void OnDateTimeAdjusted(object sender, EventArgs arguments)
	{
		this.DateTimeAdjusted?.Invoke(sender, arguments);
	}

	/// <summary>
	///   Checks whether the system/date time have been adjusted since the last call
	/// </summary>
	private void checkForTimeAdjustment()
	{
		long ticks = DateTime.UtcNow.Ticks;
		long ticks2 = Ticks;
		long num = ticks2 - lastCheckedStopwatchTicks;
		long num2 = lastCheckedDateTimeTicks + num;
		long num3 = Math.Abs(num2 - ticks);
		if (num3 > 750000)
		{
			OnDateTimeAdjusted(this, EventArgs.Empty);
		}
		lastCheckedDateTimeTicks = ticks;
		lastCheckedStopwatchTicks = ticks2;
	}
}
