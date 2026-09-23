using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Collection of points to generate a polygon.
	/// </summary>
	/// <remarks>By implementing the IList interface explicitly, we can effectively override certain methods of the
	/// base class without them being declared as virtual.</remarks>
	public class PolygonPointCollection : List<Vector2>, IList, ICollection, IEnumerable
	{
		private PolygonOrigin _origin;

		/// <summary>
		/// Gets or sets an offset vector for the shape.
		/// </summary>
		[ContentSerializerIgnore]
		public Vector2 TranslationOffset;

		/// <summary>
		/// Gets or sets the origin of the shape defined by the points in the collection.
		/// </summary>
		[ContentSerializerIgnore]
		public PolygonOrigin Origin
		{
			get
			{
				return _origin;
			}
			set
			{
				if (Origin != value)
				{
					_origin = value;
					RecalculateTranslation();
				}
			}
		}

		object IList.this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				Vector2 value2 = (Vector2)value;
				base[index] = value2;
				RecalculateTranslation();
			}
		}

		/// <summary>
		/// Recalculats the translation offset vector based on the origin type.
		/// </summary>
		private void RecalculateTranslation()
		{
			switch (Origin)
			{
			case PolygonOrigin.Default:
				GetDefaultTranslation(out TranslationOffset);
				break;
			case PolygonOrigin.Center:
				GetCenterTranslation(out TranslationOffset);
				break;
			case PolygonOrigin.Origin:
				GetOriginTranslation(out TranslationOffset);
				break;
			}
		}

		private void GetCenterTranslation(out Vector2 offset)
		{
			float num = base[0].X;
			float num2 = base[0].X;
			float num3 = base[0].Y;
			float num4 = base[0].Y;
			for (int i = 1; i < base.Count; i++)
			{
				num = ((base[i].X < num) ? base[i].X : num);
				num2 = ((base[i].X > num2) ? base[i].X : num2);
				num3 = ((base[i].Y < num3) ? base[i].Y : num3);
				num4 = ((base[i].Y > num4) ? base[i].Y : num4);
			}
			offset = new Vector2
			{
				X = 0f - (num2 - num) / 2f + num,
				Y = 0f - (num4 - num3) / 2f + num3
			};
		}

		private void GetOriginTranslation(out Vector2 offset)
		{
			offset = base[0];
		}

		private void GetDefaultTranslation(out Vector2 offset)
		{
			offset = Vector2.Zero;
		}

		int IList.Add(object value)
		{
			Vector2 item = (Vector2)value;
			Add(item);
			RecalculateTranslation();
			return IndexOf(item);
		}

		void IList.Clear()
		{
			Clear();
		}

		void IList.Remove(object value)
		{
			Vector2 item = (Vector2)value;
			if (Contains(item))
			{
				Remove(item);
			}
			RecalculateTranslation();
		}

		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
			RecalculateTranslation();
		}
	}
}
