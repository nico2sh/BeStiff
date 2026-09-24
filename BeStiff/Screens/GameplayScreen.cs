using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Be_Stiff.Screens
{
	internal class GameplayScreen : GameScreen
	{
		private ContentManager content;

		private Random random = new Random();

		private string levelName;

		private bool dead;

		private bool started;

		public ContentManager Content => content;

		public GameplayScreen(string level)
		{
			levelName = level;
			base.TransitionOnTime = TimeSpan.FromSeconds(1.5);
			base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}

		public override void LoadContent()
		{
			if (content == null)
			{
				content = new CaseInsensitiveContentManager(base.ScreenManager.Game.Services, "Content");
			}
			GameElementsControl.Initialize(this);
			GameElementsControl.LoadContent();
			GameElementsControl.LoadLevel("Levels\\" + levelName);
			string debugPos = DebugFlags.HeroPos;
			if (!string.IsNullOrEmpty(debugPos))
			{
				string[] parts = debugPos.Split(',');
				GameElementsControl.Hero.DebugTeleport(new Vector2(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture)));
			}
			base.ScreenManager.Game.ResetElapsedTime();
			base.AudioManager.StopSong();
			base.AudioManager.MusicVolume = (float)Globals.OptionMusicVolume / 10f;
			dead = false;
			GC.Collect();
		}

		public override void UnloadContent()
		{
			// Looping level sounds (elevators, platforms) would otherwise keep
			// playing after the level is left.
			base.ScreenManager.AudioManager.StopSoundLoops();
			GameElementsControl.Unload();
			content.Unload();
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
			// Level machinery only updates while gameplay is active, so hold its
			// looping sounds while a menu or end screen covers the level.
			base.ScreenManager.AudioManager.PauseSoundLoops(!base.IsActive);
			if (base.IsActive && started)
			{
				GameElementsControl.Update(gameTime);
				if (GameElementsControl.FinishByAchieve(out var time))
				{
					base.ScreenManager.AddScreen(new EndLevelScreen(levelName, GameElementsControl.Score.GetScore(), (int)time, (int)(GameElementsControl.Hero.Energy * 10f), GameElementsControl.Level.FinishText), null);
				}
				else if (GameElementsControl.FinishByFail())
				{
					base.ScreenManager.AddScreen(new DeadScreen(levelName), null);
				}
				if (GameElementsControl.Hero.IsDead())
				{
					if (!dead)
					{
						base.AudioManager.PlaySong("death");
						dead = true;
					}
				}
				else
				{
					base.AudioManager.PlaySong("trumpetSong", loop: true);
				}
				if (GameElementsControl.BulletTime)
				{
					base.AudioManager.PauseSong();
				}
				else if (base.AudioManager.IsSongPaused)
				{
					base.AudioManager.ResumeSong();
				}
			}
			else
			{
				base.AudioManager.PauseSong();
			}
			if (!started)
			{
				ConfirmStartBoxScreen screen = new ConfirmStartBoxScreen("Get Ready and Press Start", GameElementsControl.Level.StartText);
				base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
				started = true;
			}
		}

		public override void HandleInput(InputHelper input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			_ = base.ControllingPlayer.Value;
			input.UpdatePlayerInput(base.ControllingPlayer);
			if (input.IsPauseGame(base.ControllingPlayer))
			{
				if (GameElementsControl.Level.Passed)
				{
					base.ScreenManager.AddScreen(new EndLevelScreen(levelName, GameElementsControl.Score.GetScore(), (int)GameElementsControl.TotalRealTime, (int)(GameElementsControl.Hero.Energy * 10f), GameElementsControl.Level.FinishText), null);
				}
				else if (GameElementsControl.Hero.IsDead())
				{
					base.ScreenManager.AddScreen(new DeadScreen(levelName), null);
				}
				else
				{
					base.ScreenManager.AddScreen(new PauseMenuScreen(levelName), base.ControllingPlayer);
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			GameElementsControl.Draw();
			if (base.TransitionPosition > 0f)
			{
				base.ScreenManager.FadeBackBufferToBlack(255 - base.TransitionAlpha);
			}
		}
	}
}
