using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Glass : WorldObject
	{
		private const float strength = 30f;

		private const float heightPiece = 0.2f;

		private const float widthPiece = 0.1f;

		private FixedRevoluteJoint anchorJoint1;

		private FixedRevoluteJoint anchorJoint2;

		private Fixture glassFixture;

		private List<GlassPiece> piecesList;

		private Vector2 velocitiesCache = default(Vector2);

		private float angularVelocitiesCache;

		private bool broken;

		private bool breakIt;

		private double breakTime;

		private Vector2 breakPoint = Vector2.Zero;

		private Vector2 breakDirection = Vector2.Zero;

		private Vector2 startPoint;

		private Vector2 endPoint;

		private bool worldFixed;

		private GameSprite glassSprite;

		private GameSpriteVariables glassSpriteVariables;

		public Vector2 Position => mainBody.Position;

		public bool Broken => broken;

		public override float Mass => mainBody.Mass;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Glass"))
			{
				return false;
			}
			base.Name = obj.GetValue<OgmoStringValue>("ID").Value;
			worldFixed = obj.GetValue<OgmoBooleanValue>("fixed").Value;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			float num = MathHelper.ToRadians(worldScaledOgmoObject.Rotation);
			// y-down world: the pane runs from just below its origin downward.
			startPoint = worldScaledOgmoObject.Position + Vector2.Transform(ConvertUnits.ToSimUnits(0f, -4f), Matrix.CreateRotationZ(num));
			endPoint = Vector2.Transform(new Vector2(0f, worldScaledOgmoObject.Height), Matrix.CreateRotationZ(num)) + startPoint;
			GameElementsControl.LoadSprite("glass", "sprites\\objects\\glass");
			glassSprite = GameElementsControl.GetSprite("glass");
			Vector2 zero = Vector2.Zero;
			zero.Y = GameElementsControl.ConvertWorldToScreen(endPoint - startPoint).Length();
			zero.X = 1f;
			glassSpriteVariables = GameSprite.GetDefaultVariables();
			glassSpriteVariables.offset = new Vector2(0f, (0f - zero.Y) / 2f); // sprite origin is its top edge (1 px tall texture)
			glassSpriteVariables.scale = zero;
			glassSpriteVariables.rotation = num;
			Vector2 vector = Vector2.Normalize(startPoint - endPoint) * 0.02f;
			Vector2 vector2 = Vector2.Normalize(endPoint - startPoint) * 0.02f;
			startPoint -= vector;
			endPoint -= vector2;
			broken = false;
			breakTime = 0.0;
			float num2 = (startPoint - endPoint).Length();
			int num3 = (int)(num2 / 0.2f);
			float num4 = num2 - (float)num3 * 0.2f;
			if (num4 == 0f)
			{
				num3--;
				num4 = 0.2f;
			}
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			List<Vertices> list = CreateDiamondPieces(num3, num4);
			piecesList = new List<GlassPiece>();
			foreach (Vertices item in list)
			{
				PolygonShape shape = new PolygonShape(item, 10f);
				GlassPiece glassPiece = new GlassPiece(shape);
				glassPiece.Load();
				piecesList.Add(glassPiece);
				GameSprite.GetDefaultVariables();
			}
			Vertices vertices = PolygonTools.CreateRectangle(0.05f, num2 / 2f);
			PolygonShape shape2 = new PolygonShape(vertices, 10f);
			glassFixture = mainBody.CreateFixture(shape2);
			glassFixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			mainBody.Position = (startPoint + endPoint) / 2f;
			mainBody.Rotation = num;
			if (worldFixed)
			{
				anchorJoint1 = new FixedRevoluteJoint(mainBody, mainBody.GetLocalPoint(startPoint), startPoint);
				anchorJoint2 = new FixedRevoluteJoint(mainBody, mainBody.GetLocalPoint(endPoint), endPoint);
				GameElementsControl.World.AddJoint(anchorJoint1);
				GameElementsControl.World.AddJoint(anchorJoint2);
			}
			GameElementsControl.NoiseManager.LoadNoise("noiseCrash", "crash", "glassBreak", 500.0, 10f);
			GameElementsControl.AddWorldObject(this);
			return true;
		}

		private List<Vertices> CreateRectanglePieces(int numPieces, float heightLastPiece)
		{
			List<Vertices> list = new List<Vertices>();
			float num = (0f - (startPoint - endPoint).Length()) / 2f;
			for (int i = 0; i < numPieces; i++)
			{
				Vertices vertices = new Vertices();
				vertices.Add(new Vector2(0.05f, num));
				vertices.Add(new Vector2(0.05f, num + 0.2f));
				vertices.Add(new Vector2(-0.05f, num + 0.2f));
				vertices.Add(new Vector2(-0.05f, num));
				list.Add(vertices);
				num += 0.2f;
			}
			Vertices vertices2 = new Vertices();
			vertices2.Add(new Vector2(0.05f, num));
			vertices2.Add(new Vector2(0.05f, num + heightLastPiece));
			vertices2.Add(new Vector2(-0.05f, num + heightLastPiece));
			vertices2.Add(new Vector2(-0.05f, num));
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
				vertices.Add(new Vector2(0.05f, num + 0.1f));
				vertices.Add(new Vector2(0f, num + 0.2f));
				vertices.Add(new Vector2(-0.05f, num + 0.1f));
				list.Add(vertices);
				num += 0.2f;
			}
			Vertices vertices2 = new Vertices();
			vertices2.Add(new Vector2(0f, num));
			vertices2.Add(new Vector2(0.05f, num + heightLastPiece / 2f));
			vertices2.Add(new Vector2(0f, num + heightLastPiece));
			vertices2.Add(new Vector2(-0.05f, num + heightLastPiece / 2f));
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
				vertices.Add(new Vector2(0.05f, num));
				vertices.Add(new Vector2(-0.05f, num + 0.2f));
				vertices.Add(new Vector2(-0.05f, num));
				list.Add(vertices);
				Vertices vertices2 = new Vertices();
				vertices2.Add(new Vector2(0.05f, num));
				vertices2.Add(new Vector2(0.05f, num + 0.2f));
				vertices2.Add(new Vector2(-0.05f, num + 0.2f));
				list.Add(vertices2);
				num += 0.2f;
			}
			Vertices vertices3 = new Vertices();
			vertices3.Add(new Vector2(0.05f, num));
			vertices3.Add(new Vector2(-0.05f, num + heightLastPiece));
			vertices3.Add(new Vector2(-0.05f, num));
			list.Add(vertices3);
			Vertices vertices4 = new Vertices();
			vertices4.Add(new Vector2(0.05f, num));
			vertices4.Add(new Vector2(0.05f, num + heightLastPiece));
			vertices4.Add(new Vector2(-0.05f, num + heightLastPiece));
			list.Add(vertices4);
			return list;
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			if (!breakIt)
			{
				float num = CalculateRelativeSpeedFromContact(ref contact, ref oldManifold, out breakPoint, out breakDirection);
				if (wod != null)
				{
					num *= wod.Object.Mass;
				}
				if (num > 30f)
				{
					breakIt = true;
					contact.Enabled = false;
				}
			}
		}

		private void BreakGlass(Vector2 worldPoint, Vector2 direction)
		{
			Vector2 vector = Vector2.Normalize(direction) * 2f;
			GameElementsControl.NoiseManager.AddVisualNoise("noiseCrash", worldPoint);
			float num = (startPoint - endPoint).Length() / 2f;
			_ = mainBody.WorldCenter;
			_ = (startPoint - endPoint).Length() / 2f;
			for (int i = 0; i < piecesList.Count; i++)
			{
				GlassPiece glassPiece = piecesList[i];
				glassPiece.Activate(mainBody.Position, mainBody.Rotation);
				float num2 = 4f * (float)GameElementsControl.Random.NextDouble() * ((num - (glassPiece.Body.WorldCenter - worldPoint).Length()) / num);
				glassPiece.Body.AngularVelocity = angularVelocitiesCache + 5f * (float)(GameElementsControl.Random.NextDouble() - 0.5);
				glassPiece.Body.LinearVelocity = vector * num2;
			}
			GameElementsControl.World.RemoveBody(mainBody);
			breakTime = GameElementsControl.CurrentTimeInMS;
			broken = true;
		}

		private void Deactivate()
		{
			if (!Broken)
			{
				mainBody.Enabled = false;
				GameElementsControl.World.RemoveBody(mainBody);
			}
			foreach (GlassPiece pieces in piecesList)
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
			if (breakIt && !broken)
			{
				BreakGlass(breakPoint, breakDirection);
			}
			if (!Broken)
			{
				velocitiesCache = mainBody.LinearVelocity;
				angularVelocitiesCache = mainBody.AngularVelocity;
				glassSpriteVariables.rotation = 0f - mainBody.Rotation;
				return;
			}
			foreach (GlassPiece pieces in piecesList)
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
			sidePower = Vector2.Normalize(sidePower);
			if (broken)
			{
				return;
			}
			switch (hitType)
			{
			case HitType.Blunt:
				if (sidePower.Length() > 10f)
				{
					BreakGlass(position, sidePower);
				}
				break;
			case HitType.Kick:
				BreakGlass(position, sidePower);
				break;
			case HitType.Pistol:
				BreakGlass(position, sidePower);
				break;
			case HitType.Explosion:
				BreakGlass(position, sidePower);
				break;
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
			return false;
		}

		public override void Draw()
		{
			if (!broken)
			{
				glassSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), glassSpriteVariables);
			}
			else
			{
				if (!base.Enabled)
				{
					return;
				}
				foreach (GlassPiece pieces in piecesList)
				{
					pieces.Draw();
				}
			}
		}
	}
}
