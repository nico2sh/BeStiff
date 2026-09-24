using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Nuclex.Input.Devices;

namespace Nuclex.Input
{
	public interface IInputService
	{
		ReadOnlyCollection<IKeyboard> Keyboards { get; }

		ReadOnlyCollection<IMouse> Mice { get; }

		ReadOnlyCollection<IGamePad> GamePads { get; }

		ReadOnlyCollection<ITouchPanel> TouchPanels { get; }

		int SnapshotCount { get; }

		IMouse GetMouse();

		IKeyboard GetKeyboard();

		IKeyboard GetKeyboard(PlayerIndex playerIndex);

		IGamePad GetGamePad(PlayerIndex playerIndex);

		IGamePad GetGamePad(ExtendedPlayerIndex playerIndex);

		ITouchPanel GetTouchPanel();

		void Update();

		void TakeSnapshot();
	}

	/// <summary>
	/// Input service for the Nuclex GUI, polling MonoGame's keyboard, mouse and
	/// game pads. Registers itself as <see cref="IInputService"/>.
	/// </summary>
	public class InputManager : IInputService, IGameComponent, IUpdateable, IDisposable
	{
		private readonly GameServiceContainer services;

		private readonly PolledKeyboard keyboard;

		private readonly PolledMouse mouse = new PolledMouse();

		private readonly NoKeyboard noKeyboard = new NoKeyboard();

		private readonly PolledGamePad[] gamePads;

		private readonly ReadOnlyCollection<IKeyboard> keyboards;

		private readonly ReadOnlyCollection<IMouse> mice;

		private readonly ReadOnlyCollection<IGamePad> gamePadList;

		private readonly ReadOnlyCollection<ITouchPanel> touchPanels = new ReadOnlyCollection<ITouchPanel>(Array.Empty<ITouchPanel>());

		private int updateOrder = int.MinValue;

		/// <summary>
		/// Window pixels per UI pixel. Mouse positions are divided by it, for a
		/// UI drawn at a lower resolution and scaled up to the window.
		/// </summary>
		public static float MouseScale = 1f;

		public InputManager(GameServiceContainer services, GameWindow window)
		{
			this.services = services;
			keyboard = new PolledKeyboard(window);
			gamePads = new PolledGamePad[4];
			for (int i = 0; i < gamePads.Length; i++)
			{
				gamePads[i] = new PolledGamePad((PlayerIndex)i);
			}
			keyboards = new ReadOnlyCollection<IKeyboard>(new IKeyboard[] { keyboard });
			mice = new ReadOnlyCollection<IMouse>(new IMouse[] { mouse });
			gamePadList = new ReadOnlyCollection<IGamePad>(gamePads);
			services?.AddService(typeof(IInputService), this);
		}

		/// <summary>Never raised: the input manager is always enabled.</summary>
		public event EventHandler<EventArgs> EnabledChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		public event EventHandler<EventArgs> UpdateOrderChanged;

		public ReadOnlyCollection<IKeyboard> Keyboards => keyboards;

		public ReadOnlyCollection<IMouse> Mice => mice;

		public ReadOnlyCollection<IGamePad> GamePads => gamePadList;

		public ReadOnlyCollection<ITouchPanel> TouchPanels => touchPanels;

		public int SnapshotCount => 0;

		public bool Enabled => true;

		public int UpdateOrder
		{
			get
			{
				return updateOrder;
			}
			set
			{
				if (updateOrder != value)
				{
					updateOrder = value;
					UpdateOrderChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public void Initialize()
		{
		}

		public IMouse GetMouse()
		{
			return mouse;
		}

		public IKeyboard GetKeyboard()
		{
			return keyboard;
		}

		public IKeyboard GetKeyboard(PlayerIndex playerIndex)
		{
			// Player-specific keyboards are Xbox chat pads. The GUI subscribes to
			// them as well as the main keyboard, so returning the main keyboard
			// here would deliver every key and character twice.
			return noKeyboard;
		}

		public IGamePad GetGamePad(PlayerIndex playerIndex)
		{
			return gamePads[(int)playerIndex];
		}

		public IGamePad GetGamePad(ExtendedPlayerIndex playerIndex)
		{
			return gamePads[(int)playerIndex % gamePads.Length];
		}

		public ITouchPanel GetTouchPanel()
		{
			return null;
		}

		public void Update(GameTime gameTime)
		{
			Update();
		}

		public void Update()
		{
			keyboard.Update();
			mouse.Update();
			foreach (PolledGamePad gamePad in gamePads)
			{
				gamePad.Update();
			}
		}

		public void TakeSnapshot()
		{
		}

		public void Dispose()
		{
			if (services != null && services.GetService(typeof(IInputService)) == this)
			{
				services.RemoveService(typeof(IInputService));
			}
		}
	}
}
