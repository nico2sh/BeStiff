using Microsoft.Xna.Framework;

namespace Be_Stiff.Input
{
	internal interface IControl
	{
		Vector2 CursorPosition { get; }

		void Update();

		void UpdatePlayerInput(PlayerIndex? controllingPlayer);

		bool IsReservedPress(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

		bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

		bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

		bool IsMenuUp(PlayerIndex? controllingPlayer);

		bool IsMenuDown(PlayerIndex? controllingPlayer);

		bool IsMenuLeft(PlayerIndex? controllingPlayer);

		bool IsMenuRight(PlayerIndex? controllingPlayer);

		bool ControlPauseGame(PlayerIndex? controllingPlayer);

		bool ControlRight(PlayerIndex? controllingPlayer);

		bool ControlLeft(PlayerIndex? controllingPlayer);

		bool ControlUp(PlayerIndex? controllingPlayer);

		bool ControlDown(PlayerIndex? controllingPlayer);

		bool ControlJumpingDown(PlayerIndex? controllingPlayer);

		bool ControlJump(PlayerIndex? controllingPlayer);

		bool ControlHoldJump(PlayerIndex? controllingPlayer);

		bool ControlReload(PlayerIndex? controllingPlayer);

		bool ControlRun(PlayerIndex? controllingPlayer);

		bool ControlSlideDown(PlayerIndex? controllingPlayer);

		bool ControlSlideUp(PlayerIndex? controllingPlayer);

		bool ControlBulletTime(PlayerIndex? controllingPlayer);

		bool ControlShoot(PlayerIndex? controllingPlayer);

		bool ControlSecondaryShoot(PlayerIndex? controllingPlayer);

		bool ControlKick(PlayerIndex? controllingPlayer);

		bool ControlChangeWeapon(PlayerIndex? controllingPlayer);

		bool ControlGrab(PlayerIndex? controllingPlayer);

		bool ControlClimb(PlayerIndex? controllingPlayer);

		bool ControlDetach(PlayerIndex? controllingPlayer);

		float GetAngle(Vector2 refPos);
	}
}
