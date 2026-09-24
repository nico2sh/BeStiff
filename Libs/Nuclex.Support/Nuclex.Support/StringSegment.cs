using System;

namespace Nuclex.Support;

/// <summary>Delimits a section of a string</summary>
/// <remarks>
///   <para>
///     The design of this class pretty much mirrors that of the
///     <see cref="T:System.ArraySegment" /> class found in the .NET framework, but is
///     specialized to be used for strings, which can not be expressed as arrays but
///     share a lot of the characteristics of an array.
///   </para>
///   <para>
///     In certain situations, passing a StringSegment instead of the the actual
///     section from a string is useful. For example, the caller might want to know
///     from which index of the original string the substring was taken. Used internally
///     in parsers, it can also prevent needless string copying and garbage generation.
///   </para>
/// </remarks>
[Serializable]
public struct StringSegment
{
	/// <summary>String wrapped by the string segment</summary>
	private string text;

	/// <summary>Offset in the original string the segment begins at</summary>
	private int offset;

	/// <summary>Number of characters in the segment</summary>
	private int count;

	/// <summary>
	///   Gets the original string containing the range of elements that the string
	///   segment delimits
	/// </summary>
	/// <returns>
	///   The original array that was passed to the constructor, and that contains the range
	///   delimited by the <see cref="T:Nuclex.Support.StringSegment" />
	/// </returns>
	public string Text => text;

	/// <summary>
	///   Gets the position of the first element in the range delimited by the array segment,
	///   relative to the start of the original array
	/// </summary>
	/// <returns>
	///   The position of the first element in the range delimited by the
	///   <see cref="T:Nuclex.Support.StringSegment" />, relative to the start of the original array
	/// </returns>
	public int Offset => offset;

	/// <summary>
	///   Gets the number of elements in the range delimited by the array segment
	/// </summary>
	/// <returns>
	///   The number of elements in the range delimited by the <see cref="T:Nuclex.Support.StringSegment" />
	/// </returns>
	public int Count => count;

	/// <summary>
	///   Initializes a new instance of the <see cref="T:Nuclex.Support.StringSegment" /> class that delimits
	///   all the elements in the specified string
	/// </summary>
	/// <param name="text">String that will be wrapped</param>
	/// <exception cref="T:System.ArgumentNullException">String is null</exception>
	public StringSegment(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text", "Text must not be null");
		}
		this.text = text;
		offset = 0;
		count = text.Length;
	}

	/// <summary>
	///   Initializes a new instance of the <see cref="T:Nuclex.Support.StringSegment" /> class that delimits
	///   the specified range of the elements in the specified string
	/// </summary>
	/// <param name="text">The string containing the range of elements to delimit</param>
	/// <param name="offset">The zero-based index of the first element in the range</param>
	/// <param name="count">The number of elements in the range</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Offset or count is less than 0
	/// </exception>
	/// <exception cref="T:System.ArgumentException">
	///   Offset and count do not specify a valid range in array
	/// </exception>
	/// <exception cref="T:System.ArgumentNullException">String is null</exception>
	public StringSegment(string text, int offset, int count)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument out of range, non-negative number required");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Argument out of range, non-negative number required");
		}
		if (count > text.Length - offset)
		{
			throw new ArgumentException("Invalid argument, specified offset and count exceed string length");
		}
		this.text = text;
		this.offset = offset;
		this.count = count;
	}

	/// <summary>Returns the hash code for the current instance</summary>
	/// <returns>A 32-bit signed integer hash code</returns>
	public override int GetHashCode()
	{
		return text.GetHashCode() ^ offset ^ count;
	}

	/// <summary>
	///   Determines whether the specified object is equal to the current instance
	/// </summary>
	/// <returns>
	///   True if the specified object is a <see cref="T:Nuclex.Support.StringSegment" /> structure and is
	///   equal to the current instance; otherwise, false
	/// </returns>
	/// <param name="other">The object to be compared with the current instance</param>
	public override bool Equals(object other)
	{
		if (other is StringSegment)
		{
			return Equals((StringSegment)other);
		}
		return false;
	}

	/// <summary>
	///   Determines whether the specified <see cref="T:Nuclex.Support.StringSegment" /> structure is equal
	///   to the current instance
	/// </summary>
	/// <returns>
	///   True if the specified <see cref="T:Nuclex.Support.StringSegment" /> structure is equal to the
	///   current instance; otherwise, false
	/// </returns>
	/// <param name="other">
	///   The <see cref="T:Nuclex.Support.StringSegment" /> structure to be compared with the current instance
	/// </param>
	public bool Equals(StringSegment other)
	{
		if (other.text == text && other.offset == offset)
		{
			return other.count == count;
		}
		return false;
	}

	/// <summary>
	///   Indicates whether two <see cref="T:Nuclex.Support.StringSegment" /> structures are equal
	/// </summary>
	/// <returns>True if a is equal to b; otherwise, false</returns>
	/// <param name="left">
	///   The <see cref="T:Nuclex.Support.StringSegment" /> structure on the left side of the
	///   equality operator
	/// </param>
	/// <param name="right">
	///   The <see cref="T:Nuclex.Support.StringSegment" /> structure on the right side of the
	///   equality operator
	/// </param>
	public static bool operator ==(StringSegment left, StringSegment right)
	{
		return left.Equals(right);
	}

	/// <summary>
	///   Indicates whether two <see cref="T:Nuclex.Support.StringSegment" /> structures are unequal
	/// </summary>
	/// <returns>True if a is not equal to b; otherwise, false</returns>
	/// <param name="left">
	///   The <see cref="T:Nuclex.Support.StringSegment" /> structure on the left side of the
	///   inequality operator
	/// </param>
	/// <param name="right">
	///   The <see cref="T:Nuclex.Support.StringSegment" /> structure on the right side of the
	///   inequality operator
	/// </param>
	public static bool operator !=(StringSegment left, StringSegment right)
	{
		return !(left == right);
	}

	/// <summary>Returns a string representation of the string segment</summary>
	/// <returns>The string representation of the string segment</returns>
	public override string ToString()
	{
		return text.Substring(offset, count);
	}
}
