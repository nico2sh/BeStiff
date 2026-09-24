using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Screens
{
	public class ScreenManager : DrawableGameComponent
	{
		private List<GameScreen> screens = new List<GameScreen>();

		private List<GameScreen> screensToUpdate = new List<GameScreen>();

		private InputHelper input = new InputHelper();

		private SpriteBatch spriteBatch;

		private SpriteFont font;

		private Texture2D blankTexture;

		private GraphicsDeviceManager graphicsDeviceManager;

		private AudioManager audioManager;

		private bool isInitialized;

		private bool traceEnabled;

		public SpriteBatch SpriteBatch => spriteBatch;

		public ContentManager Content => base.Game.Content;

		public AudioManager AudioManager => audioManager;

		public IGraphicsDeviceService GraphicsDeviceService => (IGraphicsDeviceService)base.Game.Services.GetService(typeof(IGraphicsDeviceService));

		public SpriteFont Font => font;

		public bool TraceEnabled
		{
			get
			{
				return traceEnabled;
			}
			set
			{
				traceEnabled = value;
			}
		}

		public InputHelper InputHelper => input;

		public ScreenManager(Game game)
			: base(game)
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			isInitialized = true;
		}

		public void SetGraphicsDeviceManager(GraphicsDeviceManager graphics)
		{
			graphicsDeviceManager = graphics;
		}

		public void SetAudioManager(AudioManager aManager)
		{
			audioManager = aManager;
		}

		protected override void LoadContent()
		{
			ContentManager content = base.Game.Content;
			spriteBatch = new SpriteBatch(base.GraphicsDevice);
			font = content.Load<SpriteFont>("fonts\\mainfont");
			// Menus load this on first open; preload it so opening one mid-level
			// (fail, pause) doesn't read from disk.
			content.Load<SpriteFont>("fonts\\titlefont");
			blankTexture = content.Load<Texture2D>("blank");
			foreach (GameScreen screen in screens)
			{
				screen.LoadContent();
			}
		}

		protected override void UnloadContent()
		{
			foreach (GameScreen screen in screens)
			{
				screen.UnloadContent();
			}
		}

		/// <summary>
		/// Switches between windowed and full screen. The back buffer stays at
		/// the game's fixed size: screen layout, camera and light maps are sized
		/// from it, so full screen uses a hardware mode switch (the display
		/// scales the image) instead of borderless desktop full screen, which
		/// would resize the back buffer to the desktop resolution.
		/// </summary>
		public void SetFullScreen(bool fullScreen)
		{
			if (graphicsDeviceManager.IsFullScreen == fullScreen)
			{
				return;
			}
			graphicsDeviceManager.HardwareModeSwitch = true;
			graphicsDeviceManager.IsFullScreen = fullScreen;
			graphicsDeviceManager.ApplyChanges();
		}

		public override void Update(GameTime gameTime)
		{
			input.Update();
			screensToUpdate.Clear();
			foreach (GameScreen screen in screens)
			{
				screensToUpdate.Add(screen);
			}
			bool flag = !base.Game.IsActive;
			bool coveredByOtherScreen = false;
			while (screensToUpdate.Count > 0)
			{
				GameScreen gameScreen = screensToUpdate[screensToUpdate.Count - 1];
				screensToUpdate.RemoveAt(screensToUpdate.Count - 1);
				gameScreen.Update(gameTime, flag, coveredByOtherScreen);
				if (gameScreen.ScreenState == ScreenState.TransitionOn || gameScreen.ScreenState == ScreenState.Active)
				{
					if (!flag)
					{
						gameScreen.HandleInput(input);
						flag = true;
					}
					if (!gameScreen.IsPopup)
					{
						coveredByOtherScreen = true;
					}
				}
			}
			audioManager.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			foreach (GameScreen screen in screens)
			{
				if (screen.ScreenState != ScreenState.Hidden)
				{
					screen.Draw(gameTime);
				}
			}
		}

		public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
		{
			screen.ControllingPlayer = controllingPlayer;
			screen.ScreenManager = this;
			screen.IsExiting = false;
			if (isInitialized)
			{
				screen.LoadContent();
			}
			screens.Add(screen);
		}

		public void RemoveScreen(GameScreen screen)
		{
			if (isInitialized)
			{
				screen.UnloadContent();
			}
			screens.Remove(screen);
			screensToUpdate.Remove(screen);
		}

		public GameScreen[] GetScreens()
		{
			return screens.ToArray();
		}

		public void FadeBackBufferToBlack(int alpha)
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			spriteBatch.Begin();
			spriteBatch.Draw(blankTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, (int)(byte)alpha));
			spriteBatch.End();
		}
	}
}
