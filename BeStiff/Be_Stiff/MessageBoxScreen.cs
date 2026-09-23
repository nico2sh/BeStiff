using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal class MessageBoxScreen : GameScreen
	{
		private string message;

		private Texture2D gradientTexture;

		public event EventHandler<PlayerIndexEventArgs> Accepted;

		public event EventHandler<PlayerIndexEventArgs> Cancelled;

		public MessageBoxScreen(string message)
			: this(message, includeUsageText: true)
		{
		}

		public MessageBoxScreen(string message, bool includeUsageText)
		{
			if (includeUsageText)
			{
				this.message = message + "\nA button, Space, Enter = ok\nB button, Esc = cancel";
			}
			else
			{
				this.message = message;
			}
			base.IsPopup = true;
			base.TransitionOnTime = TimeSpan.FromSeconds(0.2);
			base.TransitionOffTime = TimeSpan.FromSeconds(0.2);
		}

		public override void LoadContent()
		{
			ContentManager content = base.ScreenManager.Game.Content;
			gradientTexture = content.Load<Texture2D>("sprites\\menubackgrounds\\gradient");
		}

		public override void HandleInput(InputHelper input)
		{
			if (input.IsMenuSelect(base.ControllingPlayer, out var playerIndex))
			{
				if (this.Accepted != null)
				{
					this.Accepted(this, new PlayerIndexEventArgs(playerIndex));
				}
				ExitScreen();
			}
			else if (input.IsMenuCancel(base.ControllingPlayer, out playerIndex))
			{
				if (this.Cancelled != null)
				{
					this.Cancelled(this, new PlayerIndexEventArgs(playerIndex));
				}
				ExitScreen();
			}
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
			SpriteFont font = base.ScreenManager.Font;
			base.ScreenManager.FadeBackBufferToBlack(base.TransitionAlpha * 2 / 3);
			Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
			Vector2 vector = new Vector2(viewport.Width, viewport.Height);
			Vector2 vector2 = font.MeasureString(message);
			Vector2 position = (vector - vector2) / 2f;
			Rectangle destinationRectangle = new Rectangle((int)position.X - 32, (int)position.Y - 16, (int)vector2.X + 64, (int)vector2.Y + 32);
			Color color = new Color(255, 255, 255, (int)base.TransitionAlpha);
			spriteBatch.Begin();
			spriteBatch.Draw(gradientTexture, destinationRectangle, color);
			spriteBatch.DrawString(font, message, position, color);
			spriteBatch.End();
		}
	}
}
