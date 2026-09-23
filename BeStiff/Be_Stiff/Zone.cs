using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff
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

		public Vector2 Center => new Vector2(position.X + width / 2f, position.Y - height / 2f);

		public float Width => width;

		public float Height => height;

		public Zone(Vector2 p, float w, float h)
		{
			position = p;
			width = w;
			height = h;
		}

		public bool Intersects(ref Vector2 p1, ref Vector2 p2, out Vector2 inter)
		{
			Vector2 zero = Vector2.Zero;
			Vector2 zero2 = Vector2.Zero;
			inter = Vector2.Zero;
			float x = position.X;
			float num = position.X + width;
			float y = position.Y;
			float num2 = position.Y - height;
			float num3 = p2.X - p1.X;
			float num4 = p2.Y - p1.Y;
			float val = (0f - (p1.X - x)) / num3;
			float val2 = (num - p1.X) / num3;
			float val3 = (0f - (p1.Y - num2)) / num4;
			float val4 = (y - p1.Y) / num4;
			float num5 = Math.Max(Math.Max(val, val3), 0f);
			float num6 = Math.Min(Math.Min(val2, val4), 1f);
			if (num5 < num6)
			{
				bool flag = false;
				bool result = false;
				if (num5 != 0f)
				{
					zero.X = p1.X + num3 * num5;
					zero.Y = p1.Y + num4 * num5;
					inter = zero;
					flag = true;
				}
				if (num6 != 1f)
				{
					zero2.X = p1.X + num3 * num6;
					zero2.Y = p2.Y + num4 * num6;
					if (flag)
					{
						if ((p1 - zero).Length() > (p1 - zero2).Length())
						{
							inter = zero2;
						}
					}
					else
					{
						inter = zero2;
					}
					result = true;
				}
				if (!flag)
				{
					return result;
				}
				return true;
			}
			return false;
		}

		public bool Contains(ref Vector2 pos)
		{
			if (pos.X > position.X && pos.X < position.X + width && pos.Y > position.Y - height && pos.Y < position.Y)
			{
				return true;
			}
			return false;
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
