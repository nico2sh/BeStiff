using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Be_Stiff
{
	internal class ControlKeyboardMenuScreen : MenuScreen
	{
		private string[,] controls;

		private int selectedControlX;

		private int selectedControlY;

		private Texture2D controlsBackground;

		private bool waitForButton;

		public ControlKeyboardMenuScreen()
			: base("Controls")
		{
			waitForButton = false;
			selectedControlX = 1;
			selectedControlY = 0;
			controls = new string[3, 13];
			SetMenuEntryText();
		}

		private void SetMenuEntryText()
		{
			controls[0, 0] = "Left";
			controls[0, 1] = "Right";
			controls[0, 2] = "Up";
			controls[0, 3] = "Down";
			controls[0, 4] = "Jump";
			controls[0, 5] = "Reload";
			controls[0, 6] = "Show Map";
			controls[0, 7] = "Switch";
			controls[0, 8] = "Slide";
			controls[0, 9] = "Slow Motion";
			controls[0, 10] = "Kick";
			controls[0, 11] = "Shoot";
			controls[0, 12] = "Sec Shoot";
			controls[1, 0] = Enum.GetName(typeof(Keys), Globals.InputKeyLeft);
			controls[1, 1] = Enum.GetName(typeof(Keys), Globals.InputKeyRight);
			controls[1, 2] = Enum.GetName(typeof(Keys), Globals.InputKeyUp);
			controls[1, 3] = Enum.GetName(typeof(Keys), Globals.InputKeyDown);
			controls[1, 4] = Enum.GetName(typeof(Keys), Globals.InputKeyJump);
			controls[1, 5] = Enum.GetName(typeof(Keys), Globals.InputKeyReload);
			controls[1, 6] = Enum.GetName(typeof(Keys), Globals.InputKeyShowMap);
			controls[1, 7] = Enum.GetName(typeof(Keys), Globals.InputKeySwitch);
			controls[1, 8] = Enum.GetName(typeof(Keys), Globals.InputKeySlide);
			controls[1, 9] = Enum.GetName(typeof(Keys), Globals.InputKeySloMo);
			controls[1, 10] = Enum.GetName(typeof(Keys), Globals.InputKeyKick);
			controls[1, 11] = Enum.GetName(typeof(Keys), Globals.InputKeyShoot);
			controls[1, 12] = Enum.GetName(typeof(Keys), Globals.InputKeySecShoot);
			controls[2, 0] = "";
			controls[2, 1] = "";
			controls[2, 2] = "";
			controls[2, 3] = "";
			controls[2, 4] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonJump);
			controls[2, 5] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonReload);
			controls[2, 6] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonShowMap);
			controls[2, 7] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonSwitch);
			controls[2, 8] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonSlide);
			controls[2, 9] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonSloMo);
			controls[2, 10] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonKick);
			controls[2, 11] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonShoot);
			controls[2, 12] = Enum.GetName(typeof(MouseButtons), Globals.InputMouseButtonSecShoot);
		}

		public override void LoadContent()
		{
			base.LoadContent();
			controlsBackground = base.ScreenManager.Content.Load<Texture2D>("sprites\\menubackgrounds\\keyboardcontrolsbackground");
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		}

		private Keys GetKey(string name)
		{
			switch (name)
			{
			case "Left":
				return Globals.InputKeyLeft;
			case "Right":
				return Globals.InputKeyRight;
			case "Up":
				return Globals.InputKeyUp;
			case "Down":
				return Globals.InputKeyDown;
			case "Jump":
				return Globals.InputKeyJump;
			case "Reload":
				return Globals.InputKeyReload;
			case "Show Map":
				return Globals.InputKeyShowMap;
			case "Switch":
				return Globals.InputKeySwitch;
			case "Slide":
				return Globals.InputKeySlide;
			case "Slow Motion":
				return Globals.InputKeySloMo;
			case "Kick":
				return Globals.InputKeyKick;
			case "Shoot":
				return Globals.InputKeyShoot;
			case "Sec Shoot":
				return Globals.InputKeySecShoot;
			default:
				return Keys.None;
			}
		}

		private void SetKey(string name, Keys newKey)
		{
			switch (name)
			{
			case "Left":
				Globals.InputKeyLeft = newKey;
				break;
			case "Right":
				Globals.InputKeyRight = newKey;
				break;
			case "Up":
				Globals.InputKeyUp = newKey;
				break;
			case "Down":
				Globals.InputKeyDown = newKey;
				break;
			case "Jump":
				Globals.InputKeyJump = newKey;
				break;
			case "Reload":
				Globals.InputKeyReload = newKey;
				break;
			case "Show Map":
				Globals.InputKeyShowMap = newKey;
				break;
			case "Switch":
				Globals.InputKeySwitch = newKey;
				break;
			case "Slide":
				Globals.InputKeySlide = newKey;
				break;
			case "Slow Motion":
				Globals.InputKeySloMo = newKey;
				break;
			case "Kick":
				Globals.InputKeyKick = newKey;
				break;
			case "Shoot":
				Globals.InputKeyShoot = newKey;
				break;
			case "Sec Shoot":
				Globals.InputKeySecShoot = newKey;
				break;
			}
		}

		private MouseButtons GetMouseButton(string name)
		{
			switch (name)
			{
			case "Jump":
				return Globals.InputMouseButtonJump;
			case "Reload":
				return Globals.InputMouseButtonReload;
			case "Show Map":
				return Globals.InputMouseButtonShowMap;
			case "Switch":
				return Globals.InputMouseButtonSwitch;
			case "Slide":
				return Globals.InputMouseButtonSlide;
			case "Slow Motion":
				return Globals.InputMouseButtonSloMo;
			case "Kick":
				return Globals.InputMouseButtonKick;
			case "Shoot":
				return Globals.InputMouseButtonShoot;
			case "Sec Shoot":
				return Globals.InputMouseButtonSecShoot;
			default:
				return MouseButtons.None;
			}
		}

		private void SetMouseButton(string name, MouseButtons newMouseButton)
		{
			switch (name)
			{
			case "Jump":
				Globals.InputMouseButtonJump = newMouseButton;
				break;
			case "Reload":
				Globals.InputMouseButtonReload = newMouseButton;
				break;
			case "Show Map":
				Globals.InputMouseButtonShowMap = newMouseButton;
				break;
			case "Switch":
				Globals.InputMouseButtonSwitch = newMouseButton;
				break;
			case "Slide":
				Globals.InputMouseButtonSlide = newMouseButton;
				break;
			case "Slow Motion":
				Globals.InputMouseButtonSloMo = newMouseButton;
				break;
			case "Kick":
				Globals.InputMouseButtonKick = newMouseButton;
				break;
			case "Shoot":
				Globals.InputMouseButtonShoot = newMouseButton;
				break;
			case "Sec Shoot":
				Globals.InputMouseButtonSecShoot = newMouseButton;
				break;
			}
		}

		private void ReadButton(InputHelper input)
		{
			if (input.IsMenuCancel(null, out var playerIndex))
			{
				waitForButton = false;
			}
			else
			{
				if (input.IsReservedPress(null, out playerIndex))
				{
					return;
				}
				string text = controls[0, selectedControlY];
				Keys newKeyPress = input.ControlKeyboard.GetNewKeyPress(null, out playerIndex);
				MouseButtons newMouseButtonPress = input.ControlKeyboard.GetNewMouseButtonPress();
				switch (selectedControlX)
				{
				case 1:
					switch (newKeyPress)
					{
					case Keys.Delete:
						SetKey(text, Keys.None);
						break;
					default:
					{
						for (int j = 0; j < controls.GetLength(1); j++)
						{
							if (GetKey(controls[0, j]) == newKeyPress && text != controls[0, j])
							{
								SetKey(controls[0, j], GetKey(text));
								break;
							}
						}
						SetKey(text, newKeyPress);
						break;
					}
					case Keys.None:
						return;
					}
					waitForButton = false;
					break;
				case 2:
					if (newKeyPress == Keys.Delete)
					{
						SetMouseButton(text, MouseButtons.None);
						waitForButton = false;
					}
					else
					{
						if (newMouseButtonPress == MouseButtons.None)
						{
							break;
						}
						for (int i = 0; i < controls.GetLength(1); i++)
						{
							if (GetMouseButton(controls[0, i]) == newMouseButtonPress && text != controls[0, i])
							{
								SetMouseButton(controls[0, i], GetMouseButton(text));
								break;
							}
						}
						SetMouseButton(text, newMouseButtonPress);
						waitForButton = false;
					}
					break;
				}
			}
		}

		public override void HandleInput(InputHelper input)
		{
			if (waitForButton)
			{
				ReadButton(input);
				SetMenuEntryText();
				return;
			}
			if (input.IsMenuUp(base.ControllingPlayer))
			{
				selectedControlY--;
				if (selectedControlY < 0)
				{
					selectedControlY = controls.GetLength(1) - 1;
				}
				while (controls[selectedControlX, selectedControlY] == "")
				{
					selectedControlY--;
					if (selectedControlY < 0)
					{
						selectedControlY = controls.GetLength(1) - 1;
					}
				}
			}
			if (input.IsMenuDown(base.ControllingPlayer))
			{
				selectedControlY++;
				if (selectedControlY >= controls.GetLength(1))
				{
					selectedControlY = 0;
				}
				while (controls[selectedControlX, selectedControlY] == "")
				{
					selectedControlY++;
					if (selectedControlY >= controls.GetLength(1))
					{
						selectedControlY = 0;
					}
				}
			}
			if (input.IsMenuLeft(base.ControllingPlayer))
			{
				selectedControlX--;
				if (selectedControlX < 1)
				{
					selectedControlX = controls.GetLength(0) - 1;
				}
				while (controls[selectedControlX, selectedControlY] == "")
				{
					selectedControlX--;
					if (selectedControlX < 1)
					{
						selectedControlX = controls.GetLength(0) - 1;
					}
				}
			}
			if (input.IsMenuRight(base.ControllingPlayer))
			{
				selectedControlX++;
				if (selectedControlX >= controls.GetLength(0))
				{
					selectedControlX = 1;
				}
				while (controls[selectedControlX, selectedControlY] == "")
				{
					selectedControlX++;
					if (selectedControlX >= controls.GetLength(0))
					{
						selectedControlX = 1;
					}
				}
			}
			if (input.IsMenuSelect(base.ControllingPlayer, out var playerIndex))
			{
				waitForButton = true;
			}
			else if (input.IsMenuCancel(base.ControllingPlayer, out playerIndex))
			{
				OnCancel(playerIndex);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			base.ScreenManager.SpriteBatch.Begin();
			Vector2 origin = new Vector2(0f, base.ScreenManager.Font.LineSpacing / 2);
			Vector2 vector = base.BasePosition + new Vector2(40f, 20f);
			float num = (float)Math.Pow(base.TransitionPosition, 2.0);
			if (base.ScreenState == ScreenState.TransitionOn)
			{
				vector.Y -= num * 256f;
			}
			else
			{
				vector.Y += num * 512f;
			}
			base.ScreenManager.SpriteBatch.Draw(controlsBackground, vector - new Vector2(35f, 50f), Color.White);
			vector += new Vector2(0f, 80f);
			Vector2 position = vector;
			for (int i = 0; i < controls.GetLength(0); i++)
			{
				for (int j = 0; j < controls.GetLength(1); j++)
				{
					Color color = Color.Wheat;
					float scale = 0.7f;
					if (i == selectedControlX && j == selectedControlY)
					{
						color = Color.Yellow;
						scale = 0.8f;
					}
					else if (i == 0)
					{
						color = ((j != selectedControlY) ? Color.LightGray : Color.DarkRed);
						scale = 1f;
					}
					else if (waitForButton)
					{
						color = Color.DarkGray;
					}
					color = new Color((int)color.R, (int)color.G, (int)color.B, (int)base.TransitionAlpha);
					base.ScreenManager.SpriteBatch.DrawString(base.ScreenManager.Font, controls[i, j], position, color, 0f, origin, scale, SpriteEffects.None, 0f);
					position.Y += (float)base.ScreenManager.Font.LineSpacing * 0.8f;
				}
				position.Y = vector.Y;
				position.X += 290f;
			}
			base.ScreenManager.SpriteBatch.End();
		}

		protected override void OnCancel(PlayerIndex playerIndex)
		{
			Globals.SaveOptions();
			base.OnCancel(playerIndex);
		}
	}
}
