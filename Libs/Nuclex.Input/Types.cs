using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Nuclex.Input
{
	[Flags]
	public enum MouseButtons
	{
		Left = 1,
		Middle = 2,
		Right = 4,
		X1 = 8,
		X2 = 0x10
	}

	public enum ExtendedPlayerIndex
	{
		One,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight
	}

	public delegate void CharacterDelegate(char character);

	public delegate void GamePadButtonDelegate(Buttons buttons);

	public delegate void KeyDelegate(Keys key);

	public delegate void MouseButtonDelegate(MouseButtons buttons);

	public delegate void MouseMoveDelegate(float x, float y);

	public delegate void MouseWheelDelegate(float ticks);

	public delegate void TouchDelegate(int id, Vector2 position);
}
