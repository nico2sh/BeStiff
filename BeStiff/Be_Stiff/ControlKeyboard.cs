using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Be_Stiff
{
	public class ControlKeyboard : IControl
	{
		private const double timeDownForSliding = 200.0;

		public readonly KeyboardState[] CurrentKeyboardStates;

		public readonly KeyboardState[] LastKeyboardStates;

		private double[] timeDownPressed;

		private bool[] doubleDownPressed;

		private double[] timeUpPressed;

		private bool[] doubleUpPressed;

		public MouseState CurrentMouseState;

		public MouseState LastMouseState;

		private int screenCenterX;

		private int screenCenterY;

		private Vector2 cursorPosition = Vector2.Zero;

		private int cursorXSide;

		public Vector2 CursorPosition => cursorPosition;

		public ControlKeyboard()
		{
			CurrentKeyboardStates = new KeyboardState[InputHelper.MaxInputs];
			LastKeyboardStates = new KeyboardState[InputHelper.MaxInputs];
			timeDownPressed = new double[InputHelper.MaxInputs];
			doubleDownPressed = new bool[InputHelper.MaxInputs];
			timeUpPressed = new double[InputHelper.MaxInputs];
			doubleUpPressed = new bool[InputHelper.MaxInputs];
			cursorPosition = Vector2.Zero;
			cursorXSide = 1;
		}

		public void Initialize(Vector2 screenCenter)
		{
			screenCenterX = (int)screenCenter.X;
			screenCenterY = (int)screenCenter.Y;
			cursorPosition = new Vector2(1f, 0f);
		}

		public void Update()
		{
			for (int i = 0; i < InputHelper.MaxInputs; i++)
			{
				ref KeyboardState reference = ref LastKeyboardStates[i];
				reference = CurrentKeyboardStates[i];
				ref KeyboardState reference2 = ref CurrentKeyboardStates[i];
				reference2 = Keyboard.GetState((PlayerIndex)i);
				if (i == 0)
				{
					reference2 = DebugKeys.Merge(reference2);
				}
			}
			LastMouseState = CurrentMouseState;
			CurrentMouseState = Mouse.GetState();
		}

		public void UpdatePlayerInput(PlayerIndex? controllingPlayer)
		{
			Mouse.SetPosition(screenCenterX, screenCenterY);
			cursorPosition.X += CurrentMouseState.X - screenCenterX;
			cursorPosition.Y += CurrentMouseState.Y - screenCenterY;
			if (cursorPosition.Length() > InputHelper.CursorRange)
			{
				cursorPosition = Vector2.Normalize(cursorPosition) * InputHelper.CursorRange;
			}
			cursorPosition.X = (int)cursorPosition.X;
			cursorPosition.Y = (int)cursorPosition.Y;
			if (cursorPosition.X != 0f)
			{
				cursorXSide = Math.Sign(cursorPosition.X);
			}
			if (IsNewKeyPress(Globals.InputKeyDown, controllingPlayer, out var playerIndex))
			{
				timeDownPressed[(int)playerIndex] = 0.0;
			}
			else if (IsCurrentKeyPress(Globals.InputKeyDown, controllingPlayer, out playerIndex))
			{
				timeDownPressed[(int)playerIndex] += GameElementsControl.RealLastFrameTimeInMS;
			}
			if (IsReleasedKeyPress(Globals.InputKeyUp, controllingPlayer, out playerIndex))
			{
				timeUpPressed[(int)playerIndex] = GameElementsControl.RealTimeInMS;
			}
		}

		public bool IsReservedPress(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) && !IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) && !IsNewKeyPress(Keys.Left, controllingPlayer, out playerIndex) && !IsNewKeyPress(Keys.Right, controllingPlayer, out playerIndex) && !IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex))
			{
				return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!IsNewKeyPress(Globals.InputKeyJump, controllingPlayer, out playerIndex))
			{
				return IsNewKeyPress(Globals.InputKeyStart, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			return IsNewKeyPress(Globals.InputKeyBack, controllingPlayer, out playerIndex);
		}

		public bool IsMenuUp(PlayerIndex? controllingPlayer)
		{
			if (!IsNewKeyPress(Keys.Up, controllingPlayer, out var playerIndex))
			{
				return IsNewKeyPress(Globals.InputKeyUp, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuDown(PlayerIndex? controllingPlayer)
		{
			if (!IsNewKeyPress(Keys.Down, controllingPlayer, out var playerIndex))
			{
				return IsNewKeyPress(Globals.InputKeyDown, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuLeft(PlayerIndex? controllingPlayer)
		{
			if (!IsNewKeyPress(Keys.Left, controllingPlayer, out var playerIndex))
			{
				return IsNewKeyPress(Globals.InputKeyLeft, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuRight(PlayerIndex? controllingPlayer)
		{
			if (!IsNewKeyPress(Keys.Right, controllingPlayer, out var playerIndex))
			{
				return IsNewKeyPress(Globals.InputKeyRight, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool ControlPauseGame(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			return IsNewKeyPress(Globals.InputKeyBack, controllingPlayer, out playerIndex);
		}

		public bool ControlRight(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentKeyPress(Globals.InputKeyRight, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlLeft(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentKeyPress(Globals.InputKeyLeft, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlUp(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentKeyPress(Globals.InputKeyUp, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlDown(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentKeyPress(Globals.InputKeyDown, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlJumpingDown(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				bool flag = IsCurrentKeyPress(Globals.InputKeyDown, controllingPlayer, out playerIndex) && IsCurrentKeyPress(Globals.InputKeyJump, controllingPlayer, out playerIndex);
				if (flag)
				{
					int value = (int)controllingPlayer.Value;
					timeDownPressed[value] = 200.0;
				}
				return flag;
			}
			return false;
		}

		public bool ControlJump(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				if (IsNewMouseButtonPress(Globals.InputMouseButtonJump) || IsNewKeyPress(Globals.InputKeyJump, controllingPlayer, out var _))
				{
					return !ControlDown(controllingPlayer);
				}
				return false;
			}
			return false;
		}

		public bool ControlHoldJump(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsCurrentMouseButtonPress(Globals.InputMouseButtonJump))
				{
					return IsCurrentKeyPress(Globals.InputKeyJump, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlReload(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsNewMouseButtonPress(Globals.InputMouseButtonReload))
				{
					return IsNewKeyPress(Globals.InputKeyReload, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlRun(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsNewMouseButtonPress(Globals.InputMouseButtonSlide))
				{
					return IsCurrentKeyPress(Globals.InputKeySlide, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlSlideDown(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				int value = (int)controllingPlayer.Value;
				if (IsReleasedKeyPress(Globals.InputKeyDown, controllingPlayer, out var _))
				{
					return timeDownPressed[value] <= 200.0;
				}
				return false;
			}
			return false;
		}

		public bool ControlSlideUp(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				_ = controllingPlayer.Value;
				PlayerIndex playerIndex;
				return IsNewKeyPress(Globals.InputKeyUp, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlBulletTime(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsNewMouseButtonPress(Globals.InputMouseButtonSloMo))
				{
					return IsNewKeyPress(Globals.InputKeySloMo, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlShoot(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsNewMouseButtonPress(Globals.InputMouseButtonShoot))
				{
					return IsNewKeyPress(Globals.InputKeyShoot, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlSecondaryShoot(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsCurrentMouseButtonPress(Globals.InputMouseButtonSecShoot))
				{
					return IsCurrentKeyPress(Globals.InputKeySecShoot, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlKick(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsNewMouseButtonPress(Globals.InputMouseButtonKick))
				{
					return IsNewKeyPress(Globals.InputKeyKick, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlChangeWeapon(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsNewMouseButtonPress(Globals.InputMouseButtonSwitch))
				{
					return IsNewKeyPress(Globals.InputKeySwitch, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlGrab(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (!IsNewMouseButtonPress(Globals.InputMouseButtonJump))
				{
					return IsCurrentKeyPress(Globals.InputKeyJump, controllingPlayer, out playerIndex);
				}
				return true;
			}
			return false;
		}

		public bool ControlClimb(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentKeyPress(Globals.InputKeyUp, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlDetach(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				return ControlSlideDown(controllingPlayer);
			}
			return false;
		}

		public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				int num = (int)playerIndex;
				if (CurrentKeyboardStates[num].IsKeyDown(key))
				{
					return LastKeyboardStates[num].IsKeyUp(key);
				}
				return false;
			}
			if (!IsNewKeyPress(key, PlayerIndex.One, out playerIndex) && !IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) && !IsNewKeyPress(key, PlayerIndex.Three, out playerIndex))
			{
				return IsNewKeyPress(key, PlayerIndex.Four, out playerIndex);
			}
			return true;
		}

		public bool IsCurrentKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				int num = (int)playerIndex;
				return CurrentKeyboardStates[num].IsKeyDown(key);
			}
			if (!IsCurrentKeyPress(key, PlayerIndex.One, out playerIndex) && !IsCurrentKeyPress(key, PlayerIndex.Two, out playerIndex) && !IsCurrentKeyPress(key, PlayerIndex.Three, out playerIndex))
			{
				return IsCurrentKeyPress(key, PlayerIndex.Four, out playerIndex);
			}
			return true;
		}

		public bool IsReleasedKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				int num = (int)playerIndex;
				if (CurrentKeyboardStates[num].IsKeyUp(key))
				{
					return LastKeyboardStates[num].IsKeyDown(key);
				}
				return false;
			}
			if (!IsReleasedKeyPress(key, PlayerIndex.One, out playerIndex) && !IsReleasedKeyPress(key, PlayerIndex.Two, out playerIndex) && !IsReleasedKeyPress(key, PlayerIndex.Three, out playerIndex))
			{
				return IsReleasedKeyPress(key, PlayerIndex.Four, out playerIndex);
			}
			return true;
		}

		public bool IsNewMouseButtonPress(MouseButtons button)
		{
			switch (button)
			{
			case MouseButtons.LeftButton:
				if (LastMouseState.LeftButton == ButtonState.Released)
				{
					return CurrentMouseState.LeftButton == ButtonState.Pressed;
				}
				return false;
			case MouseButtons.MiddleButton:
				if (LastMouseState.MiddleButton == ButtonState.Released)
				{
					return CurrentMouseState.MiddleButton == ButtonState.Pressed;
				}
				return false;
			case MouseButtons.RightButton:
				if (LastMouseState.RightButton == ButtonState.Released)
				{
					return CurrentMouseState.RightButton == ButtonState.Pressed;
				}
				return false;
			case MouseButtons.ExtraButton1:
				if (LastMouseState.XButton1 == ButtonState.Released)
				{
					return CurrentMouseState.XButton1 == ButtonState.Pressed;
				}
				return false;
			case MouseButtons.ExtraButton2:
				if (LastMouseState.XButton2 == ButtonState.Released)
				{
					return CurrentMouseState.XButton2 == ButtonState.Pressed;
				}
				return false;
			default:
				return false;
			}
		}

		public bool IsCurrentMouseButtonPress(MouseButtons button)
		{
			switch (button)
			{
			case MouseButtons.LeftButton:
				return CurrentMouseState.LeftButton == ButtonState.Pressed;
			case MouseButtons.MiddleButton:
				return CurrentMouseState.MiddleButton == ButtonState.Pressed;
			case MouseButtons.RightButton:
				return CurrentMouseState.RightButton == ButtonState.Pressed;
			case MouseButtons.ExtraButton1:
				return CurrentMouseState.XButton1 == ButtonState.Pressed;
			case MouseButtons.ExtraButton2:
				return CurrentMouseState.XButton2 == ButtonState.Pressed;
			default:
				return false;
			}
		}

		public bool IsReleasedMouseButtonPress(MouseButtons button)
		{
			switch (button)
			{
			case MouseButtons.LeftButton:
				if (LastMouseState.LeftButton == ButtonState.Pressed)
				{
					return CurrentMouseState.LeftButton == ButtonState.Released;
				}
				return false;
			case MouseButtons.MiddleButton:
				if (LastMouseState.MiddleButton == ButtonState.Pressed)
				{
					return CurrentMouseState.MiddleButton == ButtonState.Released;
				}
				return false;
			case MouseButtons.RightButton:
				if (LastMouseState.RightButton == ButtonState.Pressed)
				{
					return CurrentMouseState.RightButton == ButtonState.Released;
				}
				return false;
			case MouseButtons.ExtraButton1:
				if (LastMouseState.XButton1 == ButtonState.Pressed)
				{
					return CurrentMouseState.XButton1 == ButtonState.Released;
				}
				return false;
			case MouseButtons.ExtraButton2:
				if (LastMouseState.XButton2 == ButtonState.Pressed)
				{
					return CurrentMouseState.XButton2 == ButtonState.Released;
				}
				return false;
			default:
				return false;
			}
		}

		public Keys GetNewKeyPress(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			Keys keys = Keys.None;
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				int num = (int)playerIndex;
				Keys[] pressedKeys = CurrentKeyboardStates[num].GetPressedKeys();
				foreach (Keys keys2 in pressedKeys)
				{
					if (LastKeyboardStates[num].IsKeyUp(keys2))
					{
						keys = keys2;
						break;
					}
				}
			}
			else
			{
				keys = GetNewKeyPress(PlayerIndex.One, out playerIndex);
				if (keys == Keys.None)
				{
					keys = GetNewKeyPress(PlayerIndex.Two, out playerIndex);
				}
				else if (keys == Keys.None)
				{
					keys = GetNewKeyPress(PlayerIndex.Three, out playerIndex);
				}
				else if (keys == Keys.None)
				{
					keys = GetNewKeyPress(PlayerIndex.Four, out playerIndex);
				}
			}
			return keys;
		}

		public MouseButtons GetNewMouseButtonPress()
		{
			if (IsNewMouseButtonPress(MouseButtons.ExtraButton1))
			{
				return MouseButtons.ExtraButton1;
			}
			if (IsNewMouseButtonPress(MouseButtons.ExtraButton2))
			{
				return MouseButtons.ExtraButton2;
			}
			if (IsNewMouseButtonPress(MouseButtons.LeftButton))
			{
				return MouseButtons.LeftButton;
			}
			if (IsNewMouseButtonPress(MouseButtons.MiddleButton))
			{
				return MouseButtons.MiddleButton;
			}
			if (IsNewMouseButtonPress(MouseButtons.RightButton))
			{
				return MouseButtons.RightButton;
			}
			return MouseButtons.None;
		}

		public float GetAngle(Vector2 refPos)
		{
			Vector2 v = cursorPosition;
			v.Y = 0f - v.Y;
			if (v.X == 0f)
			{
				v.X = cursorXSide;
			}
			return 0f - v.GetAngle();
		}
	}
}
