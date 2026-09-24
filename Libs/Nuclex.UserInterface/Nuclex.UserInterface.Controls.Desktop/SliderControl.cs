using System;
using Nuclex.Input;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Base class for a slider that can be moved using the mouse</summary>
/// <remarks>
///   Implements the common functionality for a slider moving either the direction
///   of the X or the Y axis (but not both). Derive any scroll bar-like controls
///   from this class to simplify their implementation.
/// </remarks>
public abstract class SliderControl : Control
{
	/// <summary>Can be set by renderers to allow the control to locate its thumb</summary>
	public IThumbLocator ThumbLocator;

	/// <summary>Fraction of the slider filled by the thumb (0.0 .. 1.0)</summary>
	public float ThumbSize;

	/// <summary>Position of the thumb within the slider (0.0 .. 1.0)</summary>
	public float ThumbPosition;

	/// <summary>Whether the mouse cursor is hovering over the thumb</summary>
	private bool mouseOverThumb;

	/// <summary>Whether the slider's thumb is currently in the depressed state</summary>
	private bool pressedDown;

	/// <summary>X coordinate at which the thumb was picked up</summary>
	private float pickupX;

	/// <summary>Y coordinate at which the thumb was picked up</summary>
	private float pickupY;

	/// <summary>whether the mouse is currently hovering over the thumb</summary>
	public bool MouseOverThumb => mouseOverThumb;

	/// <summary>Whether the pressable control is in the depressed state</summary>
	public virtual bool ThumbDepressed
	{
		get
		{
			if (pressedDown)
			{
				return mouseOverThumb;
			}
			return false;
		}
	}

	/// <summary>Triggered when the slider has been moved</summary>
	public event EventHandler Moved;

	/// <summary>Initializes a new slider control</summary>
	public SliderControl()
	{
		ThumbPosition = 0f;
		ThumbSize = 1f;
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	protected override void OnMousePressed(MouseButtons button)
	{
		if (button == MouseButtons.Left)
		{
			RectangleF thumbRegion = GetThumbRegion();
			if (thumbRegion.Contains(pickupX, pickupY))
			{
				pressedDown = true;
				pickupX -= thumbRegion.X;
				pickupY -= thumbRegion.Y;
			}
		}
	}

	/// <summary>Called when a mouse button has been released again</summary>
	/// <param name="button">Index of the button that has been released</param>
	protected override void OnMouseReleased(MouseButtons button)
	{
		if (button == MouseButtons.Left)
		{
			pressedDown = false;
		}
	}

	/// <summary>Called when the mouse position is updated</summary>
	/// <param name="x">X coordinate of the mouse cursor on the control</param>
	/// <param name="y">Y coordinate of the mouse cursor on the control</param>
	protected override void OnMouseMoved(float x, float y)
	{
		if (pressedDown)
		{
			MoveThumb(x - pickupX, y - pickupY);
		}
		else
		{
			pickupX = x;
			pickupY = y;
		}
		mouseOverThumb = GetThumbRegion().Contains(x, y);
	}

	/// <summary>
	///   Called when the mouse has left the control and is no longer hovering over it
	/// </summary>
	protected override void OnMouseLeft()
	{
		mouseOverThumb = false;
	}

	/// <summary>Fires the slider's Moved event</summary>
	protected virtual void OnMoved()
	{
		if (this.Moved != null)
		{
			this.Moved(this, EventArgs.Empty);
		}
	}

	/// <summary>Moves the thumb to the specified location</summary>
	/// <returns>Location the thumb will be moved to</returns>
	protected abstract void MoveThumb(float x, float y);

	/// <summary>Obtains the region covered by the slider's thumb</summary>
	/// <returns>The region covered by the slider's thumb</returns>
	protected abstract RectangleF GetThumbRegion();
}
