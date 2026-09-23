namespace Be_Stiff.Characters
{
	internal class StatePunching : StatePunching2
	{
		private bool secondPunch;

		public StatePunching(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 19;
		}

		internal override void OnEnter()
		{
			base.OnEnter();
			secondPunch = false;
			hero.Skeleton.SetAnimation("PUNCH");
			punchFrame = 4;
		}

		protected override void During()
		{
			base.During();
			if (stateMachine.CheckEvent(0))
			{
				secondPunch = true;
			}
		}

		protected override void CheckRules()
		{
			base.CheckRules();
			if (secondPunch && !chainKick && hero.Skeleton.CurrenKeyFrame >= 5)
			{
				SwitchState(20, useExitTransition: false);
			}
		}
	}
}
