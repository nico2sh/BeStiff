using System;
using System.Collections;
using System.Text;

namespace Nuclex.Support.Licensing;

/// <summary>Typical license key with 5x5 alphanumerical characters</summary>
/// <remarks>
///   <para>
///     This class manages a license key like it is used in Microsoft products.
///     Althought it is probably not the exact same technique used by Microsoft,
///     the textual representation of the license keys looks identical,
///     eg. <code>O809J-RN5TD-IM3CU-4IG1O-O90X9</code>.
///   </para>
///   <para>
///     Available storage space is used efficiently and allows for up to four
///     32 bit integers to be stored within the key, that's enough for a full GUID.
///     The four integers can be modified directly, for example to store feature
///     lists, checksums or other data within the key.
///   </para>
/// </remarks>
public class LicenseKey
{
	/// <summary>Character used to delimit each 5 digit group in a license key</summary>
	/// <remarks>
	///   Required to be a char array because the .NET Compact Framework only provides
	///   an overload for char[] in the StringBuilder.Insert() method.
	/// </remarks>
	private static char[] keyDelimiter = new char[1] { '-' };

	/// <summary>Table with the individual characters in a key</summary>
	private static readonly string codeTable = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	/// <summary>Helper array containing the precalculated powers of two</summary>
	private static readonly uint[,] powersOfTwo = new uint[32, 2]
	{
		{ 0u, 1u },
		{ 0u, 2u },
		{ 0u, 4u },
		{ 0u, 8u },
		{ 0u, 16u },
		{ 0u, 32u },
		{ 0u, 64u },
		{ 0u, 128u },
		{ 0u, 256u },
		{ 0u, 512u },
		{ 0u, 1024u },
		{ 0u, 2048u },
		{ 0u, 4096u },
		{ 0u, 8192u },
		{ 0u, 16384u },
		{ 0u, 32768u },
		{ 0u, 65536u },
		{ 0u, 131072u },
		{ 0u, 262144u },
		{ 0u, 524288u },
		{ 0u, 1048576u },
		{ 0u, 2097152u },
		{ 0u, 4194304u },
		{ 0u, 8388608u },
		{ 0u, 16777216u },
		{ 0u, 33554432u },
		{ 0u, 67108864u },
		{ 0u, 134217728u },
		{ 0u, 268435456u },
		{ 0u, 536870912u },
		{ 0u, 1073741824u },
		{ 0u, 2147483648u }
	};

	/// <summary>Index list for rotating the bit arrays</summary>
	private static readonly byte[] shuffle = new byte[128]
	{
		99, 47, 19, 104, 40, 71, 35, 82, 88, 2,
		117, 118, 105, 42, 84, 48, 33, 54, 43, 27,
		78, 53, 61, 50, 109, 87, 69, 66, 25, 76,
		45, 14, 92, 16, 123, 98, 95, 37, 34, 8,
		1, 49, 20, 90, 15, 97, 22, 108, 5, 32,
		120, 106, 122, 70, 67, 55, 46, 89, 100, 0,
		26, 94, 121, 7, 56, 59, 103, 79, 107, 36,
		125, 119, 126, 44, 18, 93, 75, 116, 31, 9,
		73, 113, 3, 41, 124, 60, 77, 91, 28, 114,
		65, 12, 39, 127, 72, 17, 112, 21, 96, 111,
		83, 101, 85, 80, 23, 68, 57, 13, 4, 10,
		51, 63, 11, 30, 115, 102, 86, 81, 74, 110,
		62, 38, 29, 64, 52, 6, 24, 58
	};

	/// <summary>GUID in which the key is stored</summary>
	private Guid guid;

	/// <summary>Accesses the four integer values within a license key</summary>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   When the index lies outside of the key's fields
	/// </exception>
	public int this[int index]
	{
		get
		{
			if (index < 0 || index > 3)
			{
				throw new IndexOutOfRangeException("Index out of range");
			}
			return BitConverter.ToInt32(guid.ToByteArray(), index * 4);
		}
		set
		{
			if (index < 0 || index > 3)
			{
				throw new IndexOutOfRangeException("Index out of range");
			}
			byte[] array = guid.ToByteArray();
			Array.Copy(BitConverter.GetBytes(value), 0, array, index * 4, 4);
			guid = new Guid(array);
		}
	}

	/// <summary>Parses the license key contained in a string</summary>
	/// <param name="key">String containing a license key that is to be parsed</param>
	/// <returns>The license key parsed from provided string</returns>
	/// <exception cref="T:System.ArgumentException">
	///   When the provided string is not a license key
	/// </exception>
	public static LicenseKey Parse(string key)
	{
		key = key.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpper();
		if (key.Length != 25)
		{
			throw new ArgumentException("This is not a license key");
		}
		BitArray bitArray = new BitArray(128);
		uint num;
		for (int i = 0; i < 4; i++)
		{
			num = (uint)(codeTable.IndexOf(key[i * 6 + 5]) * 60466176 + codeTable.IndexOf(key[i * 6 + 4]) * 1679616 + codeTable.IndexOf(key[i * 6 + 3]) * 46656 + codeTable.IndexOf(key[i * 6 + 2]) * 1296 + codeTable.IndexOf(key[i * 6 + 1]) * 36 + codeTable.IndexOf(key[i * 6]));
			for (int j = 0; j < 31; j++)
			{
				bitArray[i * 31 + j] = (num & powersOfTwo[j, 1]) != 0;
			}
		}
		num = (uint)codeTable.IndexOf(key[24]);
		bitArray[124] = (num & powersOfTwo[4, 1]) != 0;
		bitArray[125] = (num & powersOfTwo[3, 1]) != 0;
		bitArray[126] = (num & powersOfTwo[2, 1]) != 0;
		bitArray[127] = (num & powersOfTwo[1, 1]) != 0;
		unmangle(bitArray);
		byte[] array = new byte[16];
		bitArray.CopyTo(array, 0);
		return new LicenseKey(new Guid(array));
	}

	/// <summary>Initializes a new, empty license key</summary>
	public LicenseKey()
		: this(Guid.Empty)
	{
	}

	/// <summary>Initializes the license key from a GUID</summary>
	/// <param name="source">GUID that is used to create the license key</param>
	public LicenseKey(Guid source)
	{
		guid = source;
	}

	/// <summary>Converts the license key into a GUID</summary>
	/// <returns>The GUID created from the license key</returns>
	public Guid ToGuid()
	{
		return guid;
	}

	/// <summary>Converts the license key into a byte array</summary>
	/// <returns>A byte array containing the converted license key</returns>
	public byte[] ToByteArray()
	{
		return guid.ToByteArray();
	}

	/// <summary>Converts the license key to a string</summary>
	/// <returns>A string containing the converted license key</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		BitArray bitArray = new BitArray(guid.ToByteArray());
		mangle(bitArray);
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 31; j++)
			{
				num |= (int)powersOfTwo[j, bitArray[i * 31 + j] ? 1 : 0];
			}
			for (int k = 0; k < 6; k++)
			{
				stringBuilder.Append(codeTable[num % 36]);
				num /= 36;
			}
		}
		stringBuilder.Append(codeTable[(int)(powersOfTwo[4, bitArray[124] ? 1 : 0] | powersOfTwo[3, bitArray[125] ? 1 : 0] | powersOfTwo[2, bitArray[126] ? 1 : 0] | powersOfTwo[1, bitArray[127] ? 1 : 0] | powersOfTwo[0, 1])]);
		stringBuilder.Insert(5, keyDelimiter, 0, 1);
		stringBuilder.Insert(11, keyDelimiter, 0, 1);
		stringBuilder.Insert(17, keyDelimiter, 0, 1);
		stringBuilder.Insert(23, keyDelimiter, 0, 1);
		return stringBuilder.ToString();
	}

	/// <summary>Mangles a bit array</summary>
	/// <param name="bits">Bit array that will be mangled</param>
	private static void mangle(BitArray bits)
	{
		BitArray bitArray = new BitArray(bits);
		for (int i = 0; i < bitArray.Length; i++)
		{
			bits[i] = bitArray[shuffle[i]];
			if ((i & 1) != 0)
			{
				bits[i] = !bits[i];
			}
		}
	}

	/// <summary>Unmangles a bit array</summary>
	/// <param name="bits">Bit array that will be unmangled</param>
	private static void unmangle(BitArray bits)
	{
		BitArray bitArray = new BitArray(bits);
		for (int i = 0; i < bitArray.Length; i++)
		{
			if ((i & 1) != 0)
			{
				bitArray[i] = !bitArray[i];
			}
			bits[shuffle[i]] = bitArray[i];
		}
	}
}
