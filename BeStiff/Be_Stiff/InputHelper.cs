using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Be_Stiff
{
	public class InputHelper
	{
		public static int MaxInputs = 4;

		public static double TimeForDoubleTap = 500.0;

		public static float CursorRange = 150f;

		private IControl[] controls;

		public ControlKeyboard ControlKeyboard => (ControlKeyboard)controls[1];

		public ControlGamePad ControlGamePad => (ControlGamePad)controls[0];

		public Vector2 CursorPosition => controls[Globals.ActiveControl].CursorPosition;

		public InputHelper()
		{
			controls = new IControl[2];
			controls[0] = new ControlGamePad();
			controls[1] = new ControlKeyboard();
		}

		public void Initialize(Vector2 screenCenter)
		{
			((ControlKeyboard)controls[1]).Initialize(screenCenter);
		}

		public void Update()
		{
			controls[0].Update();
			controls[1].Update();
		}

		public void UpdatePlayerInput(PlayerIndex? controllingPlayer)
		{
			controls[Globals.ActiveControl].UpdatePlayerInput(controllingPlayer);
		}

		public bool IsGamePadConnected()
		{
			bool flag = false;
			for (int i = 0; i < MaxInputs; i++)
			{
				flag = flag || GamePad.GetState((PlayerIndex)i).IsConnected;
			}
			return flag;
		}

		public bool IsReservedPress(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!controls[0].IsReservedPress(controllingPlayer, out playerIndex))
			{
				return controls[1].IsReservedPress(controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!controls[0].IsMenuSelect(controllingPlayer, out playerIndex))
			{
				return controls[1].IsMenuSelect(controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (!controls[0].IsMenuCancel(controllingPlayer, out playerIndex))
			{
				return controls[1].IsMenuCancel(controllingPlayer, out playerIndex);
			}
			return true;
		}

		public bool IsMenuUp(PlayerIndex? controllingPlayer)
		{
			if (!controls[0].IsMenuUp(controllingPlayer))
			{
				return controls[1].IsMenuUp(controllingPlayer);
			}
			return true;
		}

		public bool IsMenuDown(PlayerIndex? controllingPlayer)
		{
			if (!controls[0].IsMenuDown(controllingPlayer))
			{
				return controls[1].IsMenuDown(controllingPlayer);
			}
			return true;
		}

		public bool IsMenuLeft(PlayerIndex? controllingPlayer)
		{
			if (!controls[0].IsMenuLeft(controllingPlayer))
			{
				return controls[1].IsMenuLeft(controllingPlayer);
			}
			return true;
		}

		public bool IsMenuRight(PlayerIndex? controllingPlayer)
		{
			if (!controls[0].IsMenuRight(controllingPlayer))
			{
				return controls[1].IsMenuRight(controllingPlayer);
			}
			return true;
		}

		public bool IsPauseGame(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlPauseGame(controllingPlayer);
		}

		public bool ControlRight(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlRight(controllingPlayer);
		}

		public bool ControlLeft(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlLeft(controllingPlayer);
		}

		public bool ControlUp(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlUp(controllingPlayer);
		}

		public bool ControlDown(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlDown(controllingPlayer);
		}

		public bool ControlJumpingDown(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlJumpingDown(controllingPlayer);
		}

		public bool ControlJump(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlJump(controllingPlayer);
		}

		public bool ControlHoldJump(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlHoldJump(controllingPlayer);
		}

		public bool ControlReload(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlReload(controllingPlayer);
		}

		public bool ControlRun(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlRun(controllingPlayer);
		}

		public bool ControlSlideDown(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlSlideDown(controllingPlayer);
		}

		public bool ControlSlideUp(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlSlideUp(controllingPlayer);
		}

		public bool ControlBulletTime(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlBulletTime(controllingPlayer);
		}

		public bool ControlShoot(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlShoot(controllingPlayer);
		}

		public bool ControlSecondaryShoot(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlSecondaryShoot(controllingPlayer);
		}

		public bool ControlKick(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlKick(controllingPlayer);
		}

		public bool ControlChangeWeapon(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlChangeWeapon(controllingPlayer);
		}

		public bool ControlGrab(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlGrab(controllingPlayer);
		}

		public bool ControlClimb(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlClimb(controllingPlayer);
		}

		public bool ControlDetach(PlayerIndex? controllingPlayer)
		{
			return controls[Globals.ActiveControl].ControlDetach(controllingPlayer);
		}

		public float GetAngle(Vector2 refPos)
		{
			return controls[Globals.ActiveControl].GetAngle(refPos);
		}
	}
}
