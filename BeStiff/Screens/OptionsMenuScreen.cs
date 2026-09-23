using Microsoft.Xna.Framework;

namespace Be_Stiff.Screens
{
	internal class OptionsMenuScreen : MenuScreen
	{
		private MenuEntry resolutionMenuEntry;

		private MenuEntry fullScreenMenuEntry;

		private MenuEntry soundVolumeMenuEntry;

		private MenuEntry musicVolumeMenuEntry;

		private MenuEntry controlsMenuEntry;

		private MenuEntry selectedControlMenuEntry;

		private int soundVolumeValue = Globals.OptionSoundVolume;

		private int musicVolumeValue = Globals.OptionMusicVolume;

		private bool gamePadConnected;

		private static string[] resolutions = new string[3] { "4:3", "16:9", "16:10" };

		private static int currentResolution = 0;

		private static string[] controllers = new string[2] { "GamePad", "Keyboard + Mouse" };

		private static bool fullScreen = false;

		public OptionsMenuScreen()
			: base("Options")
		{
			resolutionMenuEntry = new MenuEntry(string.Empty);
			fullScreenMenuEntry = new MenuEntry(string.Empty);
			soundVolumeMenuEntry = new MenuEntry(string.Empty);
			musicVolumeMenuEntry = new MenuEntry(string.Empty);
			controlsMenuEntry = new MenuEntry("Configure Controls");
			selectedControlMenuEntry = new MenuEntry(string.Empty);
			MenuEntry menuEntry = new MenuEntry("Back");
			resolutionMenuEntry.Selected += ResolutionMenuEntrySelected;
			fullScreenMenuEntry.Selected += FullScreenMenuEntrySelected;
			soundVolumeMenuEntry.Selected += SoundVolumeMenuEntrySelected;
			musicVolumeMenuEntry.Selected += MusicVolumeMenuEntrySelected;
			controlsMenuEntry.Selected += ControlsMenuEntrySelected;
			selectedControlMenuEntry.Selected += SelectedControlMenuEntrySelected;
			menuEntry.Selected += base.OnCancel;
			AddMenuEntry(resolutionMenuEntry);
			AddMenuEntry(fullScreenMenuEntry);
			AddMenuEntry(soundVolumeMenuEntry);
			AddMenuEntry(musicVolumeMenuEntry);
			AddMenuEntry(controlsMenuEntry);
			AddMenuEntry(selectedControlMenuEntry);
			AddMenuEntry(menuEntry);
			currentResolution = Globals.OptionCurrentScreenResolution;
			fullScreen = Globals.OptionFullScreen;
			SetMenuEntryText();
		}

		public override void LoadContent()
		{
			base.LoadContent();
		}

		private void SetMenuEntryText()
		{
			resolutionMenuEntry.Text = "Aspect Ratio: " + resolutions[currentResolution];
			fullScreenMenuEntry.Text = "Full Screen: " + (fullScreen ? "on" : "off");
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
			Globals.OptionCurrentScreenResolution = currentResolution;
			Globals.OptionFullScreen = fullScreen;
			Globals.SaveOptions();
			base.OnCancel(playerIndex);
		}

		private void ResolutionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			currentResolution = (currentResolution + 1) % resolutions.Length;
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
