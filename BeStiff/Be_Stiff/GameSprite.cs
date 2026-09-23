using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	public class GameSprite
	{
		private Texture2D texture;

		private Vector2 origin;

		private int rectangleWidth;

		private int rectangleHeight;

		private Rectangle rectangle;

		public Texture2D Texture => texture;

		public Vector2 Origin => origin;

		/// <summary>
		/// Draw scale. Some object art shipped at the old half-size scale;
		/// such sprites are drawn at 2 so they match the level geometry.
		/// Rectangles passed to Draw are in world pixels and mapped back to
		/// texture space with <see cref="Src"/>.
		/// </summary>
		public float Scale { get; set; } = 1f;

		/// <summary>Content path the sprite was loaded from (for rescaling per level).</summary>
		public string TexturePath { get; set; }

		public int Width => (int)(rectangleWidth * Scale);

		public int Height => (int)(rectangleHeight * Scale);

		private Rectangle Src(Rectangle worldRect)
		{
			if (Scale == 1f)
			{
				return worldRect;
			}
			return new Rectangle((int)(worldRect.X / Scale), (int)(worldRect.Y / Scale), (int)System.Math.Round(worldRect.Width / Scale), (int)System.Math.Round(worldRect.Height / Scale));
		}

		public Rectangle Rectangle => rectangle;

		public static GameSpriteVariables GetDefaultVariables()
		{
			return new GameSpriteVariables
			{
				spriteEffects = SpriteEffects.None,
				rotation = 0f,
				scale = new Vector2(1f, 1f),
				offset = new Vector2(0f, 0f),
				color = Color.White
			};
		}

		public GameSprite(Texture2D fromTexture)
		{
			texture = fromTexture;
			origin = new Vector2(fromTexture.Width / 2, fromTexture.Height / 2);
			rectangleWidth = fromTexture.Width;
			rectangleHeight = fromTexture.Height;
			rectangle = new Rectangle(0, 0, rectangleWidth, rectangleHeight);
		}

		public void Draw(Vector2 screenPos)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, Rectangle customRectangle)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Src(customRectangle), Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, Rectangle customRectangle, Color color)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Src(customRectangle), color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, GameSpriteVariables sv)
		{
			Vector2 position = screenPos + Vector2.Transform(sv.offset, Matrix.CreateRotationZ(sv.rotation));
			position.X = (int)position.X;
			position.Y = (int)position.Y;
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, position, Rectangle, sv.color, sv.rotation, Origin, sv.scale * Scale, sv.spriteEffects, 0f);
		}

		public void Draw(Vector2 screenPos, GameSpriteVariables sv, double fromTime, double timeToDisappear)
		{
			float num = MathHelper.Clamp((float)((GameElementsControl.CurrentTimeInMS - fromTime) / timeToDisappear), 0f, 1f);
			byte alpha = (byte)((1f - num) * 255f);
			Color color = new Color((int)sv.color.R, (int)sv.color.G, (int)sv.color.B, (int)alpha);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos + Vector2.Transform(sv.offset, Matrix.CreateRotationZ(sv.rotation)), Rectangle, color, sv.rotation, Origin, sv.scale * Scale, sv.spriteEffects, 0f);
		}

		public void DrawScaled(Vector2 screenPos, GameSpriteVariables sv, double fromTime, double timeToDisappear)
		{
			double num = GameElementsControl.CurrentTimeInMS - fromTime;
			Vector2 scale = sv.scale;
			if (num < 100.0)
			{
				scale.Y *= (float)num / 70f;
			}
			else if (timeToDisappear - num < 50.0)
			{
				scale.Y *= (float)(timeToDisappear - num) / 30f;
			}
			MathHelper.Clamp((float)(num / timeToDisappear), 0f, 1f);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos + Vector2.Transform(sv.offset, Matrix.CreateRotationZ(sv.rotation)), Rectangle, sv.color, sv.rotation, Origin, scale * Scale, sv.spriteEffects, 0f);
		}

		public void DrawScaled(Vector2 screenPos, float rotation, double fromTime, double timeToDisappear)
		{
			double num = GameElementsControl.CurrentTimeInMS - fromTime;
			Vector2 one = Vector2.One;
			int alpha = 255;
			if (num < 100.0)
			{
				one.Y *= (float)num / 70f;
			}
			else if (timeToDisappear - num < 100.0)
			{
				alpha = (int)(255.0 * ((timeToDisappear - num) / 100.0));
			}
			Color color = new Color(255, 255, 255, alpha);
			MathHelper.Clamp((float)(num / timeToDisappear), 0f, 1f);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, color, rotation, Origin, one * Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, Vector2 scale)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, Color.White, 0f, Origin, scale * Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, float rotation)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, Color.White, rotation, Origin, Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, float rotation, Rectangle customRectangle)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Src(customRectangle), Color.White, rotation, Origin, Scale, SpriteEffects.None, 0f);
		}

		public void DrawNoOrigin(Vector2 screenPos, float rotation, Rectangle customRectangle)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Src(customRectangle), Color.White, rotation, Vector2.Zero, Scale, SpriteEffects.None, 0f);
		}

		public void DrawShadowed(Vector2 screenPos, float rotation)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, GameElementsControl.ShadowColor, rotation, Origin, Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, float rotation, SpriteEffects se)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, Color.White, rotation, Origin, Scale, se, 0f);
		}

		public void Draw(Vector2 screenPos, float rotation, float scale)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, Color.White, rotation, Origin, scale * Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, float rotation, Vector2 scale)
		{
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, Color.White, rotation, Origin, scale * Scale, SpriteEffects.None, 0f);
		}

		public void Draw(Vector2 screenPos, double fromTime, double timeToDisappear)
		{
			float num = MathHelper.Clamp((float)((GameElementsControl.CurrentTimeInMS - fromTime) / timeToDisappear), 0f, 1f);
			byte alpha = (byte)((1f - num) * 255f);
			Color color = new Color(255, 255, 255, (int)alpha);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(Texture, screenPos, Rectangle, color, 0f, Origin, Scale, SpriteEffects.None, 0f);
		}
	}
}
