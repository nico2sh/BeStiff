using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal class SampleGameplayScreen : GameScreen
	{
		private ContentManager content;

		private SpriteFont gameFont;

		private Vector2 playerPosition = new Vector2(100f, 100f);

		private Vector2 enemyPosition = new Vector2(100f, 100f);

		private Random random = new Random();

		public SampleGameplayScreen()
		{
			base.TransitionOnTime = TimeSpan.FromSeconds(1.5);
			base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}

		public override void LoadContent()
		{
			if (content == null)
			{
				content = new CaseInsensitiveContentManager(base.ScreenManager.Game.Services, "Content");
			}
			gameFont = content.Load<SpriteFont>("gamefont");
			Thread.Sleep(1000);
			base.ScreenManager.Game.ResetElapsedTime();
		}

		public override void UnloadContent()
		{
			content.Unload();
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
			if (base.IsActive)
			{
				enemyPosition.X += (float)(random.NextDouble() - 0.5) * 10f;
				enemyPosition.Y += (float)(random.NextDouble() - 0.5) * 10f;
				enemyPosition = Vector2.Lerp(value2: new Vector2(200f, 200f), value1: enemyPosition, amount: 0.05f);
			}
		}

		public override void HandleInput(InputHelper input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			_ = base.ControllingPlayer.Value;
			if (input.IsPauseGame(base.ControllingPlayer))
			{
				base.ScreenManager.AddScreen(new PauseMenuScreen("uhm"), base.ControllingPlayer);
			}
			else
			{
				_ = Vector2.Zero;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0f, 0);
			SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
			spriteBatch.Begin();
			spriteBatch.DrawString(gameFont, "// TODO", playerPosition, Color.Green);
			spriteBatch.DrawString(gameFont, "Insert Gameplay Here", enemyPosition, Color.DarkRed);
			spriteBatch.End();
			if (base.TransitionPosition > 0f)
			{
				base.ScreenManager.FadeBackBufferToBlack(255 - base.TransitionAlpha);
			}
		}
	}
}
