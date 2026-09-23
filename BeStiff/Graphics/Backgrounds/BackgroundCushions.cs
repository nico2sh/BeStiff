using Microsoft.Xna.Framework;

namespace Be_Stiff.Graphics.Backgrounds
{
	internal class BackgroundCushions : Background
	{
		public override void Load(Vector2 position, Color color)
		{
			backgroundLayer = new BackgroundLayer[3];
			numberOfLayers = 3;
			for (int i = 0; i < numberOfLayers; i++)
			{
				backgroundLayer[i] = new BackgroundLayer();
			}
			backgroundLayer[0].Load("cushions\\cushionback", new Vector2(0f, 0f), new Vector2(0.125f, 0.125f), new Color(0, 0, 0), horizontal: false);
			backgroundLayer[1].Load("cushions\\cushiontop", position, new Vector2(0.25f, 0.25f), new Color(0, 0, 0));
			backgroundLayer[2].Load("cushions\\cushion", position + new Vector2(0f, 5f), new Vector2(0.25f, 0.25f), new Color(37, 37, 37), horizontal: false);
			backgroundColor = color;
		}
	}
}
