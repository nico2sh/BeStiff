using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Slope : WorldObject, IShadowCaster
	{
		private GameSprite sprite;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			string text = obj.Name;
			if (!text.StartsWith("Slope"))
			{
				return false;
			}
			float rotation = 0f - MathHelper.ToRadians(obj.Rotation);
			base.Name = "Slope-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			string text2 = text.Substring(5);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Static;
			mainBody.Position = worldScaledOgmoObject.Position;
			mainBody.Rotation = rotation;
			if (text2 == "Left")
			{
				Vertices vertices = new Vertices();
				vertices.Add(new Vector2(1f, -0.5f));
				vertices.Add(new Vector2(1f, 0.5f));
				vertices.Add(new Vector2(-1f, 0.5f));
				PolygonShape shape = new PolygonShape(vertices, 10f);
				Fixture fixture = mainBody.CreateFixture(shape);
				fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
				fixture.Restitution = 0f;
				fixture.Friction = 1f;
				GameElementsControl.LoadSprite("slopeLeft", "sprites\\environment\\slopeleft");
				sprite = GameElementsControl.GetSprite("slopeLeft");
			}
			else
			{
				if (!(text2 == "Right"))
				{
					return false;
				}
				Vertices vertices2 = new Vertices();
				vertices2.Add(new Vector2(-1f, 0.5f));
				vertices2.Add(new Vector2(-1f, -0.5f));
				vertices2.Add(new Vector2(1f, 0.5f));
				PolygonShape shape2 = new PolygonShape(vertices2, 10f);
				Fixture fixture2 = mainBody.CreateFixture(shape2);
				fixture2.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
				fixture2.Restitution = 0f;
				fixture2.Friction = 1f;
				GameElementsControl.LoadSprite("slopeRight", "sprites\\environment\\sloperight");
				sprite = GameElementsControl.GetSprite("slopeRight");
			}
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override void Draw()
		{
		}

		public void DrawHull()
		{
			sprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation);
		}
	}
}
