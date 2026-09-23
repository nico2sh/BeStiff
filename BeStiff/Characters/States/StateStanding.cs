namespace Be_Stiff.Characters.States
{
	public class StateStanding : HeroState
	{
		public StateStanding(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 3;
		}

		protected override void CheckRules()
		{
			if (hero.Input.ControlKick(hero.PlayerIndex))
			{
				SwitchState(17, useExitTransition: true);
			}
			else if (stateMachine.CheckEvent(0))
			{
				SwitchState(19, useExitTransition: true);
			}
			else if (hero.MustDefend)
			{
				SwitchState(22, useExitTransition: false);
			}
			else if (hero.Input.ControlLeft(hero.PlayerIndex) && hero.SideLooking == Side.Left)
			{
				SwitchState(0, useExitTransition: true);
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex) && hero.SideLooking == Side.Right)
			{
				SwitchState(0, useExitTransition: true);
			}
			else if (hero.Input.ControlLeft(hero.PlayerIndex) && hero.SideLooking == Side.Right)
			{
				SwitchState(2, useExitTransition: true);
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex) && hero.SideLooking == Side.Left)
			{
				SwitchState(2, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				SwitchState(4, useExitTransition: true);
			}
			else if (hero.Input.ControlSlideDown(hero.PlayerIndex))
			{
				SwitchState(9, useExitTransition: true);
			}
			else if (!hero.TouchingFloor())
			{
				SwitchState(5, useExitTransition: true);
			}
		}

		internal override void OnEnter()
		{
			hero.Skeleton.SetAnimation("STAND");
			hero.ActionStopWalking();
		}

		protected override bool Entering()
		{
			return true;
		}

		protected override bool Exiting()
		{
			return true;
		}

		internal override void OnExit()
		{
		}

		protected override void During()
		{
		}
	}
}
