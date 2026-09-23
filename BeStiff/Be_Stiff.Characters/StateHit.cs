namespace Be_Stiff.Characters
{
	internal class StateHit : HeroState
	{
		private Side hitSide;

		private double hitTime;

		private bool shouldStun;

		private double timeChangeState;

		public StateHit(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 23;
		}

		public void SetData(Side side, bool stun)
		{
			hitSide = side;
			shouldStun = stun;
			if (stun)
			{
				timeChangeState = 300.0;
			}
			else
			{
				timeChangeState = 600.0;
			}
		}

		internal override void OnEnter()
		{
			hitTime = GameElementsControl.CurrentTimeInMS;
			hero.SetFixedSide(hero.SideLooking);
			if (hero.TouchingFloor())
			{
				if (hero.SideLooking == hitSide)
				{
					hero.Skeleton.SetAnimation("HIT");
				}
				else
				{
					hero.Skeleton.SetAnimation("HITBACK");
				}
			}
			else if (hero.SideLooking == hitSide)
			{
				hero.Skeleton.SetAnimation("DYINGBEFORERAGDOLL");
			}
			else
			{
				hero.Skeleton.SetAnimation("DYINGBEFORERAGDOLL2");
			}
			hero.WheelRevoluteJoint.MotorEnabled = false;
		}

		protected override void CheckRules()
		{
			if (!(GameElementsControl.CurrentTimeInMS - hitTime >= timeChangeState))
			{
				return;
			}
			if (shouldStun)
			{
				SwitchState(24, useExitTransition: false);
			}
			else if (hero.TouchingFloor())
			{
				if (hero.IsLied())
				{
					SwitchState(10, useExitTransition: false);
				}
				else
				{
					SwitchState(3, useExitTransition: false);
				}
			}
			else
			{
				SwitchState(5, useExitTransition: false);
			}
		}

		protected override void During()
		{
		}

		internal override void OnExit()
		{
			hero.ActionStopWalking();
			hero.UnsetFixedSide();
		}
	}
}
