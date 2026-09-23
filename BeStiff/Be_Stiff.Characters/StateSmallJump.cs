using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters
{
	public class StateSmallJump : StateHighJump
	{
		private bool holdingJump;

		private double timeHoldingJump;

		private double lastJumpTime;

		public StateSmallJump(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 4;
		}

		protected override void CheckRules()
		{
			if (GameElementsControl.CurrentTimeInMS - lastJumpTime > 200.0)
			{
				base.CheckRules();
			}
			if (timeHoldingJump >= 180.0)
			{
				float num = (float)Math.Sqrt(2f * GameElementsControl.Gravity.Y * hero.JumpSmallHeight);
				float num2 = num - GameElementsControl.Gravity.Y * (float)(timeHoldingJump / 1000.0);
				float num3 = num * ((float)timeHoldingJump / 1000f) - GameElementsControl.Gravity.Y * (float)Math.Pow(timeHoldingJump / 1000.0, 2.0) / 2f;
				Vector2 impulse = new Vector2(0f, (0f - hero.Mass) * ((float)Math.Sqrt(2f * GameElementsControl.Gravity.Y * (hero.JumpHighHeight - num3)) - num2));
				hero.MainBody.ApplyLinearImpulse(ref impulse);
				SwitchState(5, useExitTransition: true);
			}
		}

		internal override void OnEnter()
		{
			hero.Skeleton.SetAnimation("JUMPSTART");
			holdingJump = true;
			timeHoldingJump = 0.0;
			lastJumpTime = GameElementsControl.CurrentTimeInMS;
			hero.ActionJump(hero.JumpSmallHeight, realJump: true);
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
	}
}
