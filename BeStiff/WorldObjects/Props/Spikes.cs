using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Spikes : WorldObject, IShadowCaster
	{
		private Rectangle rectangle;

		private GameSprite spikesSprite;

		public override float Mass => 0f;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Spikes"))
			{
				return false;
			}
			base.Name = "Spikes-" + GameElementsControl.Counter;
			string value = obj.GetValue<OgmoStringValue>("Object").Value;
			if (value != "")
			{
				WorldObject worldObjectByName = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName == null)
				{
					return false;
				}
			}
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			MathHelper.ToRadians(worldScaledOgmoObject.Rotation);
			GameElementsControl.LoadSprite("spikes", "sprites\\dangers\\spikes");
			spikesSprite = GameElementsControl.GetSprite("spikes");
			rectangle = new Rectangle(0, 0, obj.Width, obj.Height + 2);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World, worldScaledOgmoObject.Position);
			mainBody.Rotation = MathHelper.ToRadians(worldScaledOgmoObject.Rotation);
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(-0.25f, 0.25f));
			vertices.Add(new Vector2(-0.25f, -0.25f));
			vertices.Add(new Vector2(worldScaledOgmoObject.Width - 0.75f, -0.25f));
			vertices.Add(new Vector2(worldScaledOgmoObject.Width - 0.75f, 0.25f));
			PolygonShape shape = new PolygonShape(vertices, 10f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.IsSensor = true;
			fixture.UserData = new WorldObjectData(WorldObjectType.Deadly, this);
			if (value == "")
			{
				mainBody.BodyType = BodyType.Static;
			}
			else
			{
				WorldObject worldObjectByName2 = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName2 == null)
				{
					return false;
				}
				mainBody.BodyType = BodyType.Dynamic;
				WeldJoint joint = new WeldJoint(mainBody, worldObjectByName2.MainBody, Vector2.Zero, worldObjectByName2.MainBody.GetLocalPoint(mainBody.GetWorldPoint(Vector2.Zero)));
				GameElementsControl.World.AddJoint(joint);
			}
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override void Draw()
		{
			spikesSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), 0f - mainBody.Rotation, rectangle);
		}

		public void DrawHull()
		{
		}
	}
}
