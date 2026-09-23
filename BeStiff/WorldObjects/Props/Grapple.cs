using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Grapple : WorldObject
	{
		private const float width = 0.3f;

		private const float springConstant = 25f;

		private const float dampingConstant = 1f;

		private const float restLenght = 1f;

		private const float hitStrength = 30f;

		private GameSprite grappleSprite;

		private Fixture fixture;

		private Body attachBody;

		private Hook mainHook;

		private bool enabled;

		private bool hooked;

		private float linkDistance;

		private CircleEmitter hitEmitter;

		public float LinkDistance => linkDistance;

		public float Width => 0.3f;

		public bool Hooked => hooked;

		public Body AttachBody => attachBody;

		public Fixture Fixture
		{
			get
			{
				return fixture;
			}
			set
			{
				fixture = value;
			}
		}

		public Vector2 LocalAnchorPoint => Vector2.Zero;

		public bool IsEnabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (!value)
				{
					mainBody.IgnoreGravity = true;
					mainBody.Enabled = false;
					hooked = false;
				}
				else
				{
					mainBody.Enabled = true;
				}
				enabled = value;
			}
		}

		public void Load(Hook hook)
		{
			mainHook = hook;
			hooked = false;
			linkDistance = 0f;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			CircleShape shape = new CircleShape(0.15f, 300f);
			fixture = mainBody.CreateFixture(shape);
			hitEmitter = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
			fixture.CollisionCategories = Category.Cat3;
			fixture.CollisionCategories |= Category.Cat7;
			fixture.CollidesWith &= ~Category.Cat3;
			fixture.CollidesWith &= ~Category.Cat8;
			fixture.CollisionGroup = -1;
			fixture.UserData = new WorldObjectData(WorldObjectType.Grapple, this);
			MainBody.IgnoreGravity = true;
			MainBody.Enabled = false;
			MainBody.IsBullet = true;
			GameElementsControl.LoadSprite("grapple", "sprites\\guns\\hook\\grapple");
			grappleSprite = GameElementsControl.GetSprite("grapple");
		}

		public void Shoot(Vector2 pos, float rotation, Vector2 initVelocity)
		{
			IsEnabled = true;
			mainBody.LinearVelocity = Vector2.Zero;
			Vector2 impulse = VectorUtil.CreateVector2(rotation, 1f);
			MainBody.Position = pos;
			MainBody.Rotation = rotation;
			impulse *= 500f;
			MainBody.Enabled = true;
			MainBody.ApplyLinearImpulse(ref impulse);
		}

		public Vector2 GetWorldAnchorPoint()
		{
			return mainBody.Position;
		}

		public void Attach()
		{
			fixture.CollidesWith = Category.None;
			fixture.CollisionCategories = Category.None;
			GameElementsControl.NoiseManager.AddNoise("hookAttach", MainBody.Position);
		}

		public void Detach()
		{
			fixture.CollisionCategories = Category.Cat3;
			fixture.CollisionCategories |= Category.Cat7;
			fixture.CollidesWith = Category.All;
			fixture.CollidesWith &= ~Category.Cat3;
			fixture.CollidesWith &= ~Category.Cat8;
		}

		public void Dissapear()
		{
			_ = hooked;
		}

		public void PullTo(Arm armLinked)
		{
		}

		public override void Update()
		{
		}

		public override void Draw()
		{
			grappleSprite.Draw(GameElementsControl.ConvertWorldToScreen(MainBody.Position));
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			Vector2 impactPoint;
			Vector2 impactNormal;
			float num = CalculateRelativeSpeedFromContact(ref contact, ref oldManifold, out impactPoint, out impactNormal);
			if (num == 0f)
			{
				return;
			}
			hitEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
			if (wod == null)
			{
				return;
			}
			if (!wod.Object.CanBeHooked())
			{
				mainBody.IgnoreGravity = false;
				wod.Object.Hit(Vector2.Normalize(mainBody.LinearVelocity) * 30f, mainBody.Position, HitType.Kick);
			}
			else if (IsEnabled)
			{
				contact.Enabled = false;
				if (!(wod.Object is Hero))
				{
					mainBody.LinearVelocity = Vector2.Zero;
					attachBody = wod.Object.MainBody;
					hooked = true;
					mainBody.IgnoreGravity = false;
				}
			}
		}

		private bool OnCollision(Fixture fixture1, Fixture fixture2, Contact manifold)
		{
			if (fixture2.IsSensor)
			{
				return false;
			}
			hitEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
			if (fixture2.UserData is WorldObjectData worldObjectData)
			{
				if (!worldObjectData.Object.CanBeHooked())
				{
					mainBody.IgnoreGravity = false;
					worldObjectData.Object.Hit(Vector2.Normalize(mainBody.LinearVelocity) * 30f, mainBody.Position, HitType.Blunt);
					return true;
				}
				if (IsEnabled)
				{
					if (worldObjectData.Object is Hero)
					{
						return false;
					}
					mainBody.LinearVelocity = Vector2.Zero;
					attachBody = fixture2.Body;
					hooked = true;
					mainBody.IgnoreGravity = false;
					return false;
				}
			}
			return true;
		}
	}
}
