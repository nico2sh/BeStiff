using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters.Old
{
	internal class StateSmallClimbing : HeroState
	{
		private PrismaticJoint climbJoint;

		private PrismaticJoint upJoint;

		private RevoluteJoint bodyHangJoint;

		private Body bodyHang;

		private Side hangingSide;

		private Vector2 attachPoint;

		private bool vertical;

		private double timeStartClimbing;

		public StateSmallClimbing(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 16;
			bodyHang = BodyFactory.CreateRectangle(GameElementsControl.World, 1f, 1f, 1f);
			bodyHang.FixedRotation = true;
			bodyHang.CollidesWith = Category.None;
			bodyHang.CollisionCategories = Category.None;
			bodyHang.BodyType = BodyType.Dynamic;
			bodyHangJoint = JointFactory.CreateRevoluteJoint(bodyHang, hero.MainBody, Vector2.Zero);
			climbJoint = new PrismaticJoint(hero.FeetBody, hero.MainBody, Vector2.Zero, Vector2.Zero, new Vector2(0f, 1f));
			climbJoint.MotorEnabled = false;
			upJoint = new PrismaticJoint(hero.FeetBody, hero.MainBody, Vector2.Zero, Vector2.Zero, new Vector2(1f, 0f));
			upJoint.MotorEnabled = false;
		}

		public void SetData(Side side, Body retBody, Vector2 bodyHangPos)
		{
			attachPoint = bodyHangPos;
			hangingSide = side;
			Vector2 localPoint = retBody.GetLocalPoint(attachPoint);
			bodyHang.LinearVelocity = Vector2.Zero;
			bodyHang.Position = attachPoint;
			bodyHangJoint.BodyA = bodyHang;
			bodyHangJoint.BodyB = retBody;
			bodyHangJoint.LocalAnchorA = Vector2.Zero;
			bodyHangJoint.LocalAnchorB = localPoint;
		}

		internal override void OnEnter()
		{
			hero.SetFixedSide(hangingSide);
			GameElementsControl.World.AddJoint(bodyHangJoint);
			Vector2 localPoint = hero.MainBody.GetLocalPoint(attachPoint);
			climbJoint.BodyA = bodyHang;
			climbJoint.LocalAnchorA = Vector2.Zero;
			climbJoint.LocalAnchorB = new Vector2((float)hangingSide * (hero.Width / 2f + 0.1f), localPoint.Y);
			climbJoint.LimitEnabled = false;
			climbJoint.MotorEnabled = true;
			climbJoint.MaxMotorForce = 10000f;
			climbJoint.MotorSpeed = 0f - hero.Height;
			GameElementsControl.World.AddJoint(climbJoint);
			vertical = true;
			hero.Skeleton.SetAnimation("QUICKCLIMB");
			timeStartClimbing = GameElementsControl.CurrentTimeInMS;
		}

		protected override void During()
		{
			if (vertical && hero.FeetPosition.Y < attachPoint.Y)
			{
				SetHorizontal();
			}
		}

		protected override void CheckRules()
		{
			if (!vertical && (float)hangingSide * (hero.Position.X - attachPoint.X) >= 0.2f)
			{
				SwitchState(3, useExitTransition: true);
			}
			else if (timeStartClimbing + 600.0 <= GameElementsControl.CurrentTimeInMS)
			{
				SwitchState(3, useExitTransition: true);
			}
			else if (hero.Input.ControlSlideDown(hero.PlayerIndex))
			{
				hero.ActionSmallJump((Side)(0 - hangingSide));
				SwitchState(5, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				hero.ActionJump(hero.Height * 1.05f, realJump: true);
				SwitchState(5, useExitTransition: true);
			}
		}

		internal override void OnExit()
		{
			GameElementsControl.World.RemoveJoint(bodyHangJoint);
			if (vertical)
			{
				GameElementsControl.World.RemoveJoint(climbJoint);
			}
			else
			{
				GameElementsControl.World.RemoveJoint(upJoint);
			}
			hero.UnsetFixedSide();
		}

		private void SetHorizontal()
		{
			vertical = false;
			GameElementsControl.World.RemoveJoint(climbJoint);
			upJoint.BodyA = bodyHang;
			upJoint.LocalAnchorA = Vector2.Zero;
			upJoint.LocalAnchorB = new Vector2(0f, hero.Height / 2f);
			upJoint.LimitEnabled = false;
			upJoint.MotorEnabled = true;
			upJoint.MaxMotorForce = 10000f;
			upJoint.MotorSpeed = (float)hangingSide * hero.Height;
			GameElementsControl.World.AddJoint(upJoint);
		}
	}
}
