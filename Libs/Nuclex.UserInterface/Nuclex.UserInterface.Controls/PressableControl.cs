using Microsoft.Xna.Framework.Input;
using Nuclex.Input;

namespace Nuclex.UserInterface.Controls;

/// <summary>User interface element the user can push down</summary>
public abstract class PressableControl : Control, IFocusable
{
	/// <summary>Whether the user can interact with the choice</summary>
	public bool Enabled;

	/// <summary>Button that can be pressed to activate this command</summary>
	public Buttons? ShortcutButton;

	/// <summary>Whether the command is pressed down using the space key</summary>
	private bool pressedDownByKeyboard;

	/// <summary>Whether the command is pressed down using the keyboard shortcut</summary>
	private bool pressedDownByKeyboardShortcut;

	/// <summary>Whether the command is pressed down using the game pad shortcut</summary>
	private bool pressedDownByGamepadShortcut;

	/// <summary>Whether the command is pressed down using the mouse</summary>
	private bool pressedDownByMouse;

	/// <summary>Whether the mouse is hovering over the command</summary>
	private bool mouseHovering;

	/// <summary>Whether the mouse pointer is hovering over the control</summary>
	public bool MouseHovering => mouseHovering;

	/// <summary>Whether the pressable control is in the depressed state</summary>
	public virtual bool Depressed
	{
		get
		{
			if ((!mouseHovering || !pressedDownByMouse) && !pressedDownByKeyboard && !pressedDownByKeyboardShortcut)
			{
				return pressedDownByGamepadShortcut;
			}
			return true;
		}
	}

	/// <summary>Whether the control currently has the input focus</summary>
	public bool HasFocus
	{
		get
		{
			if (base.Screen != null)
			{
				return object.ReferenceEquals(base.Screen.FocusedControl, this);
			}
			return false;
		}
	}

	/// <summary>Whether the control can currently obtain the input focus</summary>
	bool IFocusable.CanGetFocus => Enabled;

	/// <summary>Initializes a new command control</summary>
	public PressableControl()
	{
		Enabled = true;
	}

	/// <summary>
	///   Called when the mouse has entered the control and is now hovering over it
	/// </summary>
	protected override void OnMouseEntered()
	{
		mouseHovering = true;
	}

	/// <summary>
	///   Called when the mouse has left the control and is no longer hovering over it
	/// </summary>
	protected override void OnMouseLeft()
	{
		mouseHovering = false;
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	protected override void OnMousePressed(MouseButtons button)
	{
		if (Enabled && button == MouseButtons.Left)
		{
			pressedDownByMouse = true;
		}
	}

	/// <summary>Called when a mouse button has been released again</summary>
	/// <param name="button">Index of the button that has been released</param>
	protected override void OnMouseReleased(MouseButtons button)
	{
		if (button == MouseButtons.Left)
		{
			pressedDownByMouse = false;
			if (mouseHovering && Enabled && !Depressed)
			{
				OnPressed();
			}
		}
	}

	/// <summary>Called when a button on the gamepad has been pressed</summary>
	/// <param name="button">Button that has been pressed</param>
	/// <returns>
	///   True if the button press was handled by the control, otherwise false.
	/// </returns>
	protected override bool OnButtonPressed(Buttons button)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (ShortcutButton.HasValue && button == ShortcutButton.Value)
		{
			pressedDownByGamepadShortcut = true;
			return true;
		}
		return false;
	}

	/// <summary>Called when a button on the gamepad has been released</summary>
	/// <param name="button">Button that has been released</param>
	protected override void OnButtonReleased(Buttons button)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (ShortcutButton.HasValue && pressedDownByGamepadShortcut && button == ShortcutButton.Value)
		{
			pressedDownByGamepadShortcut = false;
			if (!Depressed)
			{
				OnPressed();
			}
		}
	}

	/// <summary>Called when a key on the keyboard has been pressed down</summary>
	/// <param name="keyCode">Code of the key that was pressed</param>
	/// <returns>
	///   True if the key press was handled by the control, otherwise false.
	/// </returns>
	protected override bool OnKeyPressed(Keys keyCode)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		if (ShortcutButton.HasValue && keyCode == keyFromButton(ShortcutButton.Value))
		{
			pressedDownByKeyboardShortcut = true;
			return true;
		}
		if (HasFocus && (int)keyCode == 32)
		{
			pressedDownByKeyboard = true;
			return true;
		}
		return false;
	}

	/// <summary>Called when a key on the keyboard has been released again</summary>
	/// <param name="keyCode">Code of the key that was released</param>
	protected override void OnKeyReleased(Keys keyCode)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (pressedDownByKeyboardShortcut && ShortcutButton.HasValue && keyCode == keyFromButton(ShortcutButton.Value))
		{
			pressedDownByKeyboardShortcut = false;
			if (!Depressed)
			{
				OnPressed();
			}
		}
		if (pressedDownByKeyboard && (int)keyCode == 32)
		{
			pressedDownByKeyboard = false;
			if (!Depressed)
			{
				OnPressed();
			}
		}
	}

	/// <summary>Called when the control is pressed</summary>
	/// <remarks>
	///   If you were to implement a button, for example, you could trigger a 'Pressed'
	///   event here are call a user-provided delegate, depending on your design.
	/// </remarks>
	protected virtual void OnPressed()
	{
	}

	/// <summary>Looks up the equivalent key to the gamepad button</summary>
	/// <param name="button">
	///   Gamepad button for which the equivalent key on the keyboard will be found
	/// </param>
	/// <returns>The key that is equivalent to the specified gamepad button</returns>
	private static Keys keyFromButton(Buttons button)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		if ((int)button <= 256)
		{
			if ((int)button <= 32)
			{
				if ((int)button == 16)
				{
					return (Keys)13;
				}
				if ((int)button == 32)
				{
					return (Keys)8;
				}
			}
			else
			{
				if ((int)button == 64)
				{
					return (Keys)162;
				}
				if ((int)button == 128)
				{
					return (Keys)163;
				}
				if ((int)button == 256)
				{
					return (Keys)76;
				}
			}
		}
		else if ((int)button <= 4096)
		{
			if ((int)button == 512)
			{
				return (Keys)82;
			}
			if ((int)button == 4096)
			{
				return (Keys)65;
			}
		}
		else
		{
			if ((int)button == 8192)
			{
				return (Keys)66;
			}
			if ((int)button == 16384)
			{
				return (Keys)88;
			}
			if ((int)button == 32768)
			{
				return (Keys)89;
			}
		}
		return (Keys)0;
	}
}
