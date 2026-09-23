using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace ProjectMercury
{
	/// <summary>
	/// Defines a Vector3 object which has a definable random variation.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.VariableFloat3TypeConverter, ProjectMercury.Design")]
	public struct VariableFloat3 : IEquatable<VariableFloat3>
	{
		/// <summary>
		/// The base value of the VariableFloat3.
		/// </summary>
		public Vector3 Value;

		/// <summary>
		/// The range of the random variation around the base value.
		/// </summary>
		public Vector3 Variation;

		/// <summary>
		/// Samples the VariableFloat3.
		/// </summary>
		/// <returns>A randomised Vector3 value.</returns>
		public Vector3 Sample()
		{
			return new Vector3
			{
				X = RandomHelper.Variation(Value.X, Variation.X),
				Y = RandomHelper.Variation(Value.Y, Variation.Y),
				Z = RandomHelper.Variation(Value.Z, Variation.Z)
			};
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:Microsoft.Xna.Framework.Vector3" /> to <see cref="T:ProjectMercury.VariableFloat3" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator VariableFloat3(Vector3 value)
		{
			return new VariableFloat3
			{
				Value = value,
				Variation = Vector3.Zero
			};
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:ProjectMercury.VariableFloat3" /> to <see cref="T:Microsoft.Xna.Framework.Vector3" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Vector3(VariableFloat3 value)
		{
			return value.Sample();
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is VariableFloat3)
			{
				return Equals((VariableFloat3)obj);
			}
			return false;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(VariableFloat3 other)
		{
			if (Value == other.Value)
			{
				return Variation == other.Variation;
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode() + Variation.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			_ = CultureInfo.CurrentCulture;
			return $"{{Value:{Value}, Variation:{Variation}}}";
		}
	}
}
