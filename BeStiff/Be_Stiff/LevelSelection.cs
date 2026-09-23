using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	internal class LevelSelection
	{
		private string name;

		private string assetName;

		private Vector2 screenPosition;

		private Texture2D levelTexture;

		private bool locked;

		private string areaName;

		private List<string> nextLevels;

		private StringBuilder scoreString;

		private StringBuilder timeString;

		private int bestTime;

		private int bestScore;

		public int BestTime => bestTime;

		public int BestScore => bestScore;

		public string Name => name;

		public string AreaName => areaName;

		public string AssetName => assetName;

		public Vector2 ScreenPosition => screenPosition;

		public bool Unlocked => !locked;

		public List<string> NextLevels => nextLevels;

		public StringBuilder ScoreString => scoreString;

		public StringBuilder TimeString => timeString;

		public Texture2D LevelIcon => levelTexture;

		public void Unlock()
		{
			locked = false;
		}

		public void LoadValues(int score, int time, bool unlocked)
		{
			bestScore = score;
			bestTime = time;
			locked = !unlocked;
			UpdateStrings();
		}

		public void UpdateScore(int newScore, int newTime)
		{
			bool flag = false;
			if (bestScore < newScore)
			{
				bestScore = newScore;
				flag = true;
			}
			if (bestTime > 0)
			{
				if (bestTime > newTime)
				{
					bestTime = newTime;
					flag = true;
				}
			}
			else
			{
				bestTime = newTime;
				flag = true;
			}
			if (flag)
			{
				Globals.SaveSaveGames();
			}
			UpdateStrings();
		}

		public LevelSelection(string levelName, string levelAssetName, Vector2 levelScreenPosition, string area)
		{
			name = levelName;
			assetName = levelAssetName;
			screenPosition = levelScreenPosition;
			areaName = area;
			scoreString = new StringBuilder();
			timeString = new StringBuilder();
			locked = true;
			nextLevels = new List<string>();
			UpdateStrings();
		}

		public void AddNextLevel(string levelAssetName)
		{
			nextLevels.Add(levelAssetName);
		}

		public void LoadContent(ContentManager content)
		{
			levelTexture = content.Load<Texture2D>("sprites\\levels\\" + assetName);
		}

		private void UpdateStrings()
		{
			scoreString.Length = 0;
			if (bestScore != 0)
			{
				scoreString = scoreString.AppendNumber(bestScore, 0);
			}
			else
			{
				scoreString.Append("Nothing");
			}
			timeString.Length = 0;
			if (bestTime != 0)
			{
				int number = bestTime / 60000;
				int number2 = bestTime / 1000 % 60;
				int number3 = bestTime % 1000 / 10;
				timeString = timeString.AppendNumber(number, 2);
				timeString.Append(':');
				timeString = timeString.AppendNumber(number2, 2);
				timeString.Append(':');
				timeString = timeString.AppendNumber(number3, 2);
			}
			else
			{
				timeString.Append("Nothing");
			}
		}

		public void Draw()
		{
		}
	}
}
