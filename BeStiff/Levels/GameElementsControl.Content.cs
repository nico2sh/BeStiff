using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using Krypton;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OgmoXNA;

namespace Be_Stiff.Levels
{
	internal static partial class GameElementsControl
	{
		public static GameSprite GetSprite(string textureName)
		{
			gameSprites.TryGetValue(textureName, out var value);
			return value;
		}

		/// <summary>
		/// Intended world size of object art, keyed by the lower-case base name
		/// of the Ogmo template's texture file. Filled when a level loads.
		/// </summary>
		private static readonly Dictionary<string, Point> templateSizes = new Dictionary<string, Point>();

		private static bool legacyLevel;

		public static void RegisterTemplateSizes(OgmoXNA.OgmoProject project, bool legacy)
		{
			legacyLevel = legacy;
			templateSizes.Clear();
			foreach (var template in project.ObjectTemplates)
			{
				if (string.IsNullOrEmpty(template.TextureFile))
				{
					continue;
				}
				string key = System.IO.Path.GetFileNameWithoutExtension(template.TextureFile).ToLowerInvariant();
				templateSizes[key] = new Point(template.Width, template.Height);
			}
			// Rescale sprites loaded before the level's templates were known.
			foreach (var pair in gameSprites)
			{
				pair.Value.Scale = pair.Value.TexturePath != null ? LegacySpriteScale(pair.Value.TexturePath, pair.Value.Texture) : 1f;
			}
		}

		/// <summary>Art that was redrawn at twice its size for the current scale.</summary>
		private static readonly string[] RedrawnSprites =
		{
			"slopeleft", "sloperight", "exitdoor", "barrel1", "barrel2", "barrel3", "barrel4",
			"chairback", "chairleg", "chairseat", "tableleg", "tablesurface",
		};

		public static bool IsLegacyLevel => legacyLevel;

		/// <summary>
		/// Draw scale for sprites whose art does not match the level's scale.
		/// Current-scale levels: 2 for art that was never redrawn and is half
		/// the size its Ogmo template expects. Legacy levels: 0.5 for art in
		/// <see cref="RedrawnSprites" />. 1 otherwise.
		/// </summary>
		private static float LegacySpriteScale(string texturePath, Texture2D texture)
		{
			if (legacyLevel)
			{
				// Old-scale level: old art as is, redrawn (2x) art at half.
				string baseName = texturePath.Replace('\\', '/');
				baseName = baseName.Substring(baseName.LastIndexOf('/') + 1).ToLowerInvariant();
				return System.Array.IndexOf(RedrawnSprites, baseName) >= 0 ? 0.5f : 1f;
			}
			string name = texturePath.Replace('\\', '/');
			name = name.Substring(name.LastIndexOf('/') + 1).ToLowerInvariant();
			Point size = Point.Zero;
			if (!templateSizes.TryGetValue(name, out size))
			{
				foreach (var pair in templateSizes)
				{
					if (pair.Key.Length >= 4 && name.StartsWith(pair.Key))
					{
						size = pair.Value;
						break;
					}
				}
			}
			if (size == Point.Zero)
			{
				return 1f;
			}
			float rx = (float)size.X / texture.Width;
			float ry = (float)size.Y / texture.Height;
			if (rx > 1.7f && rx < 2.3f && ry > 1.7f && ry < 2.3f)
			{
				return 2f;
			}
			return 1f;
		}

		public static void LoadSprite(string textureName, string texturePath)
		{
			if (!gameSprites.ContainsKey(textureName))
			{
				Texture2D fromTexture = GameScreen.Content.Load<Texture2D>(texturePath);
				GameSprite value = new GameSprite(fromTexture);
				value.TexturePath = texturePath;
				value.Scale = LegacySpriteScale(texturePath, fromTexture);
				if (value.Scale != 1f && Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null)
				{
					Console.Error.WriteLine($"  sprite {textureName} ({texturePath}) {fromTexture.Width}x{fromTexture.Height} drawn at x{value.Scale}");
				}
				gameSprites.Add(textureName, value);
			}
		}

		public static SpriteFont GetFont(string fontName)
		{
			if (fonts.ContainsKey(fontName))
			{
				return fonts[fontName];
			}
			SpriteFont spriteFont = GameScreen.Content.Load<SpriteFont>(fontName);
			fonts.Add(fontName, spriteFont);
			return spriteFont;
		}

		public static void LoadFont(string fontName, string fontPath)
		{
			if (!fonts.ContainsKey(fontName))
			{
				SpriteFont value = GameScreen.Content.Load<SpriteFont>(fontPath);
				fonts.Add(fontName, value);
			}
		}
	}
}
