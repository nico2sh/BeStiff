using Microsoft.Xna.Framework;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Horizontal slider that can be moved using the mouse</summary>
public class HorizontalSliderControl : SliderControl
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
		float num = absoluteBounds.Width * ThumbSize;
		float x = (absoluteBounds.Width - num) * ThumbPosition;
		return new RectangleF(x, 0f, num, absoluteBounds.Height);
	}

	/// <summary>Moves the thumb to the specified location</summary>
	/// <returns>Location the thumb will be moved to</returns>
	protected override void MoveThumb(float x, float y)
	{
		RectangleF absoluteBounds = GetAbsoluteBounds();
		float num = absoluteBounds.Width * ThumbSize;
		float num2 = absoluteBounds.Width - num;
		if (num2 > 0f)
		{
			ThumbPosition = MathHelper.Clamp(x / num2, 0f, 1f);
		}
		else
		{
			ThumbPosition = 0f;
		}
		OnMoved();
	}
}
