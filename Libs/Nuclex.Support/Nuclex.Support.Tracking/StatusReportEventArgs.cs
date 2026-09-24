using System;

namespace Nuclex.Support.Tracking;

/// <summary>Event arguments for reporting a status to the subscriber</summary>
public class StatusReportEventArgs : EventArgs
{
	/// <summary>Reported status</summary>
	private string status;

	/// <summary>The currently reported status</summary>
	/// <remarks>
	///   The contents of this string are up to the publisher of the event to
	///   define. Though it is recommended to report the status as a human-readable
	///   string, these strings might not in all cases be properly localized or
	///   suitable for display in a GUI.
	/// </remarks>
	public string Status => status;

	/// <summary>Initializes a new status report event arguments container</summary>
	/// <param name="status">Status to report to the event's subscribers</param>
	public StatusReportEventArgs(string status)
	{
		this.status = status;
	}
}
