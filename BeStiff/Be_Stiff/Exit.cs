using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff
{
	public class Exit : Goal, IShadowCaster
	{
		private GameSprite exitSprite;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Exit"))
			{
				return false;
			}
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			base.Name = "Exit";
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			mainBody.BodyType = BodyType.Static;
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(-0.75f, 1.25f));
			vertices.Add(new Vector2(-0.75f, -1.25f));
			vertices.Add(new Vector2(0.75f, -1.25f));
			vertices.Add(new Vector2(0.75f, 1.25f));
			PolygonShape shape = new PolygonShape(vertices, 300f);
			sensorFixture = mainBody.CreateFixture(shape);
			sensorFixture.UserData = new WorldObjectData(WorldObjectType.InteractiveSensor, this);
			mainBody.IsSensor = true;
			GameElementsControl.LoadSprite("exitDoor", "sprites\\goals\\exitdoor");
			exitSprite = GameElementsControl.GetSprite("exitDoor");
			GameElementsControl.AddShadowCasterWorldObject(this);
			Active = false;
			timeForAchieving = 0.0;
			return true;
		}

		protected override void Achieve()
		{
			GameElementsControl.Score.AddPoints(Globals.ScoreEndLevel);
			base.Achieve();
		}

		public override void Draw()
		{
			exitSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), 0f - mainBody.Rotation);
		}

		public void DrawHull()
		{
		}
	}
}
