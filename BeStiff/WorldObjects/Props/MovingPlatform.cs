using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.WorldObjects.Props
{
	/// <summary>
	/// A platform that travels along its Ogmo node path. Subclasses give the
	/// orientation-specific geometry and decoration.
	/// </summary>
	internal abstract class MovingPlatform : WorldObject, IShadowCaster
	{
		protected const float FloorThick = 0.5f;

		protected Fixture fixture;

		protected PathMover mover;

		/// <summary>Platform size in world units.</summary>
		protected Vector2 size;

		private Rectangle rectangle;

		private GameSprite platformSprite;

		private GameSprite borderSprite;

		private GameSprite pivotSprite;

		private Vector2 pivotPosition1;

		private Vector2 pivotPosition2;

		private Vector2 borderPosition1;

		private Vector2 borderPosition2;

		private float pivotRotation;

		private ShadowHull shadowHull;

		private Vector2 lastPosition;

		public override float Mass => mainBody.Mass;

		public override Body MainBody => mainBody;

		/// <summary>Ogmo object name, e.g. "MovingPlatformH".</summary>
		protected abstract string ObjectName { get; }

		/// <summary>Suffix of the platform and border sprites ("H" or "V").</summary>
		protected abstract string SpriteSuffix { get; }

		/// <summary>Flip applied to the far border sprite.</summary>
		protected abstract SpriteEffects FarBorderFlip { get; }

		protected abstract Vector2 MeasureSize(WorldScaledOgmoObject obj);

		/// <summary>Shadow hull corners, in display units relative to the body.</summary>
		protected abstract Vector2[] HullPoints();

		/// <summary>Offsets (world units) of the two pivots and the two borders.</summary>
		protected abstract void DecorOffsets(out Vector2 pivot1, out Vector2 pivot2, out Vector2 border1, out Vector2 border2);

		/// <summary>Called at the end of loading.</summary>
		protected virtual void OnLoaded()
		{
		}

		/// <summary>Called every update after the platform has moved.</summary>
		protected virtual void OnMoved()
		{
		}

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals(ObjectName))
			{
				return false;
			}
			base.Name = obj.GetValue<OgmoStringValue>("ID").Value;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			Vector2[] path = PathMover.BuildPath(worldScaledOgmoObject);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Kinematic;
			mainBody.Position = worldScaledOgmoObject.Position;
			size = MeasureSize(worldScaledOgmoObject);
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(0f, 0f));
			vertices.Add(new Vector2(0f, size.Y));
			vertices.Add(new Vector2(size.X, size.Y));
			vertices.Add(new Vector2(size.X, 0f));
			vertices.Reverse(); // y was flipped for the y-down world; keep counter-clockwise winding
			PolygonShape shape = new PolygonShape(vertices, 10f);
			fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			fixture.Restitution = 0.1f;
			fixture.Friction = 1f;
			Vector2[] points = HullPoints();
			shadowHull = ShadowHull.CreateConvex(ref points);
			shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			GameElementsControl.Krypton.Hulls.Add(shadowHull);
			rectangle = new Rectangle(0, 1, obj.Width, obj.Height);
			string platformName = "movingPlatform" + SpriteSuffix;
			string borderName = "movingPlatformBorder" + SpriteSuffix;
			GameElementsControl.LoadSprite(platformName, "sprites\\objects\\movingPlatform" + SpriteSuffix.ToLowerInvariant());
			GameElementsControl.LoadSprite("doorPivot", "sprites\\objects\\doorpivot");
			GameElementsControl.LoadSprite(borderName, "sprites\\objects\\movingPlatformborder" + SpriteSuffix.ToLowerInvariant());
			platformSprite = GameElementsControl.GetSprite(platformName);
			pivotSprite = GameElementsControl.GetSprite("doorPivot");
			borderSprite = GameElementsControl.GetSprite(borderName);
			pivotRotation = 0f;
			UpdateDecorPositions();
			lastPosition = mainBody.Position;
			GameElementsControl.ScreenManager.AudioManager.LoadSound("platformMoving", "audio\\noises\\platformmoving");
			mover = new PathMover(mainBody, path, obj.GetValue<OgmoNumberValue>("speed").Value, obj.GetValue<OgmoNumberValue>("decelerateDistance").Value, obj.GetValue<OgmoNumberValue>("timeInStop").Value, "platformMoving", startStopped: true);
			OnLoaded();
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override void Update()
		{
			mover.Update();
			if (mover.CurrentSpeed != 0f)
			{
				Vector2 direction = mover.Direction;
				if (direction.X > 0f || direction.Y > 0f)
				{
					pivotRotation += MathHelper.ToRadians(mover.CurrentSpeed);
				}
				else if (direction.X < 0f || direction.Y < 0f)
				{
					pivotRotation -= MathHelper.ToRadians(mover.CurrentSpeed);
				}
			}
			if (mainBody.Position != lastPosition)
			{
				lastPosition = mainBody.Position;
				UpdateDecorPositions();
				OnMoved();
				shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			}
		}

		private void UpdateDecorPositions()
		{
			DecorOffsets(out Vector2 pivot1, out Vector2 pivot2, out Vector2 border1, out Vector2 border2);
			pivotPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + pivot1);
			pivotPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + pivot2);
			borderPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + border1);
			borderPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + border2);
		}

		public int CurrentFloor()
		{
			return mover.CurrentFloor;
		}

		public override bool CanHanged()
		{
			return true;
		}

		public override void Draw()
		{
		}

		public void DrawHull()
		{
			platformSprite.DrawNoOrigin(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation, rectangle);
			pivotSprite.Draw(pivotPosition1, pivotRotation);
			pivotSprite.Draw(pivotPosition2, pivotRotation);
			borderSprite.Draw(borderPosition1, 0f, SpriteEffects.None);
			borderSprite.Draw(borderPosition2, 0f, FarBorderFlip);
		}
	}
}
