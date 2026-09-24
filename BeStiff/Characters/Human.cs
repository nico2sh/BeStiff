using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;
using ProjectMercury.Emitters;
using SKAnimation;

namespace Be_Stiff.Characters
{
	public class Human : WorldObject, IOneSidedAffected
	{
		protected const float jumpSideSpeed = 5f;

		protected Body bodyRect;

		protected Fixture geomRect;

		protected Body bodyCircBottom;

		protected Fixture geomCircBottom;

		private Fixture geomCircTop;

		private Body bodyBalance;

		protected WorldObjectData circBottomObjectData;

		protected WorldObject floorWorldObject;

		protected WorldObject oneSideIgnored;

		protected Arm arm;

		protected Weapon[] weapon;

		protected int activeWeapon;

		protected int numberOfWeapons;

		protected RevoluteJoint wheelRevoluteJoint;

		protected RevoluteJoint balanceRevoluteJoint;

		protected StateEnum state;

		protected StateMachine stateMachine;

		protected Side sideLooking;

		protected double lastTimeOnFloor;

		protected double currentTimeOnFloor;

		protected double lastJumpTime;

		protected bool slipping;

		protected Skeleton skeleton;

		protected ChildBone headBone;

		protected string currentAnimation;

		public bool ignoreSwitchSide;

		protected float floorAngle;

		public float BaseAngle;

		protected float sightAngle;

		private byte alphaColor;

		protected float mass;

		protected float physWidth = 0.3f;

		protected float physHeight = 1.95f;

		protected float runSpeed;

		protected float wheelRadius;

		protected float baseEnergy;

		protected float currentEnergy;

		private bool disposed;

		protected ConeEmitter blood;

		private LineEmitter bloodBlow;

		protected CircleEmitter jumpDust;

		protected double timeOfDeath;

		protected Category mainCollisionCategory;

		public WorldObject OneSidedIgnored => oneSideIgnored;

		public RevoluteJoint WheelRevoluteJoint => wheelRevoluteJoint;

		public bool Balance
		{
			get
			{
				return balanceRevoluteJoint.MotorEnabled;
			}
			set
			{
				balanceRevoluteJoint.MotorEnabled = value;
			}
		}

		public float RunSpeed => runSpeed;

		public float WheelRadius => wheelRadius;

		public Skeleton Skeleton => skeleton;

		public float Width => physWidth;

		public float Height => physHeight;

		public double TimeOfDeath => timeOfDeath;

		public Vector2 Position => bodyRect.Position;

		/// <summary>Debug helper: moves the whole character to a position (display pixels).</summary>
		public void DebugTeleport(Vector2 displayPos)
		{
			// Joints added during load are only linked to their bodies on the
			// next step; link them now so the whole body graph moves.
			GameElementsControl.World.ProcessChanges();
			Vector2 target = ConvertUnits.ToSimUnits(displayPos);
			Vector2 delta = target - bodyRect.Position;
			var moved = new System.Collections.Generic.HashSet<FarseerPhysics.Dynamics.Body>();
			var queue = new System.Collections.Generic.Queue<FarseerPhysics.Dynamics.Body>();
			queue.Enqueue(bodyRect);
			while (queue.Count > 0)
			{
				var body = queue.Dequeue();
				if (!moved.Add(body) || body.BodyType == FarseerPhysics.Dynamics.BodyType.Static)
				{
					continue;
				}
				body.Position += delta;
				body.LinearVelocity = Vector2.Zero;
				for (var edge = body.JointList; edge != null; edge = edge.Next)
				{
					queue.Enqueue(edge.Other);
				}
			}
		}

		public Vector2 FeetPosition => bodyCircBottom.Position;

		public virtual Vector2 FloorPosition => bodyCircBottom.Position + new Vector2(0f, wheelRadius); // y-down: feet are below the wheel centre

		public byte AlphaColor
		{
			get
			{
				return alphaColor;
			}
			set
			{
				alphaColor = value;
			}
		}

		public Side SideLooking => sideLooking;

		public override Body MainBody => bodyRect;

		public Body FeetBody => bodyCircBottom;

		public override float Mass => mass;

		public bool Disposed => disposed;

		public float Energy => currentEnergy;

		public float SightAngle => sightAngle;

		public float WeaponRange => weapon[activeWeapon].WeaponRange;

		public void OneSidedIgnore(WorldObject wo)
		{
			if (wo == floorWorldObject)
			{
				oneSideIgnored = wo;
			}
		}

		public Human(Vector2 size)
		{
			state = StateEnum.Standed;
			physWidth = size.X;
			physHeight = size.Y;
			slipping = false;
			lastTimeOnFloor = 0.0;
			currentTimeOnFloor = 0.0;
			activeWeapon = 0;
			floorAngle = 0f;
			BaseAngle = 0f;
			sideLooking = Side.Right;
			ignoreSwitchSide = false;
		}

		public virtual void MoveToPosition(Vector2 floorPos)
		{
			bodyRect.Rotation = 0f;
			bodyRect.Position = new Vector2(floorPos.X, floorPos.Y + physHeight / 2f);
			bodyBalance.Position = new Vector2(bodyRect.Position.X, bodyRect.Position.Y - (physHeight - physWidth) / 2f);
			bodyCircBottom.Position = new Vector2(bodyRect.Position.X, bodyRect.Position.Y - (physHeight - physWidth) / 2f);
			bodyRect.LinearVelocity = Vector2.Zero;
			bodyBalance.LinearVelocity = Vector2.Zero;
			bodyCircBottom.LinearVelocity = Vector2.Zero;
			bodyRect.AngularVelocity = 0f;
			bodyBalance.AngularVelocity = 0f;
			bodyCircBottom.AngularVelocity = 0f;
		}

		public void ApplyImpulse(ref Vector2 impulse)
		{
			bodyCircBottom.ApplyLinearImpulse(ref impulse);
		}

		public void SetFixedSide(Side fixedSide)
		{
			ignoreSwitchSide = true;
			sideLooking = fixedSide;
			skeleton.SwitchSide((int)sideLooking);
		}

		public void UnsetFixedSide()
		{
			ignoreSwitchSide = false;
		}

		public virtual bool IgnoreOneSide()
		{
			return false;
		}

		public sealed override bool LoadFromOgmo(OgmoObject obj)
		{
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			// Feet go 0.25 m below the Ogmo point (y-down), i.e. on the floor under
			// the template's origin.
			Vector2 pos = new Vector2(worldScaledOgmoObject.Position.X, worldScaledOgmoObject.Position.Y + 0.25f);
			if (obj.Name.Equals("Hero"))
			{
				Load(pos, WorldObjectType.Hero, obj);
			}
			else
			{
				if (!obj.Name.StartsWith("Enemy"))
				{
					return false;
				}
				Load(pos, WorldObjectType.Human, obj);
			}
			return true;
		}

		private void Load(Vector2 pos, WorldObjectType wodt, OgmoObject obj)
		{
			wheelRadius = physWidth / 2f;
			bodyRect = BodyFactory.CreateBody(GameElementsControl.World);
			bodyRect.BodyType = BodyType.Dynamic;
			bodyRect.Position = new Vector2(pos.X, pos.Y - physHeight / 2f);
			Vertices vertices = PolygonTools.CreateRectangle(physWidth / 2f, (physHeight - physWidth) / 2f);
			PolygonShape shape = new PolygonShape(vertices, 10f);
			geomRect = bodyRect.CreateFixture(shape);
			geomRect.UserData = new WorldObjectData(wodt, this);
			geomRect.Friction = 0.01f;
			geomRect.Restitution = 0f;
			bodyRect.AngularDamping = 20f;
			Fixture fixture = geomRect;
			fixture.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture.AfterCollision, new AfterCollisionEventHandler(AfterHumanCollision));
			CircleShape circleShape = new CircleShape(wheelRadius, 10f);
			circleShape.Position = new Vector2(0f, (0f - (physHeight - physWidth)) / 2f);
			geomCircTop = bodyRect.CreateFixture(circleShape);
			geomCircTop.UserData = new WorldObjectData(WorldObjectType.HumanHead, this);
			geomCircTop.Restitution = 0f;
			Fixture fixture2 = geomCircTop;
			fixture2.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture2.AfterCollision, new AfterCollisionEventHandler(AfterHumanCollision));
			arm = new Arm();
			bodyCircBottom = BodyFactory.CreateBody(GameElementsControl.World);
			bodyCircBottom.BodyType = BodyType.Dynamic;
			bodyCircBottom.Position = new Vector2(bodyRect.Position.X, bodyRect.Position.Y + (physHeight - physWidth) / 2f);
			circleShape = new CircleShape(wheelRadius, 50f);
			circleShape.Position = Vector2.Zero;
			geomCircBottom = bodyCircBottom.CreateFixture(circleShape);
			geomCircBottom.Friction = 10f;
			geomCircBottom.Restitution = 0f;
			Fixture fixture3 = geomCircBottom;
			fixture3.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture3.AfterCollision, new AfterCollisionEventHandler(AfterHumanCollision));
			circBottomObjectData = new WorldObjectData(WorldObjectType.HumanFeet, this);
			geomCircBottom.UserData = circBottomObjectData;
			geomCircBottom.CollisionCategories |= Category.Cat5;
			geomCircBottom.CollidesWith &= ~Category.Cat6;
			bodyBalance = BodyFactory.CreateBody(GameElementsControl.World);
			bodyBalance.BodyType = BodyType.Dynamic;
			bodyBalance.Position = new Vector2(bodyRect.Position.X, bodyRect.Position.Y + (physHeight - physWidth) / 2f);
			bodyBalance.FixedRotation = true;
			wheelRevoluteJoint = new RevoluteJoint(bodyRect, bodyCircBottom, bodyRect.GetLocalPoint(bodyCircBottom.Position), Vector2.Zero);
			GameElementsControl.World.AddJoint(wheelRevoluteJoint);
			wheelRevoluteJoint.MotorEnabled = true;
			wheelRevoluteJoint.MaxMotorTorque = 2500f;
			wheelRevoluteJoint.MotorSpeed = 0f;
			balanceRevoluteJoint = new RevoluteJoint(bodyRect, bodyBalance, bodyRect.GetLocalPoint(bodyBalance.Position), Vector2.Zero);
			GameElementsControl.World.AddJoint(balanceRevoluteJoint);
			balanceRevoluteJoint.MotorEnabled = true;
			balanceRevoluteJoint.MaxMotorTorque = 10000f;
			balanceRevoluteJoint.MotorSpeed = 0f;
			mass = bodyBalance.Mass + bodyCircBottom.Mass + bodyRect.Mass;
			runSpeed = 10f;
			baseEnergy = 100f;
			currentEnergy = baseEnergy;
			blood = (ConeEmitter)GameElementsControl.Particlesmanager.getBloodEmitter();
			bloodBlow = (LineEmitter)GameElementsControl.Particlesmanager.getBloodBlowEmitter();
			jumpDust = (CircleEmitter)GameElementsControl.Particlesmanager.getJumpDustEmitter();
			GameElementsControl.ScreenManager.AudioManager.LoadSound("heroJump", "audio\\noises\\hero\\heroJump");
			LoadCustomStuff(obj);
		}

		protected virtual void LoadCustomStuff(OgmoObject obj)
		{
		}

		protected void RecalculateMass()
		{
			mass = bodyBalance.Mass + bodyCircBottom.Mass + bodyRect.Mass;
		}

		protected virtual void SetCollisionGroup(short collisionGroup)
		{
			geomRect.CollisionGroup = collisionGroup;
			geomCircTop.CollisionGroup = collisionGroup;
			geomCircBottom.CollisionGroup = collisionGroup;
		}

		protected virtual void SetCollisionCategories()
		{
			// Characters pass through each other; punches and kicks use AABB
			// queries on categories, so fighting is unaffected.
			bodyRect.CollisionCategories = mainCollisionCategory;
			bodyRect.CollidesWith = Category.All & ~Globals.CollisionCharacters;
			bodyBalance.CollisionCategories = mainCollisionCategory;
			bodyBalance.CollidesWith = Category.All & ~Globals.CollisionCharacters;
			bodyCircBottom.CollisionCategories = mainCollisionCategory | Category.Cat5;
			bodyCircBottom.CollidesWith = Category.All & ~Globals.CollisionCharacters & ~Category.Cat6 & ~Category.Cat5;
		}

		protected virtual void Dispose()
		{
			if (!disposed)
			{
				GameElementsControl.World.RemoveBody(bodyCircBottom);
				GameElementsControl.World.RemoveBody(bodyBalance);
				GameElementsControl.World.RemoveBody(bodyRect);
				skeleton.Dispose(GameElementsControl.World);
				disposed = true;
			}
		}

		public virtual void DeactivateGeoms()
		{
			geomRect.CollidesWith = Category.None;
			geomRect.CollisionCategories = Category.None;
			geomCircBottom.CollidesWith = Category.None;
			geomCircBottom.CollisionCategories = Category.None;
			geomCircTop.CollidesWith = Category.None;
			geomCircTop.CollisionCategories = Category.None;
			geomCircTop.CollidesWith = Category.None;
			geomCircTop.CollisionCategories = Category.None;
		}

		public virtual void ReactivateGeoms()
		{
			SetCollisionCategories();
		}

		protected void AfterHumanCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			float num = CalculateMaxImpulse(contact);
			float num2 = (IsDead() ? 200 : 700);
			if (num >= 100f && num >= num2)
			{
				Crush();
			}
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			if (wod.Type == WorldObjectType.CollisionWorldObject)
			{
				float num = CalculateRelativeSpeedFromContact(ref contact, ref oldManifold);
				if (num >= 15f)
				{
					num -= 15f;
					num = num / 15f * 100f;
					Hit(Vector2.One * num, Vector2.Zero, HitType.Blunt);
				}
			}
		}

		public void ComputeFeet(Vector2 normal)
		{
			if (Math.Abs(normal.X) != 1f)
			{
				lastTimeOnFloor = GameElementsControl.CurrentTimeInMS;
				currentTimeOnFloor += GameElementsControl.LastFrameTimeInMS;
			}
			else
			{
				currentTimeOnFloor = 0.0;
			}
		}

		protected virtual void Crush()
		{
			if (!disposed)
			{
				if (!IsDead())
				{
					timeOfDeath = GameElementsControl.CurrentTimeInMS;
				}
				currentEnergy = 0f;
				state = StateEnum.Dead;
				bloodBlow.Angle = bodyRect.Rotation - (float)Math.PI / 2f;
				bloodBlow.Trigger(GameElementsControl.ConvertWorldToScreen(bodyRect.Position));
				Dispose();
			}
		}

		public bool IsDead()
		{
			return state == StateEnum.Dead;
		}

		public bool TouchingFloor()
		{
			if (lastTimeOnFloor + GameElementsControl.LastFrameTimeInMS < GameElementsControl.CurrentTimeInMS)
			{
				return false;
			}
			return true;
		}

		public bool Landed()
		{
			if (TouchingFloor() && GameElementsControl.CurrentTimeInMS - lastJumpTime > 100.0)
			{
				return currentTimeOnFloor >= 20.0;
			}
			return false;
		}

		public override void Update()
		{
			UpdateSide();
			UpdateState();
			UpdateAnimations();
			if (!TouchingFloor())
			{
				currentTimeOnFloor = 0.0;
			}
			if (state != StateEnum.Dead)
			{
				weapon[activeWeapon].Update();
				weapon[activeWeapon].UpdateDrawSide(sideLooking);
				FeetDetect();
				if (currentEnergy <= 0f)
				{
					Die();
				}
			}
		}

		protected virtual void Die()
		{
			timeOfDeath = GameElementsControl.CurrentTimeInMS;
			weapon[activeWeapon].Holster();
			balanceRevoluteJoint.MotorEnabled = false;
			state = StateEnum.Dead;
			wheelRevoluteJoint.MotorEnabled = false;
			MakePuppet();
			// Topple the torso forward so the ragdoll slumps instead of balancing
			// on its legs.
			bodyRect.AngularVelocity += (float)sideLooking * 3f;
			skeleton.ApplyRagdollAngularVelocity((float)sideLooking * 4f);
		}

		public virtual void Resurrect()
		{
			balanceRevoluteJoint.MotorEnabled = true;
			state = StateEnum.Standed;
			wheelRevoluteJoint.MotorEnabled = true;
			currentEnergy = baseEnergy;
			weapon[activeWeapon].Holster();
			skeleton.MakePuppet(puppet: false);
			ReactivateGeoms();
			UpdateAnimations();
			UpdateSide();
		}

		public void BloodyDie()
		{
			bloodBlow.Angle = bodyRect.Rotation - (float)Math.PI / 2f;
			bloodBlow.Trigger(GameElementsControl.ConvertWorldToScreen(bodyRect.Position));
			Die();
		}

		protected virtual void MakePuppet()
		{
			skeleton.MakePuppet(puppet: true);
			// The capsule keeps colliding with the floor so the body can topple
			// and lie down; without it the corpse balances on its ragdoll legs.
		}

		protected void UpdateAnimations()
		{
			int num = (int)sideLooking;
			sightAngle = MathHelper.WrapAngle(sightAngle);
			float num2 = sightAngle - bodyRect.Rotation;
			float num3 = ((num != 1) ? MathHelper.Clamp(MathHelper.WrapAngle((float)Math.PI - num2) / 2f, 0f - headBone.AngleOffset, headBone.AngleOffset) : MathHelper.Clamp(MathHelper.WrapAngle(num2) / 2f, 0f - headBone.AngleOffset, headBone.AngleOffset));
			headBone.Angle = (float)Math.PI + num3;
			skeleton.Position = GameElementsControl.ConvertWorldToScreen(Position);
			skeleton.Angle = (float)sideLooking * bodyRect.Rotation + (float)Math.PI;
		}

		protected void UpdateSide()
		{
			arm.Update();
			skeleton.Update(GameElementsControl.LastFrameTimeInMS);
			if (!ignoreSwitchSide)
			{
				Side side = sideLooking;
				Vector2 zero = Vector2.Zero;
				zero.X = 5f * (float)Math.Cos(sightAngle);
				zero.Y = 5f * (float)Math.Sin(sightAngle);
				Vector2 vector = Vector2.Transform(GameElementsControl.ConvertScreenToWorld(zero), Matrix.CreateRotationZ(0f - bodyRect.Rotation));
				if (vector.X < -0.01f)
				{
					sideLooking = Side.Left;
				}
				else if (vector.X > 0.01f)
				{
					sideLooking = Side.Right;
				}
				if (side != sideLooking)
				{
					float rotation = arm.GetRotation();
					skeleton.SwitchSide((int)sideLooking);
					arm.SetRotation(rotation);
				}
			}
		}

		protected virtual void UpdateState()
		{
			// Only the hero owns a state machine; enemies are driven by their Brain.
			stateMachine?.Update();
		}

		public virtual void Reload()
		{
			weapon[activeWeapon].Reload();
		}

		protected void RotateByFloor(double angle)
		{
			if (double.IsNaN(angle))
			{
				// Degenerate floor normal (e.g. overlapping geometry); skip this frame.
				return;
			}
			angle = MathHelper.Clamp(MathHelper.WrapAngle((float)angle), 0f - Globals.FloorAngleLimit, Globals.FloorAngleLimit);
			floorAngle = (float)angle;
			angle += (double)BaseAngle;
			float num = MathHelper.WrapAngle(bodyRect.Rotation);
			double num2 = Math.Round(angle - (double)num, 2);
			num2 = angle - (double)num;
			if (double.IsNaN(num2))
			{
				// Body rotation is NaN (physics blew up, e.g. spawned inside geometry).
				return;
			}
			if (num2 != 0.0)
			{
				float num3 = Math.Abs((float)(num2 * 1000.0 / GameElementsControl.LastFrameTimeInMS));
				float motorSpeed = ((state == StateEnum.Flipping) ? MathHelper.Clamp((float)Math.PI * 2f * (float)Math.Sign(num2) * 1.5f, 0f - num3, num3) : MathHelper.Clamp(-(float)Math.PI * (float)Math.Sign(num2) * 1.5f, 0f - num3, num3));
				balanceRevoluteJoint.MotorSpeed = motorSpeed;
			}
			else
			{
				balanceRevoluteJoint.MotorSpeed = 0f;
			}
		}

		/// <summary>Where the floor-detection ray starts (the feet wheel by default).</summary>
		protected virtual Vector2 FeetRayOrigin => bodyCircBottom.Position;

		protected void FeetDetect()
		{
			double num = 0.0;
			Vector2 position = FeetRayOrigin;
			Vector2 point = position + new Vector2(0f, physWidth * 4f);
			Vector2 norm = Vector2.Zero;
			Vector2 po = Vector2.Zero;
			float frac;
			Fixture fixture = RayCastCallBacks.RayCastOneNoHuman(position, point, out po, out norm, out frac);
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

		protected void SetFloorWorldObject(Fixture floorFixture)
		{
			if (floorFixture.UserData is WorldObjectData worldObjectData)
			{
				floorWorldObject = worldObjectData.Object;
				if (floorWorldObject != OneSidedIgnored)
				{
					oneSideIgnored = null;
				}
			}
		}

		public virtual void Shoot()
		{
			weapon[activeWeapon].Shoot();
		}

		public override void Draw()
		{
		}

		protected virtual void StateDead()
		{
		}

		protected virtual void StateAlways()
		{
		}

		protected virtual void StateStanded()
		{
			wheelRevoluteJoint.MotorEnabled = true;
			wheelRevoluteJoint.MotorSpeed = 0f;
		}

		protected virtual void StateWalking()
		{
			if (!TouchingFloor())
			{
				if (BaseAngle == 0f)
				{
					wheelRevoluteJoint.MotorEnabled = true;
				}
				state = StateEnum.Jumping;
			}
		}

		protected virtual void StateJumping()
		{
			wheelRevoluteJoint.MotorSpeed = (0f - bodyRect.LinearVelocity.X) / wheelRadius;
			if (Landed())
			{
				state = StateEnum.Standed;
				return;
			}
			Math.Abs(bodyRect.LinearVelocity.Y);
			_ = 500f;
		}

		public virtual void ActionWalk(Side side, float maxSpeed)
		{
			if (side != sideLooking)
			{
				maxSpeed /= 2f;
			}
			float num = wheelRevoluteJoint.MotorSpeed * wheelRadius;
			float num2 = maxSpeed / 500f * (float)GameElementsControl.LastFrameTimeInMS / wheelRadius;
			float motorSpeed;
			if (Math.Abs(num) < 0.5f)
			{
				motorSpeed = (float)side * 1f / wheelRadius;
			}
			else
			{
				float max = maxSpeed / wheelRadius;
				motorSpeed = (float)side * MathHelper.Clamp((float)side * num / wheelRadius + num2, 0f, max);
			}
			wheelRevoluteJoint.MotorSpeed = motorSpeed;
		}

		public virtual void ActionStopWalking()
		{
			wheelRevoluteJoint.MotorEnabled = true;
			wheelRevoluteJoint.MotorSpeed = 0f;
		}
	}
}
