namespace Be_Stiff.Characters.States
{
	public class StateRunning : StateWalking
	{
		public StateRunning(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 1;
		}

		protected override void During()
		{
			base.During();
			hero.DrainStamina();
		}

		protected override bool CheckSpecificRules()
		{
			if (!hero.Input.ControlRun(hero.PlayerIndex))
			{
				SwitchState(0, useExitTransition: false);
				return true;
			}
			if (hero.SideLooking != sideWalking)
			{
				SwitchState(2, useExitTransition: false);
				return true;
			}
			if (hero.Input.ControlLeft(hero.PlayerIndex) && sideWalking == Side.Right)
			{
				SwitchState(2, useExitTransition: true);
				return true;
			}
			if (hero.Input.ControlRight(hero.PlayerIndex) && sideWalking == Side.Left)
			{
				SwitchState(2, useExitTransition: true);
				return true;
			}
			if (hero.StaminaDrained)
			{
				SwitchState(0, useExitTransition: true);
				return true;
			}
			return false;
		}

		internal override void OnEnter()
		{
			if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				sideWalking = Side.Left;
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				sideWalking = Side.Right;
			}
			hero.Skeleton.SetAnimation("RUN");
			hero.WheelRevoluteJoint.Enabled = true;
			moveSpeed = hero.RunSpeed;
		}

		internal override void OnExit()
		{
		}

		protected override bool Exiting()
		{
			hero.ActionStopWalking();
			hero.Skeleton.SetAnimation("RUNSTOPPING");
			return hero.Skeleton.AnimationEnded();
		}
	}
}
