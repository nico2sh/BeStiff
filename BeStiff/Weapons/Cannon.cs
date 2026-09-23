using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Weapons
{
	public class Cannon
	{
		private const float length = 0.9f;

		private float rotation;

		private Vector2 position;

		private Vector2 cannonPosition;

		private GameSprite cannonSprite;

		public float Length => 0.9f;

		public float Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				rotation = value;
			}
		}

		public Vector2 Position => position;

		public Vector2 CannonPosition => cannonPosition;

		public Cannon()
		{
			GameElementsControl.LoadSprite("cannon", "sprites\\dangers\\cannon");
			cannonSprite = GameElementsControl.GetSprite("cannon");
		}

		public void Update(Vector2 pos)
		{
			position = pos;
			Vector2 zero = Vector2.Zero;
			zero.X = 0.9f * (float)Math.Cos(Rotation + (float)Math.PI / 2f);
			zero.Y = 0.9f * (float)Math.Sin(Rotation + (float)Math.PI / 2f);
			cannonPosition = pos + zero;
		}

		public void Draw()
		{
			cannonSprite.Draw(GameElementsControl.ConvertWorldToScreen(position), 0f - Rotation);
		}
	}
}
