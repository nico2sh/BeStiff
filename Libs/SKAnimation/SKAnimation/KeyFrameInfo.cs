using Microsoft.Xna.Framework;

namespace SKAnimation
{
	public struct KeyFrameInfo
	{
		public float Angle;

		public Vector2 Position;

		public int SpriteFrame;

		public KeyFrameInfo(float angle, Vector2 position)
		{
			Angle = angle;
			Position = position;
			SpriteFrame = 0;
		}
	}
}
