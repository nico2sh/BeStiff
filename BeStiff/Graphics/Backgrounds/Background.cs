using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Graphics.Backgrounds
{
	public abstract class Background
	{
		protected BackgroundLayer[] backgroundLayer;

		protected int numberOfLayers;

		protected Color backgroundColor;

		public Color Color => backgroundColor;

		public Background()
		{
		}

		public virtual void Load(Vector2 position, Color color)
		{
			throw new Exception("This must be implemented");
		}

		public virtual void Update()
		{
			for (int i = 0; i < numberOfLayers; i++)
			{
				backgroundLayer[i].Update();
			}
		}

		public void SetCameraZoom(float zoom)
		{
			for (int i = 0; i < numberOfLayers; i++)
			{
				backgroundLayer[i].SetCameraZoom(zoom);
			}
		}

		public void Draw()
		{
			for (int i = 0; i < numberOfLayers; i++)
			{
				backgroundLayer[i].Draw();
			}
		}
	}
}
