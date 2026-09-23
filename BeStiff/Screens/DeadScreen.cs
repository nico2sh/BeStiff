using Microsoft.Xna.Framework;

namespace Be_Stiff.Screens
{
	internal class DeadScreen : MenuScreen
	{
		private string levelName;

		public DeadScreen(string level)
			: base("Fail!")
		{
			base.IsPopup = true;
			levelName = level;
			MenuEntry menuEntry = new MenuEntry("Restart Level");
			MenuEntry menuEntry2 = new MenuEntry("Quit Game");
			menuEntry.Selected += Restart;
			menuEntry2.Selected += QuitGameMenuEntrySelected;
			AddMenuEntry(menuEntry);
			AddMenuEntry(menuEntry2);
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
