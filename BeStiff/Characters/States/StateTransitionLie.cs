using System;

namespace Be_Stiff.Characters.States
{
	internal class StateTransitionLie : HeroState
	{
		private double timeStartSliding;

		private Side sideLying;

		public StateTransitionLie(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 9;
		}

		internal override void OnEnter()
		{
			hero.SetFixedSide(hero.SideLooking);
			timeStartSliding = GameElementsControl.CurrentTimeInMS;
			hero.Skeleton.SetAnimation("CRAWL");
			sideLying = hero.SideLooking;
			hero.ActionSlide(sideLying);
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
		}

		protected override void CheckRules()
		{
			if (Math.Round(hero.FloorAngleDiff(), 1) == 0.0)
			{
				hero.SetPivotJoint(sideLying);
				SwitchState(10, useExitTransition: true);
			}
			else if (GameElementsControl.CurrentTimeInMS - timeStartSliding > 1000.0)
			{
				SwitchState(12, useExitTransition: true);
			}
			else if (hero.Input.ControlSlideUp(hero.PlayerIndex))
			{
				SwitchState(12, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				hero.ActionJump(1f, realJump: true);
			}
		}
	}
}
