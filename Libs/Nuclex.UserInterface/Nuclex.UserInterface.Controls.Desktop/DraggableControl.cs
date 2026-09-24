using Nuclex.Input;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Control the user can drag around with the mouse</summary>
public abstract class DraggableControl : Control
{
	/// <summary>Whether the control can be dragged</summary>
	private bool enableDragging;

	/// <summary>Whether the control is currently being dragged</summary>
	private bool beingDragged;

	/// <summary>X coordinate at which the control was picked up</summary>
	private float pickupX;

	/// <summary>Y coordinate at which the control was picked up</summary>
	private float pickupY;

	/// <summary>Whether the control can be dragged with the mouse</summary>
	protected bool EnableDragging
	{
		get
		{
			return enableDragging;
		}
		set
		{
			enableDragging = value;
			beingDragged &= value;
		}
	}

	/// <summary>Initializes a new draggable control</summary>
	public DraggableControl()
	{
		EnableDragging = true;
	}

	/// <summary>Initializes a new draggable control</summary>
	/// <param name="canGetFocus">Whether the control can obtain the input focus</param>
	public DraggableControl(bool canGetFocus)
		: base(canGetFocus)
	{
		EnableDragging = true;
	}

	/// <summary>Called when the mouse position is updated</summary>
	/// <param name="x">X coordinate of the mouse cursor on the GUI</param>
	/// <param name="y">Y coordinate of the mouse cursor on the GUI</param>
	protected override void OnMouseMoved(float x, float y)
	{
		if (beingDragged)
		{
			Bounds.Location.X.Offset += x - pickupX;
			Bounds.Location.Y.Offset += y - pickupY;
		}
		else
		{
			pickupX = x;
			pickupY = y;
		}
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	protected override void OnMousePressed(MouseButtons button)
	{
		if (button == MouseButtons.Left)
		{
			beingDragged = enableDragging;
		}
	}

	/// <summary>Called when a mouse button has been released again</summary>
	/// <param name="button">Index of the button that has been released</param>
	protected override void OnMouseReleased(MouseButtons button)
	{
		if (button == MouseButtons.Left)
		{
			beingDragged = false;
		}
	}
}
