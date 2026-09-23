using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Levels
{
	public class Zone
	{
		private Vector2 position;

		private float width;

		private float height;

		public Vector2 Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
			}
		}

		// Position is the top-left corner; y grows downwards.
		public Vector2 Center => new Vector2(position.X + width / 2f, position.Y + height / 2f);

		public float Width => width;

		public float Height => height;

		public Zone(Vector2 p, float w, float h)
		{
			position = p;
			width = w;
			height = h;
		}

		/// <summary>
		/// Clips the segment p1-p2 against the zone (slab method). Returns true
		/// when the segment crosses the zone's border; inter is the crossing
		/// closest to p1.
		/// </summary>
		public bool Intersects(ref Vector2 p1, ref Vector2 p2, out Vector2 inter)
		{
			inter = Vector2.Zero;
			Vector2 d = p2 - p1;
			float tEnter = 0f;
			float tExit = 1f;
			if (!ClipSlab(p1.X, d.X, position.X, position.X + width, ref tEnter, ref tExit)
				|| !ClipSlab(p1.Y, d.Y, position.Y, position.Y + height, ref tEnter, ref tExit)
				|| tEnter >= tExit)
			{
				return false;
			}
			if (tEnter > 0f)
			{
				inter = p1 + d * tEnter;
				return true;
			}
			if (tExit < 1f)
			{
				inter = p1 + d * tExit;
				return true;
			}
			return false;
		}

		private static bool ClipSlab(float start, float delta, float min, float max, ref float tEnter, ref float tExit)
		{
			if (delta == 0f)
			{
				return start > min && start < max;
			}
			float t1 = (min - start) / delta;
			float t2 = (max - start) / delta;
			if (t1 > t2)
			{
				(t1, t2) = (t2, t1);
			}
			tEnter = Math.Max(tEnter, t1);
			tExit = Math.Min(tExit, t2);
			return true;
		}

		public bool Contains(ref Vector2 pos)
		{
			return pos.X > position.X && pos.X < position.X + width && pos.Y > position.Y && pos.Y < position.Y + height;
		}

		public void GetDistance(ref Vector2 point, out float distance)
		{
			float num = Math.Abs(point.X - (2f * position.X + width) * 0.5f) - width * 0.5f;
			float num2 = Math.Abs(point.Y - (2f * position.Y + height) * 0.5f) - height * 0.5f;
			if (num > 0f && num2 > 0f)
			{
				distance = (float)Math.Sqrt(num * num + num2 * num2);
			}
			else
			{
				distance = Math.Max(num, num2);
			}
		}
	}
}
