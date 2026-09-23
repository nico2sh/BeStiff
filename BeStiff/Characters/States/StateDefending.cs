namespace Be_Stiff.Characters.States
{
	internal class StateDefending : HeroState
	{
		public StateDefending(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 22;
		}

		internal override void OnEnter()
		{
			hero.Skeleton.SetAnimation("FSDEFEND");
			hero.ActionStopWalking();
		}

		protected override void CheckRules()
		{
			if (!hero.MustDefend)
			{
				SwitchState(3, useExitTransition: false);
			}
		}

		protected override void During()
		{
		}

		internal override void OnExit()
		{
		}
	}
}
