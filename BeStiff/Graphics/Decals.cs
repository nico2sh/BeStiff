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

			// Screen-space bounds of the drawn (rotated, scaled) decal, for culling.
			public Rectangle Bounds;
		}

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
				item.Bounds = ComputeBounds(item);
				decals.Add(item);
				if (DebugFlags.Dump)
				{
					System.Console.Error.WriteLine($"  decal {ogmoObject.Name} pos={ogmoObject.Position} size={ogmoObject.Width}x{ogmoObject.Height} origin={ogmoObject.Origin} rot={ogmoObject.Rotation} tex={ogmoObject.Texture.Width}x{ogmoObject.Texture.Height} scale={scale}");
				}
			}
		}

		private static Rectangle ComputeBounds(Decal decal)
		{
			Matrix transform = Matrix.CreateTranslation(-decal.Origin.X, -decal.Origin.Y, 0f) * Matrix.CreateScale(decal.Scale.X, decal.Scale.Y, 1f) * Matrix.CreateRotationZ(decal.Rotation) * Matrix.CreateTranslation(decal.Position.X, decal.Position.Y, 0f);
			Vector2 min = new Vector2(float.MaxValue);
			Vector2 max = new Vector2(float.MinValue);
			foreach (Vector2 corner in new[] { Vector2.Zero, new Vector2(decal.Rectangle.Width, 0f), new Vector2(0f, decal.Rectangle.Height), new Vector2(decal.Rectangle.Width, decal.Rectangle.Height) })
			{
				Vector2 p = Vector2.Transform(corner, transform);
				min = Vector2.Min(min, p);
				max = Vector2.Max(max, p);
			}
			return new Rectangle((int)min.X - 1, (int)min.Y - 1, (int)(max.X - min.X) + 2, (int)(max.Y - min.Y) + 2);
		}

		public void Draw()
		{
			Rectangle visible = GameElementsControl.Camera.VisibleArea(16);
			foreach (Decal decal in decals)
			{
				if (!visible.Intersects(decal.Bounds))
				{
					continue;
				}
				GameElementsControl.ScreenManager.SpriteBatch.Draw(decal.Texture, decal.Position, decal.Rectangle, Color.White, decal.Rotation, decal.Origin, decal.Scale, SpriteEffects.None, 0f);
			}
		}
	}
}
