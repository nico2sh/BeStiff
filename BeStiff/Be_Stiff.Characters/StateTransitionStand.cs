using System;

namespace Be_Stiff.Characters
{
	internal class StateTransitionStand : HeroState
	{
		private double timeStartSliding;

		public StateTransitionStand(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 12;
		}

		internal override void OnEnter()
		{
			timeStartSliding = GameElementsControl.CurrentTimeInMS;
			hero.Skeleton.SetAnimation("LIED");
			hero.ActionStand();
			hero.SetPivotJoint(Side.None);
		}

		protected override bool Entering()
		{
			return true;
		}

		protected override void During()
		{
		}

		internal override void OnExit()
		{
			hero.UnsetFixedSide();
		}

		protected override void CheckRules()
		{
			if (Math.Round(hero.FloorAngleDiff(), 1) == 0.0)
			{
				SwitchState(3, useExitTransition: true);
			}
			else if (GameElementsControl.CurrentTimeInMS - timeStartSliding > 1000.0)
			{
				SwitchState(9, useExitTransition: true);
			}
			else if (hero.Input.ControlSlideDown(hero.PlayerIndex))
			{
				SwitchState(9, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				hero.ActionJump(1f, realJump: true);
			}
		}
	}
}
