using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;

namespace Be_Stiff.Characters
{
	internal class StatePunching2 : HeroState
	{
		private const float punchStrength = 30f;

		private HittingObjects hittingObjects;

		private CircleEmitter punchSmoke;

		protected int punchFrame;

		protected Side sidePunching;

		protected bool chainKick;

		private Vector2 punchPosition;

		public StatePunching2(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 20;
			punchSmoke = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
			punchFrame = 2;
			punchPosition = new Vector2(0.75f, -0.5f);
		}

		internal override void OnEnter()
		{
			chainKick = false;
			if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				sidePunching = Side.Left;
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				sidePunching = Side.Right;
			}
			else
			{
				sidePunching = hero.SideLooking;
			}
			punchFrame = 3;
			hero.SetFixedSide(sidePunching);
			hero.Skeleton.SetAnimation("JAB");
			hero.ActionStopWalking();
		}

		protected override void During()
		{
			if (hero.Input.ControlKick(hero.PlayerIndex))
			{
				chainKick = true;
			}
			if (hero.Skeleton.AnimationKeyFrameAt(punchFrame))
			{
				Punch();
			}
		}

		protected override void CheckRules()
		{
			if (chainKick)
			{
				if (hero.Skeleton.CurrenKeyFrame >= 4)
				{
					SwitchState(18, useExitTransition: false);
				}
			}
			else if (hero.Skeleton.AnimationEnded())
			{
				SwitchState(3, useExitTransition: true);
			}
		}

		internal override void OnExit()
		{
			hero.UnsetFixedSide();
		}

		private void Punch()
		{
			float num = 0.3f;
			Vector2 worldPoint = hero.MainBody.GetWorldPoint(new Vector2((float)sidePunching * punchPosition.X, punchPosition.Y));
			AABB aabb = default(AABB);
			aabb.LowerBound = worldPoint + new Vector2(0f - num, 0f - num);
			aabb.UpperBound = worldPoint + new Vector2(num, num);
			RayCastCallBacks.QueryKickAABB(ref aabb, base.Name, Category.Cat3, out hittingObjects);
			Vector2 vector = new Vector2((float)hero.SideLooking, 0f);
			hittingObjects.Hit(vector * 30f, aabb.Center, HitType.Kick);
			Vector2 impulse = vector * 30f;
			hittingObjects.ApplyLinearImpulse(impulse);
			if (hittingObjects.NumObjects > 0)
			{
				punchSmoke.Trigger(GameElementsControl.ConvertWorldToScreen(worldPoint));
				GameElementsControl.NoiseManager.AddNoise("kickHit", hero.Position);
			}
		}
	}
}
