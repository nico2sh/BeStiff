using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters.States
{
	internal class StateWallJump : StateHighJump
	{
		private bool holdingJump;

		private double timeHoldingJump;

		private double lastJumpTime;

		private Side wallJumpSide;

		public StateWallJump(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 7;
		}

		public void SetData(Side side)
		{
			wallJumpSide = side;
		}

		internal override void OnEnter()
		{
			hero.SetFixedSide(wallJumpSide);
			hero.Skeleton.SetAnimation("JUMPCONTINUE");
			holdingJump = true;
			timeHoldingJump = 0.0;
			lastJumpTime = GameElementsControl.CurrentTimeInMS;
			hero.ActionWallJump(hero.JumpSmallHeight, wallJumpSide);
			hero.ActionStopWalking();
		}

		protected override void During()
		{
			if (holdingJump)
			{
				if (hero.Input.ControlHoldJump(hero.PlayerIndex))
				{
					timeHoldingJump += GameElementsControl.LastFrameTimeInMS;
				}
				else
				{
					holdingJump = false;
					timeHoldingJump = 0.0;
				}
			}
			base.During();
		}

		protected override void CheckRules()
		{
			if (GameElementsControl.CurrentTimeInMS - lastJumpTime > 1000.0)
			{
				SwitchState(5, useExitTransition: true);
			}
			else if (timeHoldingJump >= 180.0)
			{
				if (hero.Input.ControlUp(hero.PlayerIndex))
				{
					float num = (float)Math.Sqrt(2f * GameElementsControl.Gravity.Y * hero.JumpSmallHeight);
					float num2 = num - GameElementsControl.Gravity.Y * (float)(timeHoldingJump / 1000.0);
					float num3 = num * ((float)timeHoldingJump / 1000f) - GameElementsControl.Gravity.Y * (float)Math.Pow(timeHoldingJump / 1000.0, 2.0) / 2f;
					Vector2 impulse = new Vector2(0f, (0f - hero.Mass) * ((float)Math.Sqrt(2f * GameElementsControl.Gravity.Y * (hero.JumpHighHeight - num3)) - num2));
					hero.FeetBody.ApplyLinearImpulse(ref impulse);
					StateJumpFlipping stateJumpFlipping = stateMachine.States[8] as StateJumpFlipping;
					stateJumpFlipping.SetData(wallJumpSide);
					SwitchState(8, useExitTransition: true);
				}
				else
				{
					holdingJump = true;
					timeHoldingJump = 0.0;
				}
			}
		}

		internal override void OnExit()
		{
			hero.UnsetFixedSide();
		}
	}
}
