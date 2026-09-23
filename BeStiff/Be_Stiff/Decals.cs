using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA;
using OgmoXNA.Layers;

namespace Be_Stiff
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
				Decal item = new Decal
				{
					Texture = ogmoObject.Texture,
					Position = ogmoObject.Position,
					Rectangle = new Rectangle(0, 0, ogmoObject.Width, ogmoObject.Height),
					Origin = ogmoObject.Origin,
					Rotation = MathHelper.ToRadians(ogmoObject.Rotation)
				};
				decals.Add(item);
			}
		}

		public void Draw()
		{
			foreach (Decal decal in decals)
			{
				GameElementsControl.ScreenManager.SpriteBatch.Draw(decal.Texture, decal.Position, decal.Rectangle, Color.White, decal.Rotation, decal.Origin, 1f, SpriteEffects.None, 0f);
			}
		}
	}
}
