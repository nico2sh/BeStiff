using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Be_Stiff.WorldObjects.Props
{
	public class GlassPiece : WorldObject
	{
		private Body bodyPiece;

		private GameSprite spritePiece;

		private GameSpriteVariables spritePieceVariables;

		private PolygonShape shapePiece;

		private double breakTime;

		private int touchCount;

		public Body Body => bodyPiece;

		public GlassPiece(PolygonShape shape)
		{
			shapePiece = shape;
			touchCount = 0;
		}

		public override void Load()
		{
			GameElementsControl.LoadSprite("glassPiece", "sprites\\objects\\glasspiece");
			spritePiece = GameElementsControl.GetSprite("glassPiece");
			spritePieceVariables = GameSprite.GetDefaultVariables();
			bodyPiece = BodyFactory.CreateBody(GameElementsControl.World);
			bodyPiece.BodyType = BodyType.Dynamic;
			Fixture fixture = bodyPiece.CreateFixture(shapePiece);
			fixture.CollisionCategories |= Category.Cat6;
			fixture.CollidesWith &= ~Category.Cat5;
			fixture.OnCollision = (OnCollisionEventHandler)Delegate.Combine(fixture.OnCollision, new OnCollisionEventHandler(OnCollision));
			bodyPiece.Enabled = false;
			base.Enabled = false;
		}

		private bool OnCollision(Fixture f1, Fixture f2, Contact contact)
		{
			touchCount++;
			return true;
		}

		public void Activate(Vector2 position, float rotation)
		{
			bodyPiece.Position = position;
			bodyPiece.Rotation = rotation;
			bodyPiece.Enabled = true;
			breakTime = GameElementsControl.CurrentTimeInMS;
			base.Enabled = true;
		}

		public override void Update()
		{
			spritePieceVariables.rotation = 0f - bodyPiece.Rotation;
			if (touchCount >= 3)
			{
				bodyPiece.FixtureList[0].IsSensor = true;
			}
		}

		public void Deactivate()
		{
			bodyPiece.Enabled = false;
			GameElementsControl.World.RemoveBody(bodyPiece);
			base.Enabled = false;
		}

		public override void Draw()
		{
			PolygonShape polygonShape = bodyPiece.FixtureList[0].Shape as PolygonShape;
			spritePiece.Draw(GameElementsControl.ConvertWorldToScreen(bodyPiece.GetWorldPoint(polygonShape.MassData.Centroid)), spritePieceVariables, breakTime + 1500.0, 500.0);
		}
	}
}
