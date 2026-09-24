using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Support;

namespace Nuclex.UserInterface.Visuals.Flat;

/// <summary>Graphics interface for the traditional flat GUI visualizer</summary>
/// <remarks>
///   This class is analog to System.Drawing.Graphics, but contains specialized
///   methods that allow the FlatControlRenderers to draw controls from
///   high-level elements which are controlled by loadable XML themes.
/// </remarks>
public class FlatGuiGraphics : IFlatGuiGraphics, IDisposable
{
	/// <summary>Frame that can be drawn by the GUI painter</summary>
	private class Frame
	{
		/// <summary>Modes in which text can be horizontally aligned</summary>
		public enum HorizontalTextAlignment
		{
			/// <summary>The text's base offset is placed at the left of the frame</summary>
			/// <remarks>
			///   The base offset is normally identical to the text's leftmost pixel.
			///   However, a glyph may have some eccentrics like an arc that extends to
			///   the left over the letter's actual starting position.
			/// </remarks>
			Left,
			/// <summary>
			///   The text's ending offset is placed at the right of the frame
			/// </summary>
			/// <remarks>
			///   The ending offset is normally identical to the text's rightmost pixel.
			///   However, a glyph may have some eccentrics like an arc that extends to
			///   the right over the last letter's actual ending position.
			/// </remarks>
			Right,
			/// <summary>The text is centered horizontally in the frame</summary>
			Center
		}

		/// <summary>Modes in which text can be vertically aligned</summary>
		public enum VerticalTextAlignment
		{
			/// <summary>The text's baseline is placed at the top of the frame</summary>
			Top,
			/// <summary>The text's baseline is placed at the bottom of the frame</summary>
			Bottom,
			/// <summary>The text's baseline is centered vertically in the frame</summary>
			Center
		}

		/// <summary>Defines a picture region drawn into a frame</summary>
		public struct Region
		{
			/// <summary>Identification string for the region</summary>
			/// <remarks>
			///   Used to associate regions with specific behavior
			/// </remarks>
			public string Id;

			/// <summary>Texture the picture region is taken from</summary>
			public Texture2D Texture;

			/// <summary>Area within the texture containing the picture region</summary>
			public Rectangle SourceRegion;

			/// <summary>Location in the frame where the picture region will be drawn</summary>
			public UniRectangle DestinationRegion;
		}

		/// <summary>Describes where within the frame text should be drawn</summary>
		public struct Text
		{
			/// <summary>Font to use for drawing the text</summary>
			public SpriteFont Font;

			/// <summary>Offset of the text relative to its specified placement</summary>
			public Point Offset;

			/// <summary>Horizontal placement of the text within the frame</summary>
			public HorizontalTextAlignment HorizontalPlacement;

			/// <summary>Vertical placement of the text within the frame</summary>
			public VerticalTextAlignment VerticalPlacement;

			/// <summary>Color the text will have</summary>
			public Color Color;
		}

		/// <summary>Regions that need to be drawn to render the frame</summary>
		public Region[] Regions;

		/// <summary>Locations where text can be drawn into the frame</summary>
		public Text[] Texts;

		/// <summary>Initializes a new frame</summary>
		/// <param name="regions">Regions needed to be drawn to render the frame</param>
		/// <param name="texts">Location in the frame where text can be drawn</param>
		public Frame(Region[] regions, Text[] texts)
		{
			Regions = regions;
			Texts = texts;
		}
	}

	/// <summary>Manages the scissor rectangle for the GUI graphics interface</summary>
	private class ScissorKeeper : IDisposable
	{
		/// <summary>
		///   GUI graphics interface for which the scissor rectangle is managed
		/// </summary>
		private FlatGuiGraphics flatGuiGraphics;

		/// <summary>
		///   Scissor rectangle that was previously assigned to the graphics device
		/// </summary>
		private Rectangle oldScissorRectangle;

		/// <summary>Initializes a new scissor manager</summary>
		/// <param name="flatGuiGraphics">
		///   GUI graphics interface the scissor rectangle will be managed for
		/// </param>
		public ScissorKeeper(FlatGuiGraphics flatGuiGraphics)
		{
			this.flatGuiGraphics = flatGuiGraphics;
		}

		/// <summary>Assigns the scissor rectangle to the graphics device</summary>
		/// <param name="clipRegion">Scissor rectangle that will be assigned</param>
		public void Assign(ref Rectangle clipRegion)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			flatGuiGraphics.endSpriteBatch();
			try
			{
				GraphicsDevice graphicsDevice = ((GraphicsResource)flatGuiGraphics.spriteBatch).GraphicsDevice;
				oldScissorRectangle = graphicsDevice.ScissorRectangle;
				graphicsDevice.ScissorRectangle = clipRegion;
			}
			finally
			{
				flatGuiGraphics.beginSpriteBatch();
			}
		}

		/// <summary>Releases the currently assigned scissor rectangle again</summary>
		public void Dispose()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			flatGuiGraphics.endSpriteBatch();
			try
			{
				GraphicsDevice graphicsDevice = ((GraphicsResource)flatGuiGraphics.spriteBatch).GraphicsDevice;
				graphicsDevice.ScissorRectangle = oldScissorRectangle;
			}
			finally
			{
				flatGuiGraphics.beginSpriteBatch();
			}
		}
	}

	/// <summary>Builds a region list from the regions in an frame XML node</summary>
	private class RegionListBuilder
	{
		/// <summary>Width of the frame's left border regions</summary>
		private int leftBorderWidth;

		/// <summary>Width of the frame's top border regions</summary>
		private int topBorderWidth;

		/// <summary>Width of the frame's right border regions</summary>
		private int rightBorderWidth;

		/// <summary>Width of the frame's bottom border regions</summary>
		private int bottomBorderWidth;

		/// <summary>Initializes a new frame region list builder</summary>
		private RegionListBuilder()
		{
		}

		/// <summary>
		///   Builds a region list from the regions specified in the provided frame XML node
		/// </summary>
		/// <param name="frameElement">
		///   XML node for the frame whose regions wille be processed
		/// </param>
		/// <param name="bitmaps">
		///   Bitmap lookup table used to associate a region's bitmap id to the real bitmap
		/// </param>
		/// <returns>
		///   A list of the regions that have been extracted from the frame XML node
		/// </returns>
		public static Frame.Region[] Build(XElement frameElement, IDictionary<string, Texture2D> bitmaps)
		{
			RegionListBuilder regionListBuilder = new RegionListBuilder();
			regionListBuilder.retrieveBorderSizes(frameElement);
			return regionListBuilder.createAndPlaceRegions(frameElement, bitmaps);
		}

		/// <summary>Retrieves the sizes of the border regions in a frame</summary>
		/// <param name="frameElement">
		///   XML node for the frame containing the region
		/// </param>
		private void retrieveBorderSizes(XElement frameElement)
		{
			foreach (XElement item in ((XContainer)frameElement).Descendants(((XName)"region")))
			{
				string value = item.Attribute(((XName)"hplacement")).Value;
				string value2 = item.Attribute(((XName)"w")).Value;
				if (value == "left")
				{
					leftBorderWidth = Math.Max(leftBorderWidth, int.Parse(value2));
				}
				else if (value == "right")
				{
					rightBorderWidth = Math.Max(rightBorderWidth, int.Parse(value2));
				}
				string value3 = item.Attribute(((XName)"vplacement")).Value;
				string value4 = item.Attribute(((XName)"h")).Value;
				if (value3 == "top")
				{
					topBorderWidth = Math.Max(topBorderWidth, int.Parse(value4));
				}
				else if (value3 == "bottom")
				{
					bottomBorderWidth = Math.Max(bottomBorderWidth, int.Parse(value4));
				}
			}
		}

		/// <summary>
		///   Creates and places the regions needed to be drawn to render the frame
		/// </summary>
		/// <param name="frameElement">
		///   XML node for the frame containing the region
		/// </param>
		/// <param name="bitmaps">
		///   Bitmap lookup table to associate a region's bitmap id to the real bitmap
		/// </param>
		/// <returns>The regions created for the frame</returns>
		private Frame.Region[] createAndPlaceRegions(XElement frameElement, IDictionary<string, Texture2D> bitmaps)
		{
			List<Frame.Region> list = new List<Frame.Region>();
			foreach (XElement item2 in ((XContainer)frameElement).Descendants(((XName)"region")))
			{
				XAttribute val = item2.Attribute(((XName)"id"));
				string id = ((val == null) ? null : val.Value);
				string value = item2.Attribute(((XName)"source")).Value;
				string value2 = item2.Attribute(((XName)"hplacement")).Value;
				string value3 = item2.Attribute(((XName)"vplacement")).Value;
				string value4 = item2.Attribute(((XName)"x")).Value;
				string value5 = item2.Attribute(((XName)"y")).Value;
				string value6 = item2.Attribute(((XName)"w")).Value;
				string value7 = item2.Attribute(((XName)"h")).Value;
				Frame.Region item = new Frame.Region
				{
					Id = id,
					Texture = bitmaps[value]
				};
				item.SourceRegion.X = int.Parse(value4);
				item.SourceRegion.Y = int.Parse(value5);
				item.SourceRegion.Width = int.Parse(value6);
				item.SourceRegion.Height = int.Parse(value7);
				calculateRegionPlacement(getHorizontalPlacementIndex(value2), int.Parse(value6), leftBorderWidth, rightBorderWidth, ref item.DestinationRegion.Location.X, ref item.DestinationRegion.Size.X);
				calculateRegionPlacement(getVerticalPlacementIndex(value3), int.Parse(value7), topBorderWidth, bottomBorderWidth, ref item.DestinationRegion.Location.Y, ref item.DestinationRegion.Size.Y);
				list.Add(item);
			}
			return list.ToArray();
		}

		/// <summary>
		///   Calculates the unified coordinates a region needs to be placed at
		/// </summary>
		/// <param name="placementIndex">
		///   Placement index indicating where in a frame the region will be located
		/// </param>
		/// <param name="width">Width of the region in pixels</param>
		/// <param name="lowBorderWidth">
		///   Width of the border on the lower end of the coordinate range
		/// </param>
		/// <param name="highBorderWidth">
		///   Width of the border on the higher end of the coordinate range
		/// </param>
		/// <param name="location">
		///   Receives the target location of the region in unified coordinates
		/// </param>
		/// <param name="size">
		///   Receives the size of the region in unified coordinates
		/// </param>
		private void calculateRegionPlacement(int placementIndex, int width, int lowBorderWidth, int highBorderWidth, ref UniScalar location, ref UniScalar size)
		{
			switch (placementIndex)
			{
			case -1:
			{
				int num = lowBorderWidth - width;
				location.Fraction = 0f;
				location.Offset = num;
				size.Fraction = 0f;
				size.Offset = width;
				break;
			}
			case 1:
				location.Fraction = 1f;
				location.Offset = 0f - (float)highBorderWidth;
				size.Fraction = 0f;
				size.Offset = width;
				break;
			case 0:
				location.Fraction = 0f;
				location.Offset = lowBorderWidth;
				size.Fraction = 1f;
				size.Offset = 0f - (float)(highBorderWidth + lowBorderWidth);
				break;
			}
		}

		/// <summary>Converts a horizontal placement string into a placement index</summary>
		/// <param name="placement">String containing the horizontal placement</param>
		/// <returns>A placement index that is equivalent to the provided string</returns>
		private int getHorizontalPlacementIndex(string placement)
		{
			return placement switch
			{
				"left" => -1, 
				"right" => 1, 
				_ => 0, 
			};
		}

		/// <summary>Converts a vertical placement string into a placement index</summary>
		/// <param name="placement">String containing the vertical placement</param>
		/// <returns>A placement index that is equivalent to the provided string</returns>
		private int getVerticalPlacementIndex(string placement)
		{
			return placement switch
			{
				"top" => -1, 
				"bottom" => 1, 
				_ => 0, 
			};
		}
	}

	/// <summary>Builds a text list from the regions in an frame XML node</summary>
	private class TextListBuilder
	{
		/// <summary>
		///   Builds a text list from the text placements specified in the provided node
		/// </summary>
		/// <param name="frameElement">
		///   XML node for the frame whose text placements wille be processed
		/// </param>
		/// <param name="fonts">
		///   Font lookup table used to associate a text's font id to the real font
		/// </param>
		/// <returns>
		///   A list of the texts that have been extracted from the frame XML node
		/// </returns>
		public static Frame.Text[] Build(XElement frameElement, IDictionary<string, SpriteFont> fonts)
		{
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			List<Frame.Text> list = new List<Frame.Text>();
			foreach (XElement item2 in ((XContainer)frameElement).Descendants(((XName)"text")))
			{
				string value = item2.Attribute(((XName)"font")).Value;
				string value2 = item2.Attribute(((XName)"hplacement")).Value;
				string value3 = item2.Attribute(((XName)"vplacement")).Value;
				XAttribute val = item2.Attribute(((XName)"xoffset"));
				int num = ((val != null) ? int.Parse(val.Value) : 0);
				XAttribute val2 = item2.Attribute(((XName)"yoffset"));
				int num2 = ((val2 != null) ? int.Parse(val2.Value) : 0);
				XAttribute val3 = item2.Attribute(((XName)"color"));
				Color color = ((val3 != null) ? colorFromString(val3.Value) : Color.White);
				Frame.Text item = new Frame.Text
				{
					Font = fonts[value],
					HorizontalPlacement = horizontalPlacementFromString(value2),
					VerticalPlacement = verticalPlacementFromString(value3),
					Offset = new Point(num, num2),
					Color = color
				};
				list.Add(item);
			}
			return list.ToArray();
		}

		/// <summary>Converts a string into a horizontal placement enumeration value</summary>
		/// <param name="placement">Placement string that will be converted</param>
		/// <returns>The horizontal placement enumeration value matching the string</returns>
		private static Frame.HorizontalTextAlignment horizontalPlacementFromString(string placement)
		{
			return placement switch
			{
				"left" => Frame.HorizontalTextAlignment.Left, 
				"right" => Frame.HorizontalTextAlignment.Right, 
				_ => Frame.HorizontalTextAlignment.Center, 
			};
		}

		/// <summary>Converts a string into a vertical placement enumeration value</summary>
		/// <param name="placement">Placement string that will be converted</param>
		/// <returns>The vertical placement enumeration value matching the string</returns>
		private static Frame.VerticalTextAlignment verticalPlacementFromString(string placement)
		{
			return placement switch
			{
				"top" => Frame.VerticalTextAlignment.Top, 
				"bottom" => Frame.VerticalTextAlignment.Bottom, 
				_ => Frame.VerticalTextAlignment.Center, 
			};
		}
	}

	/// <summary>Width of the caret used for text input</summary>
	private const float CaretWidth = 2f;

	/// <summary>String builder used for various purposes in this class</summary>
	private StringBuilder stringBuilder;

	/// <summary>Locates openings between letters in strings</summary>
	private OpeningLocator openingLocator;

	/// <summary>Manages the scissor rectangle and its assignment time</summary>
	private ScissorKeeper scissorManager;

	/// <summary>Batches GUI elements for faster drawing</summary>
	private SpriteBatch spriteBatch;

	/// <summary>Manages the content used to draw the GUI</summary>
	private ContentManager contentManager;

	/// <summary>Font styles known to the GUI</summary>
	private Dictionary<string, SpriteFont> fonts;

	/// <summary>Bitmaps containing resources for the GUI</summary>
	private Dictionary<string, Texture2D> bitmaps;

	/// <summary>Types of frames the painter can draw</summary>
	private Dictionary<string, Frame> frames;

	/// <summary>Rasterizer state used for drawing the GUI</summary>
	private RasterizerState rasterizerState;

	/// <summary>Needs to be called before the GUI drawing process begins</summary>
	public void BeginDrawing()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		GraphicsDevice graphicsDevice = ((GraphicsResource)spriteBatch).GraphicsDevice;
		Viewport viewport = graphicsDevice.Viewport;
		graphicsDevice.ScissorRectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);
		spriteBatch.Begin();
	}

	/// <summary>Needs to be called when the GUI drawing process has ended</summary>
	public void EndDrawing()
	{
		endSpriteBatch();
	}

	/// <summary>Sets the clipping region for any future drawing commands</summary>
	/// <param name="clipRegion">Clipping region that will be set</param>
	/// <returns>
	///   An object that will unset the clipping region upon its destruction.
	/// </returns>
	/// <remarks>
	///   Clipping regions can be stacked, though this is not very typical for
	///   a game GUI and also not recommended practice due to performance constraints.
	///   Unless clipping is implemented in software, setting up a clip region
	///   on current hardware requires the drawing queue to be flushed, negatively
	///   impacting rendering performance (in technical terms, a clipping region
	///   change likely causes 2 more DrawPrimitive() calls from the painter).
	/// </remarks>
	public IDisposable SetClipRegion(RectangleF clipRegion)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)clipRegion.X;
		int num2 = (int)clipRegion.Y;
		int val = num + (int)clipRegion.Width;
		int val2 = num2 + (int)clipRegion.Height;
		Viewport viewport = ((GraphicsResource)spriteBatch).GraphicsDevice.Viewport;
		int val3 = viewport.X + viewport.Width;
		int val4 = viewport.Y + viewport.Height;
		Rectangle clipRegion2 = new Rectangle(Math.Max(num, viewport.X), Math.Max(num2, viewport.Y), Math.Min(val, val3) - num, Math.Min(val2, val4) - num2);
		clipRegion2.Width += num - clipRegion2.X;
		clipRegion2.Height += num2 - clipRegion2.Y;
		if (clipRegion2.Width <= 0 || clipRegion2.Height <= 0)
		{
			clipRegion2 = Rectangle.Empty;
		}
		scissorManager.Assign(ref clipRegion2);
		return scissorManager;
	}

	/// <summary>Draws a GUI element onto the drawing buffer</summary>
	/// <param name="frameName">Class of the element to draw</param>
	/// <param name="bounds">Region that will be covered by the drawn element</param>
	/// <remarks>
	///   <para>
	///     GUI elements are the basic building blocks of a GUI: 
	///   </para>
	/// </remarks>
	public void DrawElement(string frameName, RectangleF bounds)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Frame frame = lookupFrame(frameName);
		for (int i = 0; i < frame.Regions.Length; i++)
		{
			Rectangle val = calculateDestinationRectangle(ref bounds, ref frame.Regions[i].DestinationRegion);
			spriteBatch.Draw(frame.Regions[i].Texture, val, (Rectangle?)frame.Regions[i].SourceRegion, Color.White);
		}
	}

	/// <summary>Draws text into the drawing buffer for the specified element</summary>
	/// <param name="frameName">Class of the element for which to draw text</param>
	/// <param name="bounds">Region that will be covered by the drawn element</param>
	/// <param name="text">Text that will be drawn</param>
	public void DrawString(string frameName, RectangleF bounds, string text)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Frame frame = lookupFrame(frameName);
		for (int i = 0; i < frame.Texts.Length; i++)
		{
			spriteBatch.DrawString(frame.Texts[i].Font, text, positionText(ref frame.Texts[i], bounds, text), frame.Texts[i].Color);
		}
	}

	/// <summary>Draws a caret for text input at the specified index</summary>
	/// <param name="frameName">Class of the element for which to draw a caret</param>
	/// <param name="bounds">Region that will be covered by the drawn element</param>
	/// <param name="text">Text for which a caret will be drawn</param>
	/// <param name="caretIndex">Index the caret will be drawn at</param>
	public void DrawCaret(string frameName, RectangleF bounds, string text, int caretIndex)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Frame frame = lookupFrame(frameName);
		stringBuilder.Remove(0, stringBuilder.Length);
		stringBuilder.Append(text, 0, caretIndex);
		for (int i = 0; i < frame.Texts.Length; i++)
		{
			Vector2 val = positionText(ref frame.Texts[i], bounds, text);
			Vector2 val2 = frame.Texts[i].Font.MeasureString(stringBuilder);
			val2.X -= 2f;
			val2.Y = 0f;
			spriteBatch.DrawString(frame.Texts[i].Font, "|", val + val2, frame.Texts[i].Color);
		}
	}

	/// <summary>Measures the extents of a string in the frame's area</summary>
	/// <param name="frameName">Class of the element whose text will be measured</param>
	/// <param name="bounds">Region that will be covered by the drawn element</param>
	/// <param name="text">Text that will be measured</param>
	/// <returns>
	///   The size and extents of the specified string within the frame
	/// </returns>
	public RectangleF MeasureString(string frameName, RectangleF bounds, string text)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Frame frame = lookupFrame(frameName);
		Vector2 val = ((frame.Texts.Length <= 0) ? Vector2.Zero : frame.Texts[0].Font.MeasureString(text));
		return new RectangleF(0f, 0f, val.X, val.Y);
	}

	/// <summary>
	///   Locates the closest gap between two letters to the provided position
	/// </summary>
	/// <param name="frameName">Class of the element in which to find the gap</param>
	/// <param name="bounds">Region that will be covered by the drawn element</param>
	/// <param name="text">Text in which the closest gap will be found</param>
	/// <param name="position">Position of which to determien the closest gap</param>
	/// <returns>The index of the gap the position is closest to</returns>
	public int GetClosestOpening(string frameName, RectangleF bounds, string text, Vector2 position)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Frame frame = lookupFrame(frameName);
		int result = -1;
		for (int i = 0; i < frame.Texts.Length; i++)
		{
			Vector2 val = positionText(ref frame.Texts[i], bounds, text);
			position.X -= val.X;
			position.Y -= val.Y;
			int num = openingLocator.FindClosestOpening(frame.Texts[i].Font, text, position.X + 2f);
			result = num;
		}
		return result;
	}

	/// <summary>Starts drawing on the sprite batch</summary>
	private void beginSpriteBatch()
	{
		spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, rasterizerState);
	}

	/// <summary>Stops drawing on the sprite batch</summary>
	private void endSpriteBatch()
	{
		spriteBatch.End();
	}

	/// <summary>Initializes a new gui painter</summary>
	/// <param name="contentManager">
	///   Content manager containing the resources for the GUI. The instance takes
	///   ownership of the content manager and will dispose it.
	/// </param>
	/// <param name="skinStream">
	///   Stream from which the skin description will be read
	/// </param>
	public FlatGuiGraphics(ContentManager contentManager, Stream skinStream)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		IGraphicsDeviceService val = (IGraphicsDeviceService)contentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
		spriteBatch = new SpriteBatch(val.GraphicsDevice);
		this.contentManager = contentManager;
		openingLocator = new OpeningLocator();
		stringBuilder = new StringBuilder(64);
		scissorManager = new ScissorKeeper(this);
		RasterizerState val2 = new RasterizerState();
		val2.ScissorTestEnable = true;
		rasterizerState = val2;
		fonts = new Dictionary<string, SpriteFont>();
		bitmaps = new Dictionary<string, Texture2D>();
		frames = new Dictionary<string, Frame>();
		loadSkin(skinStream);
	}

	/// <summary>Immediately releases all resources owned by the instance</summary>
	public void Dispose()
	{
		if (contentManager != null)
		{
			contentManager.Dispose();
			contentManager = null;
		}
		if (spriteBatch != null)
		{
			((GraphicsResource)spriteBatch).Dispose();
			spriteBatch = null;
		}
	}

	/// <summary>
	///   Positions a string within a frame according to the positioning instructions
	///   stored in the provided text anchor.
	/// </summary>
	/// <param name="anchor">Text anchor the string will be positioned for</param>
	/// <param name="bounds">Boundaries of the control the string is rendered in</param>
	/// <param name="text">String that will be positioned</param>
	/// <returns>The position of the string within the control</returns>
	private Vector2 positionText(ref Frame.Text anchor, RectangleF bounds, string text)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = anchor.Font.MeasureString(text);
		float num = anchor.HorizontalPlacement switch
		{
			Frame.HorizontalTextAlignment.Left => bounds.Left, 
			Frame.HorizontalTextAlignment.Right => bounds.Right - val.X, 
			_ => (bounds.Width - val.X) / 2f + bounds.Left, 
		};
		float num2 = anchor.VerticalPlacement switch
		{
			Frame.VerticalTextAlignment.Top => bounds.Top, 
			Frame.VerticalTextAlignment.Bottom => bounds.Bottom - (float)anchor.Font.LineSpacing, 
			_ => (bounds.Height - (float)anchor.Font.LineSpacing) / 2f + bounds.Top, 
		};
		return new Vector2(floor(num + (float)anchor.Offset.X), floor(num2 + (float)anchor.Offset.Y));
	}

	/// <summary>
	///   Calculates the absolute pixel position of a rectangle in unified coordinates
	/// </summary>
	/// <param name="bounds">Bounds of the drawing area in pixels</param>
	/// <param name="destination">Destination rectangle in unified coordinates</param>
	/// <returns>
	///   The destination rectangle converted to absolute pixel coordinates
	/// </returns>
	private static Rectangle calculateDestinationRectangle(ref RectangleF bounds, ref UniRectangle destination)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)(bounds.X + destination.Location.X.Offset);
		num += (int)(bounds.Width * destination.Location.X.Fraction);
		int num2 = (int)(bounds.Y + destination.Location.Y.Offset);
		num2 += (int)(bounds.Height * destination.Location.Y.Fraction);
		int num3 = (int)destination.Size.X.Offset;
		num3 += (int)(bounds.Width * destination.Size.X.Fraction);
		int num4 = (int)destination.Size.Y.Offset;
		num4 += (int)(bounds.Height * destination.Size.Y.Fraction);
		return new Rectangle(num, num2, num3, num4);
	}

	/// <summary>Looks up the frame with the specified name</summary>
	/// <param name="frameName">Frame that will be looked up</param>
	/// <returns>The frame with the specified name</returns>
	private Frame lookupFrame(string frameName)
	{
		if (!frames.TryGetValue(frameName, out var value))
		{
			throw new ArgumentException("Unknown frame type: '" + frameName + "'", "frameName");
		}
		return value;
	}

	/// <summary>Removes the fractional part from the floating point value</summary>
	/// <param name="value">Value whose fractional part will be removed</param>
	/// <returns>The floating point value without its fractional part</returns>
	private static float floor(float value)
	{
		return (float)Math.Floor(value);
	}

	/// <summary>Loads a skin from the specified path</summary>
	/// <param name="skinStream">Stream containing the skin description</param>
	private void loadSkin(Stream skinStream)
	{
		XmlSchema schema;
		using (Stream schemaStream = getResourceStream("Resources.skin.xsd"))
		{
			schema = XmlHelper.LoadSchema(schemaStream);
		}
		XDocument skinDocument = XmlHelper.LoadDocument(schema, skinStream);
		loadResources(skinDocument);
		loadFrames(skinDocument);
	}

	/// <summary>Loads the resources contained in a skin document</summary>
	/// <param name="skinDocument">
	///   XML document containing a skin description whose resources will be loaded
	/// </param>
	private void loadResources(XDocument skinDocument)
	{
		XElement val = ((XContainer)((XContainer)skinDocument).Element(((XName)"skin"))).Element(((XName)"resources"));
		foreach (XElement item in ((XContainer)val).Descendants(((XName)"font")))
		{
			string value = item.Attribute(((XName)"name")).Value;
			string value2 = item.Attribute(((XName)"contentPath")).Value;
			SpriteFont value3 = contentManager.Load<SpriteFont>(value2);
			fonts.Add(value, value3);
		}
		foreach (XElement item2 in ((XContainer)val).Descendants(((XName)"bitmap")))
		{
			string value4 = item2.Attribute(((XName)"name")).Value;
			string value5 = item2.Attribute(((XName)"contentPath")).Value;
			Texture2D value6 = contentManager.Load<Texture2D>(value5);
			bitmaps.Add(value4, value6);
		}
	}

	/// <summary>Loads the frames contained in a skin document</summary>
	/// <param name="skinDocument">
	///   XML document containing a skin description whose frames will be loaded
	/// </param>
	private void loadFrames(XDocument skinDocument)
	{
		XElement val = ((XContainer)((XContainer)skinDocument).Element(((XName)"skin"))).Element(((XName)"frames"));
		foreach (XElement item in ((XContainer)val).Descendants(((XName)"frame")))
		{
			string value = item.Attribute(((XName)"name")).Value;
			Frame.Region[] regions = RegionListBuilder.Build(item, bitmaps);
			Frame.Text[] texts = TextListBuilder.Build(item, fonts);
			frames.Add(value, new Frame(regions, texts));
		}
	}

	/// <summary>Returns a stream for a resource embedded in this assembly</summary>
	/// <param name="resourceName">Name of the resource for which to get a stream</param>
	/// <returns>A stream for the specified embedded resource</returns>
	private static Stream getResourceStream(string resourceName)
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		callingAssembly.GetManifestResourceNames();
		return callingAssembly.GetManifestResourceStream(typeof(GuiManager), resourceName);
	}

	/// <summary>Converts a string in the style "#rrggbb" into a Color value</summary>
	/// <param name="color">String containing a hexadecimal color value</param>
	/// <returns>The equivalent color as a Color value</returns>
	private static Color colorFromString(string color)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		string text = color.Trim();
		int num = 0;
		if (text[0] == '#')
		{
			num++;
		}
		if (text.Length - num != 6 && text.Length - num != 8)
		{
			throw new ArgumentException("Invalid color format '" + color + "'", "color");
		}
		int num2 = Convert.ToInt32(text.Substring(num, 2), 16);
		int num3 = Convert.ToInt32(text.Substring(num + 2, 2), 16);
		int num4 = Convert.ToInt32(text.Substring(num + 4, 2), 16);
		int num5 = ((text.Length - num != 8) ? 255 : Convert.ToInt32(text.Substring(num + 6, 2), 16));
		return new Color((int)(byte)num2, (int)(byte)num3, (int)(byte)num4, (int)(byte)num5);
	}
}
