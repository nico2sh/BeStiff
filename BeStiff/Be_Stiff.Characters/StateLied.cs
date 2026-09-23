using System;

namespace Be_Stiff.Characters
{
	internal class StateLied : HeroState
	{
		public StateLied(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 10;
		}

		internal override void OnEnter()
		{
			hero.Skeleton.SetAnimation("LIED");
		}

		protected override bool Entering()
		{
			return true;
		}

		protected override void During()
		{
		}

		protected override bool Exiting()
		{
			return true;
		}

		internal override void OnExit()
		{
		}

		protected override void CheckRules()
		{
			if (hero.Input.ControlSlideUp(hero.PlayerIndex))
			{
				SwitchState(12, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				hero.ActionJump(0.5f, realJump: true);
				SwitchState(6, useExitTransition: true);
			}
			else if (!hero.IsLied())
			{
				SwitchState(12, Math.Round(hero.FloorAngleDiff(), 1) != 0.0);
			}
			else
			{
				CheckSpecificRules();
			}
		}

		protected virtual void CheckSpecificRules()
		{
			if (hero.Input.ControlLeft(hero.PlayerIndex) || hero.Input.ControlRight(hero.PlayerIndex))
			{
				SwitchState(11, useExitTransition: false);
			}
			else if (!hero.Landed())
			{
				SwitchState(6, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				hero.ActionJump(1f, realJump: true);
				SwitchState(6, useExitTransition: false);
			}
		}
	}
}
