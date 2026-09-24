using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Nuclex.UserInterface.Input;

namespace Nuclex.UserInterface.Controls;

/// <summary>Represents an element in the user interface</summary>
/// <remarks>
///   <para>
///     Controls are always arranged in a tree where each control except the one at
///     the root of the tree has exactly one owner (the one at the root has no owner).
///     The design actively prevents you from assigning a control as child to
///     multiple parents.
///   </para>
///   <para>
///     The controls in the Nuclex.UserInterface library are fully independent of
///     their graphical representation. That means you can construct a dialog
///     without even having a graphics device in place, that you can move your
///     dialogs between different graphics devices and that you do not have to
///     even think about graphics device resets and similar trouble.
///   </para>
/// </remarks>
public class Control
{
	/// <summary>Mouse buttons the user is holding down over the control</summary>
	private MouseButtons heldMouseButtons;

	/// <summary>Number of keyboard keys being held down</summary>
	private int heldKeyCount;

	/// <summary>Number of game pad buttons being held down</summary>
	private int heldButtonCount;

	/// <summary>Control the mouse is currently hovering over</summary>
	private Control mouseOverControl;

	/// <summary>Control the mouse was pressed down on</summary>
	private Control activatedControl;

	/// <summary>Location and extents of the control</summary>
	public UniRectangle Bounds;

	/// <summary>Control this control is contained in</summary>
	private Control parent;

	/// <summary>GUI instance this control has been added to. Can be null.</summary>
	private Screen screen;

	/// <summary>Name of the control instance (for programmatic identification)</summary>
	private string name;

	/// <summary>Whether this control can obtain the input focus</summary>
	private bool affectsOrdering;

	/// <summary>Child controls belonging to this control</summary>
	/// <remarks>
	///   Child controls are any controls that belong to this control. They don't
	///   neccessarily need to be situated in this control's client area, but
	///   their positioning will be relative to the parent's location.
	/// </remarks>
	private ParentingControlCollection children;

	/// <summary>
	///   Whether any keys, mouse buttons or game pad buttons are beind held pressed
	/// </summary>
	private bool anyKeysOrButtonsPressed
	{
		get
		{
			if (heldMouseButtons == (MouseButtons)0 && heldKeyCount <= 0)
			{
				return heldButtonCount > 0;
			}
			return true;
		}
	}

	/// <summary>Children of the control</summary>
	public Collection<Control> Children => children;

	/// <summary>
	///   True if clicking the control or its children moves the control into
	///   the foreground of the drawing hierarchy
	/// </summary>
	public bool AffectsOrdering => affectsOrdering;

	/// <summary>Parent control this control is contained in</summary>
	/// <remarks>
	///   Can be null, but this is only the case for free-floating controls that have
	///   not been added into a Gui. The only control that really keeps this field
	///   set to null whilst the Gui is active is the root control in the Gui class.
	/// </remarks>
	public Control Parent => parent;

	/// <summary>Name that can be used to uniquely identify the control</summary>
	/// <remarks>
	///   This name acts as an unique identifier for a control. It primarily serves
	///   as a means to programmatically identify the control and as a debugging aid.
	///   Duplicate names are not allowed and will result in an exception being
	///   thrown, the only exception is when the control's name is set to null.
	/// </remarks>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (value != name)
			{
				Control control = Parent;
				if (control != null && control.children.IsNameTaken(value))
				{
					throw new DuplicateNameException("Another control is already using this name");
				}
				name = value;
			}
		}
	}

	/// <summary>GUI instance this control belongs to. Can be null.</summary>
	internal Screen Screen => screen;

	/// <summary>Control the mouse is currently over</summary>
	protected internal Control MouseOverControl => mouseOverControl;

	/// <summary>Control that currently captured incoming input</summary>
	protected internal Control ActivatedControl => activatedControl;

	/// <summary>Called when a button on the game pad has been pressed</summary>
	/// <param name="button">Button that has been pressed</param>
	/// <returns>
	///   True if the button press was processed by the control and future game pad
	///   input belongs to the control until all buttons are released again
	/// </returns>
	internal bool ProcessButtonPress(Buttons button)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (activatedControl != null)
		{
			heldButtonCount++;
			if (activatedControl != this)
			{
				activatedControl.ProcessButtonPress(button);
			}
			else
			{
				OnButtonPressed(button);
			}
			return true;
		}
		if (OnButtonPressed(button))
		{
			activatedControl = this;
			heldButtonCount++;
			return true;
		}
		bool flag = false;
		for (int i = 0; i < children.Count; i++)
		{
			Control control = children[i];
			if (control.affectsOrdering)
			{
				if (flag)
				{
					continue;
				}
				flag = true;
			}
			if (control.ProcessButtonPress(button))
			{
				activatedControl = control;
				heldButtonCount++;
				return true;
			}
		}
		return false;
	}

	/// <summary>Called when a button on the game pad has been released</summary>
	/// <param name="button">Button that has been released</param>
	internal void ProcessButtonRelease(Buttons button)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (heldButtonCount != 0)
		{
			heldButtonCount--;
			if (activatedControl != this)
			{
				activatedControl.ProcessButtonRelease(button);
			}
			else
			{
				OnButtonReleased(button);
			}
			if (!anyKeysOrButtonsPressed)
			{
				activatedControl = null;
			}
		}
	}

	/// <summary>
	///   Called when the mouse has left the control and is no longer hovering over it
	/// </summary>
	internal void ProcessMouseLeave()
	{
		if (mouseOverControl != null)
		{
			if (mouseOverControl != this)
			{
				mouseOverControl.ProcessMouseLeave();
			}
			else
			{
				OnMouseLeft();
			}
			mouseOverControl = null;
		}
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	/// <returns>Whether the control has processed the mouse press</returns>
	internal bool ProcessMousePress(MouseButtons button)
	{
		if (activatedControl == null)
		{
			activatedControl = mouseOverControl;
			if (activatedControl == null)
			{
				return false;
			}
			if (activatedControl != this && activatedControl.affectsOrdering)
			{
				children.MoveToStart(children.IndexOf(activatedControl));
			}
		}
		heldMouseButtons |= button;
		if (activatedControl != this)
		{
			return activatedControl.ProcessMousePress(button);
		}
		if (screen != null && this is IFocusable { CanGetFocus: not false })
		{
			screen.FocusedControl = this;
		}
		OnMousePressed(button);
		return true;
	}

	/// <summary>Called when a mouse button has been released again</summary>
	/// <param name="button">Index of the button that has been released</param>
	internal void ProcessMouseRelease(MouseButtons button)
	{
		if ((heldMouseButtons & button) == button)
		{
			heldMouseButtons &= ~button;
			if (activatedControl != this)
			{
				activatedControl.ProcessMouseRelease(button);
			}
			else
			{
				OnMouseReleased(button);
			}
			if (!anyKeysOrButtonsPressed)
			{
				activatedControl = null;
			}
		}
	}

	/// <summary>Processes mouse movement notifications</summary>
	/// <param name="containerWidth">Absolute width of the control's container</param>
	/// <param name="containerHeight">Absolute height of the control's container</param>
	/// <param name="x">Absolute X position of the mouse within the container</param>
	/// <param name="y">Absolute Y position of the mouse within the container</param>
	internal void ProcessMouseMove(float containerWidth, float containerHeight, float x, float y)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Bounds.Size.ToOffset(containerWidth, containerHeight);
		if (activatedControl != null)
		{
			float x2 = x - Bounds.Location.X.ToOffset(containerWidth);
			float y2 = y - Bounds.Location.Y.ToOffset(containerHeight);
			if (activatedControl != this)
			{
				activatedControl.ProcessMouseMove(val.X, val.Y, x2, y2);
			}
			else
			{
				OnMouseMoved(x2, y2);
			}
		}
		x -= Bounds.Location.X.ToOffset(containerWidth);
		y -= Bounds.Location.Y.ToOffset(containerHeight);
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].Bounds.ToOffset(val.X, val.Y).Contains(x, y))
			{
				switchMouseOverControl(children[i]);
				if (mouseOverControl != activatedControl)
				{
					mouseOverControl.ProcessMouseMove(val.X, val.Y, x, y);
				}
				return;
			}
		}
		if (x >= 0f && x < val.X && y >= 0f && y < val.Y)
		{
			switchMouseOverControl(this);
			if (activatedControl == null)
			{
				OnMouseMoved(x, y);
			}
		}
		else
		{
			ProcessMouseLeave();
		}
	}

	/// <summary>Called when the mouse wheel has been rotated</summary>
	/// <param name="ticks">Number of ticks that the mouse wheel has been rotated</param>
	internal void ProcessMouseWheel(float ticks)
	{
		if (activatedControl != null && activatedControl != this)
		{
			activatedControl.ProcessMouseWheel(ticks);
		}
		else if (mouseOverControl != null && mouseOverControl != this)
		{
			mouseOverControl.ProcessMouseWheel(ticks);
		}
		else
		{
			OnMouseWheel(ticks);
		}
	}

	/// <summary>Called when a key on the keyboard has been pressed down</summary>
	/// <param name="keyCode">Code of the key that was pressed</param>
	/// <param name="repetition">
	///   Whether the key press is due to the user holding down a key
	/// </param>
	internal bool ProcessKeyPress(Keys keyCode, bool repetition)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (activatedControl != null)
		{
			if (!repetition)
			{
				heldKeyCount++;
			}
			if (activatedControl != this)
			{
				activatedControl.ProcessKeyPress(keyCode, repetition);
			}
			else
			{
				OnKeyPressed(keyCode);
			}
			return true;
		}
		if (OnKeyPressed(keyCode))
		{
			activatedControl = this;
			heldKeyCount++;
			return true;
		}
		bool flag = false;
		for (int i = 0; i < children.Count; i++)
		{
			Control control = children[i];
			if (control.affectsOrdering)
			{
				if (flag)
				{
					continue;
				}
				flag = true;
			}
			if (control.ProcessKeyPress(keyCode, repetition))
			{
				activatedControl = control;
				heldKeyCount++;
				return true;
			}
		}
		return false;
	}

	/// <summary>Called when a key on the keyboard has been released again</summary>
	/// <param name="keyCode">Code of the key that was released</param>
	internal void ProcessKeyRelease(Keys keyCode)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		heldKeyCount--;
		if (activatedControl != this)
		{
			activatedControl.ProcessKeyRelease(keyCode);
		}
		else
		{
			OnKeyReleased(keyCode);
		}
		if (!anyKeysOrButtonsPressed)
		{
			activatedControl = null;
		}
	}

	/// <summary>Switches the mouse over control to a different control</summary>
	/// <param name="newMouseOverControl">New control the mouse is hovering over</param>
	private void switchMouseOverControl(Control newMouseOverControl)
	{
		if (mouseOverControl != newMouseOverControl)
		{
			if (mouseOverControl != null)
			{
				mouseOverControl.ProcessMouseLeave();
			}
			mouseOverControl = newMouseOverControl;
			newMouseOverControl.OnMouseEntered();
		}
	}

	/// <summary>Initializes a new control</summary>
	public Control()
		: this(affectsOrdering: false)
	{
	}

	/// <summary>Initializes a new control</summary>
	/// <param name="affectsOrdering">
	///   Whether the control comes to the top of the hierarchy when clicked
	/// </param>
	/// <remarks>
	///   <para>
	///     The <paramref name="affectsOrdering" /> parameter should be set for windows
	///     and other free-floating panels which exist in parallel and which the user
	///     might want to put on top of their siblings by clicking them. If the user
	///     clicks on a child control of such a panel/window control, the panel/window
	///     control will also be moved into the foreground.
	///   </para>
	///   <para>
	///     It should not be set for normal controls which usually have no overlap,
	///     like buttons. Otherwise, a button placed on the desktop could overdraw a
	///     window when the button is clicked. The behavior would be well-defined and
	///     controlled, but the user probably doesn't expect this ;-)
	///   </para>
	/// </remarks>
	protected Control(bool affectsOrdering)
	{
		this.affectsOrdering = affectsOrdering;
		children = new ParentingControlCollection(this);
	}

	/// <summary>Moves the control into the foreground</summary>
	public void BringToFront()
	{
		Control control = this;
		while (!object.ReferenceEquals(control.parent, null))
		{
			ParentingControlCollection parentingControlCollection = control.parent.children;
			parentingControlCollection.MoveToStart(parentingControlCollection.IndexOf(control));
			control = control.parent;
		}
	}

	/// <summary>
	///   Obtains the absolute boundaries of the control in screen coordinates
	/// </summary>
	/// <returns>The control's absolute screen coordinate boundaries</returns>
	/// <remarks>
	///   This method resolves the unified coordinates into absolute screen coordinates
	///   that can be used to do hit-testing and rendering. The control is required to
	///   be part of a GUI hierarchy that is assigned to a screen for this to work
	///   since otherwise, there's no absolute coordinate frame into which the
	///   unified coordinates could be resolved.
	/// </remarks>
	public RectangleF GetAbsoluteBounds()
	{
		if (object.ReferenceEquals(parent, null))
		{
			if (object.ReferenceEquals(screen, null))
			{
				throw new InvalidOperationException("Obtaining absolute bounds requires the control to be part of a screen");
			}
			return Bounds.ToOffset(screen.Width, screen.Height);
		}
		RectangleF absoluteBounds = parent.GetAbsoluteBounds();
		RectangleF result = Bounds.ToOffset(absoluteBounds.Width, absoluteBounds.Height);
		result.Offset(absoluteBounds.X, absoluteBounds.Y);
		return result;
	}

	/// <summary>Called when an input command was sent to the control</summary>
	/// <param name="command">Input command that has been triggered</param>
	/// <returns>Whether the command has been processed by the control</returns>
	protected virtual bool OnCommand(Command command)
	{
		return false;
	}

	/// <summary>Called when a button on the gamepad has been pressed</summary>
	/// <param name="button">Button that has been pressed</param>
	/// <returns>
	///   True if the button press was handled by the control, otherwise false.
	/// </returns>
	/// <remarks>
	///   If the control indicates that it didn't handle the key press, it will not
	///   receive the associated key release notification.
	/// </remarks>
	protected virtual bool OnButtonPressed(Buttons button)
	{
		return false;
	}

	/// <summary>Called when a button on the gamepad has been released</summary>
	/// <param name="button">Button that has been released</param>
	protected virtual void OnButtonReleased(Buttons button)
	{
	}

	/// <summary>Called when the mouse position is updated</summary>
	/// <param name="x">X coordinate of the mouse cursor on the control</param>
	/// <param name="y">Y coordinate of the mouse cursor on the control</param>
	protected virtual void OnMouseMoved(float x, float y)
	{
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	/// <returns>Whether the control has processed the mouse press</returns>
	/// <remarks>
	///   If this method states that a mouse press is processed by returning
	///   true, that means the control did something with it and the mouse press
	///   should not be acted upon by any other listener.
	/// </remarks>
	protected virtual void OnMousePressed(MouseButtons button)
	{
	}

	/// <summary>Called when a mouse button has been released again</summary>
	/// <param name="button">Index of the button that has been released</param>
	protected virtual void OnMouseReleased(MouseButtons button)
	{
	}

	/// <summary>
	///   Called when the mouse has left the control and is no longer hovering over it
	/// </summary>
	protected virtual void OnMouseLeft()
	{
	}

	/// <summary>
	///   Called when the mouse has entered the control and is now hovering over it
	/// </summary>
	protected virtual void OnMouseEntered()
	{
	}

	/// <summary>Called when the mouse wheel has been rotated</summary>
	/// <param name="ticks">Number of ticks that the mouse wheel has been rotated</param>
	protected virtual void OnMouseWheel(float ticks)
	{
	}

	/// <summary>Called when a key on the keyboard has been pressed down</summary>
	/// <param name="keyCode">Code of the key that was pressed</param>
	/// <returns>
	///   True if the key press was handled by the control, otherwise false.
	/// </returns>
	/// <remarks>
	///   If the control indicates that it didn't handle the key press, it will not
	///   receive the associated key release notification. This means that if you
	///   return false from this method, you should under no circumstances do anything
	///   with the information - you will not know when the key is released again
	///   and another control might pick it up, causing a second key response.
	/// </remarks>
	protected virtual bool OnKeyPressed(Keys keyCode)
	{
		return false;
	}

	/// <summary>Called when a key on the keyboard has been released again</summary>
	/// <param name="keyCode">Code of the key that was released</param>
	protected virtual void OnKeyReleased(Keys keyCode)
	{
	}

	/// <summary>Called when a command was sent to the control</summary>
	/// <param name="command">Command to be injected</param>
	/// <returns>Whether the command has been processed</returns>
	internal bool ProcessCommand(Command command)
	{
		switch (command)
		{
		case Command.SelectNext:
		case Command.SelectPrevious:
			return false;
		case Command.Accept:
		case Command.Cancel:
		case Command.Up:
		case Command.Down:
		case Command.Left:
		case Command.Right:
			return OnCommand(command);
		default:
			throw new ArgumentException("Invalid command", "command");
		}
	}

	/// <summary>Assigns a new parent to the control</summary>
	/// <param name="parent">New parent to assign to the control</param>
	internal void SetParent(Control parent)
	{
		this.parent = parent;
		if (this.parent != null)
		{
			if (!object.ReferenceEquals(screen, parent.screen))
			{
				SetScreen(parent.screen);
			}
		}
		else
		{
			SetScreen(null);
		}
	}

	/// <summary>Assigns a new GUI to the control</summary>
	/// <param name="gui">New GUI to assign to the control</param>
	internal void SetScreen(Screen gui)
	{
		screen = gui;
		children.SetScreen(gui);
	}
}
