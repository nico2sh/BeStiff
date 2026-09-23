using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff.WorldObjects.Props
{
	internal class LargeSlope : WorldObject, IShadowCaster
	{
		private GameSprite sprite;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			string text = obj.Name;
			if (!text.StartsWith("LargeSlope"))
			{
				return false;
			}
			float rotation = MathHelper.ToRadians(obj.Rotation);
			base.Name = "Slope-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			string text2 = text.Substring(10);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Static;
			mainBody.Position = worldScaledOgmoObject.Position;
			mainBody.Rotation = rotation;
			Vector2[] points = new Vector2[3];
			if (text2 == "Left")
			{
				Vertices vertices = new Vertices();
				vertices.Add(new Vector2(2f, -0.5f));
				vertices.Add(new Vector2(2f, 0.5f));
				vertices.Add(new Vector2(-2f, 0.5f));
				ref Vector2 reference = ref points[0];
				reference = ConvertUnits.ToDisplayUnits(2f, -0.5f);
				ref Vector2 reference2 = ref points[1];
				reference2 = ConvertUnits.ToDisplayUnits(2f, 0.5f);
				ref Vector2 reference3 = ref points[2];
				reference3 = ConvertUnits.ToDisplayUnits(-2f, 0.5f);
				PolygonShape shape = new PolygonShape(vertices, 10f);
				Fixture fixture = mainBody.CreateFixture(shape);
				fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
				fixture.Restitution = 0f;
				fixture.Friction = 1f;
				GameElementsControl.LoadSprite("largeSlopeLeft", "sprites\\environment\\largeslopeleft");
				sprite = GameElementsControl.GetSprite("largeSlopeLeft");
			}
			else
			{
				if (!(text2 == "Right"))
				{
					return false;
				}
				Vertices vertices2 = new Vertices();
				vertices2.Add(new Vector2(-2f, 0.5f));
				vertices2.Add(new Vector2(-2f, -0.5f));
				vertices2.Add(new Vector2(2f, 0.5f));
				ref Vector2 reference4 = ref points[0];
				reference4 = ConvertUnits.ToDisplayUnits(-2f, -0.5f);
				ref Vector2 reference5 = ref points[1];
				reference5 = ConvertUnits.ToDisplayUnits(-2f, 0.5f);
				ref Vector2 reference6 = ref points[2];
				reference6 = ConvertUnits.ToDisplayUnits(2f, 0.5f); // matches the body: base along the bottom
				PolygonShape shape2 = new PolygonShape(vertices2, 10f);
				Fixture fixture2 = mainBody.CreateFixture(shape2);
				fixture2.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
				fixture2.Restitution = 0f;
				fixture2.Friction = 1f;
				GameElementsControl.LoadSprite("largeSlopeRight", "sprites\\environment\\largesloperight");
				sprite = GameElementsControl.GetSprite("largeSlopeRight");
			}
			ShadowHull shadowHull = ShadowHull.CreateConvex(ref points);
			shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			shadowHull.Angle = rotation;
			GameElementsControl.Krypton.Hulls.Add(shadowHull);
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
