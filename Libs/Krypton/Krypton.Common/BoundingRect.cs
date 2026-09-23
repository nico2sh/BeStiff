using Microsoft.Xna.Framework;

namespace Krypton.Common
{
	public struct BoundingRect
	{
		public Vector2 Min;

		public Vector2 Max;

		public static BoundingRect mEmpty;

		private static BoundingRect mMinMax;

		public float Left => Min.X;

		public float Right => Max.X;

		public float Top => Max.Y;

		public float Bottom => Min.Y;

		public float Width => Max.X - Min.X;

		public float Height => Max.Y - Min.Y;

		public Vector2 Center => (Min + Max) / 2f;

		public static BoundingRect Empty => mEmpty;

		public static BoundingRect MinMax => mMinMax;

		public bool IsZero
		{
			get
			{
				if (Min.X == 0f && Min.Y == 0f && Max.X == 0f)
				{
					return Max.Y == 0f;
				}
				return false;
			}
		}

		static BoundingRect()
		{
			mEmpty = default(BoundingRect);
			mMinMax = new BoundingRect(Vector2.One * float.MinValue, Vector2.One * float.MaxValue);
		}

		public BoundingRect(float x, float y, float width, float height)
		{
			Min = Vector2.Zero;
			Max = Vector2.Zero;
			Min.X = x;
			Min.Y = y;
			Max.X = x + width;
			Max.Y = y + height;
		}

		public BoundingRect(Vector2 min, Vector2 max)
		{
			Min = min;
			Max = max;
		}

		public bool Contains(float x, float y)
		{
			if (Min.X <= x && Min.Y <= y && Max.X >= x)
			{
				return Max.Y >= y;
			}
			return false;
		}

		public bool Contains(Vector2 vector)
		{
			if (Min.X <= vector.X && Min.Y <= vector.Y && Max.X >= vector.X)
			{
				return Max.Y >= vector.Y;
			}
			return false;
		}

		public void Contains(ref Vector2 rect, out bool result)
		{
			result = Min.X <= rect.X && Min.Y <= rect.Y && Max.X >= rect.X && Max.Y >= rect.Y;
		}

		public bool Contains(BoundingRect rect)
		{
			if (Min.X <= rect.Min.X && Min.Y <= rect.Min.Y && Max.X >= rect.Max.X)
			{
				return Max.Y >= rect.Max.Y;
			}
			return false;
		}

		public void Contains(ref BoundingRect rect, out bool result)
		{
			result = Min.X <= rect.Min.X && Min.Y <= rect.Min.Y && Max.X >= rect.Max.X && Max.Y >= rect.Max.Y;
		}

		public bool Intersects(BoundingRect rect)
		{
			if (Min.X < rect.Max.X && Min.Y < rect.Max.Y && Max.X > rect.Min.X)
			{
				return Max.Y > rect.Min.Y;
			}
			return false;
		}

		public void Intersects(ref BoundingRect rect, out bool result)
		{
			result = Min.X < rect.Max.X && Min.Y < rect.Max.Y && Max.X > rect.Min.X && Max.Y > rect.Min.Y;
		}

		public static BoundingRect Intersect(BoundingRect rect1, BoundingRect rect2)
		{
			BoundingRect result = mEmpty;
			float x = rect1.Max.X;
			float x2 = rect2.Max.X;
			float y = rect1.Max.Y;
			float y2 = rect2.Max.Y;
			float num = ((rect1.Min.X > rect2.Min.X) ? rect1.Min.X : rect2.Min.X);
			float num2 = ((rect1.Min.Y > rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y);
			float num3 = ((x < x2) ? x : x2);
			float num4 = ((y < y2) ? y : y2);
			if (num3 > num && num4 > num2)
			{
				result.Min.X = num;
				result.Min.Y = num2;
				result.Max.X = num3;
				result.Max.Y = num4;
				return result;
			}
			result.Min.X = 0f;
			result.Min.Y = 0f;
			result.Max.X = 0f;
			result.Max.Y = 0f;
			return result;
		}

		public static void Intersect(ref BoundingRect rect1, ref BoundingRect rect2, out BoundingRect result)
		{
			result = mEmpty;
			float x = rect1.Max.X;
			float x2 = rect2.Max.X;
			float y = rect1.Max.Y;
			float y2 = rect2.Max.Y;
			float num = ((rect1.Min.X > rect2.Min.X) ? rect1.Min.X : rect2.Min.X);
			float num2 = ((rect1.Min.Y > rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y);
			float num3 = ((x < x2) ? x : x2);
			float num4 = ((y < y2) ? y : y2);
			if (num3 > num && num4 > num2)
			{
				result.Min.X = num;
				result.Min.Y = num2;
				result.Max.X = num3;
				result.Max.Y = num4;
			}
			result.Min.X = 0f;
			result.Min.Y = 0f;
			result.Max.X = 0f;
			result.Max.Y = 0f;
		}

		public static BoundingRect Union(BoundingRect rect1, BoundingRect rect2)
		{
			BoundingRect result = mEmpty;
			float x = rect1.Max.X;
			float x2 = rect2.Max.X;
			float y = rect1.Max.Y;
			float y2 = rect2.Max.Y;
			float x3 = ((rect1.Min.X < rect2.Min.X) ? rect1.Min.X : rect2.Min.X);
			float y3 = ((rect1.Min.Y < rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y);
			float x4 = ((x > x2) ? x : x2);
			float y4 = ((y > y2) ? y : y2);
			result.Min.X = x3;
			result.Min.Y = y3;
			result.Max.X = x4;
			result.Max.Y = y4;
			return result;
		}

		public static void Union(ref BoundingRect rect1, ref BoundingRect rect2, out BoundingRect result)
		{
			result = mEmpty;
			float x = rect1.Max.X;
			float x2 = rect2.Max.X;
			float y = rect1.Max.Y;
			float y2 = rect2.Max.Y;
			float x3 = ((rect1.Min.X < rect2.Min.X) ? rect1.Min.X : rect2.Min.X);
			float y3 = ((rect1.Min.Y < rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y);
			float x4 = ((x > x2) ? x : x2);
			float y4 = ((y > y2) ? y : y2);
			result.Min.X = x3;
			result.Min.Y = y3;
			result.Max.X = x4;
			result.Max.Y = y4;
		}

		public bool Equals(BoundingRect other)
		{
			if (Min.X == other.Min.X && Min.Y == other.Min.Y && Max.X == other.Max.X)
			{
				return Max.Y == other.Max.Y;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Min.GetHashCode() + Max.GetHashCode();
		}

		public static bool operator ==(BoundingRect a, BoundingRect b)
		{
			if (a.Min.X == b.Min.X && a.Min.Y == b.Min.Y && a.Max.X == b.Max.X)
			{
				return a.Max.Y == b.Max.Y;
			}
			return false;
		}

		public static bool operator !=(BoundingRect a, BoundingRect b)
		{
			if (a.Min.X == b.Min.X && a.Min.Y == b.Min.Y && a.Max.X == b.Max.X)
			{
				return a.Max.Y != b.Max.Y;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is BoundingRect)
			{
				return this == (BoundingRect)obj;
			}
			return false;
		}
	}
}
