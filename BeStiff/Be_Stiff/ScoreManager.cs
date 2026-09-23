using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	public class ScoreManager
	{
		private const float maxSize = 1.5f;

		private const float minSize = 0.7f;

		private const double fadeTime = 250.0;

		private const int basePower = 10;

		private int score;

		private int newValueToAdd;

		private double timeNewValueAdded;

		private bool valueInQueue;

		private float textSize;

		private SpriteFont font;

		private StringBuilder scoreString;

		private StringBuilder newScoreString;

		private Vector2 newScoreOrigin;

		private Vector2 newScorePosition;

		private Color newScoreColor;

		public ScoreManager()
		{
			score = 0;
			newValueToAdd = 0;
			valueInQueue = false;
			textSize = 0f;
			newScorePosition = Vector2.Zero;
			newScoreOrigin = Vector2.Zero;
		}

		public void Load()
		{
			GameElementsControl.LoadFont("stopWatchMinutesFont", "fonts\\stopwatchminutesfont");
			font = GameElementsControl.GetFont("stopWatchMinutesFont");
			scoreString = new StringBuilder();
			newScoreString = new StringBuilder();
			newScoreColor = Color.DarkRed;
			scoreString.Length = 0;
			scoreString = scoreString.AppendNumber(score, 8);
		}

		public void AddPoints(int value)
		{
			newValueToAdd += GetTimedValue(value);
			timeNewValueAdded = GameElementsControl.RealTimeInMS;
			valueInQueue = true;
			newScoreString.Length = 0;
			newScoreString.Append('+');
			newScoreString = newScoreString.AppendNumber(newValueToAdd, 1);
			newScoreOrigin = font.MeasureString(newScoreString);
			newScorePosition = font.MeasureString(scoreString) * 0.7f;
			textSize = 0.8f;
		}

		private int GetTimedValue(int value)
		{
			float num = (float)Math.Pow(GameElementsControl.RealTimeInMS, 0.1);
			float num2 = (float)value / num;
			return (int)num2;
		}

		public void Update()
		{
			if (valueInQueue)
			{
				if (timeNewValueAdded + 250.0 <= GameElementsControl.RealTimeInMS)
				{
					score += newValueToAdd;
					newValueToAdd = 0;
					valueInQueue = false;
					scoreString.Length = 0;
					scoreString = scoreString.AppendNumber(score, 8);
				}
				else
				{
					textSize = MathHelper.Lerp(1.5f, 0.7f, (float)((GameElementsControl.RealTimeInMS - timeNewValueAdded) / 250.0));
					newScoreColor = Color.Lerp(Color.Transparent, Color.DarkRed, (textSize - 0.7f) / 0.8f);
				}
			}
		}

		public void CloseScore()
		{
			if (valueInQueue)
			{
				score += newValueToAdd;
				newValueToAdd = 0;
				valueInQueue = false;
				textSize = 0f;
			}
		}

		public int GetScore()
		{
			return score;
		}

		public void Draw(Vector2 position)
		{
			GameElementsControl.ScreenManager.SpriteBatch.DrawString(font, scoreString, position, Color.DarkRed, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
			if (valueInQueue)
			{
				GameElementsControl.ScreenManager.SpriteBatch.DrawString(font, newScoreString, position + newScorePosition, newScoreColor, 0f, newScoreOrigin, textSize, SpriteEffects.None, 0f);
			}
		}
	}
}
