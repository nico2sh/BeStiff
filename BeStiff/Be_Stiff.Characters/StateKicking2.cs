using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;

namespace Be_Stiff.Characters
{
	internal class StateKicking2 : HeroState
	{
		private const float kickStrength = 50f;

		private HittingObjects hittingObjects;

		private CircleEmitter kickSmoke;

		protected int kickFrame;

		protected Side sideKicking;

		private Vector2 kickPosition;

		public StateKicking2(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 18;
			kickSmoke = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
			kickFrame = 2;
			kickPosition = new Vector2(1f, 0f);
		}

		internal override void OnEnter()
		{
			if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				sideKicking = Side.Left;
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				sideKicking = Side.Right;
			}
			else
			{
				sideKicking = hero.SideLooking;
			}
			if (sideKicking == hero.SideLooking)
			{
				kickFrame = 2;
				hero.Skeleton.SetAnimation("KICK");
			}
			else
			{
				kickFrame = 3;
				hero.Skeleton.SetAnimation("KICKBACK2");
			}
			GameElementsControl.NoiseManager.AddNoise("heroKick", hero.Position);
			hero.SetFixedSide(hero.SideLooking);
			hero.ActionStopWalking();
		}

		protected override void During()
		{
			if (hero.Skeleton.AnimationKeyFrameAt(kickFrame))
			{
				Kick();
			}
		}

		protected override void CheckRules()
		{
			if (hero.Skeleton.AnimationEnded())
			{
				SwitchState(3, useExitTransition: true);
			}
		}

		internal override void OnExit()
		{
			hero.UnsetFixedSide();
		}

		private void Kick()
		{
			float num = 0.4f;
			Vector2 worldPoint = hero.MainBody.GetWorldPoint(new Vector2((float)sideKicking * kickPosition.X, kickPosition.Y));
			AABB aabb = default(AABB);
			aabb.LowerBound = worldPoint + new Vector2(0f - num, 0f - num);
			aabb.UpperBound = worldPoint + new Vector2(num, num);
			RayCastCallBacks.QueryKickAABB(ref aabb, base.Name, Category.Cat3, out hittingObjects);
			Vector2 vector = new Vector2((float)sideKicking, 0f);
			hittingObjects.Hit(vector * 50f, aabb.Center, HitType.Kick);
			Vector2 impulse = vector * 50f;
			hittingObjects.ApplyLinearImpulse(impulse);
			if (hittingObjects.NumObjects > 0)
			{
				kickSmoke.Trigger(GameElementsControl.ConvertWorldToScreen(worldPoint));
				GameElementsControl.NoiseManager.AddNoise("kickHit", hero.Position);
			}
		}
	}
}
