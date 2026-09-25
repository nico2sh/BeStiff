using Microsoft.Xna.Framework;

namespace Be_Stiff.Screens
{
	internal class OptionsMenuScreen : MenuScreen
	{
		private MenuEntry viewBlurMenuEntry;

		private MenuEntry fullScreenMenuEntry;

		private MenuEntry soundVolumeMenuEntry;

		private MenuEntry musicVolumeMenuEntry;

		private MenuEntry controlsMenuEntry;

		private MenuEntry selectedControlMenuEntry;

		private int soundVolumeValue = Globals.OptionSoundVolume;

		private int musicVolumeValue = Globals.OptionMusicVolume;

		private bool gamePadConnected;

		private static string[] controllers = new string[2] { "GamePad", "Keyboard + Mouse" };

		private static bool fullScreen = false;

		public OptionsMenuScreen()
			: base("Options")
		{
			viewBlurMenuEntry = new MenuEntry(string.Empty);
			fullScreenMenuEntry = new MenuEntry(string.Empty);
			soundVolumeMenuEntry = new MenuEntry(string.Empty);
			musicVolumeMenuEntry = new MenuEntry(string.Empty);
			controlsMenuEntry = new MenuEntry("Configure Controls");
			selectedControlMenuEntry = new MenuEntry(string.Empty);
			MenuEntry menuEntry = new MenuEntry("Back");
			viewBlurMenuEntry.Selected += ViewBlurMenuEntrySelected;
			fullScreenMenuEntry.Selected += FullScreenMenuEntrySelected;
			soundVolumeMenuEntry.Selected += SoundVolumeMenuEntrySelected;
			musicVolumeMenuEntry.Selected += MusicVolumeMenuEntrySelected;
			controlsMenuEntry.Selected += ControlsMenuEntrySelected;
			selectedControlMenuEntry.Selected += SelectedControlMenuEntrySelected;
			menuEntry.Selected += base.OnCancel;
			AddMenuEntry(fullScreenMenuEntry);
			AddMenuEntry(viewBlurMenuEntry);
			AddMenuEntry(soundVolumeMenuEntry);
			AddMenuEntry(musicVolumeMenuEntry);
			AddMenuEntry(controlsMenuEntry);
			AddMenuEntry(selectedControlMenuEntry);
			AddMenuEntry(menuEntry);
			fullScreen = Globals.OptionFullScreen;
			SetMenuEntryText();
		}

		public override void LoadContent()
		{
			base.LoadContent();
		}

		private void SetMenuEntryText()
		{
			fullScreenMenuEntry.Text = "Full Screen: " + (fullScreen ? "on" : "off");
			viewBlurMenuEntry.Text = "View Blur: " + (Globals.OptionViewBlur ? "on" : "off");
			soundVolumeMenuEntry.Text = "Sound Volume: " + soundVolumeValue;
			musicVolumeMenuEntry.Text = "Music Volume: " + musicVolumeValue;
			selectedControlMenuEntry.Text = "Controller: " + controllers[Globals.ActiveControl];
		}

		public override void HandleInput(InputHelper input)
		{
			gamePadConnected = input.IsGamePadConnected();
			base.HandleInput(input);
		}

		protected override void OnCancel(PlayerIndex playerIndex)
		{
			Globals.OptionFullScreen = fullScreen;
			Globals.SaveOptions();
			base.OnCancel(playerIndex);
		}

		private void ViewBlurMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			Globals.OptionViewBlur = !Globals.OptionViewBlur;
			SetMenuEntryText();
		}

		private void FullScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			fullScreen = !fullScreen;
			base.ScreenManager.SetFullScreen(fullScreen);
			SetMenuEntryText();
		}

		private void SoundVolumeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			soundVolumeValue++;
			if (soundVolumeValue > 10)
			{
				soundVolumeValue = 0;
			}
			Globals.OptionSoundVolume = soundVolumeValue;
			base.AudioManager.SoundVolume = (float)Globals.OptionSoundVolume / 10f;
			SetMenuEntryText();
		}

		private void MusicVolumeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			musicVolumeValue++;
			if (musicVolumeValue > 10)
			{
				musicVolumeValue = 0;
			}
			Globals.OptionMusicVolume = musicVolumeValue;
			base.AudioManager.MusicVolume = (float)Globals.OptionMusicVolume / 10f;
			SetMenuEntryText();
		}

		private void SelectedControlMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			if (gamePadConnected)
			{
				Globals.OptionControl = (Globals.OptionControl + 1) % controllers.Length;
				Globals.ActiveControl = Globals.OptionControl;
			}
			else
			{
				Globals.ActiveControl = 1;
			}
			SetMenuEntryText();
		}

		private void ControlsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			if (Globals.ActiveControl == 1)
			{
				base.ScreenManager.AddScreen(new ControlKeyboardMenuScreen(), e.PlayerIndex);
			}
			else
			{
				base.ScreenManager.AddScreen(new ControlGamePadMenuScreen(), e.PlayerIndex);
			}
		}
	}
}
