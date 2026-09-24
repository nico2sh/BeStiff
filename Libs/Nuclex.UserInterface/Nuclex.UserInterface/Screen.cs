using System;
using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Nuclex.Support;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Input;

namespace Nuclex.UserInterface;

/// <summary>Manages the controls and their state on a GUI screen</summary>
/// <remarks>
///   This class manages the global state of a distinct user interface. Unlike your
///   typical GUI library, the Nuclex.UserInterface library can handle any number of
///   simultaneously active user interfaces at the same time, making the library
///   suitable for usage on virtual ingame computers and multi-client environments
///   such as split-screen games or switchable graphical terminals.
/// </remarks>
public class Screen : IInputReceiver
{
	/// <summary>Highest value in the Keys enumeration</summary>
	private static readonly int maxKeyboardKey = (int)EnumHelper.GetHighestValue<Keys>();

	/// <summary>Size of the GUI area in world units or pixels</summary>
	private Vector2 size;

	/// <summary>Control responsible for hosting the GUI's top-level controls</summary>
	private DesktopControl desktopControl;

	/// <summary>Child that currently has the input focus</summary>
	/// <remarks>
	///   If this field is non-null, all keyboard input sent to the Gui is handed
	///   over to the focused control. Otherwise, keyboard input is discarded.
	/// </remarks>
	private Nuclex.Support.WeakReference<Control> focusedControl;

	/// <summary>Control the user has activated through one of the input devices</summary>
	private Control activatedControl;

	/// <summary>Number of keys being held down on the keyboard</summary>
	private int heldKeyCount;

	/// <summary>Keys on the keyboard the user is currently holding down</summary>
	private BitArray heldKeys;

	/// <summary>Buttons on the game pad the user is currently holding down</summary>
	private Buttons heldButtons;

	/// <summary>Mouse buttons currently being held down</summary>
	private MouseButtons heldMouseButtons;

	/// <summary>Width of the screen in pixels</summary>
	public float Width
	{
		get
		{
			return size.X;
		}
		set
		{
			size.X = value;
		}
	}

	/// <summary>Height of the screen in pixels</summary>
	public float Height
	{
		get
		{
			return size.Y;
		}
		set
		{
			size.Y = value;
		}
	}

	/// <summary>Control responsible for hosting the GUI's top-level controls</summary>
	public Control Desktop => desktopControl;

	/// <summary>Whether the GUI has currently captured the input devices</summary>
	/// <remarks>
	///   <para>
	///     When you mix GUIs and gameplay (for example, in a strategy game where the GUI
	///     manages the build menu and the remainder of the screen belongs to the game),
	///     it is important to keep control of who currently owns the input devices.
	///   </para>
	///   <para>
	///     Assume the player is drawing a selection rectangle around some units using
	///     the mouse. He will press the mouse button outside any GUI elements, keep
	///     holding it down and possibly drag over the GUI. Until the player lets go
	///     of the mouse button, input exclusively belongs to the game. The same goes
	///     vice versa, of course.
	///   </para>
	///   <para>
	///     This property tells whether the GUI currently thinks that all input belongs
	///     to it. If it is true, the game should not process any input. The GUI will
	///     implement the input model as described here and respect the game's ownership
	///     of the input devices if a mouse button is pressed outside of the GUI. To
	///     correctly handle input device ownership, send all input to the GUI
	///     regardless of this property's value, then check this property and if it
	///     returns false let your game process the input.
	///   </para>
	/// </remarks>
	public bool IsInputCaptured => desktopControl.IsInputCaptured;

	/// <summary>True if the mouse is currently hovering over any GUI elements</summary>
	/// <remarks>
	///   Useful if you mix gameplay with a GUI and use different mouse cursors
	///   depending on the location of the mouse. As long as input is not captured
	///   (see <see cref="P:Nuclex.UserInterface.Screen.IsInputCaptured" />) you can use this property to know
	///   whether you should use the standard GUI mouse cursor or let your game
	///   decide which cursor to use.
	/// </remarks>
	public bool IsMouseOverGui => desktopControl.IsMouseOverGui;

	/// <summary>Child control that currently has the input focus</summary>
	public Control FocusedControl
	{
		get
		{
			Control target = focusedControl.Target;
			if (target != null && object.ReferenceEquals(target.Screen, this))
			{
				return target;
			}
			return null;
		}
		set
		{
			Control target = focusedControl.Target;
			if (!object.ReferenceEquals(value, target))
			{
				focusedControl.Target = value;
				onFocusChanged(value);
			}
		}
	}

	/// <summary>
	///   Whether any keys, mouse buttons or game pad buttons are beind held pressed
	/// </summary>
	private bool anyKeysOrButtonsPressed
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			if (heldMouseButtons == (MouseButtons)0 && heldKeyCount <= 0)
			{
				return (int)heldButtons != 0;
			}
			return true;
		}
	}

	/// <summary>Triggered when the control in focus changes</summary>
	public event EventHandler<ControlEventArgs> FocusChanged;

	/// <summary>Initializes a new GUI</summary>
	public Screen()
		: this(0f, 0f)
	{
	}

	/// <summary>Initializes a new GUI</summary>
	/// <param name="width">Width of the area the GUI can occupy</param>
	/// <param name="height">Height of the area the GUI can occupy</param>
	/// <remarks>
	///   Width and height should reflect the entire drawable area of your GUI. If you
	///   want to limit the region which the GUI is allowed to use (eg. to only use the
	///   safe area of a TV) please resize the desktop control accordingly!
	/// </remarks>
	public Screen(float width, float height)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Width = width;
		Height = height;
		heldKeys = new BitArray(maxKeyboardKey + 1);
		heldButtons = (Buttons)0;
		desktopControl = new DesktopControl();
		desktopControl.Bounds.Size.X.Fraction = 1f;
		desktopControl.Bounds.Size.Y.Fraction = 1f;
		desktopControl.SetScreen(this);
		focusedControl = new Nuclex.Support.WeakReference<Control>(null);
	}

	/// <summary>Injects a command into the processor</summary>
	/// <param name="command">Input command that will be injected</param>
	public void InjectCommand(Command command)
	{
		switch (command)
		{
		case Command.Accept:
		case Command.Cancel:
			FocusedControl?.ProcessCommand(command);
			break;
		case Command.Up:
		case Command.Down:
		case Command.Left:
		case Command.Right:
		{
			Control control = FocusedControl;
			if (control == null || control.ProcessCommand(command))
			{
				break;
			}
			float num = float.NaN;
			Control control2 = null;
			RectangleF absoluteBounds = control.Parent.GetAbsoluteBounds();
			RectangleF ownBounds = control.Bounds.ToOffset(absoluteBounds.Width, absoluteBounds.Height);
			Collection<Control> children = control.Parent.Children;
			for (int i = 0; i < children.Count; i++)
			{
				Control control3 = children[i];
				if (!object.ReferenceEquals(control3, control) && canControlGetFocus(control3))
				{
					RectangleF otherBounds = control3.Bounds.ToOffset(absoluteBounds.Width, absoluteBounds.Height);
					float directionalDistance = getDirectionalDistance(ref ownBounds, ref otherBounds, command);
					if (float.IsNaN(num) || directionalDistance < num)
					{
						control2 = control3;
						num = directionalDistance;
					}
				}
			}
			if (num != float.NaN)
			{
				FocusedControl = control2;
			}
			break;
		}
		case Command.SelectNext:
		case Command.SelectPrevious:
			break;
		}
	}

	/// <summary>Called when a key on the keyboard has been pressed down</summary>
	/// <param name="keyCode">Code of the key that was pressed</param>
	public void InjectKeyPress(Keys keyCode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected I4, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected I4, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Invalid comparison between Unknown and I4
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected I4, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected I4, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected I4, but got Unknown
		bool flag = heldKeys.Get((int)keyCode);
		if (activatedControl != null)
		{
			activatedControl.ProcessKeyPress(keyCode, flag);
			if (!flag)
			{
				heldKeyCount++;
				heldKeys.Set((int)keyCode, value: true);
			}
			return;
		}
		Control target = focusedControl.Target;
		if (target != null && target.ProcessKeyPress(keyCode, repetition: false))
		{
			activatedControl = target;
			if (!flag)
			{
				heldKeyCount++;
				heldKeys.Set((int)keyCode, value: true);
			}
		}
		else if (desktopControl.ProcessKeyPress(keyCode, repetition: false))
		{
			activatedControl = desktopControl;
			if (!flag)
			{
				heldKeyCount++;
				heldKeys.Set((int)keyCode, value: true);
			}
		}
		else if ((int)keyCode != 13)
		{
			if ((int)keyCode != 27)
			{
				switch ((int)keyCode - 37)
				{
				case 1:
					InjectCommand(Command.Up);
					break;
				case 3:
					InjectCommand(Command.Down);
					break;
				case 0:
					InjectCommand(Command.Left);
					break;
				case 2:
					InjectCommand(Command.Right);
					break;
				}
			}
			else
			{
				InjectCommand(Command.Cancel);
			}
		}
		else
		{
			InjectCommand(Command.Accept);
		}
	}

	/// <summary>Called when a key on the keyboard has been released again</summary>
	/// <param name="keyCode">Code of the key that was released</param>
	public void InjectKeyRelease(Keys keyCode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected I4, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected I4, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (heldKeys.Get((int)keyCode))
		{
			heldKeyCount--;
			heldKeys.Set((int)keyCode, value: false);
			if (activatedControl != null)
			{
				activatedControl.ProcessKeyRelease(keyCode);
			}
			if (!anyKeysOrButtonsPressed)
			{
				activatedControl = null;
			}
		}
	}

	/// <summary>Handle user text input by a physical or virtual keyboard</summary>
	/// <param name="character">Character that has been entered</param>
	public void InjectCharacter(char character)
	{
		Control target = focusedControl.Target;
		if (target is IWritable writable)
		{
			writable.OnCharacterEntered(character);
		}
	}

	/// <summary>Called when a button on the gamepad has been pressed</summary>
	/// <param name="button">Button that has been pressed</param>
	public void InjectButtonPress(Buttons button)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Buttons val = (Buttons)(heldButtons | button);
		if (val == heldButtons)
		{
			return;
		}
		heldButtons = val;
		if (activatedControl != null)
		{
			activatedControl.ProcessButtonPress(button);
			return;
		}
		Control target = focusedControl.Target;
		if (target != null && target.ProcessButtonPress(button))
		{
			activatedControl = target;
		}
		else if (desktopControl.ProcessButtonPress(button))
		{
			activatedControl = desktopControl;
		}
	}

	/// <summary>Called when a button on the gamepad has been released</summary>
	/// <param name="button">Button that has been released</param>
	public void InjectButtonRelease(Buttons button)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((heldButtons & button) != 0)
		{
			heldButtons &= ~button;
			if (activatedControl != null)
			{
				activatedControl.ProcessButtonRelease(button);
			}
			if (!anyKeysOrButtonsPressed)
			{
				activatedControl = null;
			}
		}
	}

	/// <summary>Injects a mouse position update into the GUI</summary>
	/// <param name="x">X coordinate of the mouse cursor within the screen</param>
	/// <param name="y">Y coordinate of the mouse cursor within the screen</param>
	public void InjectMouseMove(float x, float y)
	{
		desktopControl.ProcessMouseMove(size.X, size.Y, x, y);
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	public void InjectMousePress(MouseButtons button)
	{
		heldMouseButtons |= button;
		if (activatedControl != null)
		{
			activatedControl.ProcessMousePress(button);
			return;
		}
		activatedControl = desktopControl;
		desktopControl.ProcessMousePress(button);
	}

	/// <summary>Called when a mouse button has been released again</summary>
	/// <param name="button">Index of the button that has been released</param>
	public void InjectMouseRelease(MouseButtons button)
	{
		heldMouseButtons &= ~button;
		if (activatedControl != null)
		{
			activatedControl.ProcessMouseRelease(button);
		}
		if (!anyKeysOrButtonsPressed)
		{
			activatedControl = null;
		}
	}

	/// <summary>Called when the mouse wheel has been rotated</summary>
	/// <param name="ticks">Number of ticks that the mouse wheel has been rotated</param>
	public void InjectMouseWheel(float ticks)
	{
		if (activatedControl != null)
		{
			activatedControl.ProcessMouseWheel(ticks);
		}
		else
		{
			desktopControl.ProcessMouseWheel(ticks);
		}
	}

	/// <summary>Triggers the FocusChanged event</summary>
	/// <param name="focusedControl">Control that has gotten the input focus</param>
	private void onFocusChanged(Control focusedControl)
	{
		if (this.FocusChanged != null)
		{
			this.FocusChanged(this, new ControlEventArgs(focusedControl));
		}
	}

	/// <summary>
	///   Determines the distance of one rectangle to the other, also taking direction
	///   into account
	/// </summary>
	/// <param name="ownBounds">Boundaries of the base rectangle</param>
	/// <param name="otherBounds">Boundaries of the other rectangle</param>
	/// <param name="direction">Direction into which distance will be determined</param>
	/// <returns>
	///   The direction of the other rectangle of NaN if it didn't lie in that direction
	/// </returns>
	private static float getDirectionalDistance(ref RectangleF ownBounds, ref RectangleF otherBounds, Command direction)
	{
		float num4;
		if (direction == Command.Up || direction == Command.Down)
		{
			float val = ownBounds.X + ownBounds.Width / 2f;
			float num = Math.Min(Math.Max(val, otherBounds.Left), otherBounds.Right);
			float num2 = otherBounds.Y + otherBounds.Height / 2f;
			bool flag = num < ownBounds.Left;
			bool flag2 = num > ownBounds.Right;
			float num3;
			if (direction == Command.Up)
			{
				num3 = ownBounds.Top;
				if (num2 > num3 && (flag || flag2))
				{
					return float.NaN;
				}
				num4 = num3 - num2;
			}
			else
			{
				num3 = ownBounds.Bottom;
				if (num2 < num3 && (flag || flag2))
				{
					return float.NaN;
				}
				num4 = num2 - num3;
			}
			float num5 = Math.Abs(num3 - num2);
			if (flag)
			{
				float num6 = Math.Abs(ownBounds.Left - num);
				if (num6 > num5)
				{
					return float.NaN;
				}
			}
			else if (flag2)
			{
				float num7 = Math.Abs(num - ownBounds.Right);
				if (num7 > num5)
				{
					return float.NaN;
				}
			}
		}
		else
		{
			float val2 = ownBounds.Y + ownBounds.Height / 2f;
			float num = otherBounds.X + otherBounds.Width / 2f;
			float num2 = Math.Min(Math.Max(val2, otherBounds.Top), otherBounds.Bottom);
			bool flag3 = num2 < ownBounds.Top;
			bool flag4 = num2 > ownBounds.Bottom;
			float num8;
			if (direction == Command.Left)
			{
				num8 = ownBounds.Left;
				if (num > num8 && (flag3 || flag4))
				{
					return float.NaN;
				}
				num4 = num8 - num;
			}
			else
			{
				num8 = ownBounds.Right;
				if (num < num8 && (flag3 || flag4))
				{
					return float.NaN;
				}
				num4 = num - num8;
			}
			float num9 = Math.Abs(num8 - num);
			if (flag3)
			{
				float num10 = Math.Abs(ownBounds.Top - num2);
				if (num10 > num9)
				{
					return float.NaN;
				}
			}
			else if (flag4)
			{
				float num11 = Math.Abs(num2 - ownBounds.Bottom);
				if (num11 > num9)
				{
					return float.NaN;
				}
			}
		}
		if (!(num4 < 0f))
		{
			return num4;
		}
		return float.NaN;
	}

	/// <summary>Determines whether a control can obtain the input focus</summary>
	/// <param name="control">Control that will be checked for focusability</param>
	/// <returns>True if the specified control can obtain the input focus</returns>
	private static bool canControlGetFocus(Control control)
	{
		if (control is IFocusable focusable)
		{
			return focusable.CanGetFocus;
		}
		return false;
	}
}
