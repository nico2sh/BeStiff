using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders sliders in a traditional flat style</summary>
public class FlatVerticalSliderControlRenderer : IFlatControlRenderer<VerticalSliderControl>, IFlatControlRenderer
{
	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(VerticalSliderControl control, IFlatGuiGraphics graphics)
	{
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		float num = absoluteBounds.Height * control.ThumbSize;
		float num2 = (absoluteBounds.Height - num) * control.ThumbPosition;
		graphics.DrawElement("rail.vertical", absoluteBounds);
		RectangleF bounds = new RectangleF(absoluteBounds.X, absoluteBounds.Y + num2, absoluteBounds.Width, num);
		if (control.ThumbDepressed)
		{
			graphics.DrawElement("slider.vertical.depressed", bounds);
		}
		else if (control.MouseOverThumb)
		{
			graphics.DrawElement("slider.vertical.highlighted", bounds);
		}
		else
		{
			graphics.DrawElement("slider.vertical.normal", bounds);
		}
	}
}
