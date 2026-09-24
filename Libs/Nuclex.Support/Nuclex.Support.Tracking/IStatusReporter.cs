using System;

namespace Nuclex.Support.Tracking;

/// <summary>Interface for processes that report their status</summary>
public interface IStatusReporter
{
	/// <summary>Triggered when the status of the process changes</summary>
	event EventHandler<StatusReportEventArgs> AsyncStatusChanged;
}
