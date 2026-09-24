using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Nuclex.Input.Devices
{
	public interface IInputDevice
	{
		bool IsAttached { get; }

		string Name { get; }

		void Update();

		void TakeSnapshot();
	}

	public interface IKeyboard : IInputDevice
	{
		event KeyDelegate KeyPressed;

		event KeyDelegate KeyReleased;

		event CharacterDelegate CharacterEntered;

		KeyboardState GetState();
	}

	public interface IMouse : IInputDevice
	{
		event MouseMoveDelegate MouseMoved;

		event MouseButtonDelegate MouseButtonPressed;

		event MouseButtonDelegate MouseButtonReleased;

		event MouseWheelDelegate MouseWheelRotated;

		MouseState GetState();

		void MoveTo(float x, float y);
	}

	public interface IGamePad : IInputDevice
	{
		event GamePadButtonDelegate ButtonPressed;

		event GamePadButtonDelegate ButtonReleased;

		GamePadState GetState();
	}

	public interface ITouchPanel : IInputDevice
	{
	}

	/// <summary>Keyboard polled each update; characters come from the window's text input.</summary>
	internal sealed class PolledKeyboard : IKeyboard
	{
		private static readonly Keys[] AllKeys = (Keys[])Enum.GetValues(typeof(Keys));

		private readonly Queue<char> typed = new Queue<char>();

		private KeyboardState previous;

		private KeyboardState current;

		public PolledKeyboard(GameWindow window)
		{
			if (window != null)
			{
				window.TextInput += (sender, e) =>
				{
					lock (typed)
					{
						typed.Enqueue(e.Character);
					}
				};
			}
		}

		public event KeyDelegate KeyPressed;

		public event KeyDelegate KeyReleased;

		public event CharacterDelegate CharacterEntered;

		public bool IsAttached => true;

		public string Name => "Keyboard";

		public KeyboardState GetState()
		{
			return current;
		}

		public void TakeSnapshot()
		{
		}

		public void Update()
		{
			previous = current;
			current = Keyboard.GetState();
			foreach (Keys key in AllKeys)
			{
				bool down = current.IsKeyDown(key);
				bool wasDown = previous.IsKeyDown(key);
				if (down && !wasDown)
				{
					KeyPressed?.Invoke(key);
				}
				else if (!down && wasDown)
				{
					KeyReleased?.Invoke(key);
				}
			}
			while (true)
			{
				char character;
				lock (typed)
				{
					if (typed.Count == 0)
					{
						break;
					}
					character = typed.Dequeue();
				}
				CharacterEntered?.Invoke(character);
			}
		}
	}

	/// <summary>Mouse polled each update.</summary>
	internal sealed class PolledMouse : IMouse
	{
		private MouseState previous;

		private MouseState current;

		public event MouseMoveDelegate MouseMoved;

		public event MouseButtonDelegate MouseButtonPressed;

		public event MouseButtonDelegate MouseButtonReleased;

		public event MouseWheelDelegate MouseWheelRotated;

		public bool IsAttached => true;

		public string Name => "Mouse";

		public MouseState GetState()
		{
			return current;
		}

		public void MoveTo(float x, float y)
		{
			Mouse.SetPosition((int)(x * InputManager.MouseScale), (int)(y * InputManager.MouseScale));
		}

		public void TakeSnapshot()
		{
		}

		public void Update()
		{
			previous = current;
			current = Mouse.GetState();
			if (current.X != previous.X || current.Y != previous.Y)
			{
				float scale = InputManager.MouseScale;
				MouseMoved?.Invoke(current.X / scale, current.Y / scale);
			}
			CheckButton(previous.LeftButton, current.LeftButton, MouseButtons.Left);
			CheckButton(previous.MiddleButton, current.MiddleButton, MouseButtons.Middle);
			CheckButton(previous.RightButton, current.RightButton, MouseButtons.Right);
			CheckButton(previous.XButton1, current.XButton1, MouseButtons.X1);
			CheckButton(previous.XButton2, current.XButton2, MouseButtons.X2);
			int wheel = current.ScrollWheelValue - previous.ScrollWheelValue;
			if (wheel != 0)
			{
				// One wheel notch is 120 units; Nuclex reports notches.
				MouseWheelRotated?.Invoke(wheel / 120f);
			}
		}

		private void CheckButton(ButtonState before, ButtonState now, MouseButtons button)
		{
			if (now == ButtonState.Pressed && before == ButtonState.Released)
			{
				MouseButtonPressed?.Invoke(button);
			}
			else if (now == ButtonState.Released && before == ButtonState.Pressed)
			{
				MouseButtonReleased?.Invoke(button);
			}
		}
	}

	/// <summary>Game pad polled each update.</summary>
	internal sealed class PolledGamePad : IGamePad
	{
		private static readonly Buttons[] AllButtons = (Buttons[])Enum.GetValues(typeof(Buttons));

		private readonly PlayerIndex index;

		private GamePadState previous;

		private GamePadState current;

		public PolledGamePad(PlayerIndex index)
		{
			this.index = index;
		}

		public event GamePadButtonDelegate ButtonPressed;

		public event GamePadButtonDelegate ButtonReleased;

		public bool IsAttached => current.IsConnected;

		public string Name => "Game pad " + index;

		public GamePadState GetState()
		{
			return current;
		}

		public void TakeSnapshot()
		{
		}

		public void Update()
		{
			previous = current;
			current = GamePad.GetState(index);
			foreach (Buttons button in AllButtons)
			{
				bool down = current.IsButtonDown(button);
				bool wasDown = previous.IsButtonDown(button);
				if (down && !wasDown)
				{
					ButtonPressed?.Invoke(button);
				}
				else if (!down && wasDown)
				{
					ButtonReleased?.Invoke(button);
				}
			}
		}
	}
}
