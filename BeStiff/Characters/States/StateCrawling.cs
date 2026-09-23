namespace Be_Stiff.Characters.States
{
	internal class StateCrawling : StateLied
	{
		protected Side sideWalking;

		protected float moveSpeed;

		public StateCrawling(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 11;
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
			moveSpeed = hero.RunSpeed / 3f;
		}

		protected override bool Entering()
		{
			return true;
		}

		protected override void During()
		{
			if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				if (hero.LiedSide() == Side.Right)
				{
					hero.Skeleton.SetAnimation("FRONTCRAWL");
				}
				else
				{
					hero.Skeleton.SetAnimation("BACKCRAWL");
				}
			}
			if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				if (hero.LiedSide() == Side.Left)
				{
					hero.Skeleton.SetAnimation("FRONTCRAWL");
				}
				else
				{
					hero.Skeleton.SetAnimation("BACKCRAWL");
				}
			}
			hero.ActionCrawl(sideWalking, moveSpeed);
		}

		protected override bool Exiting()
		{
			return true;
		}

		internal override void OnExit()
		{
			hero.ActionStopCrawling();
		}

		protected override void CheckRules()
		{
			base.CheckRules();
		}

		protected override void CheckSpecificRules()
		{
			bool flag = hero.Input.ControlLeft(hero.PlayerIndex);
			bool flag2 = hero.Input.ControlRight(hero.PlayerIndex);
			if (!flag && !flag2)
			{
				SwitchState(10, useExitTransition: true);
			}
			else if (flag && flag2)
			{
				SwitchState(10, useExitTransition: false);
			}
			else if (flag && sideWalking == Side.Right)
			{
				SwitchState(10, useExitTransition: false);
			}
			else if (flag2 && sideWalking == Side.Left)
			{
				SwitchState(10, useExitTransition: false);
			}
			else if (!hero.Landed())
			{
				SwitchState(6, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				hero.ActionJump(1f, realJump: true);
				SwitchState(6, useExitTransition: false);
			}
		}
	}
}
