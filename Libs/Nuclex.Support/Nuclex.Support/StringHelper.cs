using System;

namespace Nuclex.Support;

/// <summary>Helper routines for working with strings</summary>
public static class StringHelper
{
	/// <summary>
	///   Searches for the first occurence of a character other than the characters
	///   listed in the <paramref name="anyNotOf" /> parameter
	/// </summary>
	/// <param name="haystack">String that will be scanned in</param>
	/// <param name="anyNotOf">Characters to not look for in the scanned string</param>
	/// <returns>
	///   The index of the first occurence of a character not in the
	///   <paramref name="anyNotOf" /> array or -1 if all characters in the string were
	///   present in the <paramref name="anyNotOf" /> array.
	/// </returns>
	public static int IndexNotOfAny(this string haystack, char[] anyNotOf)
	{
		return haystack.IndexNotOfAny(anyNotOf, 0, haystack.Length);
	}

	/// <summary>
	///   Searches for the first occurence of a character other than the characters
	///   listed in the <paramref name="anyNotOf" /> parameter
	/// </summary>
	/// <param name="haystack">String that will be scanned in</param>
	/// <param name="anyNotOf">Characters to not look for in the scanned string</param>
	/// <param name="startIndex">
	///   Index of the character in the haystack at which to start scanning
	/// </param>
	/// <returns>
	///   The index of the first occurence of a character not in the
	///   <paramref name="anyNotOf" /> array or -1 if all characters in the string were
	///   present in the <paramref name="anyNotOf" /> array.
	/// </returns>
	public static int IndexNotOfAny(this string haystack, char[] anyNotOf, int startIndex)
	{
		return haystack.IndexNotOfAny(anyNotOf, startIndex, haystack.Length - startIndex);
	}

	/// <summary>
	///   Searches for the first occurence of a character other than the characters
	///   listed in the <paramref name="anyNotOf" /> parameter
	/// </summary>
	/// <param name="haystack">String that will be scanned in</param>
	/// <param name="anyNotOf">Characters to not look for in the scanned string</param>
	/// <param name="startIndex">
	///   Index of the character in the haystack at which to start scanning
	/// </param>
	/// <param name="count">Number of characters in the haystack to scan</param>
	/// <returns>
	///   The index of the first occurence of a character not in the
	///   <paramref name="anyNotOf" /> array or -1 if all characters in the string were
	///   present in the <paramref name="anyNotOf" /> array.
	/// </returns>
	public static int IndexNotOfAny(this string haystack, char[] anyNotOf, int startIndex, int count)
	{
		int count2 = anyNotOf.Length;
		count += startIndex;
		while (startIndex < count)
		{
			char value = haystack[startIndex];
			int num = Array.IndexOf(anyNotOf, value, 0, count2);
			if (num == -1)
			{
				return startIndex;
			}
			startIndex++;
		}
		return -1;
	}

	/// <summary>
	///   Searches backwards for the first occurence of a character other than the
	///   characters listed in the <paramref name="anyNotOf" /> parameter
	/// </summary>
	/// <param name="haystack">String that will be scanned in</param>
	/// <param name="anyNotOf">Characters to not look for in the scanned string</param>
	/// <returns>
	///   The index of the first occurence of a character not in the
	///   <paramref name="anyNotOf" /> array or -1 if all characters in the string were
	///   present in the <paramref name="anyNotOf" /> array.
	/// </returns>
	public static int LastIndexNotOfAny(this string haystack, char[] anyNotOf)
	{
		return haystack.LastIndexNotOfAny(anyNotOf, haystack.Length - 1, haystack.Length);
	}

	/// <summary>
	///   Searches backwards for the first occurence of a character other than the
	///   characters listed in the <paramref name="anyNotOf" /> parameter
	/// </summary>
	/// <param name="haystack">String that will be scanned in</param>
	/// <param name="anyNotOf">Characters to not look for in the scanned string</param>
	/// <param name="startIndex">
	///   Index of the character in the haystack at which to start scanning
	/// </param>
	/// <returns>
	///   The index of the first occurence of a character not in the
	///   <paramref name="anyNotOf" /> array or -1 if all characters in the string were
	///   present in the <paramref name="anyNotOf" /> array.
	/// </returns>
	public static int LastIndexNotOfAny(this string haystack, char[] anyNotOf, int startIndex)
	{
		return haystack.LastIndexNotOfAny(anyNotOf, startIndex, startIndex + 1);
	}

	/// <summary>
	///   Searches backwards for the first occurence of a character other than the
	///   characters listed in the <paramref name="anyNotOf" /> parameter
	/// </summary>
	/// <param name="haystack">String that will be scanned in</param>
	/// <param name="anyNotOf">Characters to not look for in the scanned string</param>
	/// <param name="startIndex">
	///   Index of the character in the haystack at which to start scanning
	/// </param>
	/// <param name="count">Number of characters in the haystack to scan</param>
	/// <returns>
	///   The index of the first occurence of a character not in the
	///   <paramref name="anyNotOf" /> array or -1 if all characters in the string were
	///   present in the <paramref name="anyNotOf" /> array.
	/// </returns>
	public static int LastIndexNotOfAny(this string haystack, char[] anyNotOf, int startIndex, int count)
	{
		int count2 = anyNotOf.Length;
		count = startIndex - count;
		while (startIndex > count)
		{
			char value = haystack[startIndex];
			int num = Array.IndexOf(anyNotOf, value, 0, count2);
			if (num == -1)
			{
				return startIndex;
			}
			startIndex--;
		}
		return -1;
	}
}
