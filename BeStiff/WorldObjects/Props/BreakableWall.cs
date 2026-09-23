using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.WorldObjects.Props
{
	public class BreakableWall : WorldObject, IShadowCaster
	{
		private const float minForceForBreak = 100f;

		private const float heightPiece = 0.25f;

		private const float widthPiece = 0.5f;

		private Fixture wallFixture;

		private List<WallPiece> piecesList;

		private FixedRevoluteJoint anchorJoint1;

		private FixedRevoluteJoint anchorJoint2;

		private Vector2 velocitiesCache = default(Vector2);

		private float angularVelocitiesCache;

		private float strength = 1000f;

		private bool broken;

		private bool breakIt;

		private double breakTime;

		private Vector2 breakPoint;

		private Vector2 breakNormal;

		private Vector2 startPoint;

		private Vector2 endPoint;

		private bool worldFixed;

		private GameSprite wallSprite;

		private GameSpriteVariables wallSpriteVariables;

		private ShadowHull shadowHull;

		public bool Broken => broken;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("BWall"))
			{
				return false;
			}
			base.Name = obj.GetValue<OgmoStringValue>("ID").Value;
			worldFixed = obj.GetValue<OgmoBooleanValue>("fixed").Value;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			float num = MathHelper.ToRadians(worldScaledOgmoObject.Rotation);
			// y-down world: the wall runs from its origin's top edge downward.
			startPoint = worldScaledOgmoObject.Position + Vector2.Transform(new Vector2(0f, -0.25f), Matrix.CreateRotationZ(num));
			endPoint = Vector2.Transform(new Vector2(0f, worldScaledOgmoObject.Height), Matrix.CreateRotationZ(num)) + startPoint;
			GameElementsControl.LoadSprite("breakableWall", "sprites\\objects\\breakablewall");
			wallSprite = GameElementsControl.GetSprite("breakableWall");
			Vector2 zero = Vector2.Zero;
			zero.Y = GameElementsControl.ConvertWorldToScreen(endPoint - startPoint).Length();
			zero.X = 1f;
			wallSpriteVariables = GameSprite.GetDefaultVariables();
			wallSpriteVariables.offset = new Vector2(0f, (0f - zero.Y) / 2f); // sprite origin is its top edge (1 px tall texture)
			wallSpriteVariables.scale = zero;
			wallSpriteVariables.rotation = num;
			broken = false;
			breakTime = 0.0;
			float num2 = (startPoint - endPoint).Length();
			int num3 = (int)(num2 / 0.25f);
			float num4 = num2 - (float)num3 * 0.25f;
			if (num4 == 0f)
			{
				num3--;
				num4 = 0.25f;
			}
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			List<Vertices> list = CreateDiamondPieces(num3, num4);
			Vertices vertices = PolygonTools.CreateRectangle(0.25f, num2 / 2f);
			PolygonShape shape = new PolygonShape(vertices, 10f);
			wallFixture = mainBody.CreateFixture(shape);
			wallFixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			mainBody.Position = (startPoint + endPoint) / 2f;
			mainBody.Rotation = num;
			shadowHull = ShadowHull.CreateRectangle(ConvertUnits.ToDisplayUnits(0.5f, num2));
			shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			shadowHull.Angle = mainBody.Rotation;
			GameElementsControl.Krypton.Hulls.Add(shadowHull);
			piecesList = new List<WallPiece>();
			foreach (Vertices item in list)
			{
				PolygonShape shape2 = new PolygonShape(item, 10f);
				WallPiece wallPiece = new WallPiece(shape2);
				wallPiece.Load();
				piecesList.Add(wallPiece);
				GameSprite.GetDefaultVariables();
			}
			if (worldFixed)
			{
				anchorJoint1 = new FixedRevoluteJoint(mainBody, mainBody.GetLocalPoint(startPoint), startPoint);
				anchorJoint2 = new FixedRevoluteJoint(mainBody, mainBody.GetLocalPoint(endPoint), endPoint);
				GameElementsControl.World.AddJoint(anchorJoint1);
				GameElementsControl.World.AddJoint(anchorJoint2);
			}
			GameElementsControl.ScreenManager.AudioManager.LoadSound("wallBreak", "audio\\noises\\wallBreak");
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		private List<Vertices> CreateRectanglePieces(int numPieces, float heightLastPiece)
		{
			List<Vertices> list = new List<Vertices>();
			float num = (0f - (startPoint - endPoint).Length()) / 2f;
			for (int i = 0; i < numPieces; i++)
			{
				Vertices vertices = new Vertices();
				vertices.Add(new Vector2(0.25f, num));
				vertices.Add(new Vector2(0.25f, num + 0.25f));
				vertices.Add(new Vector2(-0.25f, num + 0.25f));
				vertices.Add(new Vector2(-0.25f, num));
				list.Add(vertices);
				num += 0.25f;
			}
			Vertices vertices2 = new Vertices();
			vertices2.Add(new Vector2(0.25f, num));
			vertices2.Add(new Vector2(0.25f, num + heightLastPiece));
			vertices2.Add(new Vector2(-0.25f, num + heightLastPiece));
			vertices2.Add(new Vector2(-0.25f, num));
			list.Add(vertices2);
			return list;
		}

		private List<Vertices> CreateDiamondPieces(int numPieces, float heightLastPiece)
		{
			List<Vertices> list = new List<Vertices>();
			float num = (0f - (startPoint - endPoint).Length()) / 2f;
			for (int i = 0; i < numPieces; i++)
			{
				Vertices vertices = new Vertices();
				vertices.Add(new Vector2(0f, num));
				vertices.Add(new Vector2(0.25f, num + 0.125f));
				vertices.Add(new Vector2(0f, num + 0.25f));
				vertices.Add(new Vector2(-0.25f, num + 0.125f));
				list.Add(vertices);
				num += 0.25f;
			}
			Vertices vertices2 = new Vertices();
			vertices2.Add(new Vector2(0f, num));
			vertices2.Add(new Vector2(0.25f, num + heightLastPiece / 2f));
			vertices2.Add(new Vector2(0f, num + heightLastPiece));
			vertices2.Add(new Vector2(-0.25f, num + heightLastPiece / 2f));
			list.Add(vertices2);
			return list;
		}

		private List<Vertices> CreateTrianglePieces(int numPieces, float heightLastPiece)
		{
			List<Vertices> list = new List<Vertices>();
			float num = (0f - (startPoint - endPoint).Length()) / 2f;
			for (int i = 0; i < numPieces; i++)
			{
				Vertices vertices = new Vertices();
				vertices.Add(new Vector2(0.25f, num));
				vertices.Add(new Vector2(-0.25f, num + 0.25f));
				vertices.Add(new Vector2(-0.25f, num));
				list.Add(vertices);
				Vertices vertices2 = new Vertices();
				vertices2.Add(new Vector2(0.25f, num));
				vertices2.Add(new Vector2(0.25f, num + 0.25f));
				vertices2.Add(new Vector2(-0.25f, num + 0.25f));
				list.Add(vertices2);
				num += 0.25f;
			}
			Vertices vertices3 = new Vertices();
			vertices3.Add(new Vector2(0.25f, num));
			vertices3.Add(new Vector2(-0.25f, num + heightLastPiece));
			vertices3.Add(new Vector2(-0.25f, num));
			list.Add(vertices3);
			Vertices vertices4 = new Vertices();
			vertices4.Add(new Vector2(0.25f, num));
			vertices4.Add(new Vector2(0.25f, num + heightLastPiece));
			vertices4.Add(new Vector2(-0.25f, num + heightLastPiece));
			list.Add(vertices4);
			return list;
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			if (!broken)
			{
				float num = CalculateRelativeSpeedFromContact(ref contact, ref oldManifold);
				if (wod != null)
				{
					num *= wod.Object.Mass;
				}
				if (Math.Abs(num) > 100f)
				{
					strength -= num;
				}
			}
		}

		private bool OnWallCollisionOld(Fixture f1, Fixture f2, Contact manifold)
		{
			if (!broken)
			{
				Vector2 linearVelocity = f2.Body.LinearVelocity;
				if (linearVelocity.Length() != 0f)
				{
					Vector2 value = startPoint - endPoint;
					float num = Vector2.Dot(value, linearVelocity) / linearVelocity.Length();
					float num2 = (float)Math.Sqrt(Math.Pow(linearVelocity.Length(), 2.0) + Math.Pow(num, 2.0));
					float num3 = 0f;
					num3 = ((!(f2.UserData is WorldObjectData worldObjectData)) ? f2.Body.Mass : worldObjectData.Object.Mass);
					float num4 = num2 * num3;
					if (num4 > 100f)
					{
						strength -= num4;
					}
				}
			}
			return true;
		}

		private void GlassPostSolve(ContactConstraint contactConstraint)
		{
			if (!Broken)
			{
				Vector2 zero = Vector2.Zero;
				float num = 0f;
				for (int i = 0; i < contactConstraint.Manifold.PointCount; i++)
				{
					num = Math.Max(num, contactConstraint.Manifold.Points[i].NormalImpulse);
					zero += contactConstraint.Manifold.Points[i].LocalPoint;
				}
				if (num > strength)
				{
					breakPoint = zero / contactConstraint.Manifold.PointCount;
					breakNormal = -contactConstraint.Manifold.LocalNormal;
					breakIt = true;
				}
			}
		}

		private void SetWallPieceCollision(Fixture fixture)
		{
		}

		private void BreakWall()
		{
			Vector2 worldCenter = mainBody.WorldCenter;
			Vector2 worldPoint = mainBody.GetWorldPoint(breakPoint);
			GameElementsControl.NoiseManager.AddNoise("wallBreak", worldPoint);
			GameElementsControl.Krypton.Hulls.Remove(shadowHull);
			float num = (startPoint - endPoint).Length() / 2f;
			for (int i = 0; i < piecesList.Count; i++)
			{
				WallPiece wallPiece = piecesList[i];
				wallPiece.Activate(mainBody.Position, mainBody.Rotation);
				float num2 = 1f * (float)GameElementsControl.Random.NextDouble() * ((num - (wallPiece.MainBody.WorldCenter - worldPoint).Length()) / num);
				Vector2 worldCenter2 = wallPiece.MainBody.WorldCenter;
				Vector2 vector = velocitiesCache + MathUtils.Cross(angularVelocitiesCache, worldCenter2 - worldCenter);
				wallPiece.MainBody.AngularVelocity = angularVelocitiesCache + 5f * (float)(GameElementsControl.Random.NextDouble() - 0.5);
				wallPiece.MainBody.LinearVelocity = vector + breakNormal * num2;
			}
			if (worldFixed)
			{
				GameElementsControl.World.RemoveJoint(anchorJoint1);
				GameElementsControl.World.RemoveJoint(anchorJoint2);
			}
			GameElementsControl.World.RemoveBody(mainBody);
			breakTime = GameElementsControl.CurrentTimeInMS;
			broken = true;
		}

		private void BreakWall(Vector2 worldPoint, Vector2 direction)
		{
			Vector2 vector = Vector2.Normalize(direction) * 2f;
			GameElementsControl.NoiseManager.AddNoise("wallBreak", worldPoint);
			GameElementsControl.Krypton.Hulls.Remove(shadowHull);
			float num = (startPoint - endPoint).Length() / 2f;
			_ = mainBody.WorldCenter;
			_ = (startPoint - endPoint).Length() / 2f;
			for (int i = 0; i < piecesList.Count; i++)
			{
				WallPiece wallPiece = piecesList[i];
				wallPiece.Activate(mainBody.Position, mainBody.Rotation);
				float num2 = 4f * (float)GameElementsControl.Random.NextDouble() * ((num - (wallPiece.MainBody.WorldCenter - worldPoint).Length()) / num);
				wallPiece.MainBody.AngularVelocity = angularVelocitiesCache + 5f * (float)(GameElementsControl.Random.NextDouble() - 0.5);
				wallPiece.MainBody.LinearVelocity = vector * num2;
			}
			GameElementsControl.World.RemoveBody(mainBody);
			breakTime = GameElementsControl.CurrentTimeInMS;
			broken = true;
		}

		private void Deactivate()
		{
			if (!Broken)
			{
				GameElementsControl.World.RemoveBody(mainBody);
			}
			foreach (WallPiece pieces in piecesList)
			{
				pieces.Deactivate();
			}
			base.Enabled = false;
		}

		public override void Update()
		{
			if (!base.Enabled)
			{
				return;
			}
			if (!broken)
			{
				if (strength <= 0f)
				{
					breakIt = true;
				}
				velocitiesCache = mainBody.LinearVelocity;
				angularVelocitiesCache = mainBody.AngularVelocity;
				if (breakIt)
				{
					BreakWall();
				}
				return;
			}
			foreach (WallPiece pieces in piecesList)
			{
				pieces.Update();
			}
			if (breakTime + 2000.0 < GameElementsControl.CurrentTimeInMS)
			{
				Deactivate();
			}
		}

		public override void Hit(Vector2 sidePower, Vector2 position, HitType hitType)
		{
			if (!broken)
			{
				switch (hitType)
				{
				case HitType.Kick:
					strength -= 400f;
					break;
				case HitType.Pistol:
					strength -= 250f;
					break;
				case HitType.Explosion:
					strength -= sidePower.Length() * 2.5f;
					break;
				}
				if (strength <= 0f)
				{
					sidePower = Vector2.Normalize(sidePower);
					BreakWall(position, sidePower);
				}
			}
		}

		public override bool CanBeSensed()
		{
			return !broken;
		}

		public override bool CanBeHooked()
		{
			return false;
		}

		public override bool CanStopBullets()
		{
			return true;
		}

		public override void Draw()
		{
		}

		public void DrawHull()
		{
			if (!broken)
			{
				wallSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), wallSpriteVariables);
			}
			else
			{
				if (!base.Enabled)
				{
					return;
				}
				foreach (WallPiece pieces in piecesList)
				{
					pieces.DrawHull();
				}
			}
		}
	}
}
