using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Control through which the user can enter text</summary>
/// <remarks>
///   <para>
///     Through this control, users can be asked to enter an arbitrary string
///     of characters, their name for example. Desktop users can enter text through
///     their normal keyboard where Windows' own key translation is used to
///     support regional settings and custom keyboard layouts.
///   </para>
///   <para>
///     XBox 360 users will open the virtual keyboard when the input box gets
///     the input focus and can add characters by selecting them from the virtual
///     keyboard's character matrix.
///   </para>
/// </remarks>
public class InputControl : Control, IWritable, IFocusable
{
	/// <summary>Title to be displayed in the on-screen keyboard</summary>
	public string GuideTitle;

	/// <summary>Description to be displayed in the on-screen keyboard</summary>
	public string GuideDescription;

	/// <summary>Whether user interaction with the control is allowed</summary>
	public bool Enabled;

	/// <summary>
	///   Can be set by renderers to enable cursor positioning by the mouse
	/// </summary>
	public IOpeningLocator OpeningLocator;

	/// <summary>Array used to store characters before they are appended</summary>
	private char[] singleCharArray;

	/// <summary>Tick count at the time the caret was last moved</summary>
	private int lastCaretMovementTicks;

	/// <summary>Text the user has entered into the text input control</summary>
	private StringBuilder text;

	/// <summary>Position of the cursor within the text</summary>
	private int caretPosition;

	/// <summary>X coordinate of the last known mouse position</summary>
	private float mouseX;

	/// <summary>Y coordinate of the last known mouse position</summary>
	private float mouseY;

	/// <summary>Text that is being displayed on the control</summary>
	public string Text
	{
		get
		{
			return text.ToString();
		}
		set
		{
			text.Remove(0, text.Length);
			text.Append(value);
			if (caretPosition > text.Length)
			{
				caretPosition = text.Length;
			}
		}
	}

	/// <summary>Position of the cursor within the text</summary>
	public int CaretPosition
	{
		get
		{
			return caretPosition;
		}
		set
		{
			if (value < 0 || value > Text.Length)
			{
				throw new ArgumentException("Invalid caret position", "CaretPosition");
			}
			caretPosition = value;
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

	/// <summary>Elapsed milliseconds since the user last moved the caret</summary>
	/// <remarks>
	///   This is an unusual property for an input box to have. It is retrieved by
	///   the renderer and could be used for several purposes, such as lighting up
	///   a control when text is entered to provide better visual tracking or
	///   preventing the cursor from blinking whilst the user is typing.
	/// </remarks>
	public int MillisecondsSinceLastCaretMovement => Environment.TickCount - lastCaretMovementTicks;

	/// <summary>Whether the control can currently obtain the input focus</summary>
	bool IFocusable.CanGetFocus => Enabled;

	/// <summary>Title to be displayed in the on-screen keyboard</summary>
	string IWritable.GuideTitle => GuideTitle;

	/// <summary>Description to be displayed in the on-screen keyboard</summary>
	string IWritable.GuideDescription => GuideDescription;

	/// <summary>Initializes a new text input control</summary>
	public InputControl()
	{
		singleCharArray = new char[1];
		text = new StringBuilder(64);
		Enabled = true;
		GuideTitle = "Text Entry";
		GuideDescription = "Please enter the text for this input field";
	}

	/// <summary>Called when the user has entered a character</summary>
	/// <param name="character">Character that has been entered</param>
	protected virtual void OnCharacterEntered(char character)
	{
		if (character != '\b')
		{
			updateLastCaretMovementTicks();
			singleCharArray[0] = character;
			text.Insert(caretPosition, singleCharArray);
			caretPosition++;
		}
	}

	/// <summary>Called when a key on the keyboard has been pressed down</summary>
	/// <param name="keyCode">Code of the key that was pressed</param>
	/// <returns>
	///   True if the key press was handles by the control, otherwise false.
	/// </returns>
	/// <remarks>
	///   If the control indicates that it didn't handle the key press, it will not
	///   receive the associated key release notification.
	/// </remarks>
	protected override bool OnKeyPressed(Keys keyCode)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected I4, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected I4, but got Unknown
		if (!HasFocus)
		{
			return false;
		}
		switch ((int)keyCode - 8)
		{
		default:
			if ((int)keyCode != 13)
			{
				switch ((int)keyCode - 35)
				{
				case 11:
					if (caretPosition < text.Length)
					{
						updateLastCaretMovementTicks();
						text.Remove(caretPosition, 1);
					}
					goto end_IL_000f;
				case 2:
					if (caretPosition > 0)
					{
						updateLastCaretMovementTicks();
						caretPosition--;
					}
					goto end_IL_000f;
				case 4:
					if (caretPosition < text.Length)
					{
						updateLastCaretMovementTicks();
						caretPosition++;
					}
					goto end_IL_000f;
				case 1:
					updateLastCaretMovementTicks();
					caretPosition = 0;
					goto end_IL_000f;
				case 0:
					updateLastCaretMovementTicks();
					caretPosition = text.Length;
					goto end_IL_000f;
				case 3:
				case 5:
					break;
				default:
					goto end_IL_000f;
				}
			}
			goto case 1;
		case 0:
			if (caretPosition > 0)
			{
				updateLastCaretMovementTicks();
				text.Remove(caretPosition - 1, 1);
				caretPosition--;
			}
			break;
		case 1:
			{
				return false;
			}
			end_IL_000f:
			break;
		}
		return true;
	}

	/// <summary>Called when the mouse position is updated</summary>
	/// <param name="x">X coordinate of the mouse cursor on the control</param>
	/// <param name="y">Y coordinate of the mouse cursor on the control</param>
	protected override void OnMouseMoved(float x, float y)
	{
		mouseX = x;
		mouseY = y;
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	protected override void OnMousePressed(MouseButtons button)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (button == MouseButtons.Left)
		{
			if (OpeningLocator != null)
			{
				RectangleF absoluteBounds = GetAbsoluteBounds();
				Vector2 position = default(Vector2);
				position = new Vector2(absoluteBounds.X + mouseX, absoluteBounds.Y + mouseY);
				caretPosition = OpeningLocator.GetClosestOpening(absoluteBounds, Text, position);
			}
			else
			{
				moveCaretToEnd();
			}
		}
	}

	/// <summary>Handles user text input by a physical keyboard</summary>
	/// <param name="character">Character that has been entered</param>
	internal void ProcessCharacter(char character)
	{
		OnCharacterEntered(character);
	}

	/// <summary>Called when the user has entered a character</summary>
	/// <param name="character">Character that has been entered</param>
	void IWritable.OnCharacterEntered(char character)
	{
		OnCharacterEntered(character);
	}

	/// <summary>Moves the caret to the end of the text</summary>
	private void moveCaretToEnd()
	{
		updateLastCaretMovementTicks();
		caretPosition = text.Length;
	}

	/// <summary>Updates the tick count when the caret was last moved</summary>
	/// <remarks>
	///   Used to prevent the caret from blinking when 
	/// </remarks>
	private void updateLastCaretMovementTicks()
	{
		lastCaretMovementTicks = Environment.TickCount;
	}
}
