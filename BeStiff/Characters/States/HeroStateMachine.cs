namespace Be_Stiff.Characters.States
{
	internal class HeroStateMachine : StateMachine
	{
		private Hero hero;

		private InputHelper input;

		public HeroStateMachine(InputHelper i, Hero h)
		{
			hero = h;
			input = i;
			AddState(new StateStanding(this, hero));
			AddState(new StateWalking(this, hero));
			AddState(new StateBackWalking(this, hero));
			AddState(new StateRunning(this, hero));
			AddState(new StateTransitionLie(this, hero));
			AddState(new StateLied(this, hero));
			AddState(new StateTransitionStand(this, hero));
			AddState(new StateCrawling(this, hero));
			AddState(new StateSmallJump(this, hero));
			AddState(new StateHighJump(this, hero));
			AddState(new StateWallSliding(this, hero));
			AddState(new StateJumpLied(this, hero));
			AddState(new StateWallJump(this, hero));
			AddState(new StateJumpFlipping(this, hero));
			AddState(new StateHanging(this, hero));
			AddState(new StateClimbing(this, hero));
			AddState(new StateKicking(this, hero));
			AddState(new StateKicking2(this, hero));
			AddState(new StatePunching(this, hero));
			AddState(new StatePunching2(this, hero));
			AddState(new StateStunned(this, hero));
			AddState(new StateHit(this, hero));
			AddState(new StateSwinging(this, hero));
			AddState(new StateDefending(this, hero));
			base.CurrentState = base.States[3];
			base.CurrentState.OnEnter();
		}

		protected override string DebugInfo()
		{
			return $"rot={hero.MainBody.Rotation:F2} pos={hero.Position} energy={hero.Energy:F1}";
		}

		public override void Update()
		{
			bool stun = CheckEvent(3);
			if (base.CurrentState.StateID != 24)
			{
				if (CheckEvent(1))
				{
					StateHit stateHit = base.States[23] as StateHit;
					stateHit.SetData(Side.Left, stun);
					base.CurrentState.SwitchState(23, useExitTransition: false);
				}
				else if (CheckEvent(2))
				{
					StateHit stateHit2 = base.States[23] as StateHit;
					stateHit2.SetData(Side.Right, stun);
					base.CurrentState.SwitchState(23, useExitTransition: false);
				}
				else if (CheckEvent(3))
				{
					base.CurrentState.SwitchState(24, useExitTransition: false);
				}
			}
			base.Update();
		}
	}
}
