using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA;

namespace Be_Stiff
{
	internal class OneSidedPlatform : WorldObject, IOneSidedWorldObject, IShadowCaster
	{
		private GameSprite bodySprite;

		private GameSprite edgeSprite;

		private Rectangle rectangle;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => true;

		public bool IsProjectile => false;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("OneSidedPlatform"))
			{
				return false;
			}
			base.Name = "OneSidedPlatform-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			GameElementsControl.LoadSprite("oneSidedPlatform", "sprites\\environment\\onesidedplatform");
			GameElementsControl.LoadSprite("oneSidedPlatformEdge", "sprites\\environment\\onesidedplatformedge");
			bodySprite = GameElementsControl.GetSprite("oneSidedPlatform");
			edgeSprite = GameElementsControl.GetSprite("oneSidedPlatformEdge");
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
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override bool CanSeeThrough(bool alert)
		{
			return true;
		}

		public override void Draw()
		{
			bodySprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0.25f, 0.125f)), mainBody.Rotation, rectangle);
			edgeSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0.25f, 0.125f)), mainBody.Rotation);
			edgeSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.GetWorldPoint(TheVertices[0] - new Vector2(0.25f, -0.125f))), mainBody.Rotation, SpriteEffects.FlipHorizontally);
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
