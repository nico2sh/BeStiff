using Microsoft.Xna.Framework;

namespace Be_Stiff.Screens
{
	internal class SelectAreaScreen : MenuScreen
	{
		private MenuEntry[] selectAreaMenuEntry;

		public SelectAreaScreen()
			: base("Select Area")
		{
			selectAreaMenuEntry = new MenuEntry[Globals.LevelsManager.Areas.Count];
			int num = 0;
			foreach (Area area in Globals.LevelsManager.Areas)
			{
				selectAreaMenuEntry[num] = new MenuEntry(area.Name);
				selectAreaMenuEntry[num].Selected += SelectAreaMenuEntrySelected;
				AddMenuEntry(selectAreaMenuEntry[num]);
			}
			MenuEntry menuEntry = new MenuEntry("Back");
			menuEntry.Selected += base.OnCancel;
			AddMenuEntry(menuEntry);
			SetMenuEntryText();
		}

		public override void LoadContent()
		{
			base.LoadContent();
		}

		private void SetMenuEntryText()
		{
		}

		protected override void OnCancel(PlayerIndex playerIndex)
		{
			base.OnCancel(playerIndex);
		}

		private void SelectAreaMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			MenuEntry menuEntry = (MenuEntry)sender;
			base.ScreenManager.AddScreen(new SelectLevelScreen(Globals.LevelsManager.GetArea(menuEntry.Text)), e.PlayerIndex);
			SetMenuEntryText();
		}
	}
}
