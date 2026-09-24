using System;

namespace Nuclex.Support.Tracking;

/// <summary>Interface for processes that report their progress</summary>
public interface IProgressReporter
{
	/// <summary>Triggered when the status of the process changes</summary>
	event EventHandler<ProgressReportEventArgs> AsyncProgressChanged;
}
