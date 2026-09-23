using Microsoft.Xna.Framework;

namespace Be_Stiff.WorldObjects
{
	public interface ISeat
	{
		Side Side { get; }

		Vector2 ReferencePoint { get; }
	}
}
