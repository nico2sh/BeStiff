using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Screens
{
	internal class BackgroundScreen : GameScreen
	{
		private ContentManager content;

		private Texture2D backgroundTexture;

		public BackgroundScreen()
		{
			base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
			base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}

		public override void LoadContent()
		{
			if (content == null)
			{
				content = new CaseInsensitiveContentManager(base.ScreenManager.Game.Services, "Content");
			}
			backgroundTexture = content.Load<Texture2D>("sprites\\MenuBackgrounds\\mainscreenbackground");
		}

		public override void UnloadContent()
		{
			content.Unload();
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
			Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
			Rectangle destinationRectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);
			byte transitionAlpha = base.TransitionAlpha;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
			spriteBatch.Draw(backgroundTexture, destinationRectangle, new Color(transitionAlpha, transitionAlpha, transitionAlpha));
			spriteBatch.End();
		}
	}
}
