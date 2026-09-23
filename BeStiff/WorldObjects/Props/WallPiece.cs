using System;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;

namespace Be_Stiff.WorldObjects.Props
{
	public class WallPiece : WorldObject, IShadowCaster
	{
		private GameSprite spritePiece;

		private GameSpriteVariables spritePieceVariables;

		private PolygonShape shapePiece;

		private double breakTime;

		private int touchCount;

		public ShadowHull[] shadowHull { get; set; }

		public WallPiece(PolygonShape shape)
		{
			shapePiece = shape;
			touchCount = 0;
			shadowHull = new ShadowHull[1];
			Vector2[] points = shape.Vertices.ToArray();
			for (int i = 0; i < points.Count(); i++)
			{
				ref Vector2 reference = ref points[i];
				reference = GameElementsControl.ConvertWorldToScreen(points[i]);
			}
			shadowHull[0] = ShadowHull.CreateConvex(ref points);
		}

		public override void Load()
		{
			GameElementsControl.LoadSprite("wallPiece", "sprites\\objects\\wallPiece");
			spritePiece = GameElementsControl.GetSprite("wallPiece");
			spritePieceVariables = GameSprite.GetDefaultVariables();
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			Fixture fixture = mainBody.CreateFixture(shapePiece);
			fixture.CollisionCategories |= Category.Cat6;
			fixture.CollidesWith &= ~Category.Cat5;
			fixture.OnCollision = (OnCollisionEventHandler)Delegate.Combine(fixture.OnCollision, new OnCollisionEventHandler(OnCollision));
			mainBody.Enabled = false;
			base.Enabled = false;
		}

		private bool OnCollision(Fixture f1, Fixture f2, Contact contact)
		{
			touchCount++;
			return true;
		}

		public void Activate(Vector2 position, float rotation)
		{
			mainBody.Position = position;
			mainBody.Rotation = rotation;
			mainBody.Enabled = true;
			breakTime = GameElementsControl.CurrentTimeInMS;
			GameElementsControl.Krypton.Hulls.Add(shadowHull[0]);
			base.Enabled = true;
		}

		public override void Update()
		{
			spritePieceVariables.rotation = 0f - mainBody.Rotation;
			if (touchCount >= 3)
			{
				mainBody.FixtureList[0].IsSensor = true;
			}
			shadowHull[0].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			shadowHull[0].Angle = 0f - mainBody.Rotation;
		}

		public void Deactivate()
		{
			mainBody.Enabled = false;
			GameElementsControl.World.RemoveBody(mainBody);
			GameElementsControl.Krypton.Hulls.Remove(shadowHull[0]);
			base.Enabled = false;
		}

		public void DrawHull()
		{
			PolygonShape polygonShape = mainBody.FixtureList[0].Shape as PolygonShape;
			spritePiece.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.GetWorldPoint(polygonShape.MassData.Centroid)), spritePieceVariables, breakTime + 1500.0, 500.0);
		}

		public override void Draw()
		{
		}
	}
}
