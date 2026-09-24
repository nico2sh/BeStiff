using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager graphics;

		private SpriteBatch spriteBatch;

		private ScreenManager screenManager;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;
			graphics.HardwareModeSwitch = true;
			base.IsFixedTimeStep = true;
			base.IsMouseVisible = false;
			base.Content = new CaseInsensitiveContentManager(base.Services, "Content");
			AudioManager audioManager = new AudioManager(this);
			screenManager = new ScreenManager(this);
			screenManager.SetGraphicsDeviceManager(graphics);
			screenManager.SetAudioManager(audioManager);
			base.Components.Add(screenManager);
			base.Components.Add(audioManager);
			Globals.Init();
		}

		protected override void Initialize()
		{
			screenManager.AddScreen(new BackgroundScreen(), null);
			string debugLevel = DebugFlags.Level;
			if (string.IsNullOrEmpty(debugLevel))
			{
				screenManager.AddScreen(new PressStartScreen(), null);
			}
			else
			{
				AutoStartLevel(debugLevel);
			}
			base.Initialize();
		}

		/// <summary>
		/// Debug helper: skips the start screen and menus and loads the given
		/// level directly. Enabled by setting the BESTIFF_LEVEL environment
		/// variable (e.g. BESTIFF_LEVEL=Tutorial1).
		/// </summary>
		private void AutoStartLevel(string levelName)
		{
			Globals.ActiveControl = 1; // keyboard
			EasyStorage.EasyStorageSettings.SetSupportedLanguages(EasyStorage.Language.English);
			EasyStorage.SharedSaveDevice saveDevice = new EasyStorage.SharedSaveDevice();
			base.Components.Add(saveDevice);
			saveDevice.PromptForDevice();
			saveDevice.DeviceSelected += delegate(object s, EventArgs e)
			{
				Globals.SaveDevice = (EasyStorage.SaveDevice)s;
				Globals.LoadOptions();
				screenManager.SetFullScreen(Globals.OptionFullScreen);
				Globals.LoadSaveGames();
				screenManager.AudioManager.MusicVolume = (float)Globals.OptionMusicVolume / 10f;
				screenManager.AudioManager.SoundVolume = (float)Globals.OptionSoundVolume / 10f;
				LoadingScreen.Load(screenManager, true, PlayerIndex.One, new GameplayScreen(levelName));
			};
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(base.GraphicsDevice);
			screenManager.AudioManager.LoadSong("actionSong", "audio\\music\\actionSong");
			screenManager.AudioManager.LoadSong("stealthSong", "audio\\music\\stealthSong");
			screenManager.AudioManager.LoadSong("death", "audio\\music\\death");
			screenManager.AudioManager.LoadSong("trumpetSong", "audio\\music\\trumpetSong");
			screenManager.AudioManager.LoadSound("menuChange", "audio\\noises\\menuChange");
			screenManager.AudioManager.LoadSound("menuSelect", "audio\\noises\\menuSelect");
			Globals.LevelsManager.LoadContent(base.Content);
		}

		protected override void UnloadContent()
		{
		}

		private bool f12WasDown;
		private bool screenshotRequested;
		private int screenshotCounter;

		protected override void Update(GameTime gameTime)
		{
			// F12 saves a screenshot (and the intermediate render targets) to the
			// user's home directory, for reporting rendering issues.
			bool f12 = Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F12);
			if (f12 && !f12WasDown)
			{
				screenshotRequested = true;
			}
			f12WasDown = f12;
			double start = DebugFlags.Perf ? FramePerf.Now : 0.0;
			base.Update(gameTime);
			if (DebugFlags.Perf)
			{
				FramePerf.AddUpdate(FramePerf.Now - start);
			}
		}

		private int drawnFrames;

		protected override void Draw(GameTime gameTime)
		{
			double drawStart = DebugFlags.Perf ? FramePerf.Now : 0.0;
			base.GraphicsDevice.Clear(Color.Black);
			base.Draw(gameTime);
			if (DebugFlags.Perf)
			{
				FramePerf.AddDraw(FramePerf.Now - drawStart);
			}
			drawnFrames++;
			if (DebugFlags.Perf)
			{
				FramePerf.EndFrame();
			}
			SaveDebugScreenshot();
		}

		/// <summary>
		/// Debug helper: when BESTIFF_SHOT is set to a .png path, saves the
		/// back buffer after BESTIFF_SHOT_FRAME frames (default 180).
		/// </summary>
		private void SaveDebugScreenshot()
		{
			string path = DebugFlags.Shot;
			if (screenshotRequested)
			{
				screenshotRequested = false;
				screenshotCounter++;
				path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "bestiff_shot" + screenshotCounter + ".png");
			}
			else
			{
				if (string.IsNullOrEmpty(path))
				{
					return;
				}
				if (drawnFrames != DebugFlags.ShotFrame)
				{
					return;
				}
			}
			int w = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
			int h = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
			Color[] data = new Color[w * h];
			base.GraphicsDevice.GetBackBufferData(data);
			using (Texture2D tex = new Texture2D(base.GraphicsDevice, w, h))
			using (System.IO.FileStream fs = System.IO.File.Create(path))
			{
				tex.SetData(data);
				tex.SaveAsPng(fs, w, h);
			}
			Console.Error.WriteLine("Saved screenshot to " + path);
			try { Console.Error.WriteLine("Hero: " + GameElementsControl.DebugHeroState()); } catch (Exception e) { Console.Error.WriteLine("hero state failed: " + e.Message); }
			try { GameElementsControl.DebugSaveRenderTargets(path.Replace(".png", "")); }
			catch (Exception e) { Console.Error.WriteLine("Render target dump failed: " + e.Message); }
		}
	}
}
