using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal class LoadingScreen : GameScreen
	{
		private bool loadingIsSlow;

		private bool otherScreensAreGone;

		private GameScreen[] screensToLoad;

		private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow, GameScreen[] screensToLoad)
		{
			this.loadingIsSlow = loadingIsSlow;
			this.screensToLoad = screensToLoad;
			base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		}

		public static void Load(ScreenManager screenManager, bool loadingIsSlow, PlayerIndex? controllingPlayer, params GameScreen[] screensToLoad)
		{
			GameScreen[] screens = screenManager.GetScreens();
			foreach (GameScreen gameScreen in screens)
			{
				gameScreen.ExitScreen();
			}
			LoadingScreen screen = new LoadingScreen(screenManager, loadingIsSlow, screensToLoad);
			screenManager.AddScreen(screen, controllingPlayer);
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
			if (!otherScreensAreGone)
			{
				return;
			}
			base.ScreenManager.RemoveScreen(this);
			GameScreen[] array = screensToLoad;
			foreach (GameScreen gameScreen in array)
			{
				if (gameScreen != null)
				{
					base.ScreenManager.AddScreen(gameScreen, base.ControllingPlayer);
				}
			}
			base.ScreenManager.Game.ResetElapsedTime();
		}

		public override void Draw(GameTime gameTime)
		{
			if (base.ScreenState == ScreenState.Active && base.ScreenManager.GetScreens().Length == 1)
			{
				otherScreensAreGone = true;
			}
			if (loadingIsSlow)
			{
				SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
				SpriteFont font = base.ScreenManager.Font;
				Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
				Vector2 vector = new Vector2(viewport.Width, viewport.Height);
				Vector2 vector2 = font.MeasureString("Loading...");
				Vector2 position = (vector - vector2) / 2f;
				Color color = new Color(255, 255, 255, (int)base.TransitionAlpha);
				spriteBatch.Begin();
				spriteBatch.DrawString(font, "Loading...", position, color);
				spriteBatch.End();
			}
		}
	}
}
