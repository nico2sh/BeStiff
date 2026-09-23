using Microsoft.Xna.Framework;

namespace Krypton
{
	public struct ShadowHullPoint
	{
		public Vector2 Position;

		public Vector2 Normal;

		public ShadowHullPoint(Vector2 position, Vector2 normal)
		{
			Position = position;
			Normal = normal;
		}
	}
}
