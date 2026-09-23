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
				Vector2 scale = ogmoObject.LegacyDrawScale;
				Decal item = new Decal
				{
					Texture = ogmoObject.Texture,
					Position = ogmoObject.Position,
					// On legacy levels the object's size was halved but the art was
					// not: take the full-size region of the texture and draw it scaled.
					Rectangle = new Rectangle(0, 0, (int)(ogmoObject.Width / scale.X), (int)(ogmoObject.Height / scale.Y)),
					Origin = ogmoObject.Origin / scale,
					Rotation = MathHelper.ToRadians(ogmoObject.Rotation),
					Scale = scale
				};
				decals.Add(item);
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
