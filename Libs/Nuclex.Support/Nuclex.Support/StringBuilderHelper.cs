using System;
using System.Text;

namespace Nuclex.Support;

/// <summary>Contains helper methods for the string builder class</summary>
public static class StringBuilderHelper
{
	/// <summary>Predefined unicode characters for the numbers 0 to 9</summary>
	private static readonly char[] numbers = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

	/// <summary>Clears the contents of a string builder</summary>
	/// <param name="builder">String builder that will be cleared</param>
	public static void Clear(this StringBuilder builder)
	{
		builder.Remove(0, builder.Length);
	}

	/// <summary>
	///   Appends an integer to a string builder without generating garbage
	/// </summary>
	/// <param name="builder">String builder to which an integer will be appended</param>
	/// <param name="value">Byte that will be appended to the string builder</param>
	/// <remarks>
	///   The normal StringBuilder.Append() method generates garbage when converting
	///   integer arguments whereas this method will avoid any garbage, albeit probably
	///   with a small performance impact compared to the built-in method.
	/// </remarks>
	public static void Append(StringBuilder builder, byte value)
	{
		recursiveAppend(builder, value);
	}

	/// <summary>
	///   Appends an integer to a string builder without generating garbage
	/// </summary>
	/// <param name="builder">String builder to which an integer will be appended</param>
	/// <param name="value">Integer that will be appended to the string builder</param>
	/// <remarks>
	///   The normal StringBuilder.Append() method generates garbage when converting
	///   integer arguments whereas this method will avoid any garbage, albeit probably
	///   with a small performance impact compared to the built-in method.
	/// </remarks>
	public static void Append(StringBuilder builder, int value)
	{
		if (value < 0)
		{
			builder.Append('-');
			recursiveAppend(builder, -value);
		}
		else
		{
			recursiveAppend(builder, value);
		}
	}

	/// <summary>
	///   Appends an long integer to a string builder without generating garbage
	/// </summary>
	/// <param name="builder">String builder to which an integer will be appended</param>
	/// <param name="value">Long integer that will be appended to the string builder</param>
	/// <remarks>
	///   The normal StringBuilder.Append() method generates garbage when converting
	///   integer arguments whereas this method will avoid any garbage, albeit probably
	///   with a small performance impact compared to the built-in method.
	/// </remarks>
	public static void Append(StringBuilder builder, long value)
	{
		if (value < 0)
		{
			builder.Append('-');
			recursiveAppend(builder, -value);
		}
		else
		{
			recursiveAppend(builder, value);
		}
	}

	/// <summary>
	///   Appends a floating point value to a string builder without generating garbage
	/// </summary>
	/// <param name="builder">String builder the value will be appended to</param>
	/// <param name="value">Value that will be appended to the string builder</param>
	/// <returns>Whether the value was inside the algorithm's supported range</returns>
	/// <remarks>
	///   Uses an algorithm that covers the sane range of possible values but will
	///   fail to render extreme values, NaNs and infinity. In these cases, false
	///   is returned and the traditional double.ToString() method can be used.
	/// </remarks>
	public static bool Append(StringBuilder builder, float value)
	{
		return Append(builder, value, int.MaxValue);
	}

	/// <summary>
	///   Appends a floating point value to a string builder without generating garbage
	/// </summary>
	/// <param name="builder">String builder the value will be appended to</param>
	/// <param name="value">Value that will be appended to the string builder</param>
	/// <param name="decimalPlaces">Maximum number of decimal places to display</param>
	/// <returns>Whether the value was inside the algorithm's supported range</returns>
	/// <remarks>
	///   Uses an algorithm that covers the sane range of possible values but will
	///   fail to render extreme values, NaNs and infinity. In these cases, false
	///   is returned and the traditional double.ToString() method can be used.
	/// </remarks>
	public static bool Append(StringBuilder builder, float value, int decimalPlaces)
	{
		int num = value.ReinterpretAsInt();
		int num2 = ((num >> 23) & 0xFF) - 127;
		int num3 = (num & 0xFFFFFF) | 0x800000;
		int num4;
		int num5;
		if (num2 >= 0)
		{
			if (num2 >= 23)
			{
				if (num2 >= 31)
				{
					return false;
				}
				num4 = num3 << num2 - 23;
				num5 = 0;
			}
			else
			{
				num4 = num3 >> 23 - num2;
				num5 = (num3 << num2 + 1) & 0xFFFFFF;
			}
		}
		else
		{
			if (num2 < -23)
			{
				return false;
			}
			num4 = 0;
			num5 = (num3 & 0xFFFFFF) >> -(num2 + 1);
		}
		if (num < 0)
		{
			builder.Append('-');
		}
		if (num4 == 0)
		{
			builder.Append('0');
		}
		else
		{
			recursiveAppend(builder, num4);
		}
		if (decimalPlaces > 0)
		{
			builder.Append('.');
			if (num5 == 0)
			{
				builder.Append('0');
			}
			else
			{
				while (num5 != 0)
				{
					num5 *= 10;
					int num6 = num5 >> 24;
					builder.Append(numbers[num6]);
					num5 &= 0xFFFFFF;
					decimalPlaces--;
					if (decimalPlaces == 0)
					{
						break;
					}
				}
			}
		}
		return true;
	}

	/// <summary>
	///   Appends a double precision floating point value to a string builder
	///   without generating garbage
	/// </summary>
	/// <param name="builder">String builder the value will be appended to</param>
	/// <param name="value">Value that will be appended to the string builder</param>
	/// <returns>Whether the value was inside the algorithm's supported range</returns>
	/// <remarks>
	///   Uses an algorithm that covers the sane range of possible values but will
	///   fail to render extreme values, NaNs and infinity. In these cases, false
	///   is returned and the traditional double.ToString() method can be used.
	/// </remarks>
	public static bool Append(StringBuilder builder, double value)
	{
		return Append(builder, value, int.MaxValue);
	}

	/// <summary>
	///   Appends a double precision floating point value to a string builder
	///   without generating garbage
	/// </summary>
	/// <param name="builder">String builder the value will be appended to</param>
	/// <param name="value">Value that will be appended to the string builder</param>
	/// <param name="decimalPlaces">Maximum number of decimal places to display</param>
	/// <returns>Whether the value was inside the algorithm's supported range</returns>
	/// <remarks>
	///   Uses an algorithm that covers the sane range of possible values but will
	///   fail to render extreme values, NaNs and infinity. In these cases, false
	///   is returned and the traditional double.ToString() method can be used.
	/// </remarks>
	public static bool Append(StringBuilder builder, double value, int decimalPlaces)
	{
		long num = value.ReinterpretAsLong();
		long num2 = ((num >> 52) & 0x7FF) - 1023;
		long num3 = (num & 0x1FFFFFFFFFFFFFL) | 0x10000000000000L;
		long num4;
		long num5;
		if (num2 >= 0)
		{
			if (num2 >= 52)
			{
				if (num2 >= 63)
				{
					return false;
				}
				num4 = num3 << (int)(num2 - 52);
				num5 = 0L;
			}
			else
			{
				num4 = num3 >> (int)(52 - num2);
				num5 = (num3 << (int)(num2 + 1)) & 0x1FFFFFFFFFFFFFL;
			}
		}
		else
		{
			if (num2 < -52)
			{
				return false;
			}
			num4 = 0L;
			num5 = (num3 & 0x1FFFFFFFFFFFFFL) >> -(int)(num2 + 1);
		}
		if (num < 0)
		{
			builder.Append('-');
		}
		if (num4 == 0)
		{
			builder.Append('0');
		}
		else
		{
			recursiveAppend(builder, num4);
		}
		if (decimalPlaces > 0)
		{
			builder.Append('.');
			if (num5 == 0)
			{
				builder.Append('0');
			}
			else
			{
				while (num5 != 0)
				{
					num5 *= 10;
					long num6 = num5 >> 53;
					builder.Append(numbers[num6]);
					num5 &= 0x1FFFFFFFFFFFFFL;
					decimalPlaces--;
					if (decimalPlaces == 0)
					{
						break;
					}
				}
			}
		}
		return true;
	}

	/// <summary>Recursively appends a number's characters to a string builder</summary>
	/// <param name="builder">String builder the number will be appended to</param>
	/// <param name="remaining">Remaining digits that will be recursively processed</param>
	private static void recursiveAppend(StringBuilder builder, int remaining)
	{
		int result;
		int num = Math.DivRem(remaining, 10, out result);
		if (num > 0)
		{
			recursiveAppend(builder, num);
		}
		builder.Append(numbers[result]);
	}

	/// <summary>Recursively appends a number's characters to a string builder</summary>
	/// <param name="builder">String builder the number will be appended to</param>
	/// <param name="remaining">Remaining digits that will be recursively processed</param>
	private static void recursiveAppend(StringBuilder builder, long remaining)
	{
		long result;
		long num = Math.DivRem(remaining, 10L, out result);
		if (num > 0)
		{
			recursiveAppend(builder, num);
		}
		builder.Append(numbers[result]);
	}
}
