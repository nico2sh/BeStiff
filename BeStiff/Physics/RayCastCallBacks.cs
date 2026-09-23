using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Physics
{
	public static class RayCastCallBacks
	{
		private static Vector2 normal;

		private static Fixture fixture;

		private static Vector2 point;

		private static FixtureList fixtureList = new FixtureList(10);

		private static WorldObject worldObject;

		private static bool hitClosest;

		private static float fraction;

		private static bool alert;

		private static string ignoreWorldObjectName;

		private static Vector2[] points = new Vector2[10];

		private static Body[] bodies = new Body[10];

		private static WorldObject[] worldObjects = new WorldObject[10];

		private static int worldObjectsNum = 0;

		private static HittingObjects hittingObjects = new HittingObjects();

		private static Category categoryToIgnore;

		private static Side side;

		private static float xPoint;

		private static AABB aabbTest;

		private static bool AABBQueryHit(Fixture fixt)
		{
			if ((fixt.CollisionCategories & categoryToIgnore) == 0 && !fixt.IsSensor && fixt.UserData is WorldObjectData worldObjectData && worldObjectData.Object.Name != ignoreWorldObjectName)
			{
				hittingObjects.Add(worldObjectData.Object, fixt);
			}
			return true;
		}

		private static bool AABBQueryAvoidObstacles(Fixture fixt)
		{
			if (fixt.UserData is WorldObjectData worldObjectData && !(worldObjectData.Object is Human) && !(worldObjectData.Object is IOneSidedWorldObject) && !fixt.IsSensor && fixt.Shape is PolygonShape polygonShape)
			{
				for (int i = 0; i < polygonShape.Vertices.Count; i++)
				{
					Vector2 vector = polygonShape.Vertices[i] + fixt.Body.Position;
					if (aabbTest.Contains(ref vector))
					{
						hitClosest = true;
						if (point.Y < vector.Y)
						{
							point = vector;
						}
					}
				}
			}
			return true;
		}

		private static bool AABBQueryWallSlide(Fixture fixt)
		{
			if (!fixt.IsSensor && fixt.UserData is WorldObjectData worldObjectData && worldObjectData.Object.Name != "Hero" && worldObjectData.Object.CanWallSlide(side, ref xPoint, ref fixture))
			{
				_ = GameElementsControl.Hero.Position.X;
				_ = side;
				_ = GameElementsControl.Hero.Width / 2f;
				hitClosest = true;
				return false;
			}
			return true;
		}

		private static bool AABBQueryWalls(Fixture fixt)
		{
			if (!fixt.IsSensor && fixt.UserData is WorldObjectData worldObjectData && worldObjectData.Object.Name != "Hero")
			{
				fixtureList.Add(fixt);
			}
			return true;
		}

		private static bool AABBQueryWalk(Fixture fixt)
		{
			if (!fixt.IsSensor && (fixt.Body.BodyType == BodyType.Static || fixt.Body.BodyType == BodyType.Kinematic))
			{
				hitClosest = true;
				return false;
			}
			return true;
		}

		private static float RCHuman(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if (CollisionFilter.CollidesWithHuman(f))
			{
				if (fr < fraction)
				{
					hitClosest = true;
					fraction = fr;
					fixture = f;
					normal = n;
					point = p;
				}
				return 1f;
			}
			return -1f;
		}

		private static float RCLineOfSight(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if (!CanLookThrough(f))
			{
				if (fr < fraction)
				{
					hitClosest = true;
					fraction = fr;
					fixture = f;
					normal = n;
					point = p;
				}
				return 1f;
			}
			return -1f;
		}

		private static float RCNoHero(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if ((f.CollisionCategories & Category.Cat3) == 0)
			{
				if (fr < fraction)
				{
					hitClosest = true;
					fraction = fr;
					fixture = f;
					normal = n;
					point = p;
				}
				return 1f;
			}
			return -1f;
		}

		private static float RCCrossHair(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if (f.UserData is WorldObjectData worldObjectData)
			{
				if ((f.CollidesWith & Category.Cat7) != Category.None && worldObjectData.Object.Name != ignoreWorldObjectName && !f.IsSensor && fr < fraction)
				{
					hitClosest = true;
					worldObject = worldObjectData.Object;
					fraction = fr;
					point = p;
				}
				return 1f;
			}
			return -1f;
		}

		private static float RCBullet(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if (worldObjectsNum < 10 && CollisionFilter.Collides(f) && f.UserData is WorldObjectData worldObjectData && (f.CollidesWith & Category.Cat7) != Category.None && worldObjectData.Object.Name != ignoreWorldObjectName)
			{
				bool flag = false;
				for (int i = 0; i < worldObjectsNum; i++)
				{
					if (worldObjects[i] == worldObjectData.Object)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					worldObjects[worldObjectsNum] = worldObjectData.Object;
					bodies[worldObjectsNum] = f.Body;
					points[worldObjectsNum] = p;
					worldObjectsNum++;
				}
			}
			return 1f;
		}

		private static bool CanLookThrough(Fixture fixture)
		{
			if (fixture.IsSensor)
			{
				return true;
			}
			if (fixture.UserData is WorldObjectData worldObjectData)
			{
				if (worldObjectData.Object.CanSeeThrough(alert))
				{
					return true;
				}
				if (worldObjectData.Object is Human)
				{
					return true;
				}
			}
			return false;
		}

		private static void ResetForRaycast()
		{
			hitClosest = false;
			fraction = float.MaxValue;
			worldObjectsNum = 0;
		}

		public static bool RayCastPistolCrossHair(Vector2 point1, Vector2 point2, string ignoreName, out Vector2 po)
		{
			ResetForRaycast();
			ignoreWorldObjectName = ignoreName;
			GameElementsControl.World.RayCast(RCCrossHair, point1, point2);
			po = point;
			return hitClosest;
		}

		public static bool RayCastHookCrossHair(Vector2 point1, Vector2 point2, string ignoreName, out WorldObject wo, out Vector2 po)
		{
			ResetForRaycast();
			ignoreWorldObjectName = ignoreName;
			GameElementsControl.World.RayCast(RCCrossHair, point1, point2);
			wo = worldObject;
			po = point;
			return hitClosest;
		}

		public static int RayCastBullet(Vector2 point1, Vector2 point2, string ignoreName, out WorldObject[] wo, out Body[] bo, out Vector2[] po)
		{
			ResetForRaycast();
			ignoreWorldObjectName = ignoreName;
			GameElementsControl.World.RayCast(RCBullet, point1, point2);
			wo = worldObjects;
			bo = bodies;
			po = points;
			return worldObjectsNum;
		}

		public static Fixture RayCastOneNoHuman(Vector2 point1, Vector2 point2, out Vector2 po, out Vector2 norm, out float frac)
		{
			ResetForRaycast();
			GameElementsControl.World.RayCast(RCHuman, point1, point2);
			norm = normal;
			frac = fraction;
			po = point;
			if (!hitClosest)
			{
				fixture = null;
			}
			return fixture;
		}

		public static bool RayCastLineOfSight(Vector2 point1, Vector2 point2, bool alertDetect)
		{
			ResetForRaycast();
			alert = alertDetect;
			GameElementsControl.World.RayCast(RCLineOfSight, point1, point2);
			return hitClosest;
		}

		public static bool RayCastNoHero(Vector2 point1, Vector2 point2)
		{
			ResetForRaycast();
			GameElementsControl.World.RayCast(RCNoHero, point1, point2);
			return hitClosest;
		}

		public static void QueryKickAABB(ref AABB aabb, string ignoreName, Category ignoreCategory, out HittingObjects hit)
		{
			hittingObjects.Clear();
			categoryToIgnore = ignoreCategory;
			ignoreWorldObjectName = ignoreName;
			GameElementsControl.World.QueryAABB(AABBQueryHit, ref aabb);
			hit = hittingObjects;
		}

		public static FixtureList QueryWallSlideAABB(ref AABB aabb)
		{
			fixtureList.Clear();
			GameElementsControl.World.QueryAABB(AABBQueryWalls, ref aabb);
			return fixtureList;
		}

		public static bool QueryWalkAABB(ref AABB aabb)
		{
			hitClosest = false;
			GameElementsControl.World.QueryAABB(AABBQueryWalk, ref aabb);
			return hitClosest;
		}

		public static bool QueryObstacleAABB(ref AABB aabb, out Vector2 vertice)
		{
			hitClosest = false;
			aabbTest = aabb;
			point = new Vector2(0f, float.MinValue);
			GameElementsControl.World.QueryAABB(AABBQueryAvoidObstacles, ref aabb);
			vertice = point;
			return hitClosest;
		}
	}
}
