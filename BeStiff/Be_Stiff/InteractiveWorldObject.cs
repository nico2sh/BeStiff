using System.Collections.Generic;
using FarseerPhysics.Dynamics;

namespace Be_Stiff
{
	public abstract class InteractiveWorldObject : WorldObject
	{
		protected double interactionTime;

		private List<WorldObject> fixturesInteracting;

		protected Fixture sensorFixture;

		protected bool interacting;

		public bool Active;

		public InteractiveWorldObject()
		{
			interacting = false;
			interactionTime = 0.0;
			fixturesInteracting = new List<WorldObject>(8);
			Active = true;
		}

		public virtual bool Interacts(WorldObjectType type)
		{
			return true;
		}

		public virtual void StartInteracting(WorldObject worldObject)
		{
			if (Active)
			{
				interacting = true;
				if (!fixturesInteracting.Contains(worldObject))
				{
					fixturesInteracting.Add(worldObject);
				}
			}
		}

		public virtual void StopInteracting(WorldObject worldObject)
		{
			if (Active)
			{
				fixturesInteracting.Remove(worldObject);
				if (fixturesInteracting.Count == 0)
				{
					interacting = false;
					interactionTime = 0.0;
				}
			}
		}
	}
}
