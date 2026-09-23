using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters.States
{
	internal class StateStunned : HeroState
	{
		private double stunTime;

		public StateStunned(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 24;
		}

		internal override void OnEnter()
		{
			stunTime = GameElementsControl.CurrentTimeInMS;
			hero.Skeleton.MakePuppet(puppet: true);
			hero.Balance = false;
			hero.SetFixedSide(hero.SideLooking);
			hero.ActionStopWalking();
			hero.ActionStopCrawling();
		}

		protected override void During()
		{
			float num = MathHelper.WrapAngle(hero.MainBody.Rotation);
			if (num < (float)Math.PI * -3f / 4f || num > (float)Math.PI * 3f / 4f)
			{
				hero.Balance = true;
				float num2 = Math.Abs(num + (float)Math.PI * 3f / 4f);
				float num3 = Math.Abs(num - (float)Math.PI * 3f / 4f);
				if (num2 < num3)
				{
					hero.BaseAngle = -(float)Math.PI / 2f;
				}
				else
				{
					hero.BaseAngle = (float)Math.PI / 2f;
				}
			}
			else
			{
				hero.Balance = false;
			}
		}

		protected override void CheckRules()
		{
			if (stunTime + 2500.0 < GameElementsControl.CurrentTimeInMS)
			{
				float num = MathHelper.WrapAngle(hero.MainBody.Rotation);
				if (num <= (0f - Globals.FloorAngleLimit) * 1.5f)
				{
					hero.SetFixedSide(Side.Right);
					SwitchState(9, useExitTransition: false);
				}
				else if (num >= Globals.FloorAngleLimit * 1.5f)
				{
					hero.SetFixedSide(Side.Left);
					SwitchState(9, useExitTransition: false);
				}
				else
				{
					SwitchState(12, useExitTransition: false);
				}
			}
		}

		internal override void OnExit()
		{
			hero.Balance = true;
			hero.Skeleton.Angle = (float)(0 - hero.SideLooking) * hero.MainBody.Rotation + (float)Math.PI;
			hero.Skeleton.MakePuppet(puppet: false);
			hero.UnsetFixedSide();
		}
	}
}
