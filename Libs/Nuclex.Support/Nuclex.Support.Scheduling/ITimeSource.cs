using System;
using System.Threading;

namespace Nuclex.Support.Scheduling;

/// <summary>Provides time measurement and change notification services</summary>
public interface ITimeSource
{
	/// <summary>Current system time in UTC format</summary>
	DateTime CurrentUtcTime { get; }

	/// <summary>How long the time source has been running</summary>
	/// <remarks>
	///   There is no guarantee this value starts at zero (or anywhere near it) when
	///   the time source is created. The only requirement for this value is that it
	///   keeps increasing with the passing of time and that it stays unaffected
	///   (eg. doesn't skip or jump back) when the system date/time are changed.
	/// </remarks>
	long Ticks { get; }

	/// <summary>Called when the system date/time are adjusted</summary>
	/// <remarks>
	///   An adjustment is a change out of the ordinary, eg. when a time synchronization
	///   alters the current system time, when daylight saving time takes effect or
	///   when the user manually adjusts the system date/time.
	/// </remarks>
	event EventHandler DateTimeAdjusted;

	/// <summary>Waits for an AutoResetEvent to become signalled</summary>
	/// <param name="waitHandle">WaitHandle the method will wait for</param>
	/// <param name="ticks">Number of ticks to wait</param>
	/// <returns>
	///   True if the WaitHandle was signalled, false if the timeout was reached
	///   or the time source thinks its time to recheck the system date/time.
	/// </returns>
	/// <remarks>
	///   Depending on whether the system will provide notifications when date/time
	///   is adjusted, the time source will be forced to let this method block for
	///   less than the indicated time before returning a timeout in order to give
	///   the caller a chance to recheck the system time.
	/// </remarks>
	bool WaitOne(AutoResetEvent waitHandle, long ticks);
}
