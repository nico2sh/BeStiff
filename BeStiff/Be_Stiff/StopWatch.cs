using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	public class StopWatch
	{
		private SpriteFont minutesFont;

		private SpriteFont secondsFont;

		private SpriteFont centisecondsFont;

		private StringBuilder minutesString;

		private StringBuilder secondsString;

		private StringBuilder centisecondsString;

		private Vector2 minutesTextSize;

		private Vector2 secondsOffsetPos;

		private Vector2 centisecondsOffsetPos;

		private bool stopped;

		private int minutes;

		private int seconds;

		private int centiseconds;

		public StopWatch()
		{
			minutesString = new StringBuilder();
			secondsString = new StringBuilder();
			centisecondsString = new StringBuilder();
			secondsOffsetPos = new Vector2(5f, 2f);
			centisecondsOffsetPos = new Vector2(5f, 18f);
			stopped = false;
		}

		public void Load()
		{
			GameElementsControl.LoadFont("stopWatchMinutesFont", "fonts\\stopwatchminutesfont");
			GameElementsControl.LoadFont("stopWatchSecondsFont", "fonts\\stopwatchsecondssfont");
			GameElementsControl.LoadFont("stopWatchCentiSecondsFont", "fonts\\stopwatchcentisecondsfont");
			minutesFont = GameElementsControl.GetFont("stopWatchMinutesFont");
			secondsFont = GameElementsControl.GetFont("stopWatchSecondsFont");
			centisecondsFont = GameElementsControl.GetFont("stopWatchCentiSecondsFont");
		}

		public void Update()
		{
			if (!stopped)
			{
				minutes = (int)GameElementsControl.RealTimeInMS / 60000;
				seconds = (int)(GameElementsControl.RealTimeInMS / 1000.0) % 60;
				centiseconds = (int)(GameElementsControl.RealTimeInMS % 1000.0) / 10;
				minutesString.Length = 0;
				minutesString = minutesString.AppendNumber(minutes, 2);
				minutesString.Append(':');
				secondsString.Length = 0;
				secondsString = secondsString.AppendNumber(seconds, 2);
				centisecondsString.Length = 0;
				centisecondsString = centisecondsString.AppendNumber(centiseconds, 2);
				minutesTextSize = minutesFont.MeasureString(minutesString);
				minutesTextSize.Y = 0f;
			}
		}

		public void Stop()
		{
			stopped = true;
		}

		public void Resume()
		{
			stopped = false;
		}

		public void Start()
		{
			stopped = false;
			minutes = 0;
			seconds = 0;
			centiseconds = 0;
		}

		public void Draw(Vector2 position)
		{
			GameElementsControl.ScreenManager.SpriteBatch.DrawString(minutesFont, minutesString, position - minutesTextSize, Color.DarkRed);
			GameElementsControl.ScreenManager.SpriteBatch.DrawString(secondsFont, secondsString, position + secondsOffsetPos, Color.DarkRed);
			GameElementsControl.ScreenManager.SpriteBatch.DrawString(centisecondsFont, centisecondsString, position + centisecondsOffsetPos, Color.DarkRed);
		}
	}
}
