using System;

namespace Nuclex.Support.Tracking;

/// <summary>Event arguments for an idle state change notification</summary>
public class IdleStateEventArgs : EventArgs
{
	/// <summary>Current idle state</summary>
	private bool idle;

	/// <summary>Current idle state</summary>
	public bool Idle => idle;

	/// <summary>Initializes the idle state change notification</summary>
	/// <param name="idle">The new idle state</param>
	public IdleStateEventArgs(bool idle)
	{
		this.idle = idle;
	}
}
