using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders window controls in a traditional flat style</summary>
public class FlatWindowControlRenderer : IFlatControlRenderer<WindowControl>, IFlatControlRenderer
{
	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(WindowControl control, IFlatGuiGraphics graphics)
	{
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		graphics.DrawElement("window", absoluteBounds);
		if (control.Title != null)
		{
			graphics.DrawString("window", absoluteBounds, control.Title);
		}
	}
}
