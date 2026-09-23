using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters.States
{
	internal class StateJumpLied : StateLied
	{
		protected float moveSpeed;

		private double timeStartSliding;

		public StateJumpLied(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 6;
		}

		internal override void OnEnter()
		{
			moveSpeed = hero.RunSpeed / 3f;
			timeStartSliding = GameElementsControl.CurrentTimeInMS;
			if (hero.LiedSide() != hero.SideLooking)
			{
				hero.Skeleton.SetAnimation("BACKCRAWL");
			}
			else
			{
				hero.Skeleton.SetAnimation("FRONTCRAWL");
			}
		}

		protected override bool Entering()
		{
			if (Math.Round(hero.FloorAngleDiff(), 1) == 0.0)
			{
				hero.ActionCrawlJump();
				hero.Skeleton.SetAnimation("LIED");
				return true;
			}
			if (GameElementsControl.CurrentTimeInMS - timeStartSliding > 500.0)
			{
				hero.ActionCrawlJump();
				return true;
			}
			return false;
		}

		protected override void During()
		{
			if (hero.LiedSide() != hero.SideLooking)
			{
				hero.Skeleton.SetAnimation("BACKCRAWL");
			}
			else
			{
				hero.Skeleton.SetAnimation("FRONTCRAWL");
			}
		}

		protected override bool Exiting()
		{
			return true;
		}

		internal override void OnExit()
		{
			hero.ActionCrawlLand();
		}

		protected override void CheckRules()
		{
			base.CheckRules();
		}

		protected override void CheckSpecificRules()
		{
			float value = MathHelper.WrapAngle(hero.MainBody.Rotation);
			if (Math.Abs(value) < 0.8f && GameElementsControl.CurrentTimeInMS - timeStartSliding > 500.0)
			{
				SwitchState(12, useExitTransition: true);
			}
			else if (hero.Landed())
			{
				SwitchState(10, useExitTransition: true);
			}
			else if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				AirMove(Side.Left);
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				AirMove(Side.Right);
			}
		}

		private void AirMove(Side side)
		{
			float value = moveSpeed * (float)side - hero.MainBody.LinearVelocity.X;
			float num = hero.RunSpeed / 2000f * (float)GameElementsControl.LastFrameTimeInMS;
			value = ((side != Side.Left) ? MathHelper.Clamp(value, 0f, num) : MathHelper.Clamp(value, 0f - num, 0f));
			Vector2 impulse = new Vector2(value * hero.Mass, 0f);
			hero.MainBody.ApplyLinearImpulse(ref impulse);
		}
	}
}
