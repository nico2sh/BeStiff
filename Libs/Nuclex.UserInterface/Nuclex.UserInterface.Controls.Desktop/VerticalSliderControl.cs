using Microsoft.Xna.Framework;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Vertical slider that can be moved using the mouse</summary>
public class VerticalSliderControl : SliderControl
{
	/// <summary>Obtains the region covered by the slider's thumb</summary>
	/// <returns>The region covered by the slider's thumb</returns>
	protected override RectangleF GetThumbRegion()
	{
		RectangleF absoluteBounds = GetAbsoluteBounds();
		if (ThumbLocator != null)
		{
			return ThumbLocator.GetThumbPosition(absoluteBounds, ThumbPosition, ThumbSize);
		}
		float num = absoluteBounds.Height * ThumbSize;
		float y = (absoluteBounds.Height - num) * ThumbPosition;
		return new RectangleF(0f, y, absoluteBounds.Width, num);
	}

	/// <summary>Moves the thumb to the specified location</summary>
	/// <param name="x">X coordinate for the new left border of the thumb</param>
	/// <param name="y">Y coordinate for the new upper border of the thumb</param>
	protected override void MoveThumb(float x, float y)
	{
		RectangleF absoluteBounds = GetAbsoluteBounds();
		float num = absoluteBounds.Height * ThumbSize;
		float num2 = absoluteBounds.Height - num;
		if (num2 > 0f)
		{
			ThumbPosition = MathHelper.Clamp(y / num2, 0f, 1f);
		}
		else
		{
			ThumbPosition = 0f;
		}
		OnMoved();
	}
}
