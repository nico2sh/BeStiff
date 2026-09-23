using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BackgroundClouds : Background
	{
		private Vector2 offset;

		public override void Load(Vector2 position, Color color)
		{
			backgroundLayer = new BackgroundLayer[3];
			numberOfLayers = 3;
			for (int i = 0; i < numberOfLayers; i++)
			{
				backgroundLayer[i] = new BackgroundLayer();
			}
			backgroundLayer[0].Load("clouds\\sky", new Vector2(0f, 0f), new Vector2(0f, 0f), new Color(124, 160, 184));
			backgroundLayer[1].Load("clouds\\cloudsback", new Vector2(0f, 90f), new Vector2(0.01f, 0.01f));
			backgroundLayer[2].Load("clouds\\cloudsfront", new Vector2(0f, 100f), new Vector2(0.05f, 0.05f));
			backgroundColor = color;
			offset = new Vector2(100f, 0f);
		}

		public override void Update()
		{
			for (int i = 0; i < numberOfLayers; i++)
			{
				backgroundLayer[i].Offset += offset * (float)GameElementsControl.LastFrameTimeInMS / 1000f;
			}
			base.Update();
		}
	}
}
