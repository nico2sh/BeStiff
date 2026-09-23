using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters.States
{
	public class StateWalking : HeroState
	{
		protected Side sideWalking;

		protected float moveSpeed;

		protected double timeAllowedToJumpSlope;

		private float slopeJumpHeight;

		public StateWalking(StateMachine sm, Hero h)
			: base(sm, h)
		{
			slopeJumpHeight = hero.Width / 2f;
			base.StateID = 0;
		}

		protected override void CheckRules()
		{
			if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				SwitchState(4, useExitTransition: false);
			}
			else if (hero.Input.ControlSlideDown(hero.PlayerIndex))
			{
				SwitchState(9, useExitTransition: false);
			}
			else if (hero.Input.ControlKick(hero.PlayerIndex))
			{
				SwitchState(17, useExitTransition: false);
			}
			else if (stateMachine.CheckEvent(0))
			{
				SwitchState(19, useExitTransition: false);
			}
			else if (hero.MustDefend)
			{
				SwitchState(22, useExitTransition: false);
			}
			else if (!hero.Input.ControlLeft(hero.PlayerIndex) && !hero.Input.ControlRight(hero.PlayerIndex))
			{
				SwitchState(3, useExitTransition: true);
			}
			else if (hero.Input.ControlLeft(hero.PlayerIndex) && hero.Input.ControlRight(hero.PlayerIndex))
			{
				SwitchState(3, useExitTransition: true);
			}
			else if (!hero.TouchingFloor())
			{
				SwitchState(5, useExitTransition: false);
			}
			else
			{
				CheckSpecificRules();
			}
		}

		protected virtual bool CheckSpecificRules()
		{
			if (hero.Input.ControlRun(hero.PlayerIndex) && !hero.StaminaDrained)
			{
				SwitchState(1, useExitTransition: false);
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
			return false;
		}

		internal override void OnEnter()
		{
			timeAllowedToJumpSlope = 0.0;
			if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				sideWalking = Side.Left;
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				sideWalking = Side.Right;
			}
			hero.Skeleton.SetAnimation("WALK");
			hero.WheelRevoluteJoint.MotorEnabled = true;
			moveSpeed = hero.RunSpeed / 3f;
		}

		internal override void OnExit()
		{
		}

		protected override bool Exiting()
		{
			hero.ActionStopWalking();
			hero.Skeleton.SetAnimation("WALKSTOP");
			return hero.Skeleton.AnimationEnded();
		}

		protected override void During()
		{
			hero.ActionWalk(sideWalking, moveSpeed);
			if (timeAllowedToJumpSlope < GameElementsControl.CurrentTimeInMS)
			{
				CheckSlopes();
			}
		}

		private void CheckSlopes()
		{
			float num = Math.Abs((float)Math.Sqrt(2f * slopeJumpHeight / (GameElementsControl.Gravity.Y * 3f)));
			if (Math.Abs(hero.MainBody.LinearVelocity.X) > 10f)
			{
				int num2 = 0;
				num2++;
			}
			_ = hero.MainBody.LinearVelocity.X;
			Vector2 worldVector = hero.MainBody.GetWorldVector(new Vector2(1f, 0f));
			_ = worldVector.Y / worldVector.X;
			Vector2 worldPoint = hero.MainBody.GetWorldPoint(new Vector2(hero.MainBody.LinearVelocity.X * num, hero.Height / 2f - slopeJumpHeight));
			if (worldPoint.Y > hero.FloorPosition.Y)
			{
				worldPoint.Y = hero.FloorPosition.Y;
			}
			Vector2 point = new Vector2(worldPoint.X, worldPoint.Y + slopeJumpHeight * 2f);
			float frac = float.MaxValue;
			Vector2 norm = Vector2.Zero;
			Vector2 po = Vector2.Zero;
			Fixture fixture = RayCastCallBacks.RayCastOneNoHuman(worldPoint, point, out po, out norm, out frac);
			if (fixture != null)
			{
				float num3 = norm.GetAngle() + (float)Math.PI / 2f;
				num3 = MathHelper.Clamp(MathHelper.WrapAngle(num3), 0f - Globals.FloorAngleLimit, Globals.FloorAngleLimit);
				float num4 = MathHelper.WrapAngle(hero.MainBody.Rotation);
				double num5 = ((!(hero.MainBody.LinearVelocity.X > 0f)) ? Math.Round(num4 - num3, 1) : Math.Round(num3 - num4, 1));
				if (num5 < 0.0)
				{
					timeAllowedToJumpSlope = GameElementsControl.CurrentTimeInMS + 500.0;
					ActionJumpSlope();
				}
			}
		}

		protected virtual void ActionJumpSlope()
		{
			if (hero.Landed())
			{
				Vector2 impulse = new Vector2(0f, (0f - hero.Mass) * (float)Math.Sqrt(2f * GameElementsControl.Gravity.Y * slopeJumpHeight));
				hero.ApplyImpulse(ref impulse);
			}
		}
	}
}
