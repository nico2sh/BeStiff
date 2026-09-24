using Nuclex.UserInterface.Controls.Arcade;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders panel controls in a traditional flat style</summary>
public class FlatPanelControlRenderer : IFlatControlRenderer<PanelControl>, IFlatControlRenderer
{
	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(PanelControl control, IFlatGuiGraphics graphics)
	{
		graphics.DrawElement("window", control.GetAbsoluteBounds());
	}
}
