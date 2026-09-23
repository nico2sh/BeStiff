using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff
{
	internal class InfoSign : InteractiveWorldObject, IShadowCaster
	{
		private GameSprite infoSprite;

		private string infoText;

		private string typedText;

		private double typedTextLength;

		private int delayInMilliseconds;

		private bool isDoneDrawing;

		private SpriteFont font;

		private Texture2D boxBackground;

		private Vector2 boxPosition;

		private Vector2 textPadding;

		private Rectangle textBoxFrame;

		private Rectangle textBoxBackground;

		private int frameWidth;

		private Color frameColor;

		private Color backgroundColor;

		private Color fontColor;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Info"))
			{
				return false;
			}
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			base.Name = "InfoSign-" + GameElementsControl.Counter;
			textBoxFrame = new Rectangle(5, 5, 180, 100);
			textPadding = new Vector2(10f, 10f);
			frameWidth = 5;
			GameElementsControl.LoadSprite("whitePixel", "sprites\\whitepixel");
			GameElementsControl.LoadSprite("info", "sprites\\objects\\info");
			GameElementsControl.LoadFont("infoSignFont", "fonts\\infosignfont");
			font = GameElementsControl.GetFont("infoSignFont");
			infoSprite = GameElementsControl.GetSprite("info");
			boxBackground = GameElementsControl.GetSprite("whitePixel").Texture;
			infoText = ParseText(obj.GetValue<OgmoStringValue>("text").Value);
			textBoxFrame.Height = (int)(font.MeasureString(infoText).Y + textPadding.Y * 2f);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Static;
			mainBody.Position = worldScaledOgmoObject.Position;
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(-0.5f, 0.75f));
			vertices.Add(new Vector2(-0.5f, -0.75f));
			vertices.Add(new Vector2(0.5f, -0.75f));
			vertices.Add(new Vector2(0.5f, 0.75f));
			PolygonShape shape = new PolygonShape(vertices, 300f);
			sensorFixture = mainBody.CreateFixture(shape);
			sensorFixture.UserData = new WorldObjectData(WorldObjectType.InteractiveSensor, this);
			mainBody.IsSensor = true;
			boxPosition = GameElementsControl.ConvertWorldToScreen(mainBody.Position) - new Vector2(textBoxFrame.Width / 2, textBoxFrame.Height + 30);
			boxPosition = Vector2.Subtract(value2: new Vector2(obj.GetValue<OgmoNumberValue>("OffsetX").Value, obj.GetValue<OgmoNumberValue>("OffsetY").Value), value1: boxPosition);
			textBoxFrame.X = (int)boxPosition.X;
			textBoxFrame.Y = (int)boxPosition.Y;
			textBoxBackground = new Rectangle((int)boxPosition.X + frameWidth, (int)boxPosition.Y + frameWidth, textBoxFrame.Width - 2 * frameWidth, textBoxFrame.Height - 2 * frameWidth);
			frameColor = new Color(0, 0, 0, 50);
			backgroundColor = new Color(134, 115, 98, 50);
			fontColor = new Color(73, 55, 23, 255);
			delayInMilliseconds = 30;
			isDoneDrawing = false;
			GameElementsControl.ScreenManager.AudioManager.LoadSound("infoSign", "audio\\noises\\infoSign");
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		private string ParseText(string text)
		{
			string text2 = string.Empty;
			string text3 = string.Empty;
			string[] array = text.Split(new char[1] { ' ' });
			string[] array2 = array;
			foreach (string text4 in array2)
			{
				if (font.MeasureString(text2 + text4).Length() > (float)textBoxFrame.Width - textPadding.X * 2f)
				{
					text3 = text3 + text2 + '\n';
					text2 = string.Empty;
				}
				text2 = text2 + text4 + ' ';
			}
			return text3 + text2;
		}

		public override void Update()
		{
			if (isDoneDrawing || !interacting)
			{
				return;
			}
			if (delayInMilliseconds == 0)
			{
				isDoneDrawing = true;
			}
			else if (typedTextLength < (double)infoText.Length)
			{
				int num = (int)typedTextLength;
				typedTextLength += GameElementsControl.LastFrameTimeInMS / (double)delayInMilliseconds;
				if (num < (int)typedTextLength)
				{
					GameElementsControl.NoiseManager.AddNoise("infoSign", mainBody.Position);
				}
				if (typedTextLength >= (double)infoText.Length)
				{
					typedTextLength = infoText.Length;
					isDoneDrawing = true;
				}
				typedText = infoText.Substring(0, (int)typedTextLength);
			}
		}

		public override bool Interacts(WorldObjectType type)
		{
			return type == WorldObjectType.Hero;
		}

		public override void StopInteracting(WorldObject worldObject)
		{
			base.StopInteracting(worldObject);
			if (!interacting)
			{
				typedText = "";
				isDoneDrawing = false;
				typedTextLength = 0.0;
			}
		}

		public override void Draw()
		{
			infoSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
		}

		public void DrawHull()
		{
			if (interacting)
			{
				GameElementsControl.ScreenManager.SpriteBatch.Draw(boxBackground, textBoxFrame, frameColor);
				GameElementsControl.ScreenManager.SpriteBatch.Draw(boxBackground, textBoxBackground, backgroundColor);
				GameElementsControl.ScreenManager.SpriteBatch.DrawString(font, typedText, boxPosition + textPadding, fontColor);
			}
		}
	}
}
