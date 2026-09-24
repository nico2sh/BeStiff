using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Nuclex.Input.Devices;

namespace Nuclex.UserInterface.Input;

/// <summary>Default implementation of an input capturer</summary>
public class DefaultInputCapturer : IInputCapturer, IDisposable
{
	/// <summary>Dummy receiver for input events</summary>
	private class DummyInputReceiver : IInputReceiver
	{
		/// <summary>Default instance of the dummy receiver</summary>
		public static readonly DummyInputReceiver Default = new DummyInputReceiver();

		/// <summary>Injects an input command into the input receiver</summary>
		/// <param name="command">Input command to be injected</param>
		public void InjectCommand(Command command)
		{
		}

		/// <summary>Called when a button on the gamepad has been pressed</summary>
		/// <param name="button">Button that has been pressed</param>
		public void InjectButtonPress(Buttons button)
		{
		}

		/// <summary>Called when a button on the gamepad has been released</summary>
		/// <param name="button">Button that has been released</param>
		public void InjectButtonRelease(Buttons button)
		{
		}

		/// <summary>Injects a mouse position update into the receiver</summary>
		/// <param name="x">New X coordinate of the mouse cursor on the screen</param>
		/// <param name="y">New Y coordinate of the mouse cursor on the screen</param>
		public void InjectMouseMove(float x, float y)
		{
		}

		/// <summary>Called when a mouse button has been pressed down</summary>
		/// <param name="button">Index of the button that has been pressed</param>
		public void InjectMousePress(MouseButtons button)
		{
		}

		/// <summary>Called when a mouse button has been released again</summary>
		/// <param name="button">Index of the button that has been released</param>
		public void InjectMouseRelease(MouseButtons button)
		{
		}

		/// <summary>Called when the mouse wheel has been rotated</summary>
		/// <param name="ticks">Number of ticks that the mouse wheel has been rotated</param>
		public void InjectMouseWheel(float ticks)
		{
		}

		/// <summary>Called when a key on the keyboard has been pressed down</summary>
		/// <param name="keyCode">Code of the key that was pressed</param>
		public void InjectKeyPress(Keys keyCode)
		{
		}

		/// <summary>Called when a key on the keyboard has been released again</summary>
		/// <param name="keyCode">Code of the key that was released</param>
		public void InjectKeyRelease(Keys keyCode)
		{
		}

		/// <summary>Handle user text input by a physical or virtual keyboard</summary>
		/// <param name="character">Character that has been entered</param>
		public void InjectCharacter(char character)
		{
		}
	}

	/// <summary>Player index this input capturer is working with</summary>
	private ExtendedPlayerIndex playerIndex;

	/// <summary>Current receiver of input events</summary>
	/// <remarks>
	///   Always valid. If no input receiver is assigned, this field will be set
	///   to a dummy receiver.
	/// </remarks>
	private IInputReceiver inputReceiver;

	/// <summary>Input service the capturer is currently subscribed to</summary>
	private IInputService inputService;

	/// <summary>Keyboard the input capturer is subscribed to</summary>
	private IKeyboard subscribedKeyboard;

	/// <summary>Mouse the input capturer is subscribed to</summary>
	private IMouse subscribedMouse;

	/// <summary>Game pad the input capturer is subscribed to</summary>
	private IGamePad subscribedGamePad;

	/// <summary>Chat pad the input capturer is subscribed to</summary>
	private IKeyboard subscribedChatPad;

	/// <summary>Delegate for the keyPressed() method</summary>
	private KeyDelegate keyPressedDelegate;

	/// <summary>Delegate for the keyReleased() method</summary>
	private KeyDelegate keyReleasedDelegate;

	/// <summary>Delegate for the characterEntered() method</summary>
	private CharacterDelegate characterEnteredDelegate;

	/// <summary>Delegate for the mouseButtonPressed() method</summary>
	private MouseButtonDelegate mouseButtonPressedDelegate;

	/// <summary>Delegate for the mouseButtonReleased() method</summary>
	private MouseButtonDelegate mouseButtonReleasedDelegate;

	/// <summary>Delegate for the mouseMoved() method</summary>
	private MouseMoveDelegate mouseMovedDelegate;

	/// <summary>Delegate for the mouseWheelRotated() method</summary>
	private MouseWheelDelegate mouseWheelRotatedDelegate;

	/// <summary>Delegate for the buttonPressed() method</summary>
	private GamePadButtonDelegate buttonPressedDelegate;

	/// <summary>Delegate for the buttonReleased() method</summary>
	private GamePadButtonDelegate buttonReleasedDelegate;

	/// <summary>Input receiver any captured input will be sent to</summary>
	public IInputReceiver InputReceiver
	{
		get
		{
			if (object.ReferenceEquals(inputReceiver, DummyInputReceiver.Default))
			{
				return null;
			}
			return inputReceiver;
		}
		set
		{
			if (value == null)
			{
				inputReceiver = DummyInputReceiver.Default;
			}
			else
			{
				inputReceiver = value;
			}
		}
	}

	/// <summary>
	///   Initializes a new input capturer, taking the input service from a service provider
	/// </summary>
	/// <param name="serviceProvider">
	///   Service provider the input capturer will take the input service from
	/// </param>
	public DefaultInputCapturer(IServiceProvider serviceProvider)
		: this(getInputService(serviceProvider))
	{
	}

	/// <summary>
	///   Initializes a new input capturer using the specified input service
	/// </summary>
	/// <param name="inputService">
	///   Input service the capturer will subscribe to
	/// </param>
	public DefaultInputCapturer(IInputService inputService)
	{
		this.inputService = inputService;
		inputReceiver = new DummyInputReceiver();
		playerIndex = ExtendedPlayerIndex.One;
		keyPressedDelegate = keyPressed;
		keyReleasedDelegate = keyReleased;
		characterEnteredDelegate = characterEntered;
		mouseButtonPressedDelegate = mouseButtonPressed;
		mouseButtonReleasedDelegate = mouseButtonReleased;
		mouseMovedDelegate = mouseMoved;
		mouseWheelRotatedDelegate = mouseWheelRotated;
		buttonPressedDelegate = buttonPressed;
		buttonReleasedDelegate = buttonReleased;
		subscribeInputDevices();
	}

	/// <summary>Immediately releases all resources owned by the instance</summary>
	public void Dispose()
	{
		if (inputService != null)
		{
			unsubscribeInputDevices();
			inputService = null;
		}
	}

	/// <summary>Changes the controller which can interact with the GUI</summary>
	/// <param name="playerIndex">
	///   Index of the player whose controller will be allowed to interact with the GUI
	/// </param>
	public void ChangePlayerIndex(PlayerIndex playerIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		ChangePlayerIndex((ExtendedPlayerIndex)playerIndex);
	}

	/// <summary>Changes the controller which can interact with the GUI</summary>
	/// <param name="playerIndex">
	///   Index of the player whose controller will be allowed to interact with the GUI
	/// </param>
	public void ChangePlayerIndex(ExtendedPlayerIndex playerIndex)
	{
		unsubscribePlayerSpecificInputDevices();
		this.playerIndex = playerIndex;
		subscribePlayerSpecificDevices();
	}

	/// <summary>Subscribes to the events of all input devices</summary>
	private void subscribeInputDevices()
	{
		subscribedKeyboard = inputService.GetKeyboard();
		subscribedKeyboard.KeyPressed += keyPressedDelegate;
		subscribedKeyboard.KeyReleased += keyReleasedDelegate;
		subscribedKeyboard.CharacterEntered += characterEnteredDelegate;
		subscribedMouse = inputService.GetMouse();
		subscribedMouse.MouseButtonPressed += mouseButtonPressedDelegate;
		subscribedMouse.MouseButtonReleased += mouseButtonReleasedDelegate;
		subscribedMouse.MouseMoved += mouseMovedDelegate;
		subscribedMouse.MouseWheelRotated += mouseWheelRotatedDelegate;
		subscribePlayerSpecificDevices();
	}

	/// <summary>Subscribes to the events of all player-specific input devices</summary>
	private void subscribePlayerSpecificDevices()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		subscribedGamePad = inputService.GetGamePad(playerIndex);
		subscribedGamePad.ButtonPressed += buttonPressedDelegate;
		subscribedGamePad.ButtonReleased += buttonReleasedDelegate;
		if (playerIndex < ExtendedPlayerIndex.Five)
		{
			PlayerIndex val = (PlayerIndex)playerIndex;
			subscribedChatPad = inputService.GetKeyboard(val);
			subscribedChatPad.KeyPressed += keyPressedDelegate;
			subscribedChatPad.KeyReleased += keyReleasedDelegate;
			subscribedChatPad.CharacterEntered += characterEnteredDelegate;
		}
	}

	/// <summary>Unsubscribes from the events of all input devices</summary>
	private void unsubscribeInputDevices()
	{
		unsubscribePlayerSpecificInputDevices();
		if (subscribedKeyboard != null)
		{
			subscribedKeyboard.CharacterEntered -= characterEnteredDelegate;
			subscribedKeyboard.KeyReleased -= keyReleasedDelegate;
			subscribedKeyboard.KeyPressed -= keyPressedDelegate;
			subscribedKeyboard = null;
		}
		if (subscribedMouse != null)
		{
			subscribedMouse.MouseWheelRotated -= mouseWheelRotatedDelegate;
			subscribedMouse.MouseMoved -= mouseMovedDelegate;
			subscribedMouse.MouseButtonReleased -= mouseButtonReleasedDelegate;
			subscribedMouse.MouseButtonPressed -= mouseButtonPressedDelegate;
			subscribedMouse = null;
		}
	}

	/// <summary>Unsubscribes from the events of all player-specific input devices</summary>
	private void unsubscribePlayerSpecificInputDevices()
	{
		if (subscribedChatPad != null)
		{
			subscribedChatPad.CharacterEntered -= characterEnteredDelegate;
			subscribedChatPad.KeyReleased -= keyReleasedDelegate;
			subscribedChatPad.KeyPressed -= keyPressedDelegate;
			subscribedChatPad = null;
		}
		if (subscribedGamePad != null)
		{
			subscribedGamePad.ButtonPressed -= buttonPressedDelegate;
			subscribedGamePad.ButtonReleased -= buttonReleasedDelegate;
			subscribedGamePad = null;
		}
	}

	/// <summary>Called when a button on the game pad has been released</summary>
	/// <param name="buttons">Button that has been released</param>
	private void buttonReleased(Buttons buttons)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		inputReceiver.InjectButtonRelease(buttons);
	}

	/// <summary>Called when a button on the game pad has been pressed</summary>
	/// <param name="buttons">Button that has been pressed</param>
	private void buttonPressed(Buttons buttons)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (((int)buttons & 1) != 0)
		{
			inputReceiver.InjectCommand(Command.Up);
		}
		else if (((int)buttons & 2) != 0)
		{
			inputReceiver.InjectCommand(Command.Down);
		}
		else if (((int)buttons & 4) != 0)
		{
			inputReceiver.InjectCommand(Command.Left);
		}
		else if (((int)buttons & 8) != 0)
		{
			inputReceiver.InjectCommand(Command.Right);
		}
		else
		{
			inputReceiver.InjectButtonPress(buttons);
		}
	}

	/// <summary>Called when the mouse wheel has been rotated</summary>
	/// <param name="ticks">Number of ticks the wheel was rotated</param>
	private void mouseWheelRotated(float ticks)
	{
		inputReceiver.InjectMouseWheel(ticks);
	}

	/// <summary>Called when the mouse cursor has been moved</summary>
	/// <param name="x">New X coordinate of the mouse cursor</param>
	/// <param name="y">New Y coordinate of the mouse cursor</param>
	private void mouseMoved(float x, float y)
	{
		inputReceiver.InjectMouseMove(x, y);
	}

	/// <summary>Called when a mouse button has been released</summary>
	/// <param name="buttons">Mouse button that has been released</param>
	private void mouseButtonReleased(MouseButtons buttons)
	{
		inputReceiver.InjectMouseRelease(buttons);
	}

	/// <summary>Called when a mouse button has been pressed</summary>
	/// <param name="buttons">Mouse button that has been pressed</param>
	private void mouseButtonPressed(MouseButtons buttons)
	{
		inputReceiver.InjectMousePress(buttons);
	}

	/// <summary>Called when a character has been entered on the keyboard</summary>
	/// <param name="character">Character that has been entered</param>
	private void characterEntered(char character)
	{
		inputReceiver.InjectCharacter(character);
	}

	/// <summary>Called when a key has been released</summary>
	/// <param name="key">Key that was released</param>
	private void keyReleased(Keys key)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		inputReceiver.InjectKeyRelease(key);
	}

	/// <summary>Called when a key has been pressed</summary>
	/// <param name="key">Key that was pressed</param>
	private void keyPressed(Keys key)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		inputReceiver.InjectKeyPress(key);
	}

	/// <summary>Retrieves the input service from a service provider</summary>
	/// <param name="serviceProvider">
	///   Service provider the service is taken from
	/// </param>
	/// <returns>The input service stored in the service provider</returns>
	private static IInputService getInputService(IServiceProvider serviceProvider)
	{
		IInputService inputService = (IInputService)serviceProvider.GetService(typeof(IInputService));
		if (inputService == null)
		{
			throw new InvalidOperationException("Using the GUI with the DefaultInputCapturer requires the IInputService. Please either add the IInputService to Game.Services by using the Nuclex.Input.InputManager in your game or provide a custom IInputCapturer implementation for the GUI and assign it before GuiManager.Initialize() is called.");
		}
		return inputService;
	}
}
