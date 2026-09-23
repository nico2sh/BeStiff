using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Levels
{
	internal class Area
	{
		private string name;

		private List<LevelSelection> levels;

		private int levelCount;

		public string Name => name;

		public List<LevelSelection> Levels => levels;

		public Area(string areaName)
		{
			name = areaName;
			levels = new List<LevelSelection>();
			levelCount = 0;
		}

		public LevelSelection AddLevel(string levelName, string levelAssetName, Vector2 levelScreenPosition)
		{
			LevelSelection levelSelection = new LevelSelection(levelName, levelAssetName, levelScreenPosition, name);
			levels.Add(levelSelection);
			levelCount++;
			return levelSelection;
		}

		public LevelSelection GetLevel(int levelNumber)
		{
			if (levels.Count <= levelNumber)
			{
				return null;
			}
			return levels[levelNumber];
		}

		public LevelSelection GetLevel(string levelAssetName)
		{
			foreach (LevelSelection level in levels)
			{
				if (string.Equals(level.AssetName, levelAssetName, StringComparison.OrdinalIgnoreCase))
				{
					return level;
				}
			}
			return null;
		}

		public int GetNumberOfLevels()
		{
			return levels.Count;
		}

		public int GetAchievedLevels()
		{
			int num = 0;
			foreach (LevelSelection level in levels)
			{
				if (level.Unlocked)
				{
					num++;
				}
			}
			return num;
		}
	}
}
