using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Nuclex.UserInterface;

/// <summary>Two-dimensional rectangle using floating point coordinates</summary>
[Serializable]
public struct RectangleF : IEquatable<RectangleF>
{
	/// <summary>Specifies the x-coordinate of the rectangle</summary>
	public float X;

	/// <summary>Specifies the y-coordinate of the rectangle</summary>
	public float Y;

	/// <summary>Specifies the width of the rectangle</summary>
	public float Width;

	/// <summary>Specifies the height of the rectangle</summary>
	public float Height;

	/// <summary>An empty rectangle</summary>
	private static RectangleF empty = default(RectangleF);

	/// <summary>Returns the x-coordinate of the left side of the rectangle</summary>
	/// <returns>The x-coordinate of the left side of the rectangle</returns>
	public float Left => X;

	/// <summary>Returns the x-coordinate of the right side of the rectangle</summary>
	/// <returns>The x-coordinate of the right side of the rectangle</returns>
	public float Right => X + Width;

	/// <summary>Returns the y-coordinate of the top of the rectangle</summary>
	/// <returns>The y-coordinate of the top of the rectangle</returns>
	public float Top => Y;

	/// <summary>Returns the y-coordinate of the bottom of the rectangle</summary>
	/// <returns>The y-coordinate of the bottom of the rectangle</returns>
	public float Bottom => Y + Height;

	/// <summary>Returns a Rectangle with all of its values set to zero</summary>
	/// <returns>An empty Rectangle</returns>
	public static RectangleF Empty => empty;

	/// <summary>Initializes a floating point rectangle</summary>
	/// <param name="x">The x-coordinate of the rectangle's lower right corner</param>
	/// <param name="y">The y-coordinate of the rectangle's lower right corner</param>
	/// <param name="width">Width of the rectangle</param>
	/// <param name="height">Height of the rectangle</param>
	public RectangleF(float x, float y, float width, float height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	/// <summary>Changes the position of the Rectangle</summary>
	/// <param name="amount">The values to adjust the position of the rectangle by</param>
	public void Offset(Vector2 amount)
	{
		Offset(amount.X, amount.Y);
	}

	/// <summary>Changes the position of the Rectangle</summary>
	/// <param name="offsetX">Change in the x-position</param>
	/// <param name="offsetY">Change in the y-position</param>
	public void Offset(float offsetX, float offsetY)
	{
		X += offsetX;
		Y += offsetY;
	}

	/// <summary>
	///   Pushes the edges of the Rectangle out by the horizontal and
	///   vertical values specified
	/// </summary>
	/// <param name="horizontalAmount">Value to push the sides out by</param>
	/// <param name="verticalAmount">Value to push the top and bottom out by</param>
	public void Inflate(float horizontalAmount, float verticalAmount)
	{
		X -= horizontalAmount;
		Y -= verticalAmount;
		Width += horizontalAmount * 2f;
		Height += verticalAmount * 2f;
	}

	/// <summary>Determines whether the rectangle contains a specified Point</summary>
	/// <param name="point">The point to evaluate</param>
	/// <returns>
	///   True if the specified point is contained within this rectangle; false otherwise
	/// </returns>
	public bool Contains(Vector2 point)
	{
		return Contains(point.X, point.Y);
	}

	/// <summary>Determines whether the rectangle contains a specified Point</summary>
	/// <param name="point">The point to evaluate</param>
	/// <param name="result">
	///   True if the specified point is contained within this rectangle; false otherwise
	/// </param>
	public void Contains(ref Vector2 point, out bool result)
	{
		result = Contains(point.X, point.Y);
	}

	/// <summary>
	///   Determines whether this Rectangle contains a specified point represented by
	///   its x- and y-coordinates
	/// </summary>
	/// <param name="x">The x-coordinate of the specified point</param>
	/// <param name="y">The y-coordinate of the specified point</param>
	/// <returns>
	///   True if the specified point is contained within this rectangle; false otherwise
	/// </returns>
	public bool Contains(float x, float y)
	{
		if (x >= X && y >= Y && x < X + Width)
		{
			return y < Y + Height;
		}
		return false;
	}

	/// <summary>
	///   Determines whether the rectangle contains another rectangle in its entirety
	/// </summary>
	/// <param name="other">The rectangle to evaluate</param>
	/// <returns>
	///   True if the rectangle entirely contains the specified rectangle; false otherwise
	/// </returns>
	public bool Contains(RectangleF other)
	{
		Contains(ref other, out var result);
		return result;
	}

	/// <summary>
	///   Determines whether this rectangle entirely contains a specified rectangle
	/// </summary>
	/// <param name="other">The rectangle to evaluate</param>
	/// <param name="result">
	///   On exit, is true if this rectangle entirely contains the specified rectangle,
	///   or false if not
	/// </param>
	public void Contains(ref RectangleF other, out bool result)
	{
		result = other.X >= X && other.Y >= Y && other.X + other.Width <= X + Width && other.Y + other.Height <= Y + Height;
	}

	/// <summary>
	///   Determines whether a specified rectangle intersects with this rectangle
	/// </summary>
	/// <param name="rectangle">The rectangle to evaluate</param>
	/// <returns>
	///   True if the specified rectangle intersects with this one; false otherwise
	/// </returns>
	public bool Intersects(RectangleF rectangle)
	{
		Intersects(ref rectangle, out var result);
		return result;
	}

	/// <summary>
	///   Determines whether a specified rectangle intersects with this rectangle
	/// </summary>
	/// <param name="rectangle">The rectangle to evaluate</param>
	/// <param name="result">
	///   True if the specified rectangle intersects with this one; false otherwise
	/// </param>
	public void Intersects(ref RectangleF rectangle, out bool result)
	{
		result = rectangle.X < X + Width && rectangle.Y < Y + Height && rectangle.X + rectangle.Width > X && rectangle.Y + rectangle.Height > Y;
	}

	/// <summary>
	///   Determines whether the specified rectangle is equal to this rectangle
	/// </summary>
	/// <param name="other">The rectangle to compare with this rectangle</param>
	/// <returns>
	///   True if the specified rectangle is equal to the this rectangle; false otherwise
	/// </returns>
	public bool Equals(RectangleF other)
	{
		if (X == other.X && Y == other.Y && Width == other.Width)
		{
			return Height == other.Height;
		}
		return false;
	}

	/// <summary>
	///   Returns a value that indicates whether the current instance is equal to a
	///   specified object
	/// </summary>
	/// <param name="other">Object to make the comparison with</param>
	/// <returns>
	///   True if the current instance is equal to the specified object; false otherwise
	/// </returns>
	public override bool Equals(object other)
	{
		if (!(other is RectangleF))
		{
			return false;
		}
		return Equals((RectangleF)other);
	}

	/// <summary>Retrieves a string representation of the current object</summary>
	/// <returns>String that represents the object</returns>
	public override string ToString()
	{
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		return string.Format(currentCulture, "{{X:{0} Y:{1} Width:{2} Height:{3}}}", X.ToString(currentCulture), Y.ToString(currentCulture), Width.ToString(currentCulture), Height.ToString(currentCulture));
	}

	/// <summary>Gets the hash code for this object</summary>
	/// <returns>Hash code for this object</returns>
	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
	}

	/// <summary>Compares two rectangles for equality</summary>
	/// <param name="first">Source rectangle</param>
	/// <param name="second">Source rectangle</param>
	/// <returns>True if the rectangles are equal; false otherwise</returns>
	public static bool operator ==(RectangleF first, RectangleF second)
	{
		if (first.X == second.X && first.Y == second.Y && first.Width == second.Width)
		{
			return first.Height == second.Height;
		}
		return false;
	}

	/// <summary>Compares two rectangles for inequality</summary>
	/// <param name="first">Source rectangle</param>
	/// <param name="second">Source rectangle</param>
	/// <returns>True if the rectangles are not equal; false otherwise</returns>
	public static bool operator !=(RectangleF first, RectangleF second)
	{
		if (first.X == second.X && first.Y == second.Y && first.Width == second.Width)
		{
			return first.Height != second.Height;
		}
		return true;
	}
}
