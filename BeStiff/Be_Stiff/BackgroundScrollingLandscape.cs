using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BackgroundScrollingLandscape : Background
	{
		private Vector2 offset;

		public override void Load(Vector2 position, Color color)
		{
			backgroundLayer = new BackgroundLayer[3];
			numberOfLayers = 3;
			backgroundLayer[0].Load("clouds", new Vector2(0f, -250f), new Vector2(0f, 0f), new Color(0, 64, 110));
			backgroundLayer[1].Load("skyline2", position - new Vector2(0f, 260f), new Vector2(0.5f, 0.5f), new Color(0, 0, 0));
			backgroundLayer[2].Load("skyline", position, new Vector2(0.75f, 0.75f), new Color(37, 37, 37));
			backgroundColor = color;
			offset = new Vector2(-300f, 0f);
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
