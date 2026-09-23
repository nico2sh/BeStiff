using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal class ConfirmStartBoxScreen : GameScreen
	{
		private string message;

		private string infoText;

		private Texture2D gradientTexture;

		private Texture2D infoSign;

		private Texture2D frameBoxBackground;

		private Texture2D boxBackgroundTop;

		private Texture2D boxBackgroundMiddle;

		private Texture2D boxBackgroundBottom;

		private Vector2 boxPosition;

		private Vector2 textPadding;

		private Rectangle textBoxFrame;

		private Rectangle textBoxBackground;

		private Vector2 textBoxBackgroundPosition;

		private Color frameColor;

		private Color fontColor;

		public event EventHandler<PlayerIndexEventArgs> Start;

		public ConfirmStartBoxScreen(string message, string startMessage)
		{
			this.message = message;
			infoText = startMessage;
			textBoxFrame = new Rectangle(5, 5, 650, 100);
			textPadding = new Vector2(10f, -10f);
			base.IsPopup = true;
			base.TransitionOnTime = TimeSpan.FromSeconds(0.2);
			base.TransitionOffTime = TimeSpan.FromSeconds(0.2);
		}

		public override void LoadContent()
		{
			ContentManager content = base.ScreenManager.Game.Content;
			frameBoxBackground = content.Load<Texture2D>("sprites\\whitepixel");
			boxBackgroundTop = content.Load<Texture2D>("sprites\\menubackgrounds\\startBoxFrameTop");
			boxBackgroundMiddle = content.Load<Texture2D>("sprites\\menubackgrounds\\startBoxFrameMiddle");
			boxBackgroundBottom = content.Load<Texture2D>("sprites\\menubackgrounds\\startBoxFrameBottom");
			gradientTexture = content.Load<Texture2D>("sprites\\menubackgrounds\\gradient");
			infoSign = content.Load<Texture2D>("sprites\\menubackgrounds\\info");
			infoText = ParseText(infoText);
			textBoxFrame.Height = (int)(base.ScreenManager.Font.MeasureString(infoText).Y + textPadding.Y * 2f);
			boxPosition = new Vector2(base.ScreenManager.GraphicsDevice.Viewport.Width / 2, 0f) - new Vector2(textBoxFrame.Width / 2, -100f);
			textBoxFrame.X = (int)boxPosition.X;
			textBoxFrame.Y = (int)boxPosition.Y;
			textBoxBackground = new Rectangle(0, 0, textBoxFrame.Width, textBoxFrame.Height);
			textBoxBackgroundPosition = new Vector2(boxPosition.X, boxPosition.Y);
			frameColor = new Color(0, 0, 0, 50);
			fontColor = Color.LightGray;
		}

		private string ParseText(string text)
		{
			string text2 = string.Empty;
			string text3 = string.Empty;
			string[] array = text.Split(new char[1] { ' ' });
			string[] array2 = array;
			foreach (string text4 in array2)
			{
				if (base.ScreenManager.Font.MeasureString(text2 + text4).Length() > (float)textBoxFrame.Width - textPadding.X * 2f)
				{
					text3 = text3 + text2 + '\n';
					text2 = string.Empty;
				}
				text2 = text2 + text4 + ' ';
			}
			return text3 + text2;
		}

		public override void HandleInput(InputHelper input)
		{
			// Debug: when a level is auto-started (BESTIFF_LEVEL), skip the
			// "get ready" confirmation so unattended test runs proceed.
			bool autoStart = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BESTIFF_LEVEL"));
			if (input.IsMenuSelect(base.ControllingPlayer, out var playerIndex) || autoStart)
			{
				if (this.Start != null)
				{
					this.Start(this, new PlayerIndexEventArgs(playerIndex));
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
			base.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null);
			spriteBatch.Draw(gradientTexture, destinationRectangle, color);
			spriteBatch.DrawString(font, message, position, color);
			spriteBatch.Draw(infoSign, boxPosition - new Vector2(150f, 0f), Color.White);
			spriteBatch.Draw(boxBackgroundTop, textBoxBackgroundPosition - new Vector2(0f, 30f), Color.White);
			spriteBatch.Draw(boxBackgroundMiddle, textBoxBackgroundPosition, textBoxBackground, Color.White);
			spriteBatch.Draw(boxBackgroundBottom, textBoxBackgroundPosition + new Vector2(0f, textBoxBackground.Height), Color.White);
			spriteBatch.DrawString(font, infoText, boxPosition + textPadding, fontColor);
			spriteBatch.End();
		}
	}
}
