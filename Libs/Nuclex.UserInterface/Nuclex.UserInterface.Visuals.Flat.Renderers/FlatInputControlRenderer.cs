using Microsoft.Xna.Framework;
using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders text input controls in a traditional flat style</summary>
public class FlatInputControlRenderer : IFlatControlRenderer<InputControl>, IFlatControlRenderer, IOpeningLocator
{
	/// <summary>Style from the skin this renderer uses</summary>
	private const string Style = "input.normal";

	/// <summary>Graphics interface we used for the last draw call</summary>
	private IFlatGuiGraphics graphics;

	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(InputControl control, IFlatGuiGraphics graphics)
	{
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		graphics.DrawElement("input.normal", absoluteBounds);
		using (graphics.SetClipRegion(absoluteBounds))
		{
			string text = control.Text ?? string.Empty;
			float num = 0f;
			if (control.HasFocus)
			{
				RectangleF rectangleF = graphics.MeasureString("input.normal", absoluteBounds, text.Substring(0, control.CaretPosition));
				while (rectangleF.Width + num > absoluteBounds.Width)
				{
					num -= absoluteBounds.Width / 10f;
				}
			}
			absoluteBounds.X += num;
			graphics.DrawString("input.normal", absoluteBounds, control.Text);
			if (control.HasFocus && control.MillisecondsSinceLastCaretMovement % 500 < 250)
			{
				graphics.DrawCaret("input.normal", absoluteBounds, control.Text, control.CaretPosition);
			}
		}
		control.OpeningLocator = this;
		this.graphics = graphics;
	}

	/// <summary>
	///   Calculates which opening between two letters is closest to a position
	/// </summary>
	/// <param name="bounds">
	///   Boundaries of the control, should be in absolute coordinates
	/// </param>
	/// <param name="text">Text in which the opening will be looked for</param>
	/// <param name="position">
	///   Position to which the closest opening will be found,
	///   should be in absolute coordinates
	/// </param>
	/// <returns>The index of the opening closest to the provided position</returns>
	public int GetClosestOpening(RectangleF bounds, string text, Vector2 position)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return graphics.GetClosestOpening("input.normal", bounds, text, position);
	}
}
