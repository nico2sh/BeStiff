using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class MainMenuScreen : MenuScreen
	{
		public MainMenuScreen()
			: base("Main Menu")
		{
			MenuEntry menuEntry = new MenuEntry("Play Game");
			MenuEntry menuEntry2 = new MenuEntry("Options");
			MenuEntry menuEntry3 = new MenuEntry("Exit");
			menuEntry.Selected += PlayGameMenuEntrySelected;
			menuEntry2.Selected += OptionsMenuEntrySelected;
			menuEntry3.Selected += base.OnCancel;
			AddMenuEntry(menuEntry);
			AddMenuEntry(menuEntry2);
			AddMenuEntry(menuEntry3);
		}

		public override void LoadContent()
		{
			base.LoadContent();
			base.AudioManager.PlaySong("actionSong", loop: true);
		}

		private void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			base.AudioManager.StopSong();
			base.ScreenManager.AddScreen(new SelectAreaScreen(), e.PlayerIndex);
		}

		private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			base.ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
		}

		protected override void OnCancel(PlayerIndex playerIndex)
		{
			MessageBoxScreen messageBoxScreen = new MessageBoxScreen("Are you sure you want to exit this sample?");
			messageBoxScreen.Accepted += ConfirmExitMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen, playerIndex);
		}

		private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
		{
			base.ScreenManager.Game.Exit();
		}
	}
}
