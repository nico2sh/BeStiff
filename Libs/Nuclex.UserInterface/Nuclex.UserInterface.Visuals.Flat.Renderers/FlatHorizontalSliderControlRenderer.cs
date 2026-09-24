using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders horizontal sliders in a traditional flat style</summary>
public class FlatHorizontalSliderControlRenderer : IFlatControlRenderer<HorizontalSliderControl>, IFlatControlRenderer
{
	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(HorizontalSliderControl control, IFlatGuiGraphics graphics)
	{
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		float num = absoluteBounds.Width * control.ThumbSize;
		float num2 = (absoluteBounds.Width - num) * control.ThumbPosition;
		graphics.DrawElement("rail.horizontal", absoluteBounds);
		RectangleF bounds = new RectangleF(absoluteBounds.X + num2, absoluteBounds.Y, num, absoluteBounds.Height);
		if (control.ThumbDepressed)
		{
			graphics.DrawElement("slider.horizontal.depressed", bounds);
		}
		else if (control.MouseOverThumb)
		{
			graphics.DrawElement("slider.horizontal.highlighted", bounds);
		}
		else
		{
			graphics.DrawElement("slider.horizontal.normal", bounds);
		}
	}
}
