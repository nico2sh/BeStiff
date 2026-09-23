namespace Be_Stiff.Characters.States
{
	internal class StateKicking : StateKicking2
	{
		private bool secondKick;

		private bool chainPunch;

		public StateKicking(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 17;
			kickFrame = 3;
		}

		internal override void OnEnter()
		{
			base.OnEnter();
			secondKick = false;
			chainPunch = false;
			if (sideKicking == hero.SideLooking)
			{
				kickFrame = 3;
				hero.Skeleton.SetAnimation("KICK2");
			}
			else
			{
				kickFrame = 3;
				hero.Skeleton.SetAnimation("KICKBACK");
			}
		}

		protected override void During()
		{
			base.During();
			if (!secondKick && stateMachine.CheckEvent(0))
			{
				chainPunch = true;
			}
			if (!chainPunch && hero.Input.ControlKick(hero.PlayerIndex))
			{
				secondKick = true;
			}
		}

		protected override void CheckRules()
		{
			if (chainPunch)
			{
				if (hero.Skeleton.CurrenKeyFrame >= 4)
				{
					SwitchState(20, useExitTransition: false);
					return;
				}
			}
			else if (secondKick && hero.Skeleton.CurrenKeyFrame >= 4)
			{
				SwitchState(18, useExitTransition: false);
				return;
			}
			base.CheckRules();
		}
	}
}
