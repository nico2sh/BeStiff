using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
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

		public bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
		{
			if (!bFullScreen)
			{
				if (iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
				{
					graphicsDeviceManager.PreferredBackBufferWidth = iWidth;
					graphicsDeviceManager.PreferredBackBufferHeight = iHeight;
					graphicsDeviceManager.IsFullScreen = bFullScreen;
					graphicsDeviceManager.ApplyChanges();
					return true;
				}
			}
			else
			{
				foreach (DisplayMode supportedDisplayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
				{
					if (supportedDisplayMode.Width == iWidth && supportedDisplayMode.Height == iHeight)
					{
						graphicsDeviceManager.PreferredBackBufferWidth = iWidth;
						graphicsDeviceManager.PreferredBackBufferHeight = iHeight;
						graphicsDeviceManager.IsFullScreen = bFullScreen;
						graphicsDeviceManager.ApplyChanges();
						return true;
					}
				}
			}
			return false;
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
