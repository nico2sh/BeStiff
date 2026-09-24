namespace Nuclex.Support;

/// <summary>Helper methods for working with integer types</summary>
public static class IntegerHelper
{
	/// <summary>Returns the next highest power of 2 from the specified value</summary>
	/// <param name="value">Value of which to return the next highest power of 2</param>
	/// <returns>The next highest power of 2 to the value</returns>
	public static long NextPowerOf2(this long value)
	{
		return (long)((ulong)value).NextPowerOf2();
	}

	/// <summary>Returns the next highest power of 2 from the specified value</summary>
	/// <param name="value">Value of which to return the next highest power of 2</param>
	/// <returns>The next highest power of 2 to the value</returns>
	public static ulong NextPowerOf2(this ulong value)
	{
		if (value == 0)
		{
			return 1uL;
		}
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		value |= value >> 32;
		value++;
		return value;
	}

	/// <summary>Returns the next highest power of 2 from the specified value</summary>
	/// <param name="value">Value of which to return the next highest power of 2</param>
	/// <returns>The next highest power of 2 to the value</returns>
	public static int NextPowerOf2(this int value)
	{
		return (int)((uint)value).NextPowerOf2();
	}

	/// <summary>Returns the next highest power of 2 from the specified value</summary>
	/// <param name="value">Value of which to return the next highest power of 2</param>
	/// <returns>The next highest power of 2 to the value</returns>
	public static uint NextPowerOf2(this uint value)
	{
		if (value == 0)
		{
			return 1u;
		}
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		value++;
		return value;
	}

	/// <summary>Returns the number of bits set in an </summary>
	/// <param name="value">Value whose bits will be counted</param>
	/// <returns>The number of bits set in the integer</returns>
	public static int CountBits(this int value)
	{
		return ((uint)value).CountBits();
	}

	/// <summary>Returns the number of bits set in an unsigned integer</summary>
	/// <param name="value">Value whose bits will be counted</param>
	/// <returns>The number of bits set in the unsigned integer</returns>
	/// <remarks>
	///   Based on a trick revealed here:
	///   http://stackoverflow.com/questions/109023
	/// </remarks>
	public static int CountBits(this uint value)
	{
		value -= (value >> 1) & 0x55555555;
		value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
		return (int)(((value + (value >> 4)) & 0xF0F0F0F) * 16843009 >> 24);
	}

	/// <summary>Returns the number of bits set in a long integer</summary>
	/// <param name="value">Value whose bits will be counted</param>
	/// <returns>The number of bits set in the long integer</returns>
	public static int CountBits(this long value)
	{
		return ((ulong)value).CountBits();
	}

	/// <summary>Returns the number of bits set in an unsigned long integer</summary>
	/// <param name="value">Value whose bits will be counted</param>
	/// <returns>The number of bits set in the unsigned long integer</returns>
	/// <remarks>
	///   Based on a trick revealed here:
	///   http://stackoverflow.com/questions/2709430
	/// </remarks>
	public static int CountBits(this ulong value)
	{
		value -= (value >> 1) & 0x5555555555555555L;
		value = (value & 0x3333333333333333L) + ((value >> 2) & 0x3333333333333333L);
		return (int)(((value + (value >> 4)) & 0xF0F0F0F0F0F0F0FL) * 72340172838076673L >> 56);
	}
}
