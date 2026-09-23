using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff.WorldObjects.Props
{
	internal class SlidingDoor : InteractiveWorldObject, IShadowCaster
	{
		private GameSprite doorSprite;

		private GameSprite pivotSprite;

		private ShadowHull shadowHull;

		private Vector2 closedPosition;

		private Vector2 openPosition;

		private Vector2 pivotPosition;

		private float pivotRotation;

		private float openSpeed;

		private bool moving;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("SlidingDoor"))
			{
				return false;
			}
			GameElementsControl.LoadSprite("slidingDoor", "sprites\\objects\\slidingdoor");
			GameElementsControl.LoadSprite("doorPivot", "sprites\\objects\\doorpivot");
			doorSprite = GameElementsControl.GetSprite("slidingDoor");
			pivotSprite = GameElementsControl.GetSprite("doorPivot");
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			base.Name = "SlidingDoor-" + GameElementsControl.Counter;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			closedPosition = worldScaledOgmoObject.Position;
			mainBody.Position = closedPosition;
			openPosition = closedPosition + new Vector2(0f, 3f);
			pivotPosition = closedPosition + new Vector2(0f, 1.5f);
			pivotRotation = 0f;
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(-0.25f, 1.5f));
			vertices.Add(new Vector2(-0.25f, -1.5f));
			vertices.Add(new Vector2(0.25f, -1.5f));
			vertices.Add(new Vector2(0.25f, 1.5f));
			PolygonShape shape = new PolygonShape(vertices, 300f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			shadowHull = ShadowHull.CreateRectangle(GameElementsControl.ConvertWorldToScreen(new Vector2(0.5f, 3f)));
			shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			mainBody.BodyType = BodyType.Kinematic;
			Body body = BodyFactory.CreateRectangle(GameElementsControl.World, 3.5f, 3f, 1f);
			body.IsSensor = true;
			body.BodyType = BodyType.Static;
			body.Position = mainBody.Position;
			sensorFixture = body.FixtureList[0];
			sensorFixture.UserData = new WorldObjectData(WorldObjectType.InteractiveSensor, this);
			openSpeed = 20f;
			moving = false;
			GameElementsControl.ScreenManager.AudioManager.LoadSound("slidingDoor", "audio\\noises\\slidingDoor");
			GameElementsControl.Krypton.Hulls.Add(shadowHull);
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override void Update()
		{
			shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			if (interacting)
			{
				Open();
			}
			else
			{
				Close();
			}
		}

		private void Open()
		{
			if (mainBody.Position != openPosition)
			{
				if (!moving)
				{
					moving = true;
					GameElementsControl.NoiseManager.AddNoise("slidingDoor", mainBody.Position);
				}
				float num = (mainBody.Position - openPosition).Length();
				float num2 = (float)((double)(num * 1000f) / GameElementsControl.LastFrameTimeInMS);
				Vector2 vector = VectorUtil.SafeNormalize(openPosition - mainBody.Position);
				float num3 = ((!(num2 < openSpeed)) ? openSpeed : num2);
				pivotRotation += MathHelper.ToRadians(num3);
				mainBody.LinearVelocity = vector * num3;
			}
			else
			{
				mainBody.LinearVelocity = Vector2.Zero;
				moving = false;
			}
		}

		private void Close()
		{
			if (mainBody.Position != closedPosition)
			{
				if (!moving)
				{
					moving = true;
					GameElementsControl.NoiseManager.AddNoise("slidingDoor", mainBody.Position);
				}
				float num = (mainBody.Position - closedPosition).Length();
				float num2 = (float)((double)(num * 1000f) / GameElementsControl.LastFrameTimeInMS);
				Vector2 vector = VectorUtil.SafeNormalize(closedPosition - mainBody.Position);
				float num3 = ((!(num2 < openSpeed)) ? openSpeed : num2);
				pivotRotation -= MathHelper.ToRadians(num3);
				mainBody.LinearVelocity = vector * num3;
			}
			else
			{
				mainBody.LinearVelocity = Vector2.Zero;
				moving = false;
			}
		}

		public override bool Interacts(WorldObjectType type)
		{
			if (type != WorldObjectType.Hero)
			{
				return type == WorldObjectType.Human;
			}
			return true;
		}

		public override void Draw()
		{
		}

		public void DrawHull()
		{
			doorSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
			pivotSprite.Draw(GameElementsControl.ConvertWorldToScreen(pivotPosition), pivotRotation);
		}
	}
}
