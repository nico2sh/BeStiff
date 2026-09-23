using System;
using System.Globalization;

namespace ProjectMercury
{
	/// <summary>
	/// Defines a range (or interval) of floating point values.
	/// </summary>
	[Serializable]
	public struct Range : IEquatable<Range>, IFormattable
	{
		/// <summary>
		/// Gets or sets the inclusive minimum value in the range.
		/// </summary>
		public float Minimum;

		/// <summary>
		/// Gets or sets the inclusive maximum value in the range.
		/// </summary>
		public float Maximum;

		/// <summary>
		/// Gets the size of the range.
		/// </summary>
		public float Size => Calculator.Abs(Maximum - Minimum);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ProjectMercury.Range" /> struct.
		/// </summary>
		/// <param name="minimum">The inclusive minimum value.</param>
		/// <param name="maximum">The inclusive maximum value.</param>
		public Range(float minimum, float maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		/// <summary>
		/// Returns true if the specified range is completely contained within this range.
		/// </summary>
		public bool Contains(Range range)
		{
			if (Minimum <= range.Minimum)
			{
				return Maximum >= range.Maximum;
			}
			return false;
		}

		/// <summary>
		/// Returns true if the specified value falls within the range.
		/// </summary>
		public bool Contains(float value)
		{
			if (Minimum <= value)
			{
				return Maximum >= value;
			}
			return false;
		}

		/// <summary>
		/// Merges the specifed range into this range with a boolean union.
		/// </summary>
		public void Merge(Range value)
		{
			Minimum = ((Minimum < value.Minimum) ? Minimum : value.Minimum);
			Maximum = ((Maximum > value.Maximum) ? Maximum : value.Maximum);
		}

		/// <summary>
		/// Intersects the specified range with this range with a boolean intersection.
		/// </summary>
		public void Intersect(Range value)
		{
			Minimum = ((Minimum > value.Minimum) ? Minimum : value.Minimum);
			Maximum = ((Maximum < value.Maximum) ? Maximum : value.Maximum);
		}

		/// <summary>
		/// Subtracts the specified range from this range with a boolean difference.
		/// </summary>
		public void Subtract(Range value)
		{
			Range range = Intersect(this, value);
			if (range.Minimum > Minimum)
			{
				Maximum = range.Minimum;
			}
			else if (range.Maximum > Minimum)
			{
				Minimum = range.Maximum;
			}
		}

		/// <summary>
		/// Creates a new range which is the boolean union of two specified ranges.
		/// </summary>
		/// <param name="x">Input range.</param>
		/// <param name="y">Input range.</param>
		public static Range Union(Range x, Range y)
		{
			return new Range
			{
				Minimum = ((x.Minimum < y.Minimum) ? x.Minimum : y.Minimum),
				Maximum = ((x.Maximum > y.Maximum) ? x.Maximum : y.Maximum)
			};
		}

		/// <summary>
		/// Creates a new range which is the boolean intersection of two specified ranges.
		/// </summary>
		/// <param name="x">Input range.</param>
		/// <param name="y">Input range.</param>
		public static Range Intersect(Range x, Range y)
		{
			return new Range
			{
				Minimum = ((x.Minimum > y.Minimum) ? x.Minimum : y.Minimum),
				Maximum = ((x.Maximum < y.Maximum) ? x.Maximum : y.Maximum)
			};
		}

		/// <summary>
		/// Creates a new range which is the boolean difference between two specified ranges.
		/// </summary>
		/// <param name="x">Input range.</param>
		/// <param name="y">Input range.</param>
		public static Range Subtract(Range x, Range y)
		{
			Range range = Intersect(x, y);
			Range result = default(Range);
			if (range.Minimum > x.Minimum)
			{
				result.Maximum = range.Minimum;
			}
			else if (range.Maximum > x.Maximum)
			{
				result.Minimum = range.Maximum;
			}
			return result;
		}

		/// <summary>
		/// Creates a new range by parsing an ISO 31-11 string representation of a closed interval.
		/// </summary>
		/// <param name="value">Input string value.</param>
		/// <exception cref="T:System.FormatException">Thrown if the input string is not in a valid ISO 31-11 closed interval format.</exception>
		/// <remarks>Example of a well formed ISO 31-11 closed interval: <i>"[0,1]"</i>. Open intervals are not supported.</remarks>
		public static Range Parse(string value)
		{
			return Parse(value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Creates a new range by parsing an ISO 31-11 string representation of a closed interval.
		/// </summary>
		/// <param name="value">Input stirng value.</param>
		/// <param name="format">The format provider.</param>
		/// <remarks>Example of a well formed ISO 31-11 closed interval: <i>"[0,1]"</i>. Open intervals are not supported.</remarks>
		public static Range Parse(string value, IFormatProvider format)
		{
			if (value.StartsWith("[") && value.EndsWith("]"))
			{
				NumberFormatInfo instance = NumberFormatInfo.GetInstance(format);
				char[] separator = instance.NumberGroupSeparator.ToCharArray();
				string[] array = value.Trim('[', ']').Split(separator);
				if (array.Length == 2)
				{
					return new Range
					{
						Minimum = float.Parse(array[0], NumberStyles.Float, instance),
						Maximum = float.Parse(array[1], NumberStyles.Float, instance)
					};
				}
			}
			throw new FormatException("Value is not in ISO 31-11 format for a closed interval.");
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj != null && obj is Range)
			{
				return Equals((Range)obj);
			}
			return false;
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:ProjectMercury.Range" /> is equal to this instance.
		/// </summary>
		/// <param name="value">The <see cref="T:ProjectMercury.Range" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:ProjectMercury.Range" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(Range value)
		{
			if (Minimum.Equals(value.Minimum))
			{
				return Maximum.Equals(value.Maximum);
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Minimum.GetHashCode() ^ Maximum.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return ToString("G", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public string ToString(IFormatProvider formatProvider)
		{
			return ToString("G", formatProvider);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			NumberFormatInfo instance = NumberFormatInfo.GetInstance(formatProvider);
			string text = Minimum.ToString(format, instance);
			string text2 = Maximum.ToString(format, instance);
			string numberGroupSeparator = instance.NumberGroupSeparator;
			return string.Format(formatProvider, "[{0}{1}{2}]", new object[3] { text, numberGroupSeparator, text2 });
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="x">The lvalue.</param>
		/// <param name="y">The rvalue.</param>
		/// <returns>The boolean union of the lvalue and rvalue.</returns>
		public static Range operator +(Range x, Range y)
		{
			return Union(x, y);
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="x">The lvalue.</param>
		/// <param name="y">the rvalue.</param>
		/// <returns>The boolean difference of the lvalue and rvalue.</returns>
		public static Range operator -(Range x, Range y)
		{
			return Subtract(x, y);
		}

		/// <summary>
		/// Implements the operator |.
		/// </summary>
		/// <param name="x">The lvalue.</param>
		/// <param name="y">The rvalue.</param>
		/// <returns>The boolean intersection of the lvalue and rvalue.</returns>
		public static Range operator |(Range x, Range y)
		{
			return Intersect(x, y);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="x">The lvalue.</param>
		/// <param name="y">The rvalue.</param>
		/// <returns>
		/// 	<c>true</c> if the lvalue <see cref="T:ProjectMercury.Range" /> is equal to the rvalue; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator ==(Range x, Range y)
		{
			return x.Equals(y);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="x">The lvalue.</param>
		/// <param name="y">The rvalue.</param>
		/// <returns>
		/// 	<c>true</c> if the lvalue <see cref="T:ProjectMercury.Range" /> is not equal to the rvalue; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator !=(Range x, Range y)
		{
			return !x.Equals(y);
		}
	}
}
