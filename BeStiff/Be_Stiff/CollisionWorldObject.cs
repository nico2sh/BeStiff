using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class CollisionWorldObject : WorldObject, IShadowCaster
	{
		private Fixture fixture;

		private Vertices vertices;

		private float leftPosX;

		private float rightPosX;

		public ShadowHull[] shadowHull { get; set; }

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
		}

		public bool LoadFromOgmo(Rectangle rec)
		{
			base.Name = "block-" + GameElementsControl.Counter;
			float x = ConvertUnits.ToSimUnits(rec.Width);
			float y = ConvertUnits.ToSimUnits(rec.Height);
			vertices = new Vertices();
			vertices.Add(new Vector2(x, 0f));
			vertices.Add(new Vector2(x, y));
			vertices.Add(new Vector2(0f, y));
			vertices.Add(new Vector2(0f, 0f));
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Static;
			mainBody.Position = ConvertUnits.ToSimUnits(rec.X, rec.Y);
			mainBody.Rotation = 0f;
			PolygonShape shape = new PolygonShape(vertices, 10f);
			fixture = mainBody.CreateFixture(shape);
			fixture.Restitution = 0.1f;
			fixture.Friction = 1f;
			fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			GameElementsControl.AddWorldStaticObject(this);
			float num = float.MaxValue;
			float num2 = float.MinValue;
			Vector2[] points = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, rec.Height),
				new Vector2(rec.Width, rec.Height),
				new Vector2(rec.Width, 0f)
			};
			foreach (Vector2 vertex in vertices)
			{
				if (vertex.X < num)
				{
					num = vertex.X;
				}
				if (vertex.X > num2)
				{
					num2 = vertex.X;
				}
			}
			shadowHull = new ShadowHull[1];
			shadowHull[0] = ShadowHull.CreateConvex(ref points);
			shadowHull[0].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			GameElementsControl.Krypton.Hulls.Add(shadowHull[0]);
			leftPosX = mainBody.GetWorldPoint(new Vector2(num, 0f)).X;
			rightPosX = mainBody.GetWorldPoint(new Vector2(num2, 0f)).X;
			return true;
		}

		public override void Load()
		{
		}

		public override bool CanWallSlide(Side side, ref float posX, ref Fixture wsFixture)
		{
			if (side == Side.Left)
			{
				posX = rightPosX;
			}
			else
			{
				posX = leftPosX;
			}
			wsFixture = fixture;
			return true;
		}

		public override bool CanHanged()
		{
			return true;
		}

		public override void StepOver(Vector2 position, WorldObjectType type, bool running)
		{
			switch (type)
			{
			case WorldObjectType.Human:
				GameElementsControl.NoiseManager.AddVisualNoise("stepWalking", position);
				break;
			case WorldObjectType.Hero:
				if (running)
				{
					GameElementsControl.NoiseManager.AddVisualNoise("stepWalkingHero", position);
				}
				else
				{
					GameElementsControl.NoiseManager.AddNoise("step", position);
				}
				break;
			}
		}

		public override void Draw()
		{
		}

		public void DrawHull()
		{
			GameElementsControl.ScreenManager.SpriteBatch.DrawString(GameElementsControl.ScreenManager.Font, base.Name, GameElementsControl.ConvertScreenToWorld(mainBody.Position), Color.Black);
		}
	}
}
