using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders button controls in a traditional flat style</summary>
public class FlatButtonControlRenderer : IFlatControlRenderer<ButtonControl>, IFlatControlRenderer
{
	/// <summary>Names of the states the button control can be in</summary>
	/// <remarks>
	///   Storing this as full strings instead of building them dynamically prevents
	///   any garbage from forming during rendering.
	/// </remarks>
	private static readonly string[] states = new string[4] { "button.disabled", "button.normal", "button.highlighted", "button.depressed" };

	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(ButtonControl control, IFlatGuiGraphics graphics)
	{
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		int num = 0;
		if (control.Enabled)
		{
			num = (control.Depressed ? 3 : ((!control.MouseHovering && !control.HasFocus) ? 1 : 2));
		}
		graphics.DrawElement(states[num], absoluteBounds);
		if (!string.IsNullOrEmpty(control.Text))
		{
			graphics.DrawString(states[num], absoluteBounds, control.Text);
		}
	}
}
