using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal class SelectLevelScreen : MenuScreen
	{
		private MenuEntry[] selectAreaMenuEntry;

		private Area currentArea;

		private int selectedLevel;

		private bool areaLocked;

		public SelectLevelScreen(Area area)
			: base("Select Level")
		{
			selectAreaMenuEntry = new MenuEntry[Globals.LevelsManager.Areas.Count];
			selectedLevel = 0;
			currentArea = area;
			MenuEntry menuEntry = new MenuEntry("");
			MenuEntry menuEntry2 = new MenuEntry("Back");
			menuEntry.Selected += SelectLevel;
			menuEntry2.Selected += base.OnCancel;
			menuEntry2.SetCustomPosition(base.BasePosition + new Vector2(0f, 400f));
			AddMenuEntry(menuEntry);
			AddMenuEntry(menuEntry2);
			SetMenuEntryText();
		}

		public override void LoadContent()
		{
			base.LoadContent();
			int num = -1;
			areaLocked = true;
			for (int i = 0; i < currentArea.Levels.Count; i++)
			{
				if (currentArea.Levels[i].Unlocked)
				{
					num = i;
					areaLocked = false;
					break;
				}
			}
			selectedLevel = num;
		}

		public override void HandleInput(InputHelper input)
		{
			base.HandleInput(input);
			if (selectedEntry == 0)
			{
				PlayerIndex playerIndex;
				if (input.IsMenuRight(base.ControllingPlayer))
				{
					SelectNextLevel();
				}
				else if (input.IsMenuLeft(base.ControllingPlayer))
				{
					SelectPreviousLevel();
				}
				else if (!input.IsMenuSelect(base.ControllingPlayer, out playerIndex))
				{
				}
			}
			else if (input.IsMenuRight(base.ControllingPlayer))
			{
				SelectNextLevel();
				selectedEntry = 0;
			}
			else if (input.IsMenuLeft(base.ControllingPlayer))
			{
				SelectPreviousLevel();
				selectedEntry = 0;
			}
		}

		private void SelectNextLevel()
		{
			if (areaLocked)
			{
				return;
			}
			selectedLevel++;
			if (selectedLevel >= currentArea.Levels.Count)
			{
				selectedLevel = 0;
			}
			while (!currentArea.Levels[selectedLevel].Unlocked)
			{
				selectedLevel++;
				if (selectedLevel >= currentArea.Levels.Count)
				{
					selectedLevel = 0;
				}
			}
		}

		private void SelectPreviousLevel()
		{
			if (areaLocked)
			{
				return;
			}
			selectedLevel--;
			if (selectedLevel < 0)
			{
				selectedLevel = currentArea.Levels.Count - 1;
			}
			while (!currentArea.Levels[selectedLevel].Unlocked)
			{
				selectedLevel--;
				if (selectedLevel < 0)
				{
					selectedLevel = currentArea.Levels.Count - 1;
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			base.ScreenManager.SpriteBatch.Begin();
			float num = 0.8f;
			float num2 = 0.5f;
			for (int i = 0; i < currentArea.Levels.Count; i++)
			{
				float num3 = 80f;
				LevelSelection levelSelection = currentArea.Levels[i];
				Vector2 vector = base.BasePosition + levelSelection.ScreenPosition;
				Vector2 position = vector + new Vector2((0f - base.ScreenManager.Font.MeasureString(levelSelection.ScoreString).Y) * num2, num3 + base.ScreenManager.Font.MeasureString(levelSelection.Name).Y * num);
				Vector2 position2 = vector + new Vector2(num3 - base.ScreenManager.Font.MeasureString(levelSelection.TimeString).X * num2, num3 + base.ScreenManager.Font.MeasureString(levelSelection.Name).Y * num + 5f);
				Vector2 position3 = vector + new Vector2(0f, base.ScreenManager.Font.MeasureString(levelSelection.Name).Y * num);
				Color color = Color.Gray;
				Color color2 = Color.Gray;
				if (!levelSelection.Unlocked)
				{
					color = Color.Black;
					color2 = Color.Black;
				}
				else if (i == selectedLevel && selectedEntry == 0)
				{
					color = Color.White;
					color2 = Color.White;
				}
				base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, levelSelection.Name, vector, color, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
				base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, levelSelection.ScoreString, position, color, -(float)Math.PI / 2f, Vector2.Zero, num2, SpriteEffects.None, 0f);
				base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, levelSelection.TimeString, position2, color, 0f, Vector2.Zero, num2, SpriteEffects.None, 0f);
				base.ScreenManager.SpriteBatch.Draw(levelSelection.LevelIcon, position3, color2);
			}
			base.ScreenManager.SpriteBatch.End();
		}

		private void SetMenuEntryText()
		{
		}

		protected override void OnCancel(PlayerIndex playerIndex)
		{
			base.OnCancel(playerIndex);
		}

		private void SelectLevel(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(base.ScreenManager, true, e.PlayerIndex, new GameplayScreen(currentArea.Levels[selectedLevel].AssetName));
		}

		private void SelectAreaMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			SetMenuEntryText();
		}
	}
}
