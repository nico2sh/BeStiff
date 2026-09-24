using System;
using System.Runtime.InteropServices;

namespace Nuclex.Support;

/// <summary>Helper routines for working with floating point numbers</summary>
/// <remarks>
///   <para>
///     The floating point comparison code is based on this excellent article:
///     http://www.cygnus-software.com/papers/comparingfloats/comparingfloats.htm
///   </para>
///   <para>
///     "ULP" means Unit in the Last Place and in the context of this library refers to
///     the distance between two adjacent floating point numbers. IEEE floating point
///     numbers can only represent a finite subset of natural numbers, with greater
///     accuracy for smaller numbers and lower accuracy for very large numbers.
///   </para>
///   <para>
///     If a comparison is allowed "2 ulps" of deviation, that means the values are
///     allowed to deviate by up to 2 adjacent floating point values, which might be
///     as low as 0.0000001 for small numbers or as high as 10.0 for large numbers.
///   </para>
/// </remarks>
public static class FloatHelper
{
	/// <summary>Union of a floating point variable and an integer</summary>
	[StructLayout(LayoutKind.Explicit)]
	private struct FloatIntUnion
	{
		/// <summary>The union's value as a floating point variable</summary>
		[FieldOffset(0)]
		public float Float;

		/// <summary>The union's value as an integer</summary>
		[FieldOffset(0)]
		public int Int;

		/// <summary>The union's value as an unsigned integer</summary>
		[FieldOffset(0)]
		public uint UInt;
	}

	/// <summary>Union of a double precision floating point variable and a long</summary>
	[StructLayout(LayoutKind.Explicit)]
	private struct DoubleLongUnion
	{
		/// <summary>The union's value as a double precision floating point variable</summary>
		[FieldOffset(0)]
		public double Double;

		/// <summary>The union's value as a long</summary>
		[FieldOffset(0)]
		public long Long;

		/// <summary>The union's value as an unsigned long</summary>
		[FieldOffset(0)]
		public ulong ULong;
	}

	/// <summary>Compares two floating point values for equality</summary>
	/// <param name="left">First floating point value to be compared</param>
	/// <param name="right">Second floating point value t be compared</param>
	/// <param name="maxUlps">
	///   Maximum number of representable floating point values that are allowed to
	///   be between the left and the right floating point values
	/// </param>
	/// <returns>True if both numbers are equal or close to being equal</returns>
	/// <remarks>
	///   <para>
	///     Floating point values can only represent a finite subset of natural numbers.
	///     For example, the values 2.00000000 and 2.00000024 can be stored in a float,
	///     but nothing inbetween them.
	///   </para>
	///   <para>
	///     This comparison will count how many possible floating point values are between
	///     the left and the right number. If the number of possible values between both
	///     numbers is less than or equal to maxUlps, then the numbers are considered as
	///     being equal.
	///   </para>
	///   <para>
	///     Implementation partially follows the code outlined here (link now defunct):
	///     http://www.anttirt.net/2007/08/19/proper-floating-point-comparisons/
	///   </para>
	/// </remarks>
	public static bool AreAlmostEqual(float left, float right, int maxUlps)
	{
		FloatIntUnion floatIntUnion = default(FloatIntUnion);
		FloatIntUnion floatIntUnion2 = default(FloatIntUnion);
		floatIntUnion.Float = left;
		floatIntUnion2.Float = right;
		uint num = floatIntUnion.UInt >> 31;
		uint num2 = floatIntUnion2.UInt >> 31;
		uint num3 = (uint)(int.MinValue - (int)floatIntUnion.UInt) & num;
		floatIntUnion.UInt = num3 | (floatIntUnion.UInt & ~num);
		uint num4 = (uint)(int.MinValue - (int)floatIntUnion2.UInt) & num2;
		floatIntUnion2.UInt = num4 | (floatIntUnion2.UInt & ~num2);
		return Math.Abs(floatIntUnion.Int - floatIntUnion2.Int) <= maxUlps;
	}

	/// <summary>Compares two double precision floating point values for equality</summary>
	/// <param name="left">First double precision floating point value to be compared</param>
	/// <param name="right">Second double precision floating point value t be compared</param>
	/// <param name="maxUlps">
	///   Maximum number of representable double precision floating point values that are
	///   allowed to be between the left and the right double precision floating point values
	/// </param>
	/// <returns>True if both numbers are equal or close to being equal</returns>
	/// <remarks>
	///   <para>
	///     Double precision floating point values can only represent a limited series of
	///     natural numbers. For example, the values 2.0000000000000000 and 2.0000000000000004
	///     can be stored in a double, but nothing inbetween them.
	///   </para>
	///   <para>
	///     This comparison will count how many possible double precision floating point
	///     values are between the left and the right number. If the number of possible
	///     values between both numbers is less than or equal to maxUlps, then the numbers
	///     are considered as being equal.
	///   </para>
	///   <para>
	///     Implementation partially follows the code outlined here:
	///     http://www.anttirt.net/2007/08/19/proper-floating-point-comparisons/
	///   </para>
	/// </remarks>
	public static bool AreAlmostEqual(double left, double right, long maxUlps)
	{
		DoubleLongUnion doubleLongUnion = default(DoubleLongUnion);
		DoubleLongUnion doubleLongUnion2 = default(DoubleLongUnion);
		doubleLongUnion.Double = left;
		doubleLongUnion2.Double = right;
		ulong num = doubleLongUnion.ULong >> 63;
		ulong num2 = doubleLongUnion2.ULong >> 63;
		ulong num3 = (ulong)(long.MinValue - (long)doubleLongUnion.ULong) & num;
		doubleLongUnion.ULong = num3 | (doubleLongUnion.ULong & ~num);
		ulong num4 = (ulong)(long.MinValue - (long)doubleLongUnion2.ULong) & num2;
		doubleLongUnion2.ULong = num4 | (doubleLongUnion2.ULong & ~num2);
		return Math.Abs(doubleLongUnion.Long - doubleLongUnion2.Long) <= maxUlps;
	}

	/// <summary>
	///   Reinterprets the memory contents of a floating point value as an integer value
	/// </summary>
	/// <param name="value">
	///   Floating point value whose memory contents to reinterpret
	/// </param>
	/// <returns>
	///   The memory contents of the floating point value interpreted as an integer
	/// </returns>
	public static int ReinterpretAsInt(this float value)
	{
		FloatIntUnion floatIntUnion = new FloatIntUnion
		{
			Float = value
		};
		return floatIntUnion.Int;
	}

	/// <summary>
	///   Reinterprets the memory contents of a double precision floating point
	///   value as an integer value
	/// </summary>
	/// <param name="value">
	///   Double precision floating point value whose memory contents to reinterpret
	/// </param>
	/// <returns>
	///   The memory contents of the double precision floating point value
	///   interpreted as an integer
	/// </returns>
	public static long ReinterpretAsLong(this double value)
	{
		DoubleLongUnion doubleLongUnion = new DoubleLongUnion
		{
			Double = value
		};
		return doubleLongUnion.Long;
	}

	/// <summary>
	///   Reinterprets the memory contents of an integer as a floating point value
	/// </summary>
	/// <param name="value">Integer value whose memory contents to reinterpret</param>
	/// <returns>
	///   The memory contents of the integer value interpreted as a floating point value
	/// </returns>
	public static float ReinterpretAsFloat(this int value)
	{
		FloatIntUnion floatIntUnion = new FloatIntUnion
		{
			Int = value
		};
		return floatIntUnion.Float;
	}

	/// <summary>
	///   Reinterprets the memory contents of an integer value as a double precision
	///   floating point value
	/// </summary>
	/// <param name="value">Integer whose memory contents to reinterpret</param>
	/// <returns>
	///   The memory contents of the integer interpreted as a double precision
	///   floating point value
	/// </returns>
	public static double ReinterpretAsDouble(this long value)
	{
		DoubleLongUnion doubleLongUnion = new DoubleLongUnion
		{
			Long = value
		};
		return doubleLongUnion.Double;
	}
}
