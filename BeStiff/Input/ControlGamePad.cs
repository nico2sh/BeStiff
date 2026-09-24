using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Be_Stiff.Input
{
	public class ControlGamePad : IControl
	{
		public readonly GamePadState[] CurrentGamePadStates;

		public readonly GamePadState[] LastGamePadStates;

		private GamePadDeadZone GamePadDeadZone;

		private double[] timeDownPressed;

		private bool[] doubleDownPressed;

		private double[] timeUpPressed;

		private bool[] doubleUpPressed;

		public readonly bool[] GamePadWasConnected;

		private Vector2 cursorPosition = Vector2.Zero;

		private int cursorXSide;

		public Vector2 CursorPosition => cursorPosition;

		public ControlGamePad()
		{
			CurrentGamePadStates = new GamePadState[InputHelper.MaxInputs];
			LastGamePadStates = new GamePadState[InputHelper.MaxInputs];
			timeDownPressed = new double[InputHelper.MaxInputs];
			doubleDownPressed = new bool[InputHelper.MaxInputs];
			timeUpPressed = new double[InputHelper.MaxInputs];
			doubleUpPressed = new bool[InputHelper.MaxInputs];
			GamePadDeadZone = GamePadDeadZone.Circular;
			GamePadWasConnected = new bool[InputHelper.MaxInputs];
			cursorPosition = Vector2.Zero;
			cursorXSide = 1;
		}

		public void Update()
		{
			for (int i = 0; i < InputHelper.MaxInputs; i++)
			{
				ref GamePadState reference = ref LastGamePadStates[i];
				reference = CurrentGamePadStates[i];
				ref GamePadState reference2 = ref CurrentGamePadStates[i];
				reference2 = GamePad.GetState((PlayerIndex)i);
				if (CurrentGamePadStates[i].IsConnected)
				{
					GamePadWasConnected[i] = true;
				}
			}
		}

		public void UpdatePlayerInput(PlayerIndex? controllingPlayer)
		{
			Vector2 value = Vector2.Zero;
			if (controllingPlayer.HasValue)
			{
				int value2 = (int)controllingPlayer.Value;
				value = ((!(CurrentGamePadStates[value2].ThumbSticks.Right == Vector2.Zero)) ? (CurrentGamePadStates[value2].ThumbSticks.Right * InputHelper.CursorRange) : (CurrentGamePadStates[value2].ThumbSticks.Left * InputHelper.CursorRange));
				value.Y = 0f - value.Y;
			}
			Vector2 value3 = Vector2.Subtract(value, cursorPosition);
			float num = 0.5f * (float)GameElementsControl.RealLastFrameTimeInMS;
			if (value3.Length() <= num)
			{
				cursorPosition = value;
			}
			else
			{
				cursorPosition += Vector2.Normalize(value3) * num;
			}
			if (cursorPosition.X != 0f)
			{
				cursorXSide = Math.Sign(cursorPosition.X);
			}
			if (IsReleasedButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out var playerIndex) || IsReleasedButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex))
			{
				timeDownPressed[(int)playerIndex] = GameElementsControl.RealTimeInMS;
			}
			if (IsReleasedButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex) || IsReleasedButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex))
			{
				timeUpPressed[(int)playerIndex] = GameElementsControl.RealTimeInMS;
			}
		}

		public bool IsReservedPress(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!IsNewButtonPress(Buttons.BigButton, controllingPlayer, out playerIndex) && !IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex))
			{
				return IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!IsNewButtonPress(Globals.InputButtonJump, controllingPlayer, out playerIndex))
			{
				return IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!IsNewButtonPress(Globals.InputButtonSlide, controllingPlayer, out playerIndex))
			{
				return IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuUp(PlayerIndex? controllingPlayer)
		{
			if (!IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out var playerIndex))
			{
				return IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuDown(PlayerIndex? controllingPlayer)
		{
			if (!IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out var playerIndex))
			{
				return IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuLeft(PlayerIndex? controllingPlayer)
		{
			if (!IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out var playerIndex))
			{
				return IsNewButtonPress(Buttons.LeftThumbstickLeft, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuRight(PlayerIndex? controllingPlayer)
		{
			if (!IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out var playerIndex))
			{
				return IsNewButtonPress(Buttons.LeftThumbstickRight, controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool ControlPauseGame(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				bool result = LastGamePadStates[(int)controllingPlayer.Value].IsConnected && !CurrentGamePadStates[(int)controllingPlayer.Value].IsConnected;
				if (!IsNewButtonPress(Buttons.Back, controllingPlayer, out var playerIndex) && !IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex))
				{
					return result;
				}
				return true;
			}
			return false;
		}

		public bool ControlRight(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				if (!(CurrentGamePadStates[(int)controllingPlayer.Value].ThumbSticks.Left.X >= 0.5f))
				{
					return CurrentGamePadStates[(int)controllingPlayer.Value].DPad.Right == ButtonState.Pressed;
				}
				return true;
			}
			return false;
		}

		public bool ControlLeft(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				if (!(CurrentGamePadStates[(int)controllingPlayer.Value].ThumbSticks.Left.X <= -0.5f))
				{
					return CurrentGamePadStates[(int)controllingPlayer.Value].DPad.Left == ButtonState.Pressed;
				}
				return true;
			}
			return false;
		}

		public bool ControlUp(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				if (!(CurrentGamePadStates[(int)controllingPlayer.Value].ThumbSticks.Left.Y >= 0.5f))
				{
					return CurrentGamePadStates[(int)controllingPlayer.Value].DPad.Up == ButtonState.Pressed;
				}
				return true;
			}
			return false;
		}

		public bool ControlDown(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				if (!(CurrentGamePadStates[(int)controllingPlayer.Value].ThumbSticks.Left.Y <= -0.5f))
				{
					return CurrentGamePadStates[(int)controllingPlayer.Value].DPad.Down == ButtonState.Pressed;
				}
				return true;
			}
			return false;
		}

		public bool ControlJumpingDown(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				PlayerIndex playerIndex;
				if (CurrentGamePadStates[(int)controllingPlayer.Value].ThumbSticks.Left.Y <= -0.5f || CurrentGamePadStates[(int)controllingPlayer.Value].DPad.Down == ButtonState.Pressed)
				{
					return IsCurrentButtonPress(Globals.InputButtonJump, controllingPlayer, out playerIndex);
				}
				return false;
			}
			return false;
		}

		public bool ControlJump(PlayerIndex? controllingPlayer)
		{
			if (controllingPlayer.HasValue)
			{
				if (IsNewButtonPress(Globals.InputButtonJump, controllingPlayer, out var _))
				{
					return !ControlDown(controllingPlayer);
				}
				return false;
			}
			return false;
		}

		public bool ControlHoldJump(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentButtonPress(Globals.InputButtonJump, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlReload(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsNewButtonPress(Globals.InputButtonReload, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlRun(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentButtonPress(Globals.InputButtonRun, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlSlideDown(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsNewButtonPress(Globals.InputButtonSlide, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlSlideUp(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsNewButtonPress(Globals.InputButtonSlide, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlBulletTime(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsNewButtonPress(Globals.InputButtonSloMo, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlShoot(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsNewButtonPress(Globals.InputButtonShoot, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlSecondaryShoot(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentButtonPress(Globals.InputButtonSecShoot, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlKick(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsNewButtonPress(Globals.InputButtonKick, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlChangeWeapon(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsNewButtonPress(Globals.InputButtonSwitch, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlGrab(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;
			if (controllingPlayer.HasValue)
			{
				return IsCurrentButtonPress(Globals.InputButtonJump, controllingPlayer, out playerIndex);
			}
			return false;
		}

		public bool ControlClimb(PlayerIndex? controllingPlayer)
		{
			if (ControlUp(controllingPlayer))
			{
				return ControlJump(controllingPlayer);
			}
			return false;
		}

		public bool ControlDetach(PlayerIndex? controllingPlayer)
		{
			return ControlJumpingDown(controllingPlayer);
		}

		public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				int num = (int)playerIndex;
				if (CurrentGamePadStates[num].IsButtonDown(button))
				{
					return LastGamePadStates[num].IsButtonUp(button);
				}
				return false;
			}
			if (!IsNewButtonPress(button, PlayerIndex.One, out playerIndex) && !IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) && !IsNewButtonPress(button, PlayerIndex.Three, out playerIndex))
			{
				return IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
			}
			return true;
		}

		public bool IsCurrentButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				int num = (int)playerIndex;
				return CurrentGamePadStates[num].IsButtonDown(button);
			}
			if (!IsCurrentButtonPress(button, PlayerIndex.One, out playerIndex) && !IsCurrentButtonPress(button, PlayerIndex.Two, out playerIndex) && !IsCurrentButtonPress(button, PlayerIndex.Three, out playerIndex))
			{
				return IsCurrentButtonPress(button, PlayerIndex.Four, out playerIndex);
			}
			return true;
		}

		public bool IsReleasedButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				int num = (int)playerIndex;
				if (CurrentGamePadStates[num].IsButtonUp(button))
				{
					return LastGamePadStates[num].IsButtonDown(button);
				}
				return false;
			}
			if (!IsReleasedButtonPress(button, PlayerIndex.One, out playerIndex) && !IsReleasedButtonPress(button, PlayerIndex.Two, out playerIndex) && !IsReleasedButtonPress(button, PlayerIndex.Three, out playerIndex))
			{
				return IsReleasedButtonPress(button, PlayerIndex.Four, out playerIndex);
			}
			return true;
		}

		public bool GetNewButtonPress(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex, out Buttons button)
		{
			bool flag = false;
			button = Buttons.Back;
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;
				if (IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex))
				{
					button = Buttons.A;
					return true;
				}
				if (IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex))
				{
					button = Buttons.B;
					return true;
				}
				if (IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex))
				{
					button = Buttons.Back;
					return true;
				}
				if (IsNewButtonPress(Buttons.BigButton, controllingPlayer, out playerIndex))
				{
					button = Buttons.BigButton;
					return true;
				}
				if (IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex))
				{
					button = Buttons.DPadDown;
					return true;
				}
				if (IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out playerIndex))
				{
					button = Buttons.DPadLeft;
					return true;
				}
				if (IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out playerIndex))
				{
					button = Buttons.DPadRight;
					return true;
				}
				if (IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex))
				{
					button = Buttons.DPadUp;
					return true;
				}
				if (IsNewButtonPress(Buttons.LeftShoulder, controllingPlayer, out playerIndex))
				{
					button = Buttons.LeftShoulder;
					return true;
				}
				if (IsNewButtonPress(Buttons.LeftStick, controllingPlayer, out playerIndex))
				{
					button = Buttons.LeftStick;
					return true;
				}
				if (IsNewButtonPress(Buttons.LeftTrigger, controllingPlayer, out playerIndex))
				{
					button = Buttons.LeftTrigger;
					return true;
				}
				if (IsNewButtonPress(Buttons.RightShoulder, controllingPlayer, out playerIndex))
				{
					button = Buttons.RightShoulder;
					return true;
				}
				if (IsNewButtonPress(Buttons.RightStick, controllingPlayer, out playerIndex))
				{
					button = Buttons.RightStick;
					return true;
				}
				if (IsNewButtonPress(Buttons.RightTrigger, controllingPlayer, out playerIndex))
				{
					button = Buttons.RightTrigger;
					return true;
				}
				if (IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex))
				{
					button = Buttons.Start;
					return true;
				}
				if (IsNewButtonPress(Buttons.X, controllingPlayer, out playerIndex))
				{
					button = Buttons.X;
					return true;
				}
				if (IsNewButtonPress(Buttons.Y, controllingPlayer, out playerIndex))
				{
					button = Buttons.Y;
					return true;
				}
			}
			else
			{
				flag = GetNewButtonPress(PlayerIndex.One, out playerIndex, out button);
				if (!flag)
				{
					flag = GetNewButtonPress(PlayerIndex.Two, out playerIndex, out button);
				}
				else if (!flag)
				{
					flag = GetNewButtonPress(PlayerIndex.Three, out playerIndex, out button);
				}
				else if (!flag)
				{
					flag = GetNewButtonPress(PlayerIndex.Four, out playerIndex, out button);
				}
			}
			return flag;
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
