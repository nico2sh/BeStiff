namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>A window for hosting other controls</summary>
public class WindowControl : DraggableControl
{
	/// <summary>Text in the title bar of the window</summary>
	public string Title;

	/// <summary>Whether the window is currently open</summary>
	public bool IsOpen => base.Screen != null;

	/// <summary>Whether the window can be dragged with the mouse</summary>
	public new bool EnableDragging
	{
		get
		{
			return base.EnableDragging;
		}
		set
		{
			base.EnableDragging = value;
		}
	}

	/// <summary>Initializes a new window control</summary>
	public WindowControl()
		: base(canGetFocus: true)
	{
	}

	/// <summary>Closes the window</summary>
	public void Close()
	{
		if (IsOpen)
		{
			base.Parent.Children.Remove(this);
		}
	}
}
