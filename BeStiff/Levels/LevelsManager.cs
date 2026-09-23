using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Be_Stiff.Levels
{
	internal class LevelsManager
	{
		private List<Area> areas;

		public List<Area> Areas => areas;

		public LevelsManager()
		{
			areas = new List<Area>();
		}

		public void LoadContent(ContentManager content)
		{
			foreach (Area area in areas)
			{
				foreach (LevelSelection level in area.Levels)
				{
					level.LoadContent(content);
				}
			}
		}

		public void AddNewArea(string areaName)
		{
			Area area = GetArea(areaName);
			if (area == null)
			{
				areas.Add(new Area(areaName));
			}
		}

		public Area GetArea(string areaName)
		{
			foreach (Area area in areas)
			{
				if (area.Name == areaName)
				{
					return area;
				}
			}
			return null;
		}

		public LevelSelection GetLevel(string levelAssetName)
		{
			foreach (Area area in areas)
			{
				foreach (LevelSelection level in area.Levels)
				{
					if (level.AssetName == levelAssetName)
					{
						return level;
					}
				}
			}
			return null;
		}

		public void Clear()
		{
			foreach (Area area in areas)
			{
				area.Levels.Clear();
			}
			areas.Clear();
		}

		public void UnlockNextLevels(string currentLevelAssetName)
		{
			List<string> nextLevels = GetLevel(currentLevelAssetName).NextLevels;
			foreach (string item in nextLevels)
			{
				GetLevel(item).Unlock();
			}
		}

		public string GetNextLevel(string currentLevelAssetName)
		{
			foreach (Area area in areas)
			{
				for (int i = 0; i < area.Levels.Count; i++)
				{
					if (area.Levels[i].AssetName == currentLevelAssetName)
					{
						int num = i + 1;
						if (num < area.Levels.Count)
						{
							return area.Levels[num].AssetName;
						}
					}
				}
			}
			return "";
		}
	}
}
