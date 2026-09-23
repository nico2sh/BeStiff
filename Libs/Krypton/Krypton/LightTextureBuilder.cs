using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton
{
	public static class LightTextureBuilder
	{
		public static Texture2D CreatePointLight(GraphicsDevice device, int size)
		{
			return CreateConicLight(device, size, (float)Math.PI * 2f, 0f);
		}

		public static Texture2D CreateConicLight(GraphicsDevice device, int size, float FOV)
		{
			return CreateConicLight(device, size, FOV, 0f);
		}

		public static Texture2D CreateConicLight(GraphicsDevice device, int size, float FOV, float nearPlaneDistance)
		{
			float[,] array = new float[size, size];
			float num = size / 2;
			FOV /= 2f;
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					float num2 = Vector2.Distance(new Vector2(i, j), new Vector2(num));
					Vector2 vector = new Vector2(i, j) - new Vector2(num);
					float value = (float)Math.Atan2(vector.Y, vector.X);
					if (num2 <= num && num2 >= nearPlaneDistance && Math.Abs(value) <= FOV)
					{
						array[i, j] = (num - num2) / num;
					}
					else
					{
						array[i, j] = 0f;
					}
				}
			}
			Texture2D texture2D = new Texture2D(device, size, size);
			Color[] array2 = new Color[size * size];
			for (int k = 0; k < size; k++)
			{
				for (int l = 0; l < size; l++)
				{
					ref Color reference = ref array2[k + l * size];
					reference = new Color(new Vector3(array[k, l]));
				}
			}
			texture2D.SetData(array2);
			return texture2D;
		}

		private static bool IsPowerOfTwo(int x)
		{
			return (x & (x - 1)) == 0;
		}
	}
}
