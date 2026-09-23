using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Be_Stiff.Audio
{
	internal class NoiseManager
	{
		private struct Noise
		{
			public GameSprite noiseSprite;

			public string soundName;

			public float radius;

			public GameSpriteVariables spriteVariables;

			public double duration;

			public Noise(GameSprite gs, string stringSoundName, double noiseDuration, float noiseRadius)
			{
				noiseSprite = gs;
				soundName = stringSoundName;
				spriteVariables = GameSprite.GetDefaultVariables();
				radius = noiseRadius;
				duration = noiseDuration;
			}
		}

		private const int MAX_NOISES = 20;

		private int currentNoises;

		private Dictionary<string, Noise> loadedNoises;

		private Noise[] noiseList;

		private double[] noiseTimeList;

		private Vector3[] noisePositionRotationList;

		private AudioManager audioManager;

		public static float MaxDistanceToHear = 20f;

		public float Pitch;

		public NoiseManager(AudioManager aManager)
		{
			noiseList = new Noise[20];
			noiseTimeList = new double[20];
			noisePositionRotationList = new Vector3[20];
			loadedNoises = new Dictionary<string, Noise>();
			currentNoises = 0;
			SoundEffect.MasterVolume = (float)Globals.OptionSoundVolume / 10f;
			audioManager = aManager;
		}

		public void Load()
		{
		}

		public void LoadNoise(string name, string gameSpriteString, string stringSoundName, double noiseDuration, float noiseRadius)
		{
			if (!loadedNoises.TryGetValue(name, out var value))
			{
				GameElementsControl.LoadSprite(gameSpriteString, "sprites\\noises\\" + gameSpriteString);
				GameElementsControl.ScreenManager.AudioManager.LoadSound(stringSoundName, "audio\\noises\\" + stringSoundName);
				value = new Noise(GameElementsControl.GetSprite(gameSpriteString), stringSoundName, noiseDuration, noiseRadius);
				loadedNoises.Add(name, value);
			}
		}

		public void AddVisualNoise(string noiseName, Vector2 position)
		{
			loadedNoises.TryGetValue(noiseName, out var value);
			if (currentNoises < 20)
			{
				noiseList[currentNoises] = value;
				noiseTimeList[currentNoises] = GameElementsControl.CurrentTimeInMS;
				ref Vector3 reference = ref noisePositionRotationList[currentNoises];
				reference = new Vector3(position, (float)(GameElementsControl.Random.NextDouble() - 0.5));
				currentNoises++;
			}
			float volume = MathHelper.Clamp(1f - (GameElementsControl.Hero.Position - position).Length() / MaxDistanceToHear, 0f, 1f);
			audioManager.PlaySound(value.soundName, volume, Pitch, 0f);
			Vector3 noise = new Vector3(position, value.radius);
			GameElementsControl.RegisterNoise(noise);
		}

		public void AddNoise(string soundName, Vector2 position)
		{
			float volume = MathHelper.Clamp(1f - (GameElementsControl.Hero.Position - position).Length() / MaxDistanceToHear, 0f, 1f);
			float pan = MathHelper.Clamp((position.X - GameElementsControl.Hero.Position.X) * 2f / MaxDistanceToHear, -1f, 1f);
			audioManager.PlaySound(soundName, volume, Pitch, pan);
		}

		public void Update()
		{
			for (int num = currentNoises - 1; num >= 0; num--)
			{
				Noise noise = noiseList[num];
				double num2 = noiseTimeList[num];
				if (num2 + noise.duration <= GameElementsControl.CurrentTimeInMS)
				{
					RemoveNoiseAt(num);
				}
			}
		}

		private void RemoveNoiseAt(int index)
		{
			for (int i = index; i < currentNoises - 1; i++)
			{
				ref Noise reference = ref noiseList[i];
				reference = noiseList[i + 1];
				noiseTimeList[i] = noiseTimeList[i + 1];
				ref Vector3 reference2 = ref noisePositionRotationList[i];
				reference2 = noisePositionRotationList[i + 1];
			}
			currentNoises--;
		}

		public void Draw()
		{
			for (int num = currentNoises - 1; num >= 0; num--)
			{
				Noise noise = noiseList[num];
				double fromTime = noiseTimeList[num];
				Vector2 pos = new Vector2(noisePositionRotationList[num].X, noisePositionRotationList[num].Y);
				float z = noisePositionRotationList[num].Z;
				noise.noiseSprite.DrawScaled(GameElementsControl.ConvertWorldToScreen(pos), z, fromTime, noise.duration);
			}
		}
	}
}
