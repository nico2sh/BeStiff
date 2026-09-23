namespace Be_Stiff.Characters
{
	public class StateBackWalking : StateWalking
	{
		public StateBackWalking(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 2;
		}

		protected override bool CheckSpecificRules()
		{
			if (hero.SideLooking == sideWalking)
			{
				SwitchState(0, useExitTransition: false);
				return true;
			}
			if (hero.Input.ControlLeft(hero.PlayerIndex) && sideWalking == Side.Right)
			{
				SwitchState(0, useExitTransition: true);
				return true;
			}
			if (hero.Input.ControlRight(hero.PlayerIndex) && sideWalking == Side.Left)
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
			hero.Skeleton.SetAnimation("BACKWALK");
			hero.WheelRevoluteJoint.Enabled = true;
			moveSpeed = hero.RunSpeed / 2f;
		}

		internal override void OnExit()
		{
		}

		protected override bool Exiting()
		{
			hero.ActionStopWalking();
			hero.Skeleton.SetAnimation("BACKWALKSTOP");
			return hero.Skeleton.AnimationEnded();
		}
	}
}
