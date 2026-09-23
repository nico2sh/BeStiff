using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BackgroundSkyline : Background
	{
		public override void Load(Vector2 position, Color color)
		{
			backgroundLayer = new BackgroundLayer[3];
			numberOfLayers = 3;
			for (int i = 0; i < numberOfLayers; i++)
			{
				backgroundLayer[i] = new BackgroundLayer();
			}
			backgroundLayer[0].Load("nightSky", new Vector2(0f, 0f), new Vector2(0f, 0f), new Color(40, 31, 54));
			backgroundLayer[1].Load("skyline2", position - new Vector2(0f, 60f), new Vector2(0.05f, 0.05f), new Color(0, 0, 0));
			backgroundLayer[2].Load("skyline", position, new Vector2(0.1f, 0.1f), new Color(37, 37, 37));
			backgroundColor = color;
		}
	}
}
