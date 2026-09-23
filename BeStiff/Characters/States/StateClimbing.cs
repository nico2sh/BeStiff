using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters.States
{
	internal class StateClimbing : HeroState
	{
		private PrismaticJoint climbJoint;

		private PrismaticJoint upJoint;

		private Body bodyHang;

		private RevoluteJoint bodyHangJoint;

		private Side sideClimbing;

		private bool vertical;

		private double timeStartClimbing;

		private bool smallClimbing;

		public StateClimbing(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 15;
			climbJoint = new PrismaticJoint(hero.FeetBody, hero.MainBody, Vector2.Zero, Vector2.Zero, new Vector2(0f, 1f));
			climbJoint.MotorEnabled = false;
			upJoint = new PrismaticJoint(hero.FeetBody, hero.MainBody, Vector2.Zero, Vector2.Zero, new Vector2(1f, 0f));
			upJoint.MotorEnabled = false;
		}

		public void SetData(Body bh, RevoluteJoint bhj, bool sClimb)
		{
			bodyHang = bh;
			bodyHangJoint = bhj;
			smallClimbing = sClimb;
		}

		internal override void OnEnter()
		{
			vertical = true;
			timeStartClimbing = GameElementsControl.CurrentTimeInMS;
			sideClimbing = (Side)Math.Sign(bodyHang.Position.X - hero.MainBody.Position.X);
			hero.SetFixedSide(sideClimbing);
			if (smallClimbing)
			{
				hero.Skeleton.SetAnimation("QUICKCLIMB");
				double num = Math.Floor((1f - (hero.FloorPosition.Y - bodyHang.Position.Y) / (hero.Height / 2f)) * 3f);
				hero.Skeleton.SetAnimationFrame((int)num);
				Vector2 localPoint = hero.MainBody.GetLocalPoint(bodyHang.Position);
				climbJoint.LocalAnchorB = new Vector2((float)sideClimbing * (hero.Width / 2f + 0.1f), localPoint.Y);
				climbJoint.MotorSpeed = 0f;
			}
			else
			{
				hero.Skeleton.SetAnimation("CLIMBLADDER");
				climbJoint.LocalAnchorB = hero.MainBody.GetLocalPoint(bodyHang.Position);
				climbJoint.MotorSpeed = (0f - hero.Height) / 24f;
			}
			climbJoint.BodyA = bodyHang;
			climbJoint.LocalAnchorA = Vector2.Zero;
			climbJoint.LimitEnabled = false;
			climbJoint.MotorEnabled = true;
			climbJoint.MaxMotorForce = 10000f;
			GameElementsControl.World.AddJoint(climbJoint);
			GameElementsControl.World.AddJoint(bodyHangJoint);
			GameElementsControl.NoiseManager.AddNoise("heroClimb", hero.Position);
		}

		protected override void During()
		{
			if (!vertical)
			{
				return;
			}
			if (hero.FloorPosition.Y <= bodyHang.Position.Y)
			{
				SetHorizontal();
				return;
			}
			if (smallClimbing)
			{
				if (timeStartClimbing + 200.0 <= GameElementsControl.CurrentTimeInMS)
				{
					climbJoint.MotorSpeed = 0f - hero.Height * 0.9f;
				}
				return;
			}
			if (hero.Skeleton.AnimationKeyFrameAt(1))
			{
				climbJoint.MotorSpeed = (0f - hero.Height) * 1.8f;
			}
			if (hero.Skeleton.AnimationKeyFrameAt(2))
			{
				climbJoint.MotorSpeed = 0f;
			}
			if (hero.Skeleton.AnimationKeyFrameAt(3))
			{
				climbJoint.MotorSpeed = 0f - hero.Height;
			}
			if (hero.Skeleton.AnimationKeyFrameAt(5))
			{
				climbJoint.MotorSpeed = 0f;
			}
			if (hero.Skeleton.AnimationKeyFrameAt(8))
			{
				climbJoint.MotorSpeed = (0f - hero.Height) * 0.8f;
			}
			if (hero.Skeleton.AnimationKeyFrameAt(10))
			{
				climbJoint.MotorSpeed = 0f;
			}
		}

		protected override void CheckRules()
		{
			if (!vertical && (float)sideClimbing * (hero.Position.X - bodyHang.Position.X) >= 0.2f)
			{
				SwitchState(3, useExitTransition: true);
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
			upJoint.MotorSpeed = (float)sideClimbing * hero.Height;
			GameElementsControl.World.AddJoint(upJoint);
		}
	}
}
