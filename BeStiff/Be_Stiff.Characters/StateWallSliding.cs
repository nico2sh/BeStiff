using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters
{
	internal class StateWallSliding : HeroState
	{
		private Fixture wallSlideFixture;

		private float wallSlideXSide;

		private Side wallSlideSide;

		private PrismaticJoint wallSlideJoint;

		public StateWallSliding(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 13;
			wallSlideJoint = new PrismaticJoint(hero.FeetBody, hero.MainBody, Vector2.Zero, Vector2.Zero, new Vector2(0f, 1f));
		}

		public void SetData(Fixture wsf, float xSide, Side side)
		{
			wallSlideFixture = wsf;
			wallSlideXSide = xSide;
			wallSlideSide = side;
		}

		internal override void OnEnter()
		{
			hero.SetFixedSide((Side)(0 - wallSlideSide));
			ActionWallSlide(wallSlideSide);
			hero.Skeleton.SetAnimation("WALLSLIDEBACKWARD");
			hero.SetLinearVelocity(new Vector2(hero.MainBody.LinearVelocity.X, 0f));
		}

		protected override void During()
		{
		}

		protected override void CheckRules()
		{
			if (!CheckWallSlide())
			{
				SwitchState(5, useExitTransition: false);
			}
			else if (hero.Landed())
			{
				SwitchState(3, useExitTransition: false);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				StateWallJump stateWallJump = stateMachine.States[7] as StateWallJump;
				stateWallJump.SetData(wallSlideSide);
				SwitchState(7, useExitTransition: true);
			}
			else if (hero.HookAttached)
			{
				SwitchState(21, useExitTransition: false);
			}
		}

		internal override void OnExit()
		{
			hero.UnsetFixedSide();
			GameElementsControl.World.RemoveJoint(wallSlideJoint);
		}

		private void ActionWallSlide(Side side)
		{
			hero.ActionStopWalking();
			Vector2 worldPoint = new Vector2(wallSlideXSide - (float)side * (hero.Width / 2f + 0.05f), wallSlideFixture.Body.Position.Y);
			worldPoint = wallSlideFixture.Body.GetLocalPoint(worldPoint);
			wallSlideJoint.BodyA = wallSlideFixture.Body;
			wallSlideJoint.LocalAnchorA = worldPoint;
			wallSlideJoint.LocalAnchorB = Vector2.Zero;
			wallSlideJoint.LimitEnabled = false;
			Vector2 linearVelocity = hero.MainBody.LinearVelocity;
			linearVelocity.X = 0f;
			if (linearVelocity.Y < 0f)
			{
				linearVelocity.Y = 0f;
			}
			hero.SetLinearVelocity(linearVelocity);
			wallSlideJoint.MotorEnabled = true;
			wallSlideJoint.MaxMotorForce = 70f;
			wallSlideJoint.MotorSpeed = 0f;
			GameElementsControl.World.AddJoint(wallSlideJoint);
		}

		private bool CheckWallSlide()
		{
			float x = wallSlideFixture.Body.LinearVelocity.X;
			if (x != 0f)
			{
				if (wallSlideSide == Side.Left && x < 0f)
				{
					return false;
				}
				if (wallSlideSide == Side.Right && x > 0f)
				{
					return false;
				}
			}
			Vector2 zero = Vector2.Zero;
			zero.X = hero.MainBody.Position.X;
			zero.Y = hero.MainBody.Position.Y - hero.Height / 4f;
			Vector2 zero2 = Vector2.Zero;
			float num = hero.Width / 2f + 0.15f;
			zero2.X = zero.X + (float)wallSlideSide * num;
			zero2.Y = zero.Y;
			bool flag = false;
			RayCastInput input = default(RayCastInput);
			input.Point1 = zero;
			input.Point2 = zero2;
			input.MaxFraction = 50f;
			flag = wallSlideFixture.RayCast(out var output, ref input, 0);
			bool flag2 = false;
			zero.Y = hero.MainBody.Position.Y + (hero.Height - hero.Width) / 2f;
			zero2.Y = zero.Y;
			input.Point1 = zero;
			input.Point2 = zero2;
			flag2 = wallSlideFixture.RayCast(out output, ref input, 0);
			if (flag)
			{
				return flag2;
			}
			return false;
		}
	}
}
