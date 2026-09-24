using Nuclex.UserInterface.Controls;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders progress bars in a traditional flat style</summary>
public class FlatProgressControlRenderer : IFlatControlRenderer<ProgressControl>, IFlatControlRenderer
{
	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(ProgressControl control, IFlatGuiGraphics graphics)
	{
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		graphics.DrawElement("progress", absoluteBounds);
		absoluteBounds.Width *= control.Progress;
		graphics.DrawElement("progress.bar", absoluteBounds);
	}
}
