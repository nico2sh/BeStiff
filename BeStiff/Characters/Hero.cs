using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Krypton;
using Krypton.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OgmoXNA;
using OgmoXNA.Values;
using ProjectMercury.Emitters;
using SKAnimation;

namespace Be_Stiff.Characters
{
	public class Hero : Human
	{
		private const float jumpSmallHeight = 1.6f;

		private const float jumpHighHeight = 3.1f;

		private const double bulletTimeDuration = 5000.0;

		private const double bulletTimeChargeTime = 5000.0;

		private const double staminaTimeDuration = 1500.0;

		private const double staminaTimeChargeTime = 5000.0;

		private const float kickStrength = 50f;

		private const double hitTimeDecrease = 500.0;

		private Body bodyCircBottomSlide;

		private RevoluteJoint wheelSlideRevoluteJoint;

		private Fixture geomCircBottomSlide;

		private float wheelSlideRadius;

		private Vector2[] edgePoints;

		private GameSprite crossHairPairSprite;

		private bool bulletTime;

		private float bulletTimeCharge;

		private float staminaCharge;

		private bool staminaDrained;

		private bool categoriesSliding;

		public bool MustDefend;

		public bool HookAttached;

		private Color shadowColor;

		private Color baseShadowColor;

		private CircleEmitter kickSmoke;

		private Light2D heroLight;

		private Texture2D heroDeadLightTexture;

		private BoneAnimation reloadAnimation;

		public float JumpSmallHeight => 1.6f;

		public float JumpHighHeight => 3.1f;

		public InputHelper Input => GameElementsControl.Input;

		public PlayerIndex? PlayerIndex => GameElementsControl.GameScreen.ControllingPlayer;

		public bool BulletTime => bulletTime;

		public float BulletTimeGauge => bulletTimeCharge;

		public float StaminaChargeGauge => staminaCharge;

		public bool StaminaDrained => staminaDrained;

		public Color ShadowColor => shadowColor;

		public Weapon Weapon => weapon[activeWeapon];

		public override Vector2 FloorPosition
		{
			get
			{
				if (bodyCircBottom.Position.Y + wheelRadius > bodyCircBottomSlide.Position.Y + wheelSlideRadius)
				{
					return new Vector2(bodyCircBottom.Position.X, bodyCircBottom.Position.Y + wheelRadius);
				}
				return new Vector2(bodyCircBottomSlide.Position.X, bodyCircBottomSlide.Position.Y + wheelSlideRadius);
			}
		}

		public GameSprite WeaponSprite => ((IWeaponHero)weapon[activeWeapon]).UISprite;

		public int WeaponInfoNumber => ((IWeaponHero)weapon[activeWeapon]).InfoNumber;

		public int WeaponSecondaryInfoNumber => ((IWeaponHero)weapon[activeWeapon]).SecondaryInfoNumber;

		public Hero()
			: base(new Vector2(0.3f, 1.95f))
		{
		}

		public Vector2 GetCenterPosition()
		{
			return bodyRect.Position;
		}

		public float FloorAngleDiff()
		{
			float num = MathHelper.WrapAngle(bodyRect.Rotation);
			return floorAngle - num + BaseAngle;
		}

		public void SetLinearVelocity(Vector2 vel)
		{
			bodyRect.LinearVelocity = vel;
			bodyCircBottom.LinearVelocity = vel;
			bodyCircBottomSlide.LinearVelocity = vel;
		}

		public Vector2[] GetLineOfSight()
		{
			ref Vector2 reference = ref edgePoints[0];
			reference = bodyRect.Position;
			ref Vector2 reference2 = ref edgePoints[1];
			reference2 = bodyRect.GetWorldPoint(new Vector2(0f, (physHeight - physWidth) / 2f));
			ref Vector2 reference3 = ref edgePoints[2];
			reference3 = bodyRect.GetWorldPoint(new Vector2(0f, (0f - (physHeight - physWidth)) / 2f));
			return edgePoints;
		}

		public bool IsLied()
		{
			return BaseAngle != 0f;
		}

		public Side LiedSide()
		{
			if (BaseAngle < 0f)
			{
				return Side.Left;
			}
			if (BaseAngle > 0f)
			{
				return Side.Right;
			}
			return Side.None;
		}

		public override bool IgnoreOneSide()
		{
			return Input.ControlJumpingDown(PlayerIndex);
		}

		protected override void LoadCustomStuff(OgmoObject obj)
		{
			wheelSlideRadius = physWidth / 2f + 0.03f;
			base.Name = "Hero";
			bodyCircBottomSlide = BodyFactory.CreateBody(GameElementsControl.World);
			bodyCircBottomSlide.BodyType = BodyType.Dynamic;
			bodyCircBottomSlide.Position = bodyRect.Position;
			CircleShape circleShape = new CircleShape(wheelSlideRadius, 40f);
			circleShape.Position = Vector2.Zero;
			geomCircBottomSlide = bodyCircBottomSlide.CreateFixture(circleShape);
			geomCircBottomSlide.Friction = 5f;
			geomCircBottomSlide.Restitution = 0f;
			Fixture fixture = geomCircBottomSlide;
			fixture.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture.AfterCollision, new AfterCollisionEventHandler(base.AfterHumanCollision));
			geomCircBottomSlide.UserData = new WorldObjectData(WorldObjectType.HumanFeet, this);
			wheelSlideRevoluteJoint = new RevoluteJoint(bodyRect, bodyCircBottomSlide, Vector2.Zero, Vector2.Zero);
			wheelSlideRevoluteJoint.MotorEnabled = true;
			wheelSlideRevoluteJoint.MotorSpeed = 0f;
			wheelSlideRevoluteJoint.MaxMotorTorque = 2500f;
			GameElementsControl.World.AddJoint(wheelSlideRevoluteJoint);
			geomCircBottomSlide.IsSensor = true;
			RecalculateMass();
			mass += bodyCircBottomSlide.Mass;
			mainCollisionCategory = Category.Cat3;
			SetCollisionCategories();
			bulletTimeCharge = 100f;
			staminaCharge = 100f;
			staminaDrained = false;
			BoneReader boneReader = new BoneReader();
			skeleton = boneReader.CreateFromFile(GameElementsControl.ScreenManager.Content, "skeletons\\hero\\humanskeleton", "skeletons\\hero\\heroanimations", "sprites\\hero\\", GameElementsControl.World, ConvertUnits.DisplayToSimUnitsRatio, bodyRect, GameElementsControl.IsLegacyLevel ? 0.5f : 1f, false, GameElementsControl.IsLegacyLevel ? 0.5f : 1f);
			skeleton.SetUserData(new WorldObjectData(WorldObjectType.Human, this));
			skeleton.Angle = (float)Math.PI;
			arm = new Arm(skeleton.GetBoneByName("rightUpArm"), skeleton.GetBoneByName("rightLoArm"));
			arm.IgnoreAnimation(value: true);
			headBone = skeleton.GetBoneByName("head");
			headBone.IgnoreAnimation = IgnoreAnimationType.Angle;
			reloadAnimation = new BoneAnimation("RELOAD", hasToLoop: false, canNotBeInterrupted: false);
			KeyFrame keyFrame = new KeyFrame();
			keyFrame.Time = 250.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.5f, new Vector2(-30f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-0.7f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-0.85f, new Vector2(-30f, 0f)));
			reloadAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 250.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.2f, new Vector2(-30f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.3f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-0.85f, new Vector2(-30f, 0f)));
			reloadAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 250.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.5f, new Vector2(-30f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-0.7f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-0.85f, new Vector2(-30f, 0f)));
			reloadAnimation.AddKeyFrame(keyFrame);
			keyFrame = new KeyFrame();
			keyFrame.Time = 250.0;
			keyFrame.KeyFrameInfo.Add("leftUpArm", new KeyFrameInfo(-0.2f, new Vector2(-30f, 0f)));
			keyFrame.KeyFrameInfo.Add("leftLoArm", new KeyFrameInfo(-1.3f, Vector2.Zero));
			keyFrame.KeyFrameInfo.Add("rightUpArm", new KeyFrameInfo(-0.85f, new Vector2(-30f, 0f)));
			reloadAnimation.AddKeyFrame(keyFrame);
			currentAnimation = "STAND";
			Fixture fixture2 = skeleton.GetBoneByName("torso").BoneBody.FixtureList[0];
			fixture2.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture2.AfterCollision, new AfterCollisionEventHandler(base.AfterHumanCollision));
			skeleton.SetCollisionCategory(Category.Cat3);
			skeleton.SetCollidesWith(Category.Cat1 | Category.Cat2 | Category.Cat4 | Category.Cat5 | Category.Cat6 | Category.Cat7 | Category.Cat8 | Category.Cat9 | Category.Cat10 | Category.Cat11 | Category.Cat12 | Category.Cat13 | Category.Cat14 | Category.Cat15 | Category.Cat16 | Category.Cat17 | Category.Cat18 | Category.Cat19 | Category.Cat20 | Category.Cat21 | Category.Cat22 | Category.Cat23 | Category.Cat24 | Category.Cat25 | Category.Cat26 | Category.Cat27 | Category.Cat28 | Category.Cat29 | Category.Cat30 | Category.Cat31);
			GameElementsControl.LoadSprite("crossHair", "sprites\\Hero\\crosshair");
			crossHairPairSprite = GameElementsControl.GetSprite("crossHair");
			weapon = new Weapon[3];
			int num = 0;
			numberOfWeapons = 0;
			if (numberOfWeapons == 0)
			{
				HeroNoWeapon heroNoWeapon = new HeroNoWeapon(arm, skeleton.GetBoneByName("leftLoArm"));
				heroNoWeapon.Load();
				heroNoWeapon.Init(base.Name);
				heroNoWeapon.SetDelayBetweenPunches(0.0);
				weapon[0] = heroNoWeapon;
				numberOfWeapons++;
				num++;
			}
			if (obj.GetValue<OgmoBooleanValue>("pistol").Value)
			{
				weapon[num] = CreateHeroPistol();
				num++;
				numberOfWeapons++;
			}
			if (obj.GetValue<OgmoBooleanValue>("hook").Value)
			{
				Hook hook = CreateHeroHook();
				weapon[num] = hook;
				num++;
				numberOfWeapons++;
			}
			weapon[activeWeapon].SetArmAnimation();
			shadowColor = Color.Black;
			baseShadowColor = Color.Black;
			kickSmoke = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
			edgePoints = new Vector2[3];
			categoriesSliding = false;
			heroLight = new Light2D();
			GameElementsControl.LoadSprite("heroLight", "sprites\\masks\\herolight");
			heroLight.Texture = GameElementsControl.GetSprite("heroLight").Texture;
			heroDeadLightTexture = LightTextureBuilder.CreatePointLight(GameElementsControl.ScreenManager.GraphicsDevice, 320);
			heroLight.Position = new Vector2(0f, 0f);
			heroLight.Color = new Color(204, 204, 204);
			heroLight.Angle = 0f;
			heroLight.Fov = (float)Math.PI * 2f;
			heroLight.Range = 360f;
			MustDefend = false;
			GameElementsControl.Krypton.Lights.Add(heroLight);
			GameElementsControl.ScreenManager.AudioManager.LoadSound("heroClimb", "audio\\noises\\hero\\heroClimb");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("heroDamage", "audio\\noises\\hero\\heroDamage");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("heroFallDamage", "audio\\noises\\hero\\heroFallDamage");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("heroKick", "audio\\noises\\hero\\heroKick");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("kickHit", "audio\\noises\\kickHit");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("bulletTime", "audio\\noises\\hero\\bullettime");
			stateMachine = new HeroStateMachine(Input, this);
		}

		public HeroPistol CreateHeroPistol()
		{
			HeroPistol heroPistol = new HeroPistol(arm);
			heroPistol.Init(this);
			heroPistol.Load();
			return heroPistol;
		}

		public Hook CreateHeroHook()
		{
			Hook hook = new Hook(arm, this);
			hook.Init();
			hook.Load();
			return hook;
		}

		public void PickupPistol(HeroPistol pistol)
		{
			if (numberOfWeapons == 1)
			{
				base.weapon[numberOfWeapons] = pistol;
				activeWeapon = numberOfWeapons;
				base.weapon[activeWeapon].SetArmAnimation();
				numberOfWeapons++;
				return;
			}
			bool flag = false;
			for (int i = 0; i < numberOfWeapons; i++)
			{
				Weapon weapon = base.weapon[i];
				if (weapon.WeaponType == WeaponType.Pistol)
				{
					flag = true;
					HeroPistol heroPistol = weapon as HeroPistol;
					heroPistol.RefillFromPistol(pistol);
				}
			}
			if (!flag)
			{
				Weapon weapon2 = base.weapon[1];
				base.weapon[1] = pistol;
				base.weapon[numberOfWeapons] = weapon2;
				activeWeapon = 1;
				base.weapon[1].SetArmAnimation();
				numberOfWeapons++;
			}
		}

		public int LoadPistol(int bullets)
		{
			bool flag = false;
			for (int i = 0; i < numberOfWeapons; i++)
			{
				Weapon weapon = base.weapon[i];
				if (weapon.WeaponType == WeaponType.Pistol)
				{
					flag = true;
					HeroPistol heroPistol = weapon as HeroPistol;
					heroPistol.RefillBullets(bullets);
				}
			}
			if (flag)
			{
				return 0;
			}
			return bullets;
		}

		public int LoadGrenade(int grenades)
		{
			for (int i = 0; i < numberOfWeapons; i++)
			{
				Weapon weapon = base.weapon[i];
				if (weapon.WeaponType == WeaponType.Pistol)
				{
					HeroPistol heroPistol = weapon as HeroPistol;
					grenades = heroPistol.RefillGrenades(grenades);
				}
			}
			return grenades;
		}

		public bool PickupHook(Hook hook)
		{
			bool flag = false;
			if (numberOfWeapons == 1)
			{
				weapon[numberOfWeapons] = hook;
				activeWeapon = numberOfWeapons;
				weapon[activeWeapon].SetArmAnimation();
				numberOfWeapons++;
			}
			else
			{
				for (int i = 0; i < numberOfWeapons; i++)
				{
					flag = true;
				}
				if (!flag)
				{
					weapon[numberOfWeapons] = hook;
					activeWeapon = numberOfWeapons;
					weapon[activeWeapon].SetArmAnimation();
					numberOfWeapons++;
				}
			}
			return !flag;
		}

		public void DrainStamina()
		{
			if (!staminaDrained)
			{
				staminaCharge = MathHelper.Clamp(staminaCharge - (float)(GameElementsControl.LastFrameTimeInMS / 1500.0) * 100f, 0f, 100f);
				if (staminaCharge == 0f)
				{
					staminaDrained = true;
				}
			}
		}

		protected override void SetCollisionGroup(short collisionGroup)
		{
			base.SetCollisionGroup(collisionGroup);
			geomCircBottomSlide.CollisionGroup = collisionGroup;
		}

		protected override void SetCollisionCategories()
		{
			base.SetCollisionCategories();
			bodyCircBottomSlide.CollisionCategories = mainCollisionCategory;
			bodyCircBottomSlide.CollidesWith = Category.All & ~mainCollisionCategory;
		}

		private void UpdateArmPosition()
		{
			sightAngle = Input.GetAngle(arm.Position);
			if (weapon[activeWeapon].ManualAim(out var angle))
			{
				arm.SetRotation(sightAngle);
			}
			else
			{
				arm.SetRotation(angle);
			}
			heroLight.Angle = Input.GetAngle(arm.Position);
		}

		private void CheckCategoriesSliding()
		{
			if (!categoriesSliding && BaseAngle != 0f)
			{
				mainCollisionCategory = Category.Cat2 | Category.Cat3;
				SetCollisionCategories();
				categoriesSliding = true;
			}
			else if (categoriesSliding && BaseAngle == 0f)
			{
				mainCollisionCategory = Category.Cat3;
				SetCollisionCategories();
				categoriesSliding = false;
			}
		}

		public override void Update()
		{
			if (state != StateEnum.Dead)
			{
				CheckCategoriesSliding();
				if (bulletTime)
				{
					if (bulletTimeCharge == 0f)
					{
						bulletTime = false;
						baseShadowColor = Color.Black;
					}
					else
					{
						bulletTimeCharge = MathHelper.Clamp(bulletTimeCharge - (float)(GameElementsControl.RealLastFrameTimeInMS / 5000.0) * 100f, 0f, 100f);
					}
				}
				else
				{
					bulletTimeCharge = MathHelper.Clamp(bulletTimeCharge + (float)(GameElementsControl.RealLastFrameTimeInMS / 5000.0) * 100f, 0f, 100f);
				}
				staminaCharge = MathHelper.Clamp(staminaCharge + (float)(GameElementsControl.LastFrameTimeInMS / 5000.0) * 100f, 0f, 100f);
				if (staminaCharge == 100f)
				{
					staminaDrained = false;
				}
				UpdateArmPosition();
				heroLight.Position = GameElementsControl.ConvertWorldToScreen(bodyRect.GetWorldPoint(new Vector2(0f, (0f - physHeight) / 2f + 0.1f)));
				if (currentEnergy < baseEnergy)
				{
					currentEnergy = MathHelper.Clamp(currentEnergy + 1f * (float)GameElementsControl.LastFrameTimeInMS / 1000f, 0f, baseEnergy);
				}
			}
			else
			{
				if (heroLight.Fov < (float)Math.PI * 2f)
				{
					heroLight.Fov += (float)GameElementsControl.RealLastFrameTimeInMS * 0.0031415927f;
					if (heroLight.Fov > (float)Math.PI * 2f)
					{
						heroLight.Fov = (float)Math.PI * 2f;
					}
				}
				baseShadowColor = Color.Red;
				heroLight.Position = GameElementsControl.ConvertWorldToScreen(bodyRect.Position);
			}
			shadowColor = Color.Lerp(shadowColor, baseShadowColor, (float)(GameElementsControl.RealLastFrameTimeInMS / 500.0));
			base.Update();
			if (Input.ControlKeyboard.IsNewKeyPress(Keys.P, null, out var playerIndex))
			{
				skeleton.MakePuppet(puppet: true);
			}
			if (Input.ControlKeyboard.IsNewKeyPress(Keys.O, null, out playerIndex))
			{
				skeleton.MakePuppet(puppet: false);
			}
		}

		protected override void Dispose()
		{
			base.Dispose();
			GameElementsControl.World.RemoveBody(bodyCircBottomSlide);
		}

		public override void DeactivateGeoms()
		{
			base.DeactivateGeoms();
			geomCircBottomSlide.CollidesWith = Category.None;
			geomCircBottomSlide.CollisionCategories = Category.None;
		}

		public override void Draw()
		{
			if (!IsDead() && stateMachine.CurrentState.StateID != 24)
			{
				weapon[activeWeapon].Draw();
			}
			skeleton.Draw(GameElementsControl.ScreenManager.SpriteBatch);
		}

		public void DrawCrossHair()
		{
			crossHairPairSprite.Draw(arm.ScreenPosition + GameElementsControl.Input.CursorPosition);
		}

		protected override void UpdateState()
		{
			base.UpdateState();
			if (Input.ControlBulletTime(PlayerIndex))
			{
				ActionBulletTime();
			}
			if (stateMachine.CurrentState.StateID == 24)
			{
				return;
			}
			if (Input.ControlShoot(PlayerIndex) && activeWeapon == 0)
			{
				stateMachine.InsertEvent(0);
			}
			if (Input.ControlChangeWeapon(PlayerIndex))
			{
				if (numberOfWeapons > 1)
				{
					weapon[activeWeapon].Holster();
					activeWeapon = (activeWeapon + 1) % numberOfWeapons;
				}
				weapon[activeWeapon].SetArmAnimation();
			}
			else
			{
				((IWeaponHero)weapon[activeWeapon]).HandleInput(Input, PlayerIndex);
			}
		}

		protected override void FeetDetect()
		{
			double num = 0.0;
			Vector2 vector = ((BaseAngle != 0f) ? bodyCircBottomSlide.Position : bodyCircBottom.Position);
			Vector2 point = vector + new Vector2(0f, physWidth * 4f);
			Vector2 po = Vector2.Zero;
			Vector2 norm = Vector2.Zero;
			float frac;
			Fixture fixture = RayCastCallBacks.RayCastOneNoHuman(vector, point, out po, out norm, out frac);
			if (fixture != null)
			{
				if (frac < 0.5f)
				{
					if (Math.Abs(norm.X) != 1f)
					{
						lastTimeOnFloor = GameElementsControl.CurrentTimeInMS;
						currentTimeOnFloor += GameElementsControl.LastFrameTimeInMS;
					}
					SetFloorWorldObject(fixture);
				}
				num = norm.GetAngle() + (float)Math.PI / 2f;
			}
			else
			{
				num = 0.0;
			}
			RotateByFloor(num);
		}

		protected override void StateDead()
		{
		}

		public override void Reload()
		{
			if (weapon[activeWeapon].Reload() && state != StateEnum.Hanging)
			{
				skeleton.InsertTempAnimation(reloadAnimation);
			}
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			if (wod.Type == WorldObjectType.CollisionWorldObject)
			{
				float num = CalculateRelativeSpeedFromContact(ref contact, ref oldManifold, out var impactPoint, out var _);
				if (num >= 15f)
				{
					num -= 15f;
					num = num / 15f * 100f;
					Hit(Vector2.One * num, Vector2.Zero, HitType.Blunt);
				}
				if (stateMachine.CurrentState.StateID == 8 && num > 1f && Math.Round(MathHelper.WrapAngle(BaseAngle), 1) != 0.0)
				{
					jumpDust.Trigger(GameElementsControl.ConvertWorldToScreen(impactPoint));
					stateMachine.InsertEvent(3);
				}
			}
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			if (stateMachine.CurrentState is StateDefending)
			{
				if (Math.Sign(hitDirection.X) != (int)sideLooking)
				{
					hitDirection /= 2f;
				}
				if (hitDirection.Length() <= 10f)
				{
					return;
				}
			}
			int num = Math.Sign(hitDirection.X);
			num = ((num != 1) ? 1 : 2);
			switch (hitType)
			{
			case HitType.Blunt:
				shadowColor = Color.Red;
				currentEnergy -= hitDirection.Length();
				GameElementsControl.NoiseManager.AddNoise("heroFallDamage", base.Position);
				stateMachine.InsertEvent(num);
				break;
			case HitType.Kick:
			{
				shadowColor = Color.Red;
				float num2 = hitDirection.Length();
				currentEnergy -= num2;
				if (num2 > 35f)
				{
					stateMachine.InsertEvent(3);
					bodyRect.ApplyLinearImpulse(hitDirection + new Vector2(0f, 30f));
				}
				GameElementsControl.NoiseManager.AddNoise("heroDamage", base.Position);
				stateMachine.InsertEvent(num);
				break;
			}
			case HitType.Pistol:
				shadowColor = Color.Red;
				currentEnergy -= hitDirection.Length();
				GameElementsControl.NoiseManager.AddNoise("heroDamage", base.Position);
				stateMachine.InsertEvent(num);
				break;
			case HitType.Explosion:
				if (hitDirection.Length() >= 30f)
				{
					stateMachine.InsertEvent(3);
				}
				currentEnergy -= hitDirection.Length() * 0.25f;
				shadowColor = Color.Red;
				GameElementsControl.NoiseManager.AddNoise("heroDamage", base.Position);
				stateMachine.InsertEvent(num);
				break;
			}
		}

		private void ActionBulletTime()
		{
			if (!bulletTime && bulletTimeCharge == 100f)
			{
				GameElementsControl.NoiseManager.AddNoise("bulletTime", base.Position);
				bulletTime = true;
				baseShadowColor = Color.Blue;
			}
		}

		public override void ActionWalk(Side side, float maxSpeed)
		{
			if (Landed())
			{
				if (currentAnimation == "RUN")
				{
					if (skeleton.AnimationKeyFrameAt(0) || skeleton.AnimationKeyFrameAt(2))
					{
						floorWorldObject.StepOver(bodyCircBottom.Position, WorldObjectType.Hero, running: true);
					}
				}
				else if (currentAnimation == "WALK")
				{
					if (skeleton.AnimationKeyFrameAt(0) || skeleton.AnimationKeyFrameAt(3))
					{
						floorWorldObject.StepOver(bodyCircBottom.Position, WorldObjectType.Hero, running: false);
					}
				}
				else if (currentAnimation == "BACKWALK" && (skeleton.AnimationKeyFrameAt(0) || skeleton.AnimationKeyFrameAt(2)))
				{
					floorWorldObject.StepOver(bodyCircBottom.Position, WorldObjectType.Hero, running: false);
				}
			}
			base.ActionWalk(side, maxSpeed);
		}

		private void CheckSlopes()
		{
			float num = Math.Abs((float)Math.Sqrt(2f * (0f - GameElementsControl.Gravity.Y) * (physWidth / 3f)) / GameElementsControl.Gravity.Y);
			float num2 = bodyRect.LinearVelocity.X * num;
			Vector2 worldVector = bodyRect.GetWorldVector(new Vector2(1f, 0f));
			float num3 = worldVector.Y / worldVector.X;
			Vector2 point = bodyCircBottom.Position + new Vector2(num2, num3 * num2);
			if (point.Y < bodyCircBottom.Position.Y)
			{
				point.Y = bodyCircBottom.Position.Y;
			}
			Vector2 point2 = new Vector2(point.X, bodyCircBottom.Position.Y - physWidth * 2f);
			float frac = float.MaxValue;
			Vector2 norm = Vector2.Zero;
			Vector2 po = Vector2.Zero;
			Fixture fixture = RayCastCallBacks.RayCastOneNoHuman(point, point2, out po, out norm, out frac);
			if (fixture != null)
			{
				float num4 = norm.GetAngle() + (float)Math.PI / 2f;
				num4 = MathHelper.Clamp(MathHelper.WrapAngle(num4), 0f - Globals.FloorAngleLimit, Globals.FloorAngleLimit) + BaseAngle;
				float num5 = 0f - MathHelper.WrapAngle(bodyRect.Rotation);
				double num6 = ((!(bodyRect.LinearVelocity.X > 0f)) ? Math.Round(num5 - num4, 1) : Math.Round(num4 - num5, 1));
				if (num6 < 0.0)
				{
					ActionJump(physWidth / 3f, realJump: false);
				}
			}
		}

		public void ActionWallJump(float meters, Side side)
		{
			jumpDust.Trigger(GameElementsControl.ConvertWorldToScreen(base.FeetPosition));
			Vector2 impulse = new Vector2((float)(0 - side) * 70f, (0f - Mass) * ((float)Math.Sqrt(2f * GameElementsControl.Gravity.Y * meters) + bodyRect.LinearVelocity.Y));
			bodyCircBottom.ApplyLinearImpulse(ref impulse);
			lastJumpTime = GameElementsControl.CurrentTimeInMS;
			GameElementsControl.NoiseManager.AddNoise("heroJump", base.Position);
		}

		public void ActionCrawl(Side side, float maxSpeed)
		{
			if (bodyRect.LinearVelocity.X * (float)side < maxSpeed)
			{
				float num = maxSpeed / 2000f * (float)GameElementsControl.LastFrameTimeInMS / wheelSlideRadius;
				if (TouchingFloor())
				{
					wheelSlideRevoluteJoint.MotorEnabled = true;
					float value = (0f - maxSpeed) / wheelSlideRadius;
					MathHelper.Min(Math.Abs(value), Math.Abs(bodyRect.LinearVelocity.X / wheelSlideRadius) + num);
					wheelSlideRevoluteJoint.MotorSpeed = (float)side * maxSpeed / wheelSlideRadius;
				}
				else
				{
					float value2 = maxSpeed * (float)side - bodyRect.LinearVelocity.X;
					value2 = ((side != Side.Left) ? MathHelper.Clamp(value2, 0f, num) : MathHelper.Clamp(value2, 0f - num, 0f));
					Vector2 impulse = new Vector2(value2 * Mass, 0f);
					bodyRect.ApplyLinearImpulse(ref impulse);
				}
			}
		}

		public override void ActionStopWalking()
		{
			base.ActionStopWalking();
			wheelSlideRevoluteJoint.MotorSpeed = 0f;
		}

		public void ActionStopCrawling()
		{
			wheelSlideRevoluteJoint.MotorEnabled = true;
			wheelSlideRevoluteJoint.MotorSpeed = 0f;
		}

		public void ActionSlide(Side side)
		{
			int num = ((side != Side.Right) ? 1 : (-1));
			float num2 = 0.1f - base.Width / 2f + wheelSlideRadius / 2f;
			double num3 = Math.Acos(num2 / (physHeight / 2f));
			wheelSlideRevoluteJoint.MotorEnabled = false;
			wheelRevoluteJoint.MotorSpeed = 0f;
			wheelRevoluteJoint.MotorEnabled = false;
			BaseAngle = (float)num * (float)num3;
			geomCircBottomSlide.IsSensor = false;
			circBottomObjectData.Type = WorldObjectType.Hero;
		}

		public void SetPivotJoint(Side side)
		{
			wheelSlideRevoluteJoint.LocalAnchorA = new Vector2((float)(0 - side) * 0.1f, 0f);
		}

		public void ActionStand()
		{
			BaseAngle = 0f;
			wheelSlideRevoluteJoint.MotorEnabled = false;
			_ = wheelRevoluteJoint.MotorEnabled;
			geomCircBottomSlide.IsSensor = true;
			circBottomObjectData.Type = WorldObjectType.HumanFeet;
			SetPivotJoint(Side.None);
		}

		public void ActionCrawlJump()
		{
			wheelSlideRevoluteJoint.MotorEnabled = false;
			wheelRevoluteJoint.MotorSpeed = 0f;
			wheelRevoluteJoint.MotorEnabled = false;
			balanceRevoluteJoint.MotorEnabled = false;
		}

		public void ActionCrawlLand()
		{
			wheelSlideRevoluteJoint.MotorEnabled = true;
			wheelRevoluteJoint.MotorSpeed = 0f;
			balanceRevoluteJoint.MotorEnabled = true;
		}

		public void ActionJump(float meters, bool realJump)
		{
			if (realJump)
			{
				jumpDust.Trigger(GameElementsControl.ConvertWorldToScreen(base.FeetPosition));
				GameElementsControl.NoiseManager.AddNoise("heroJump", base.Position);
			}
			Vector2 impulse = new Vector2(0f, (0f - mass) * (float)Math.Sqrt(2f * GameElementsControl.Gravity.Y * meters));
			lastJumpTime = GameElementsControl.CurrentTimeInMS;
			bodyCircBottom.ApplyLinearImpulse(ref impulse);
		}

		public void ActionSmallJump(Side side)
		{
			Vector2 impulse = new Vector2((float)(0 - side) * 20f, 0f);
			bodyCircBottom.ApplyLinearImpulse(ref impulse);
		}

		protected override void Die()
		{
			heroLight.Texture = heroDeadLightTexture;
			wheelSlideRevoluteJoint.MotorEnabled = false;
			base.Die();
		}
	}
}
