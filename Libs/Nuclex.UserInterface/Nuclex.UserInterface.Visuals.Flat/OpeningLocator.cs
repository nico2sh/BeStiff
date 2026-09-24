using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nuclex.UserInterface.Visuals.Flat;

/// <summary>
///   Locates the opening between characters in a string that is nearest
///   to a user-defined location
/// </summary>
/// <remarks>
///   <para>
///     This is a class rather than a static class to prevent garbage production
///     which would then have to be cleaned up again by the garbage collector.
///     If you create an instance of it and keep reusing it, garbage and allocation
///     will amortize.
///   </para>
///   <para>
///     The method used to calculate the openings seems to be not terribly accurate.
///     As of XNA 3.1, SpriteFonts don't do kerning, so the only thing left would be
///     a variable space appended to the end of characters. This could be compensated
///     for by always appending character with a known length for which no kerning is
///     possible, for example, the pipe sign (|).
///   </para>
/// </remarks>
internal class OpeningLocator
{
	/// <summary>Used by GetClosestOpening() to avoid garbage production</summary>
	private StringBuilder textBuilder;

	/// <summary>Initializes a new text opening locator</summary>
	public OpeningLocator()
	{
		textBuilder = new StringBuilder(64);
	}

	/// <summary>
	///   Locates the opening between two letters that is closest to
	///   the specified position
	/// </summary>
	/// <param name="font">Font that opening search will use</param>
	/// <param name="text">Text that will be searched for the opening</param>
	/// <param name="x">X coordinate closest to which an opening will be found</param>
	/// <returns>The opening closest to the specified X coordinate</returns>
	public int FindClosestOpening(SpriteFont font, string text, float x)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		textBuilder.Remove(0, textBuilder.Length);
		textBuilder.Append(text);
		Vector2 val = font.MeasureString(textBuilder);
		int num = 0;
		float num2 = 0f;
		int num3 = text.Length;
		float x2 = val.X;
		while (true)
		{
			if (x <= num2)
			{
				return num;
			}
			if (x >= x2)
			{
				return num3;
			}
			if (num3 - num <= 1)
			{
				break;
			}
			int num4 = (num3 + num) / 2;
			textBuilder.Remove(num4, num3 - num4);
			val = font.MeasureString(textBuilder);
			if (x < val.X)
			{
				num3 = num4;
				x2 = val.X;
			}
			else
			{
				textBuilder.Append(text, num4, num3 - num4);
				num = num4;
				num2 = val.X;
			}
		}
		if (x - num2 <= x2 - x)
		{
			return num;
		}
		return num3;
	}
}
