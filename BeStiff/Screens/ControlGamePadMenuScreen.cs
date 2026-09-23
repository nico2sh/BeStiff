using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Be_Stiff.Screens
{
	internal class ControlGamePadMenuScreen : MenuScreen
	{
		private string[,] controls;

		private int selectedControlX;

		private int selectedControlY;

		private Texture2D controlsBackground;

		private bool waitForButton;

		public ControlGamePadMenuScreen()
			: base("Controls")
		{
			waitForButton = false;
			selectedControlX = 1;
			selectedControlY = 0;
			controls = new string[2, 13];
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
			controls[1, 0] = "";
			controls[1, 1] = "";
			controls[1, 2] = "";
			controls[1, 3] = "";
			controls[1, 4] = Enum.GetName(typeof(Buttons), Globals.InputButtonJump);
			controls[1, 5] = Enum.GetName(typeof(Buttons), Globals.InputButtonReload);
			controls[1, 6] = Enum.GetName(typeof(Buttons), Globals.InputButtonShowMap);
			controls[1, 7] = Enum.GetName(typeof(Buttons), Globals.InputButtonSwitch);
			controls[1, 8] = Enum.GetName(typeof(Buttons), Globals.InputButtonSlide);
			controls[1, 9] = Enum.GetName(typeof(Buttons), Globals.InputButtonSloMo);
			controls[1, 10] = Enum.GetName(typeof(Buttons), Globals.InputButtonKick);
			controls[1, 11] = Enum.GetName(typeof(Buttons), Globals.InputButtonShoot);
			controls[1, 12] = Enum.GetName(typeof(Buttons), Globals.InputButtonSecShoot);
		}

		public override void LoadContent()
		{
			base.LoadContent();
			controlsBackground = base.ScreenManager.Content.Load<Texture2D>("sprites\\menubackgrounds\\gamepadcontrolsbackground");
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		}

		private Buttons GetButton(string name)
		{
			switch (name)
			{
			case "Jump":
				return Globals.InputButtonJump;
			case "Reload":
				return Globals.InputButtonReload;
			case "Show Map":
				return Globals.InputButtonShowMap;
			case "Switch":
				return Globals.InputButtonSwitch;
			case "Slide":
				return Globals.InputButtonSlide;
			case "Slow Motion":
				return Globals.InputButtonSloMo;
			case "Kick":
				return Globals.InputButtonKick;
			case "Shoot":
				return Globals.InputButtonShoot;
			case "Sec Shoot":
				return Globals.InputButtonSecShoot;
			default:
				return Buttons.Back;
			}
		}

		private void SetButton(string name, Buttons newButton)
		{
			switch (name)
			{
			case "Jump":
				Globals.InputButtonJump = newButton;
				break;
			case "Reload":
				Globals.InputButtonReload = newButton;
				break;
			case "Show Map":
				Globals.InputButtonShowMap = newButton;
				break;
			case "Switch":
				Globals.InputButtonSwitch = newButton;
				break;
			case "Slide":
				Globals.InputButtonSlide = newButton;
				break;
			case "Slow Motion":
				Globals.InputButtonSloMo = newButton;
				break;
			case "Kick":
				Globals.InputButtonKick = newButton;
				break;
			case "Shoot":
				Globals.InputButtonShoot = newButton;
				break;
			case "Sec Shoot":
				Globals.InputButtonSecShoot = newButton;
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
				Buttons button;
				bool newButtonPress = input.ControlGamePad.GetNewButtonPress(null, out playerIndex, out button);
				int num = selectedControlX;
				if (num != 1 || !newButtonPress)
				{
					return;
				}
				for (int i = 0; i < controls.GetLength(1); i++)
				{
					if (GetButton(controls[0, i]) == button && text != controls[0, i])
					{
						SetButton(controls[0, i], GetButton(text));
						break;
					}
				}
				SetButton(text, button);
				waitForButton = false;
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
