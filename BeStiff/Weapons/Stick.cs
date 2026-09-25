using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury.Emitters;
using SKAnimation;

namespace Be_Stiff.Weapons
{
	public class Stick : Weapon, IWeaponHero
	{
		private const float hitStrength = 40f;

		private GameSprite stickSprite;

		private GameSpriteVariables stickSpriteVariables;

		private GameSprite uiSprite;

		private BoneAnimation stickAnimation;

		private Skeleton mainSkeleton;

		private bool hitting;

		private bool hit;

		private double timeStartHitting;

		private CircleEmitter hitSmoke;

		private string ownerName;

		private HittingObjects hittingObjects;

		public int InfoNumber => 0;

		public int SecondaryInfoNumber => 0;

		public GameSprite UISprite => uiSprite;

		// Ignoring Cat2 (other enemies) also skips a lying or sliding hero,
		// who takes Cat2 too: the swing goes over them.
		public override bool CanHitLyingTarget => false;

		public override double EnemyAttackInterval => 1200.0;

		public Stick(Arm arm)
			: base(arm)
		{
			base.WeaponType = WeaponType.Other;
			mainSkeleton = arm.GetMainSkeleton();
			// Keep the AI's attack range inside the swing's reach (hand, 0.65
			// from the shoulder per skeleton scale, plus the hit box's 0.65):
			// the half-size legacy skeleton only reaches about 1.5.
			weaponRange = GameElementsControl.IsLegacyLevel ? 1.4f : 2f;
			hittingObjects = new HittingObjects();
		}

		public void Init(string owner)
		{
			ownerName = owner;
		}

		public override void Load()
		{
			GameElementsControl.LoadSprite("stick", "sprites\\guns\\stick\\stick");
			GameElementsControl.LoadSprite("pistolUI", "sprites\\ui\\pistolui");
			stickSprite = GameElementsControl.GetSprite("stick");
			uiSprite = GameElementsControl.GetSprite("pistolUI");
			stickSpriteVariables = GameSprite.GetDefaultVariables();
			stickAnimation = new BoneAnimation("STICK", hasToLoop: false);
			KeyFrame keyFrame = new KeyFrame();
			keyFrame.Time = 150.0;
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-2.5f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-1.2f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.25f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.3f, Vector2.Zero));
			stickAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 120.0;
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-1.05f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-0.75f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-1.05f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.75f, Vector2.Zero));
			stickAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 130.0;
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-0.25f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-0.4f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.65f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-0.8f, Vector2.Zero));
			stickAnimation.AddKeyFrame(keyFrame);
			hitSmoke = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
		}

		public override void Update()
		{
			if (hitting && timeStartHitting + 270.0 <= GameElementsControl.CurrentTimeInMS)
			{
				if (!hit)
				{
					Hit();
					hit = true;
				}
				else if (timeStartHitting + 410.0 <= GameElementsControl.CurrentTimeInMS)
				{
					hitting = false;
				}
			}
		}

		private void Hit()
		{
			Vector2 vector = GameElementsControl.ConvertScreenToWorld(ownerArm.HandPosition());
			float num = 1.3f;
			AABB aabb = new AABB(vector + new Vector2(0f, 0.2f), num, num * 2f);
			RayCastCallBacks.QueryKickAABB(ref aabb, ownerName, Category.Cat2, out hittingObjects);
			Vector2 vector2 = new Vector2(Math.Sign((ownerArm.HandPosition() - ownerArm.ScreenPosition).X), 0f);
			Vector2 vector3 = vector2 * 40f;
			hittingObjects.Hit(vector3, aabb.Center, HitType.Kick);
			hittingObjects.ApplyLinearImpulse(vector3 / 2f);
			if (DebugFlags.HitLog)
			{
				Hero hero = GameElementsControl.Hero;
				Vector2 heroCenter = hero.GetCenterPosition();
				Console.Error.WriteLine($"stick {ownerName} t={GameElementsControl.CurrentTimeInMS:F0} shoulder={ownerArm.Position} hand={vector} box={aabb.LowerBound}..{aabb.UpperBound} hero={heroCenter} reach={(vector - ownerArm.Position).Length():F2} heroDist={(heroCenter - ownerArm.Position).Length():F2} lied={hero.IsLied()} hits={hittingObjects.NumObjects}");
			}
			if (hittingObjects.NumObjects > 0)
			{
				hitSmoke.Trigger(ownerArm.HandPosition());
				GameElementsControl.NoiseManager.AddNoise("kickHit", vector);
			}
		}

		public void HandleInput(InputHelper input, PlayerIndex? playerIndex)
		{
			if (input.ControlShoot(playerIndex))
			{
				Shoot();
			}
		}

		public override bool ManualAim(out float angle)
		{
			angle = 0f;
			return true;
		}

		public override void UpdateDrawSide(Side side)
		{
			if (side == Side.Left)
			{
				stickSpriteVariables.spriteEffects = SpriteEffects.FlipVertically;
			}
			else
			{
				stickSpriteVariables.spriteEffects = SpriteEffects.None;
			}
			stickSpriteVariables.rotation = ownerArm.LoArm.AbsoluteAngle;
		}

		public override void Holster()
		{
			hitting = false;
			mainSkeleton.CancelTempAnimation();
		}

		public override void SetArmAnimation()
		{
			ownerArm.IgnoreAnimation(value: false);
		}

		public override void Shoot()
		{
			if (!hitting)
			{
				mainSkeleton.InsertTempAnimation(stickAnimation);
				hitting = true;
				hit = false;
				timeStartHitting = GameElementsControl.CurrentTimeInMS;
			}
		}

		public override void Draw()
		{
			stickSprite.Draw(ownerArm.HandPosition(), stickSpriteVariables);
		}
	}
}
