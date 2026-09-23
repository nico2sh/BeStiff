using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA;
using OgmoXNA.Layers;

namespace Be_Stiff.Graphics
{
	internal class Decals
	{
		private struct Decal
		{
			public Texture2D Texture;

			public Vector2 Position;

			public Rectangle Rectangle;

			public Vector2 Origin;

			public float Rotation;

			public Vector2 Scale;
		}

		private static readonly bool DebugDump = System.Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null;

		private List<Decal> decals;

		public Decals()
		{
			decals = new List<Decal>();
		}

		public void Load(OgmoObjectLayer layer)
		{
			OgmoObject[] objects = layer.Objects;
			foreach (OgmoObject ogmoObject in objects)
			{
				// On legacy levels the art is drawn at 1/factor. Origins come from the
				// current templates and were only halved on non-resized axes, so
				// undo that to get the origin in texture pixels.
				float factor = ogmoObject.LegacyFactor;
				Vector2 scale = new Vector2(1f / factor);
				Decal item = new Decal
				{
					Texture = ogmoObject.Texture,
					Position = ogmoObject.Position,
					Rectangle = new Rectangle(0, 0, ogmoObject.Width * (int)factor, ogmoObject.Height * (int)factor),
					Origin = ogmoObject.Origin / ogmoObject.LegacyDrawScale,
					Rotation = MathHelper.ToRadians(ogmoObject.Rotation),
					Scale = scale
				};
				decals.Add(item);
				if (DebugDump)
				{
					System.Console.Error.WriteLine($"  decal {ogmoObject.Name} pos={ogmoObject.Position} size={ogmoObject.Width}x{ogmoObject.Height} origin={ogmoObject.Origin} rot={ogmoObject.Rotation} tex={ogmoObject.Texture.Width}x{ogmoObject.Texture.Height} scale={scale}");
				}
			}
		}

		public void Draw()
		{
			foreach (Decal decal in decals)
			{
				GameElementsControl.ScreenManager.SpriteBatch.Draw(decal.Texture, decal.Position, decal.Rectangle, Color.White, decal.Rotation, decal.Origin, decal.Scale, SpriteEffects.None, 0f);
			}
		}
	}
}
