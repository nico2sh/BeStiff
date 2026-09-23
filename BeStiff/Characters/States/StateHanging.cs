using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters.States
{
	internal class StateHanging : HeroState
	{
		private static float HeightFromCenterToClimb = -0.1f;

		private RevoluteJoint hangJoint;

		private RevoluteJoint bodyHangJoint;

		private Body bodyHang;

		private Side hangingSide;

		private Vector2 attachPoint;

		private bool climbNow;

		public StateHanging(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 14;
			bodyHang = BodyFactory.CreateRectangle(GameElementsControl.World, 1f, 1f, 1f);
			bodyHang.FixedRotation = true;
			bodyHang.CollidesWith = Category.None;
			bodyHang.CollisionCategories = Category.None;
			bodyHang.BodyType = BodyType.Dynamic;
			bodyHangJoint = JointFactory.CreateRevoluteJoint(bodyHang, hero.MainBody, Vector2.Zero);
			hangJoint = new RevoluteJoint(hero.MainBody, hero.FeetBody, Vector2.Zero, Vector2.Zero);
			hangJoint.CollideConnected = false;
		}

		public static bool WillClimb(Vector2 attachPos, Vector2 heroPos, float heroHeight)
		{
			return attachPos.Y > heroPos.Y + heroHeight * HeightFromCenterToClimb;
		}

		public void SetData(Side side, Body retBody, Vector2 bodyHangPos)
		{
			attachPoint = bodyHangPos;
			hangingSide = side;
			Vector2 localPoint = retBody.GetLocalPoint(attachPoint);
			if (retBody.FixtureList.Count > 0 && retBody.FixtureList[0].UserData is WorldObjectData wod && wod.Object is GirderSmall)
				System.Console.Error.WriteLine($"HANG girder: attach={attachPoint} local={localPoint} hero={hero.MainBody.Position} heroMass={hero.MainBody.Mass} girderPos={retBody.Position} girderMass={retBody.Mass} girderRot={retBody.Rotation:F2} side={side} climb={WillClimb(attachPoint, hero.MainBody.Position, hero.Height)}");
			bodyHang.LinearVelocity = Vector2.Zero;
			bodyHang.Position = attachPoint;
			bodyHangJoint.BodyA = bodyHang;
			bodyHangJoint.BodyB = retBody;
			bodyHangJoint.LocalAnchorA = Vector2.Zero;
			bodyHangJoint.LocalAnchorB = localPoint;
			if (WillClimb(attachPoint, hero.MainBody.Position, hero.Height))
			{
				climbNow = true;
			}
			else
			{
				climbNow = false;
			}
		}

		internal override void OnEnter()
		{
			GameElementsControl.World.AddJoint(bodyHangJoint);
			hero.ActionStopWalking();
			if (!climbNow)
			{
				hangJoint.LocalAnchorA = new Vector2((float)hangingSide * (hero.Width / 2f + 0.1f), (0f - (hero.Height - hero.Width)) / 2f + 0.1f);
				hangJoint.BodyB = bodyHang;
				hangJoint.LocalAnchorB = Vector2.Zero;
				GameElementsControl.World.AddJoint(hangJoint);
			}
			hero.Balance = false;
		}

		protected override void During()
		{
			if (hero.SideLooking == hangingSide)
			{
				hero.Skeleton.SetAnimation("HANGFRONT");
			}
			else
			{
				hero.Skeleton.SetAnimation("HANGBACK");
			}
		}

		protected override void CheckRules()
		{
			if (hero.Input.ControlSlideDown(hero.PlayerIndex))
			{
				hero.ActionSmallJump((Side)(0 - hangingSide));
				SwitchState(5, useExitTransition: true);
			}
			else if (hero.Input.ControlJump(hero.PlayerIndex))
			{
				hero.ActionJump(hero.Height * 1.05f, realJump: true);
				SwitchState(5, useExitTransition: true);
			}
			else if (hero.Input.ControlClimb(hero.PlayerIndex) || climbNow)
			{
				StateClimbing stateClimbing = stateMachine.States[15] as StateClimbing;
				stateClimbing.SetData(bodyHang, bodyHangJoint, climbNow);
				SwitchState(15, useExitTransition: true);
			}
			else if (hero.HookAttached)
			{
				SwitchState(21, useExitTransition: false);
			}
		}

		internal override void OnExit()
		{
			GameElementsControl.World.RemoveJoint(bodyHangJoint);
			if (!climbNow)
			{
				GameElementsControl.World.RemoveJoint(hangJoint);
			}
			hero.Balance = true;
		}
	}
}
