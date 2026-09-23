using System;
using System.ComponentModel;
using System.Globalization;

namespace ProjectMercury
{
	/// <summary>
	/// Defines a floating point object which has a definable random variation.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.VariableFloatTypeConverter, ProjectMercury.Design")]
	public struct VariableFloat : IEquatable<VariableFloat>
	{
		/// <summary>
		/// The base value for the VariableFloat.
		/// </summary>
		public float Value;

		/// <summary>
		/// The range of the random variation around the base value.
		/// </summary>
		public float Variation;

		/// <summary>
		/// Samples the VariableFloat.
		/// </summary>
		/// <returns>A randomised float value.</returns>
		public float Sample()
		{
			if (Calculator.Abs(Variation) <= float.Epsilon)
			{
				return Value;
			}
			return RandomHelper.NextFloat(Value - Variation, Value + Variation);
		}

		/// <summary>
		/// Samples the VariableFloat, and clamps the result to be within the specified range.
		/// </summary>
		/// <param name="clampRange">The range of allowable values.</param>
		/// <returns>A randomised float value.</returns>
		public float Sample(Range clampRange)
		{
			if (Calculator.Abs(Variation) <= float.Epsilon)
			{
				return Calculator.Clamp(Value, clampRange);
			}
			float value = RandomHelper.NextFloat(Value - Variation, Value + Variation);
			return Calculator.Clamp(value, clampRange);
		}

		/// <summary>
		/// Implicit cast operator from float to VariableFloat.
		/// </summary>
		public static implicit operator VariableFloat(float value)
		{
			return new VariableFloat
			{
				Value = value,
				Variation = 0f
			};
		}

		/// <summary>
		/// Implicit cast operation from VariableFloat to float.
		/// </summary>
		public static implicit operator float(VariableFloat value)
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
			if (obj is VariableFloat)
			{
				return Equals((VariableFloat)obj);
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
		public bool Equals(VariableFloat other)
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
		/// Returns a <see cref="T:System.String" /> representation of the object.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> representation of the object.
		/// </returns>
		public override string ToString()
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			return $"{{Value:{Value.ToString(currentCulture)}, Variation:{Variation.ToString(currentCulture)}}}";
		}
	}
}
