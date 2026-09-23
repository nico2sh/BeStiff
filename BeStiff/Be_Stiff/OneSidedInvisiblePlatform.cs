using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff
{
	internal class OneSidedInvisiblePlatform : WorldObject, IOneSidedWorldObject
	{
		private Rectangle rectangle;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => true;

		public bool IsProjectile => false;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("OneSidedInvisiblePlatform"))
			{
				return false;
			}
			base.Name = "OneSidedPlatform-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			rectangle = new Rectangle(0, 0, obj.Width, obj.Height);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World, worldScaledOgmoObject.Position);
			mainBody.Rotation = MathHelper.ToRadians(worldScaledOgmoObject.Rotation);
			mainBody.BodyType = BodyType.Static;
			TheVertices = new Vertices();
			TheVertices.Add(new Vector2(0f, 0f));
			TheVertices.Add(new Vector2(0f, 0.25f));
			TheVertices.Add(new Vector2(worldScaledOgmoObject.Width, 0.25f));
			TheVertices.Add(new Vector2(worldScaledOgmoObject.Width, 0f));
			TheVertices.Reverse(); // y flipped for the y-down world; keep counter-clockwise winding
			PolygonShape shape = new PolygonShape(TheVertices, 10f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.CollisionCategories = Category.Cat8;
			fixture.CollidesWith &= ~Category.Cat7;
			fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			GameElementsControl.AddWorldObject(this);
			return true;
		}

		public override bool CanSeeThrough(bool alert)
		{
			return true;
		}

		public override void Draw()
		{
		}

		public bool ConditionalContacts(WorldObject wo)
		{
			return true;
		}

		public void DrawHull()
		{
		}
	}
}
