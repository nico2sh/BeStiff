using System;

namespace Nuclex.UserInterface.Controls;

/// <summary>Event argument class that carries a control instance</summary>
public class ControlEventArgs : EventArgs
{
	/// <summary>Control that will be accessible to the event subscribers</summary>
	private Control control;

	/// <summary>Control that has been provided for the event</summary>
	public Control Control => control;

	/// <summary>Initializes a new control event args instance</summary>
	/// <param name="control">Control to provide to the subscribers of the event</param>
	public ControlEventArgs(Control control)
	{
		this.control = control;
	}
}
