using Microsoft.Xna.Framework;

namespace Be_Stiff.Screens
{
	internal class PauseMenuScreen : MenuScreen
	{
		private string levelName;

		public PauseMenuScreen(string level)
			: base("Paused")
		{
			base.IsPopup = true;
			levelName = level;
			MenuEntry menuEntry = new MenuEntry("Resume Game");
			MenuEntry menuEntry2 = new MenuEntry("Options");
			MenuEntry menuEntry3 = new MenuEntry("Restart Level");
			MenuEntry menuEntry4 = new MenuEntry("Quit Game");
			menuEntry.Selected += base.OnCancel;
			menuEntry2.Selected += OptionsMenuEntrySelected;
			menuEntry3.Selected += Restart;
			menuEntry4.Selected += QuitGameMenuEntrySelected;
			AddMenuEntry(menuEntry);
			AddMenuEntry(menuEntry2);
			AddMenuEntry(menuEntry3);
			AddMenuEntry(menuEntry4);
		}

		private void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			MessageBoxScreen messageBoxScreen = new MessageBoxScreen("Are you sure you want to quit this game?");
			messageBoxScreen.Accepted += ConfirmQuitMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
		}

		private void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(base.ScreenManager, true, null, new BackgroundScreen(), new MainMenuScreen());
		}

		private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			base.ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
		}

		private void Restart(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(base.ScreenManager, true, e.PlayerIndex, new GameplayScreen(levelName));
		}

		public override void Draw(GameTime gameTime)
		{
			base.ScreenManager.FadeBackBufferToBlack(base.TransitionAlpha * 2 / 3);
			base.Draw(gameTime);
		}
	}
}
