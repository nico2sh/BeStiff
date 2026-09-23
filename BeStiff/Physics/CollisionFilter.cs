using FarseerPhysics.Dynamics;

namespace Be_Stiff.Physics
{
	internal static class CollisionFilter
	{
		public static bool Collides(Fixture fixture)
		{
			return fixture.CollisionCategories != Category.None && fixture.Body.Enabled && !fixture.IsSensor;
		}

		public static bool CollidesWithHuman(Fixture fixture)
		{
			return Collides(fixture) && !(fixture.UserData is WorldObjectData worldObjectData && worldObjectData.Object is Human);
		}
	}
}
