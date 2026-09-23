using Microsoft.Xna.Framework;

namespace Be_Stiff.WorldObjects
{
	internal interface IOneSidedAffected
	{
		Vector2 FloorPosition { get; }

		WorldObject OneSidedIgnored { get; }

		bool IgnoreOneSide();

		void OneSidedIgnore(WorldObject wo);
	}
}
