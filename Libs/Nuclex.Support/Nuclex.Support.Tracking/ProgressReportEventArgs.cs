using System;

namespace Nuclex.Support.Tracking;

/// <summary>Event arguments for a progress update notification</summary>
public class ProgressReportEventArgs : EventArgs
{
	/// <summary>Achieved progress</summary>
	private float progress;

	/// <summary>Currently achieved progress</summary>
	public float Progress => progress;

	/// <summary>Initializes the progress update informations</summary>
	/// <param name="progress">Achieved progress ranging from 0.0 to 1.0</param>
	public ProgressReportEventArgs(float progress)
	{
		this.progress = progress;
	}
}
