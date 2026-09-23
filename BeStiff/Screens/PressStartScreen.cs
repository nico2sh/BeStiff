using System;
using EasyStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Screens
{
	internal class PressStartScreen : GameScreen
	{
		private IAsyncSaveDevice saveDevice;

		private Texture2D backgroundTexture;

		public override void LoadContent()
		{
			base.LoadContent();
			backgroundTexture = base.ScreenManager.Content.Load<Texture2D>("sprites\\MenuBackgrounds\\startscreen");
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
			base.Draw(gameTime);
		}

		public override void HandleInput(InputHelper input)
		{
			if (input.IsGamePadConnected())
			{
				Globals.ActiveControl = Globals.OptionControl;
			}
			else
			{
				Globals.ActiveControl = 1;
			}
			if (input.IsMenuSelect(base.ControllingPlayer, out var _))
			{
				PromptMe();
			}
		}

		private void PromptMe()
		{
			EasyStorageSettings.SetSupportedLanguages(Language.English);
			SharedSaveDevice sharedSaveDevice = new SharedSaveDevice();
			base.ScreenManager.Game.Components.Add(sharedSaveDevice);
			saveDevice = sharedSaveDevice;
			sharedSaveDevice.PromptForDevice();
			sharedSaveDevice.DeviceSelected += delegate(object s, EventArgs e)
			{
				Globals.SaveDevice = (SaveDevice)s;
				Globals.LoadOptions();
				base.ScreenManager.SetFullScreen(Globals.OptionFullScreen);
				Globals.LoadSaveGames();
				base.AudioManager.MusicVolume = (float)Globals.OptionMusicVolume / 10f;
				base.AudioManager.SoundVolume = (float)Globals.OptionSoundVolume / 10f;
				base.ScreenManager.AddScreen(new MainMenuScreen(), PlayerIndex.One);
			};
		}
	}
}
