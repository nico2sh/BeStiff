using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Screens
{
	internal class EndLevelScreen : MenuScreen
	{
		private string levelName;

		private string infoText;

		private int partialPoints;

		private StringBuilder timeString;

		private StringBuilder totalString;

		private StringBuilder oldTimeString;

		private StringBuilder oldTotalString;

		private Color scoreColor;

		private Color timeColor;

		private Vector2 scorePosition;

		private Texture2D scoreBackground;

		private Texture2D boxBackground;

		private Vector2 boxPosition;

		private Vector2 textPadding;

		private Rectangle textBoxFrame;

		private Rectangle textBoxBackground;

		private Vector2 textBoxBackgroundPosition;

		private int frameWidth;

		private Color frameColor;

		private Color fontColor;

		public EndLevelScreen(string level, int partialScore, int time, int energy, string finishMessage)
			: base("Excellent!")
		{
			levelName = level;
			infoText = finishMessage;
			textBoxFrame = new Rectangle(5, 5, 400, 100);
			textPadding = new Vector2(40f, 60f);
			frameWidth = 5;
			LevelSelection level2 = Globals.LevelsManager.GetLevel(levelName);
			oldTimeString = new StringBuilder("Best Time (so far): ");
			oldTimeString.Append((object)level2.TimeString);
			oldTotalString = new StringBuilder("Best Score (so far): ");
			oldTotalString.Append((object)level2.ScoreString);
			scorePosition = base.BasePosition + new Vector2(0f, 305f);
			partialPoints = partialScore;
			int number = time / 60000;
			int number2 = time / 1000 % 60;
			int number3 = time % 1000 / 10;
			timeString = new StringBuilder("Time: ");
			timeString = timeString.AppendNumber(number, 2);
			timeString.Append(':');
			timeString = timeString.AppendNumber(number2, 2);
			timeString.Append(':');
			timeString = timeString.AppendNumber(number3, 2);
			totalString = new StringBuilder("Total Score: ");
			totalString = totalString.AppendNumber(partialPoints, 1);
			timeColor = Color.White;
			scoreColor = Color.White;
			if (time < level2.BestTime || level2.BestTime == 0)
			{
				timeColor = Color.OrangeRed;
			}
			if (partialPoints > level2.BestScore)
			{
				scoreColor = Color.OrangeRed;
			}
			level2.UpdateScore(partialPoints, time);
			Globals.LevelsManager.UnlockNextLevels(level2.AssetName);
			Globals.SaveSaveGames();
			base.IsPopup = true;
			MenuEntry menuEntry = new MenuEntry("Next Level");
			MenuEntry menuEntry2 = new MenuEntry("Replay Level");
			MenuEntry menuEntry3 = new MenuEntry("Quit Game");
			menuEntry.Selected += Next;
			menuEntry2.Selected += Replay;
			menuEntry3.Selected += QuitGameMenuEntrySelected;
			AddMenuEntry(menuEntry);
			AddMenuEntry(menuEntry2);
			AddMenuEntry(menuEntry3);
		}

		public override void LoadContent()
		{
			base.LoadContent();
			ContentManager content = base.ScreenManager.Game.Content;
			scoreBackground = content.Load<Texture2D>("sprites\\menubackgrounds\\scorebackground");
			boxBackground = content.Load<Texture2D>("sprites\\menubackgrounds\\finishmessagebackground");
			infoText = ParseText(infoText);
			textBoxFrame.Height = (int)(base.ScreenManager.Font.MeasureString(infoText).Y + textPadding.Y * 2f);
			boxPosition = new Vector2(800f, 100f);
			textBoxFrame.X = (int)boxPosition.X;
			textBoxFrame.Y = (int)boxPosition.Y;
			textBoxBackground = new Rectangle(0, 0, textBoxFrame.Width - 2 * frameWidth, textBoxFrame.Height - 2 * frameWidth);
			textBoxBackgroundPosition = new Vector2(boxPosition.X + (float)frameWidth, boxPosition.Y + (float)frameWidth);
			frameColor = new Color(0, 0, 0, 50);
			fontColor = new Color(0, 0, 0, 130);
		}

		private string ParseText(string text)
		{
			string text2 = string.Empty;
			string text3 = string.Empty;
			string[] array = text.Split(new char[1] { ' ' });
			string[] array2 = array;
			foreach (string text4 in array2)
			{
				if (base.ScreenManager.Font.MeasureString(text2 + text4).Length() > (float)textBoxFrame.Width - textPadding.X * 2f)
				{
					text3 = text3 + text2 + '\n';
					text2 = string.Empty;
				}
				text2 = text2 + text4 + ' ';
			}
			return text3 + text2;
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

		private void Replay(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(base.ScreenManager, true, e.PlayerIndex, new GameplayScreen(levelName));
		}

		private void Next(object sender, PlayerIndexEventArgs e)
		{
			string nextLevel = Globals.LevelsManager.GetNextLevel(levelName);
			if (nextLevel != "")
			{
				LoadingScreen.Load(base.ScreenManager, true, e.PlayerIndex, new GameplayScreen(nextLevel));
			}
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteFont font = base.ScreenManager.Font;
			base.ScreenManager.FadeBackBufferToBlack(base.TransitionAlpha * 2 / 3);
			Vector2 vector = scorePosition;
			base.Draw(gameTime);
			base.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null);
			base.ScreenManager.SpriteBatch.Draw(scoreBackground, vector - new Vector2(35f, 150f), Color.White);
			base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, timeString, vector, timeColor);
			base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, oldTimeString, vector + new Vector2(250f, 0f), Color.LightGray);
			vector.Y += base.ScreenManager.Font.MeasureString(timeString).Y;
			base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, totalString, vector, scoreColor);
			base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, oldTotalString, vector + new Vector2(250f, 0f), Color.LightGray);
			base.ScreenManager.SpriteBatch.Draw(boxBackground, textBoxBackgroundPosition, Color.White);
			base.ScreenManager.SpriteBatch.DrawString(font, infoText, boxPosition + textPadding, Color.LightGray);
			base.ScreenManager.SpriteBatch.End();
		}
	}
}
