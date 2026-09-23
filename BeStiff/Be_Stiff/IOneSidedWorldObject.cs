using FarseerPhysics.Common;

namespace Be_Stiff
{
	public interface IOneSidedWorldObject
	{
		Vertices TheVertices { get; set; }

		bool ActiveOneSide { get; }

		bool IsProjectile { get; }

		bool ConditionalContacts(WorldObject wo);
	}
}
