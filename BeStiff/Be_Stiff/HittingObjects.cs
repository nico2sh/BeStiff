using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	public class HittingObjects
	{
		private const int MAX_OBJECTS = 10;

		private WorldObject[] hittingObjects;

		private int hittingObjectsNum;

		private Body[] hittingBodies;

		private int hittingBodiesNum;

		public int NumObjects => hittingObjectsNum;

		public int NumBodies => hittingBodiesNum;

		public HittingObjects()
		{
			hittingObjects = new WorldObject[10];
			hittingObjectsNum = 0;
			hittingBodies = new Body[10];
			hittingBodiesNum = 0;
		}

		public void Add(WorldObject wo, Fixture fix)
		{
			if (hittingObjectsNum < 10)
			{
				bool flag = false;
				for (int i = 0; i < hittingObjectsNum; i++)
				{
					if (hittingObjects[i] == wo)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					hittingObjects[hittingObjectsNum] = wo;
					hittingObjectsNum++;
				}
			}
			if (hittingBodiesNum >= 10)
			{
				return;
			}
			bool flag2 = false;
			for (int j = 0; j < hittingBodiesNum; j++)
			{
				if (hittingBodies[j] == fix.Body)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				hittingBodies[hittingBodiesNum] = fix.Body;
				hittingBodiesNum++;
			}
		}

		public void Clear()
		{
			hittingObjectsNum = 0;
			hittingBodiesNum = 0;
		}

		public void Hit(Vector2 direction, Vector2 position, HitType hitType)
		{
			for (int i = 0; i < hittingObjectsNum; i++)
			{
				hittingObjects[i].Hit(direction, position, hitType);
			}
		}

		public void ApplyLinearImpulse(Vector2 impulse)
		{
			for (int i = 0; i < hittingBodiesNum; i++)
			{
				hittingBodies[i].ApplyLinearImpulse(impulse);
			}
		}
	}
}
