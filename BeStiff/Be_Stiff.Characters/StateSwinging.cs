using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters
{
	internal class StateSwinging : HeroState
	{
		private Side sideSwinging;

		private Side sideLooking;

		public StateSwinging(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 21;
		}

		internal override void OnEnter()
		{
			UpdateSides();
			if (hero.IsLied())
			{
				hero.Balance = false;
			}
		}

		protected override void During()
		{
			if (sideLooking != hero.SideLooking || sideSwinging != (Side)Math.Sign(hero.MainBody.LinearVelocity.X))
			{
				UpdateSides();
			}
			if (hero.IsLied())
			{
				float value = MathHelper.WrapAngle(hero.MainBody.Rotation);
				if (Math.Abs(value) < 0.1f)
				{
					hero.ActionStand();
					hero.Balance = true;
					hero.WheelRevoluteJoint.MotorEnabled = true;
				}
			}
			if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				ActionSwing(Side.Right, hero.RunSpeed / 2f);
			}
			else if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				ActionSwing(Side.Left, hero.RunSpeed / 2f);
			}
		}

		protected override void CheckRules()
		{
			if (hero.Weapon.WeaponType == WeaponType.Hook)
			{
				if (hero.Input.ControlJump(hero.PlayerIndex))
				{
					hero.ActionJump(hero.JumpSmallHeight, realJump: true);
					SwitchState(5, useExitTransition: false);
				}
				else if (!hero.HookAttached)
				{
					SwitchState(5, useExitTransition: false);
				}
				else if (hero.TouchingFloor())
				{
					hero.Balance = true;
					if (hero.IsLied())
					{
						SwitchState(11, useExitTransition: false);
					}
					else
					{
						SwitchState(3, useExitTransition: false);
					}
				}
				return;
			}
			throw new Exception("This is weird, I should have a Hook");
		}

		internal override void OnExit()
		{
		}

		private void UpdateSides()
		{
			sideSwinging = (Side)Math.Sign(hero.MainBody.LinearVelocity.X);
			sideLooking = hero.SideLooking;
			if (sideSwinging == sideLooking)
			{
				hero.Skeleton.SetAnimation("SWINGFORWARD");
			}
			else
			{
				hero.Skeleton.SetAnimation("SWINGBACK");
			}
		}

		private void ActionSwing(Side side, float maxSpeed)
		{
			float value = (maxSpeed * (float)side - hero.MainBody.LinearVelocity.X) / 2f;
			value = ((side != Side.Left) ? MathHelper.Clamp(value, 0f, 2f) : MathHelper.Clamp(value, -2f, 0f));
			Vector2 force = new Vector2(value * hero.Mass, 0f);
			hero.MainBody.ApplyForce(ref force);
		}

		protected void ActionJumpSwinging(float meters)
		{
			Vector2 impulse = new Vector2(0f, hero.Mass * (float)Math.Sqrt(2f * (0f - GameElementsControl.Gravity.Y) * meters));
			hero.FeetBody.ApplyLinearImpulse(ref impulse);
			GameElementsControl.NoiseManager.AddNoise("heroJump", hero.Position);
		}
	}
}
