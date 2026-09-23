using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;
using SKAnimation;

namespace Be_Stiff
{
	internal class NoWeapon : Weapon
	{
		private BoneAnimation punchAnimation;

		private BoneAnimation punchAnimation2;

		protected Skeleton mainSkeleton;

		protected bool punching;

		protected bool punched;

		protected double timeStartPunching;

		private float punchStrength = 20f;

		private CircleEmitter kickSmoke;

		private string ownerName;

		protected double delayBetweenPunch;

		protected double minDelayBetweenPunch = 710.0;

		private ChildBone leftArm;

		private bool withLeftArm;

		private bool chainTwoPunches;

		private bool inSecondPunch;

		private HittingObjects hittingObjects;

		public void SetPunchStrength(float strength)
		{
			punchStrength = strength;
		}

		public void SetDelayBetweenPunches(double delay)
		{
			delayBetweenPunch = MathHelper.Max((float)minDelayBetweenPunch, (float)delay);
		}

		public NoWeapon(Arm arm, ChildBone otherArm)
			: base(arm)
		{
			base.WeaponType = WeaponType.Other;
			mainSkeleton = arm.GetMainSkeleton();
			punching = false;
			punched = false;
			weaponRange = 1f;
			delayBetweenPunch = minDelayBetweenPunch;
			hittingObjects = new HittingObjects();
			leftArm = otherArm;
			chainTwoPunches = false;
		}

		public void Init(string owner)
		{
			ownerName = owner;
		}

		public override void Load()
		{
			punchAnimation = new BoneAnimation("PUNCH", hasToLoop: false);
			KeyFrame keyFrame = new KeyFrame();
			keyFrame.Time = 140.0;
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(0.9f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-1.2f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.25f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.3f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.5708f, new Vector2(0f, 9f)));
			punchAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 100.0;
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-1.05f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-0.75f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-1.05f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.75f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.4208f, new Vector2(-2f, 9f)));
			punchAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 60.0;
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-1.5f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-0.3f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(1.4f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.5f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.3708f, new Vector2(-1f, 9f)));
			punchAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 300.0;
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-0.25f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-0.4f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(0.65f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-0.8f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.5708f, new Vector2(0f, 9f)));
			punchAnimation.AddKeyFrame(keyFrame);
			punchAnimation.Overrideable = true;
			punchAnimation2 = new BoneAnimation("PUNCH2", hasToLoop: false);
			keyFrame = new KeyFrame();
			keyFrame.Time = 140.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(0.9f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.2f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-0.25f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-1.3f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.5708f, new Vector2(0f, 9f)));
			punchAnimation2.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 100.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-1.05f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-0.75f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-1.05f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-1.75f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.4208f, new Vector2(-2f, 9f)));
			punchAnimation2.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 60.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-1.5f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-0.3f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(1.4f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-1.5f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.3708f, new Vector2(-1f, 9f)));
			punchAnimation2.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 300.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.25f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-0.4f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(0.65f, new Vector2(-17f, 0f)));
			keyFrame.KeyFrameInfo.Add("rightLoArm", new KeyFrameInfo(-0.8f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("torso", new KeyFrameInfo(-1.5708f, new Vector2(0f, 9f)));
			punchAnimation2.AddKeyFrame(keyFrame);
			punchAnimation2.Overrideable = true;
			withLeftArm = false;
			kickSmoke = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
		}

		public override bool ManualAim(out float angle)
		{
			angle = 0f;
			return false;
		}

		public override void SetArmAnimation()
		{
			ownerArm.IgnoreAnimation(value: false);
		}

		public override void Shoot()
		{
			if (!punching)
			{
				if (withLeftArm)
				{
					mainSkeleton.InsertTempAnimation(punchAnimation);
				}
				else
				{
					mainSkeleton.InsertTempAnimation(punchAnimation2);
				}
				punching = true;
				punched = false;
				withLeftArm = !withLeftArm;
				timeStartPunching = GameElementsControl.CurrentTimeInMS;
			}
			else if (!inSecondPunch)
			{
				chainTwoPunches = true;
			}
		}

		public override void Holster()
		{
			punching = false;
			mainSkeleton.CancelTempAnimation();
		}

		public override void Update()
		{
			if (!punching || !(timeStartPunching + 300.0 <= GameElementsControl.CurrentTimeInMS))
			{
				return;
			}
			if (!punched)
			{
				Punch();
				punched = true;
				if (chainTwoPunches)
				{
					punching = false;
					Shoot();
					inSecondPunch = true;
					chainTwoPunches = false;
				}
			}
			else if (timeStartPunching + delayBetweenPunch <= GameElementsControl.CurrentTimeInMS)
			{
				withLeftArm = false;
				punching = false;
				inSecondPunch = false;
			}
		}

		protected void Punch()
		{
			Vector2 vector = ((!withLeftArm) ? ownerArm.HandPosition() : leftArm.GetEndPosition());
			Vector2 vector2 = GameElementsControl.ConvertScreenToWorld(vector);
			float num = 0.5f;
			AABB aabb = new AABB(vector2, num, num);
			RayCastCallBacks.QueryKickAABB(ref aabb, ownerName, Category.None, out hittingObjects);
			Vector2 vector3 = Vector2.Normalize(vector - ownerArm.ScreenPosition);
			vector3.Y = 0f - vector3.Y;
			Vector2 vector4 = vector3 * punchStrength;
			hittingObjects.Hit(vector4, aabb.Center, HitType.Kick);
			hittingObjects.ApplyLinearImpulse(vector4);
			if (hittingObjects.NumObjects > 0)
			{
				kickSmoke.Trigger(vector);
				GameElementsControl.NoiseManager.AddNoise("kickHit", vector2);
			}
		}
	}
}
