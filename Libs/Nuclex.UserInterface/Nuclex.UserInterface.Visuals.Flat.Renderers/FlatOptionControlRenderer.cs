using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders option controls in a traditional flat style</summary>
public class FlatOptionControlRenderer : IFlatControlRenderer<OptionControl>, IFlatControlRenderer
{
	/// <summary>Names of the states the option control can be in</summary>
	/// <remarks>
	///   Storing this as full strings instead of building them dynamically prevents
	///   any garbage from forming during rendering.
	/// </remarks>
	private static readonly string[] states = new string[8] { "check.off.disabled", "check.off.normal", "check.off.highlighted", "check.off.depressed", "check.on.disabled", "check.on.normal", "check.on.highlighted", "check.on.depressed" };

	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(OptionControl control, IFlatGuiGraphics graphics)
	{
		int num = (control.Selected ? 4 : 0);
		if (control.Enabled)
		{
			num = (control.Depressed ? (num + 3) : ((!control.MouseHovering) ? (num + 1) : (num + 2)));
		}
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		float width = absoluteBounds.Width;
		absoluteBounds.Width = absoluteBounds.Height;
		graphics.DrawElement(states[num], absoluteBounds);
		if (control.Text != null)
		{
			absoluteBounds.Width = width - absoluteBounds.Height;
			absoluteBounds.X += absoluteBounds.Height;
			graphics.DrawString(states[num], absoluteBounds, control.Text);
		}
	}
}
