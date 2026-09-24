namespace Nuclex.UserInterface.Controls;

/// <summary>Control used to represent the desktop</summary>
internal class DesktopControl : Control
{
	/// <summary>True if the mouse is currently hovering over a GUI element</summary>
	public bool IsMouseOverGui
	{
		get
		{
			if (base.MouseOverControl == null)
			{
				return false;
			}
			return !object.ReferenceEquals(base.MouseOverControl, this);
		}
	}

	/// <summary>Whether the GUI holds ownership of the input devices</summary>
	public bool IsInputCaptured
	{
		get
		{
			if (base.ActivatedControl == null)
			{
				return false;
			}
			return !object.ReferenceEquals(base.ActivatedControl, this);
		}
	}

	/// <summary>Initializes a new control</summary>
	public DesktopControl()
	{
	}
}
