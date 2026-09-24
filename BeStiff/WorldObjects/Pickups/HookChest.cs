using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff.WorldObjects.Pickups
{
	internal class HookChest : InteractiveWorldObject, IOneSidedWorldObject
	{
		private GameSprite gunChestSprite;

		private GameSprite gunChestEmptySprite;

		private Hook containingHook;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => true;

		public bool IsProjectile => false;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("HookChest"))
			{
				return false;
			}
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			base.Name = "HookChest-" + GameElementsControl.Counter;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			TheVertices = new Vertices();
			TheVertices.Add(new Vector2(-1f, 0.5f));
			TheVertices.Add(new Vector2(-1f, -0.5f));
			TheVertices.Add(new Vector2(1f, -0.5f));
			TheVertices.Add(new Vector2(1f, 0.5f));
			PolygonShape shape = new PolygonShape(TheVertices, 300f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.CollisionCategories = Category.Cat8;
			fixture.CollidesWith &= ~Category.Cat7;
			fixture.UserData = new WorldObjectData(WorldObjectType.Goal, this);
			sensorFixture = FixtureFactory.AttachRectangle(1.6f, 0.6f, 1f, Vector2.Zero, mainBody, new WorldObjectData(WorldObjectType.InteractiveSensor, this));
			sensorFixture.IsSensor = true;
			sensorFixture.CollisionCategories = Category.Cat8;
			sensorFixture.CollidesWith &= ~Category.Cat7;
			GameElementsControl.ScreenManager.AudioManager.LoadSound("reload", "audio\\noises\\reload");
			GameElementsControl.LoadSprite("hookChest", "sprites\\objects\\hookchest");
			GameElementsControl.LoadSprite("gunChestEmpty", "sprites\\objects\\gunchestempty");
			gunChestSprite = GameElementsControl.GetSprite("hookChest");
			gunChestEmptySprite = GameElementsControl.GetSprite("gunChestEmpty");
			containingHook = GameElementsControl.Hero.CreateHeroHook();
			Active = true;
			GameElementsControl.AddWorldObject(this);
			return true;
		}

		public override bool Interacts(WorldObjectType type)
		{
			return type == WorldObjectType.Hero;
		}

		public override void StartInteracting(WorldObject wo)
		{
			if (Active && GameElementsControl.Hero.PickupHook(containingHook))
			{
				Active = false;
				GameElementsControl.NoiseManager.AddNoise("reload", mainBody.Position);
				containingHook = null;
			}
		}

		public override void StopInteracting(WorldObject worldObject)
		{
		}

		public bool ConditionalContacts(WorldObject wo)
		{
			return true;
		}

		public override void Draw()
		{
			if (Active)
			{
				gunChestSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation);
			}
			else
			{
				gunChestEmptySprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation);
			}
		}
	}
}
