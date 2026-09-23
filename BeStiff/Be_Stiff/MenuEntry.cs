using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal class MenuEntry
	{
		private string text;

		private Vector2 customPosition;

		private bool hasCustomPosition;

		private float selectionFade;

		private Color colorText;

		private Color colorSelected = Color.DarkRed;

		private Color colorUnSelected = Color.LightGray;

		private float scale;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}

		public float Scale => scale;

		public event EventHandler<PlayerIndexEventArgs> Selected;

		protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
		{
			if (this.Selected != null)
			{
				this.Selected(this, new PlayerIndexEventArgs(playerIndex));
			}
		}

		public MenuEntry(string text)
		{
			this.text = text;
			customPosition = Vector2.Zero;
			hasCustomPosition = false;
		}

		public void SetCustomPosition(Vector2 pos)
		{
			customPosition = pos;
			hasCustomPosition = true;
		}

		public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds * 4f;
			if (isSelected)
			{
				selectionFade = Math.Min(selectionFade + num, 1f);
			}
			else
			{
				selectionFade = Math.Max(selectionFade - num, 0f);
			}
			colorText = Color.Lerp(colorUnSelected, colorSelected, selectionFade);
		}

		public virtual void Draw(MenuScreen screen, Vector2 position, bool isSelected, GameTime gameTime)
		{
			Color color = colorText;
			_ = gameTime.TotalGameTime.TotalSeconds;
			scale = 1f + 0.2f * selectionFade;
			color = new Color((int)color.R, (int)color.G, (int)color.B, (int)screen.TransitionAlpha);
			ScreenManager screenManager = screen.ScreenManager;
			SpriteBatch spriteBatch = screenManager.SpriteBatch;
			SpriteFont font = screenManager.Font;
			Vector2 zero = Vector2.Zero;
			if (hasCustomPosition)
			{
				position = customPosition;
			}
			spriteBatch.DrawString(font, text, position, color, 0f, zero, scale, SpriteEffects.None, 0f);
		}

		public virtual int GetHeight(MenuScreen screen)
		{
			return screen.ScreenManager.Font.LineSpacing;
		}
	}
}
