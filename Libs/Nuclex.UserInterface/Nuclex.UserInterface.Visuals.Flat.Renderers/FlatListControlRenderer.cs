using System;
using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers;

/// <summary>Renders text input controls in a traditional flat style</summary>
public class FlatListControlRenderer : IFlatControlRenderer<ListControl>, IFlatControlRenderer, IListRowLocator
{
	/// <summary>Style used to draw this control</summary>
	private const string Style = "list";

	/// <summary>Height of a single row in the list</summary>
	private float rowHeight = float.NaN;

	/// <summary>Graphics interface we used for the last draw call</summary>
	private IFlatGuiGraphics graphics;

	/// <summary>
	///   Renders the specified control using the provided graphics interface
	/// </summary>
	/// <param name="control">Control that will be rendered</param>
	/// <param name="graphics">
	///   Graphics interface that will be used to draw the control
	/// </param>
	public void Render(ListControl control, IFlatGuiGraphics graphics)
	{
		this.graphics = graphics;
		RectangleF absoluteBounds = control.GetAbsoluteBounds();
		graphics.DrawElement("list", absoluteBounds);
		float num = control.Items.Count;
		float num2 = GetRowHeight(absoluteBounds);
		float num3 = absoluteBounds.Height / num2;
		float num4 = Math.Max(num - num3, 0f);
		float num5 = control.Slider.ThumbPosition * num4;
		int num6 = (int)num5;
		int val = (int)Math.Ceiling(num5 + num3);
		val = Math.Min(val, control.Items.Count);
		RectangleF bounds = absoluteBounds;
		bounds.Y -= (num5 - (float)num6) * num2;
		bounds.Height = num2;
		using (graphics.SetClipRegion(absoluteBounds))
		{
			for (int i = num6; i < val; i++)
			{
				if (control.SelectedItems.Contains(i))
				{
					graphics.DrawElement("list.selection", bounds);
				}
				graphics.DrawString("list", bounds, control.Items[i]);
				bounds.Y += num2;
			}
		}
		control.ListRowLocator = this;
	}

	/// <summary>Calculates the list row the cursor is in</summary>
	/// <param name="bounds">
	///   Boundaries of the control, should be in absolute coordinates
	/// </param>
	/// <param name="thumbPosition">
	///   Position of the thumb in the list's slider
	/// </param>
	/// <param name="itemCount">
	///   Number of items contained in the list
	/// </param>
	/// <param name="y">Vertical position of the cursor</param>
	/// <returns>The row the cursor is over</returns>
	public int GetRow(RectangleF bounds, float thumbPosition, int itemCount, float y)
	{
		float num = itemCount;
		float num2 = GetRowHeight(bounds);
		float num3 = bounds.Height / num2;
		float num4 = num - num3;
		float num5 = thumbPosition * num4;
		return (int)(y / GetRowHeight(bounds) + num5);
	}

	/// <summary>Determines the height of a row displayed in the list</summary>
	/// <param name="bounds">
	///   Boundaries of the control, should be in absolute coordinates
	/// </param>
	/// <returns>The height of a single row in the list</returns>
	public float GetRowHeight(RectangleF bounds)
	{
		if (float.IsNaN(rowHeight))
		{
			rowHeight = graphics.MeasureString("list", bounds, "qyjpMAW!").Height;
			rowHeight += 2f;
		}
		return rowHeight;
	}
}
