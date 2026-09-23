using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal abstract class MenuScreen : GameScreen
	{
		private List<MenuEntry> menuEntries = new List<MenuEntry>();

		protected int selectedEntry;

		private string menuTitle;

		private Vector2 basePosition;

		private Vector2 menuTitlePosition;

		private SpriteFont titleFont;

		protected Vector2 BasePosition
		{
			get
			{
				return basePosition;
			}
			set
			{
				basePosition = value;
			}
		}

		protected void AddMenuEntry(MenuEntry menuEntry)
		{
			menuEntries.Add(menuEntry);
		}

		public MenuScreen(string menuTitle)
		{
			this.menuTitle = menuTitle;
			base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
			base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
			basePosition = new Vector2(120f, 100f);
		}

		public override void LoadContent()
		{
			base.LoadContent();
			titleFont = base.ScreenManager.Content.Load<SpriteFont>("fonts\\titlefont");
			if (menuTitle.Length > 0)
			{
				Vector2 vector = titleFont.MeasureString(menuTitle);
				menuTitlePosition = basePosition + new Vector2(0f - vector.Y - 20f, vector.X);
			}
		}

		public override void HandleInput(InputHelper input)
		{
			if (input.IsMenuUp(base.ControllingPlayer))
			{
				selectedEntry--;
				if (selectedEntry < 0)
				{
					selectedEntry = menuEntries.Count - 1;
				}
				base.AudioManager.PlaySound("menuChange");
			}
			if (input.IsMenuDown(base.ControllingPlayer))
			{
				selectedEntry++;
				if (selectedEntry >= menuEntries.Count)
				{
					selectedEntry = 0;
				}
				base.AudioManager.PlaySound("menuChange");
			}
			if (input.IsMenuSelect(base.ControllingPlayer, out var playerIndex))
			{
				OnSelectEntry(selectedEntry, playerIndex);
				base.AudioManager.PlaySound("menuSelect");
			}
			else if (input.IsMenuCancel(base.ControllingPlayer, out playerIndex))
			{
				OnCancel(playerIndex);
				base.AudioManager.PlaySound("menuSelect");
			}
		}

		protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
		{
			menuEntries[selectedEntry].OnSelectEntry(playerIndex);
		}

		protected virtual void OnCancel(PlayerIndex playerIndex)
		{
			ExitScreen();
		}

		protected void OnCancel(object sender, PlayerIndexEventArgs e)
		{
			OnCancel(e.PlayerIndex);
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
			for (int i = 0; i < menuEntries.Count; i++)
			{
				bool isSelected = base.IsActive && i == selectedEntry;
				menuEntries[i].Update(this, isSelected, gameTime);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
			_ = base.ScreenManager.Font;
			Vector2 position = basePosition;
			float num = (float)Math.Pow(base.TransitionPosition, 2.0);
			if (base.ScreenState == ScreenState.TransitionOn)
			{
				position.X -= num * 256f;
			}
			else
			{
				position.X += num * 512f;
			}
			spriteBatch.Begin();
			for (int i = 0; i < menuEntries.Count; i++)
			{
				MenuEntry menuEntry = menuEntries[i];
				bool isSelected = base.IsActive && i == selectedEntry;
				menuEntry.Draw(this, position, isSelected, gameTime);
				position.Y += (float)menuEntry.GetHeight(this) * menuEntry.Scale;
			}
			Vector2 position2 = menuTitlePosition;
			Color color = new Color(192, 192, 192, (int)base.TransitionAlpha);
			float scale = 1f;
			position2.X -= num * 200f;
			float rotation = num * ((float)Math.PI / 2f) - (float)Math.PI / 2f;
			spriteBatch.DrawString(titleFont, menuTitle, position2, color, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
			spriteBatch.End();
		}
	}
}
