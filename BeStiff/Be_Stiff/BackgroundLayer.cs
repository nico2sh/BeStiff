using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	public class BackgroundLayer
	{
		private Vector2 parallaxFactor;

		private GameSprite backgroundSprite;

		private Vector2 basePosition;

		private Vector2 drawPosition;

		private int cameraXOffset;

		private GameSprite bottomFrameSprite;

		private Color bottomFrameColor;

		private Vector2 bottomDrawPosition;

		private Vector2 halfLevelPos;

		private Vector2 offset;

		private int tw;

		private int th;

		private bool noBottom;

		private Rectangle drawRectangle;

		private Rectangle drawRectangleBottom;

		private bool tileHorizontal;

		private Vector2 cameraPos;

		public Vector2 Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public BackgroundLayer()
		{
			drawPosition = Vector2.Zero;
			offset = Vector2.Zero;
			noBottom = false;
			cameraXOffset = 0;
			tw = GameElementsControl.ScreenManager.GraphicsDevice.Viewport.Width;
			th = GameElementsControl.ScreenManager.GraphicsDevice.Viewport.Height;
		}

		public void Load(string backgroundName, Vector2 bPosition, Vector2 pFactor, Color colorBottom)
		{
			basePosition = bPosition;
			parallaxFactor = Vector2.Clamp(pFactor, Vector2.Zero, Vector2.One);
			GameElementsControl.LoadSprite("background-" + backgroundName, "sprites\\backgrounds\\" + backgroundName);
			GameElementsControl.LoadSprite("whitePixel", "sprites\\whitepixel");
			backgroundSprite = GameElementsControl.GetSprite("background-" + backgroundName);
			bottomFrameSprite = GameElementsControl.GetSprite("whitePixel");
			bottomFrameColor = colorBottom;
			bottomDrawPosition = new Vector2(0f, basePosition.Y + (float)backgroundSprite.Height);
			halfLevelPos = (GameElementsControl.Level.UpLeft + GameElementsControl.Level.DownRight) / 2f;
			drawRectangle = default(Rectangle);
			drawRectangle = default(Rectangle);
			tileHorizontal = true;
			SetCameraZoom(GameElementsControl.Camera.Zoom);
		}

		public void Load(string backgroundName, Vector2 bPosition, Vector2 pFactor)
		{
			basePosition = bPosition;
			parallaxFactor = Vector2.Clamp(pFactor, Vector2.Zero, Vector2.One);
			GameElementsControl.LoadSprite("background-" + backgroundName, "sprites\\backgrounds\\" + backgroundName);
			GameElementsControl.LoadSprite("whitePixel", "sprites\\whitepixel");
			backgroundSprite = GameElementsControl.GetSprite("background-" + backgroundName);
			noBottom = true;
			halfLevelPos = (GameElementsControl.Level.UpLeft + GameElementsControl.Level.DownRight) / 2f;
			drawRectangle = default(Rectangle);
			drawRectangle = default(Rectangle);
			tileHorizontal = true;
			SetCameraZoom(GameElementsControl.Camera.Zoom);
		}

		public void Load(string backgroundName, Vector2 bPosition, Vector2 pFactor, Color colorBottom, bool horizontal)
		{
			basePosition = bPosition;
			parallaxFactor = Vector2.Clamp(pFactor, Vector2.Zero, Vector2.One);
			GameElementsControl.LoadSprite("background-" + backgroundName, "sprites\\backgrounds\\" + backgroundName);
			GameElementsControl.LoadSprite("whitePixel", "sprites\\whitepixel");
			backgroundSprite = GameElementsControl.GetSprite("background-" + backgroundName);
			bottomFrameSprite = GameElementsControl.GetSprite("whitePixel");
			bottomFrameColor = colorBottom;
			bottomDrawPosition = new Vector2(0f, basePosition.Y + (float)backgroundSprite.Height);
			halfLevelPos = (GameElementsControl.Level.UpLeft + GameElementsControl.Level.DownRight) / 2f;
			drawRectangle = default(Rectangle);
			drawRectangle = default(Rectangle);
			tileHorizontal = horizontal;
			if (offset.X > (float)backgroundSprite.Width)
			{
				offset.X = (float)backgroundSprite.Width % offset.X;
			}
			if (offset.Y > (float)backgroundSprite.Height)
			{
				offset.Y = (float)backgroundSprite.Height % offset.Y;
			}
			SetCameraZoom(GameElementsControl.Camera.Zoom);
		}

		public void Update()
		{
			cameraPos = GameElementsControl.Camera.Position;
			if (tileHorizontal)
			{
				drawRectangle = new Rectangle((int)((offset.X + cameraPos.X) * parallaxFactor.X), 0, tw - cameraXOffset, backgroundSprite.Height);
				drawPosition = basePosition - new Vector2(0f, (int)((cameraPos.Y - halfLevelPos.Y) * offset.Y * parallaxFactor.Y));
			}
			else if (basePosition.Y == 0f)
			{
				drawRectangle = new Rectangle((int)(cameraPos.X * parallaxFactor.X), (int)(cameraPos.Y * parallaxFactor.Y), tw - cameraXOffset, th);
				drawPosition = basePosition;
			}
			else
			{
				drawRectangle = new Rectangle((int)((offset.X + cameraPos.X) * parallaxFactor.X), 0, tw - cameraXOffset, th);
				drawPosition = basePosition - new Vector2(0f, (int)((cameraPos.Y - halfLevelPos.Y) * offset.Y * parallaxFactor.Y));
			}
			drawPosition.X = cameraPos.X + (float)cameraXOffset;
			bottomDrawPosition = new Vector2(0f, drawPosition.Y + (float)backgroundSprite.Height);
		}

		public void SetCameraZoom(float zoom)
		{
			cameraXOffset = -tw / 2;
			while ((float)cameraXOffset > (float)(-(tw / 2)) / zoom)
			{
				cameraXOffset -= backgroundSprite.Width;
			}
			drawRectangleBottom = new Rectangle(0, 0, (int)((float)tw / zoom), (int)((float)th / zoom));
		}

		public void Draw()
		{
			backgroundSprite.Draw(drawPosition, drawRectangle);
			if (tileHorizontal && !noBottom)
			{
				bottomFrameSprite.Draw(bottomDrawPosition, drawRectangleBottom, bottomFrameColor);
			}
		}
	}
}
