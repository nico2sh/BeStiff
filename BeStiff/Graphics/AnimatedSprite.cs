using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Graphics
{
	public class AnimatedSprite
	{
		private GameSprite spriteSheet;

		private double elapsedTime;

		private double timePerFrame;

		private int currentFrame;

		private bool recentlyChanged;

		private Vector2 origin;

		private int frameWidth;

		private int frameHeight;

		private int loopStartFrame;

		private int framesPerRow;

		private int rows;

		private int framesOnLastRow;

		private SpriteEffects effect;

		private bool paused;

		private bool randomLoop;

		private double timeBetweenLoops;

		private float probabilityForLoop;

		public int CurrentFrame => currentFrame;

		public bool RecentlyChanged => recentlyChanged;

		public AnimatedSprite(GameSprite gameSprite, double timeFrame, int colsOnFirstRow, int colsOnLastRow, int rowNumber, int loop)
		{
			if (rowNumber == 0)
			{
				rowNumber = 1;
			}
			timePerFrame = timeFrame;
			elapsedTime = 0.0;
			spriteSheet = gameSprite;
			frameWidth = gameSprite.Width / colsOnFirstRow;
			frameHeight = gameSprite.Height / rowNumber;
			rows = rowNumber;
			loopStartFrame = loop - 1;
			framesPerRow = colsOnFirstRow;
			framesOnLastRow = colsOnLastRow;
			origin = new Vector2(frameWidth, frameHeight) / 2f;
			paused = false;
			randomLoop = false;
			timeBetweenLoops = 0.0;
			recentlyChanged = false;
		}

		public AnimatedSprite(GameSprite gameSprite, double timeFrame, int colsOnFirstRow, int loop)
		{
			timePerFrame = timeFrame;
			elapsedTime = 0.0;
			spriteSheet = gameSprite;
			frameWidth = gameSprite.Width / colsOnFirstRow;
			frameHeight = gameSprite.Height;
			rows = 1;
			loopStartFrame = loop - 1;
			framesPerRow = colsOnFirstRow;
			framesOnLastRow = colsOnFirstRow;
			origin = new Vector2(frameWidth, frameHeight) / 2f;
			paused = false;
			randomLoop = false;
			timeBetweenLoops = 0.0;
		}

		public AnimatedSprite(GameSprite gameSprite, double timeFrame, int colsOnFirstRow)
		{
			timePerFrame = timeFrame;
			elapsedTime = 0.0;
			spriteSheet = gameSprite;
			frameWidth = gameSprite.Width / colsOnFirstRow;
			frameHeight = gameSprite.Height;
			rows = 1;
			loopStartFrame = 0;
			framesPerRow = colsOnFirstRow;
			framesOnLastRow = colsOnFirstRow;
			origin = new Vector2(frameWidth, frameHeight) / 2f;
			paused = false;
			randomLoop = false;
			timeBetweenLoops = 0.0;
		}

		public void Pause()
		{
			paused = true;
		}

		public void Play()
		{
			paused = false;
		}

		public void SetRandomLoop(double timeBtLoops, float prob)
		{
			randomLoop = true;
			timeBetweenLoops = timeBtLoops;
			probabilityForLoop = prob;
		}

		public void Resetframes()
		{
			elapsedTime = 0.0;
			currentFrame = 0;
		}

		public void Update(SpriteEffects sEffect)
		{
			Update();
			effect = sEffect;
		}

		public void Update()
		{
			if (!paused)
			{
				elapsedTime += GameElementsControl.LastFrameTimeInMS;
				if (elapsedTime > timePerFrame)
				{
					currentFrame++;
					recentlyChanged = true;
					if (currentFrame >= framesPerRow * (rows - 1) + framesOnLastRow)
					{
						currentFrame = loopStartFrame;
						if (randomLoop)
						{
							paused = true;
						}
					}
					elapsedTime -= timePerFrame;
				}
				else
				{
					recentlyChanged = false;
				}
			}
			else
			{
				if (!randomLoop)
				{
					return;
				}
				elapsedTime += GameElementsControl.LastFrameTimeInMS;
				if (elapsedTime > timeBetweenLoops)
				{
					if (GameElementsControl.Random.NextDouble() < (double)probabilityForLoop)
					{
						paused = false;
					}
					elapsedTime -= timeBetweenLoops;
				}
			}
		}

		public void SetTimeFrame(double newTimeFrame)
		{
			timePerFrame = newTimeFrame;
		}

		public void Draw(Vector2 screenPos, Vector2 scale)
		{
			int y = currentFrame / framesPerRow * frameHeight;
			int num = currentFrame % framesPerRow;
			Rectangle value = new Rectangle(frameWidth * num, y, frameWidth, frameHeight);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(spriteSheet.Texture, screenPos, value, Color.White, 0f, origin, scale, effect, 0f);
		}

		public void Draw(Vector2 screenPos, float rotation)
		{
			int y = currentFrame / framesPerRow * frameHeight;
			int num = currentFrame % framesPerRow;
			Rectangle value = new Rectangle(frameWidth * num, y, frameWidth, frameHeight);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(spriteSheet.Texture, screenPos, value, Color.White, rotation, origin, 1f, effect, 0f);
		}

		public void Draw(Vector2 screenPos, double fromTime, double timeToDisappear)
		{
			float num = MathHelper.Clamp((float)((GameElementsControl.CurrentTimeInMS - fromTime) / timeToDisappear), 0f, 1f);
			byte alpha = (byte)((1f - num) * 255f);
			Color color = new Color(255, 255, 255, (int)alpha);
			int y = currentFrame / framesPerRow * frameHeight;
			int num2 = currentFrame % framesPerRow;
			Rectangle value = new Rectangle(frameWidth * num2, y, frameWidth, frameHeight);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(spriteSheet.Texture, screenPos, value, color, 0f, origin, 1f, effect, 0f);
		}

		public void Draw(Vector2 screenPos)
		{
			int y = currentFrame / framesPerRow * frameHeight;
			int num = currentFrame % framesPerRow;
			Rectangle value = new Rectangle(frameWidth * num, y, frameWidth, frameHeight);
			GameElementsControl.ScreenManager.SpriteBatch.Draw(spriteSheet.Texture, screenPos, value, Color.White, 0f, origin, 1f, effect, 0f);
		}
	}
}
