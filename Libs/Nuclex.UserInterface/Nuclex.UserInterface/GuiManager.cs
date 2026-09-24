using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;
using Nuclex.UserInterface.Input;
using Nuclex.UserInterface.Resources;
using Nuclex.UserInterface.Visuals;
using Nuclex.UserInterface.Visuals.Flat;

namespace Nuclex.UserInterface;

/// <summary>Manages the state of the user interfaces and renders it</summary>
public class GuiManager : IGameComponent, IUpdateable, IDrawable, IDisposable, IGuiService
{
	/// <summary>Game service container the GUI has registered itself in</summary>
	private GameServiceContainer gameServices;

	/// <summary>Graphics device servide the GUI uses</summary>
	private IGraphicsDeviceService graphicsDeviceService;

	/// <summary>Input service the GUI uses</summary>
	private IInputService inputService;

	/// <summary>Update order rank relative to other game components</summary>
	private int updateOrder;

	/// <summary>Draw order rank relative to other game components</summary>
	private int drawOrder;

	/// <summary>Whether the GUI should be drawn by Game.Draw()</summary>
	private bool visible = true;

	/// <summary>Captures user input for the XNA game</summary>
	private IInputCapturer inputCapturer;

	/// <summary>
	///   The IInputCapturer under its IUpdateable interface, if implemented
	/// </summary>
	private IUpdateable updateableInputCapturer;

	/// <summary>Draws the GUI</summary>
	private IGuiVisualizer guiVisualizer;

	/// <summary>
	///   The IGuiVisualizer under its IUpdateable interface, if implemented
	/// </summary>
	private IUpdateable updateableGuiVisualizer;

	/// <summary>The GUI screen representing the desktop</summary>
	private Screen screen;

	/// <summary>GUI that is being rendered</summary>
	/// <remarks>
	///   The GUI manager renders one GUI full-screen onto the primary render target
	///   (the backbuffer). This property holds the GUI that is being managed by
	///   the GUI manager component. You can replace it at any time, for example,
	///   if the player opens or closes your ingame menu.
	/// </remarks>
	public Screen Screen
	{
		get
		{
			return screen;
		}
		set
		{
			screen = value;
			if (inputCapturer != null)
			{
				inputCapturer.InputReceiver = screen;
			}
		}
	}

	/// <summary>Input capturer that collects data from the input devices</summary>
	/// <remarks>
	///   The GuiManager will dispose its input capturer together with itself. If you
	///   want to keep the input capturer, unset it before disposing the GuiManager.
	///   If you want to replace the GuiManager's input capturer after it has constructed
	///   the default one, you should dispose the GuiManager's default input capturer
	///   after assigning your own.
	/// </remarks>
	public IInputCapturer InputCapturer
	{
		get
		{
			return inputCapturer;
		}
		set
		{
			if (!object.ReferenceEquals(value, inputCapturer))
			{
				if (inputCapturer != null)
				{
					inputCapturer.InputReceiver = null;
				}
				inputCapturer = value;
				updateableInputCapturer = (IUpdateable)((value is IUpdateable) ? value : null);
				if (inputCapturer != null)
				{
					inputCapturer.InputReceiver = screen;
				}
			}
		}
	}

	/// <summary>Visualizer that draws the GUI onto the screen</summary>
	/// <remarks>
	///   The GuiManager will dispose its visualizer together with itself. If you want
	///   to keep the visualizer, unset it before disposing the GuiManager. If you want
	///   to replace the GuiManager's visualizer after it has constructed the default
	///   one, you should dispose the GuiManager's default visualizer after assigning
	///   your own.
	/// </remarks>
	public IGuiVisualizer Visualizer
	{
		get
		{
			return guiVisualizer;
		}
		set
		{
			guiVisualizer = value;
			updateableGuiVisualizer = (IUpdateable)((value is IUpdateable) ? value : null);
		}
	}

	/// <summary>
	///   Indicates when the game component should be updated relative to other game
	///   components. Lower values are updated first.
	/// </summary>
	public int UpdateOrder
	{
		get
		{
			return updateOrder;
		}
		set
		{
			if (value != updateOrder)
			{
				updateOrder = value;
				OnUpdateOrderChanged();
			}
		}
	}

	/// <summary>
	///   The order in which to draw this object relative to other objects. Objects
	///   with a lower value are drawn first.
	/// </summary>
	public int DrawOrder
	{
		get
		{
			return drawOrder;
		}
		set
		{
			if (value != drawOrder)
			{
				drawOrder = value;
				OnDrawOrderChanged();
			}
		}
	}

	/// <summary>Whether the GUI should be drawn during Game.Draw()</summary>
	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			if (value != visible)
			{
				visible = value;
				OnVisibleChanged();
			}
		}
	}

	/// <summary>Whether the component should be updated during Game.Update()</summary>
	bool IUpdateable.Enabled => true;

	/// <summary>Fired when the DrawOrder property changes</summary>
	public event EventHandler<EventArgs> DrawOrderChanged;

	/// <summary>Fired when the Visible property changes</summary>
	public event EventHandler<EventArgs> VisibleChanged;

	/// <summary>Fired when the UpdateOrder property changes</summary>
	public event EventHandler<EventArgs> UpdateOrderChanged;

	/// <summary>Fired when the enabled property changes, which is never</summary>
	event EventHandler<EventArgs> IUpdateable.EnabledChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>
	///   Initializes a new GUI manager using the XNA service container
	/// </summary>
	/// <param name="gameServices">
	///   Game service container the GuiManager will register itself in and
	///   to take the services it consumes from.
	/// </param>
	public GuiManager(GameServiceContainer gameServices)
	{
		this.gameServices = gameServices;
		gameServices.AddService(typeof(IGuiService), (object)this);
	}

	/// <summary>
	///   Initializes a new GUI manager without using the XNA service container
	/// </summary>
	/// <param name="graphicsDeviceService">
	///   Graphics device service the GUI will be rendered with
	/// </param>
	/// <param name="inputService">
	///   Input service used to read data from the input devices
	/// </param>
	/// <remarks>
	///   This constructor is provided for users of dependency injection frameworks.
	/// </remarks>
	public GuiManager(IGraphicsDeviceService graphicsDeviceService, IInputService inputService)
	{
		this.graphicsDeviceService = graphicsDeviceService;
		this.inputService = inputService;
	}

	/// <summary>Initializes a new GUI manager using explicit services</summary>
	/// <param name="gameServices">
	///   Game service container the GuiManager will register itself in
	/// </param>
	/// <param name="graphicsDeviceService">
	///   Graphics device service the GUI will be rendered with
	/// </param>
	/// <param name="inputService">
	///   Input service used to read data from the input devices
	/// </param>
	/// <remarks>
	///   This constructor is provided for users of dependency injection frameworks
	///   or if you just want to be more explicit in stating which manager consumes
	///   what services.
	/// </remarks>
	public GuiManager(GameServiceContainer gameServices, IGraphicsDeviceService graphicsDeviceService, IInputService inputService)
		: this(gameServices)
	{
		this.graphicsDeviceService = graphicsDeviceService;
		this.inputService = inputService;
	}

	/// <summary>Immediately releases all resources used the GUI manager</summary>
	public void Dispose()
	{
		if (gameServices != null)
		{
			object service = gameServices.GetService(typeof(IGuiService));
			if (object.ReferenceEquals(service, this))
			{
				gameServices.RemoveService(typeof(IGuiService));
			}
		}
		if (inputCapturer != null)
		{
			if (inputCapturer is IDisposable disposable)
			{
				disposable.Dispose();
			}
			updateableInputCapturer = null;
			inputCapturer = null;
		}
		if (guiVisualizer != null)
		{
			if (guiVisualizer is IDisposable disposable2)
			{
				disposable2.Dispose();
			}
			updateableGuiVisualizer = null;
			guiVisualizer = null;
		}
	}

	/// <summary>Handles second-stage initialization of the GUI manager</summary>
	public void Initialize()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		if (inputCapturer == null)
		{
			if (inputService == null)
			{
				inputService = getInputService((IServiceProvider)gameServices);
			}
			inputCapturer = new DefaultInputCapturer(inputService);
			if (screen != null)
			{
				inputCapturer.InputReceiver = screen;
			}
		}
		if (guiVisualizer == null)
		{
			if (graphicsDeviceService == null)
			{
				graphicsDeviceService = getGraphicsDeviceService((IServiceProvider)gameServices);
			}
			GameServiceContainer val = new GameServiceContainer();
			val.AddService(typeof(IGraphicsDeviceService), (object)graphicsDeviceService);
			Visualizer = FlatGuiVisualizer.FromResource((IServiceProvider)val, SuaveSkinResources.ResourceManager, "SuaveSkin");
		}
	}

	/// <summary>Called when the component needs to update its state.</summary>
	/// <param name="gameTime">Provides a snapshot of the Game's timing values</param>
	public void Update(GameTime gameTime)
	{
		if (updateableInputCapturer != null)
		{
			updateableInputCapturer.Update(gameTime);
		}
		if (updateableGuiVisualizer != null)
		{
			updateableGuiVisualizer.Update(gameTime);
		}
	}

	/// <summary>Called when the drawable component needs to draw itself.</summary>
	/// <param name="gameTime">Provides a snapshot of the game's timing values</param>
	public void Draw(GameTime gameTime)
	{
		if (guiVisualizer != null && screen != null)
		{
			guiVisualizer.Draw(screen);
		}
	}

	/// <summary>Fires the UpdateOrderChanged event</summary>
	protected void OnUpdateOrderChanged()
	{
		if (this.UpdateOrderChanged != null)
		{
			this.UpdateOrderChanged(this, EventArgs.Empty);
		}
	}

	/// <summary>Fires the DrawOrderChanged event</summary>
	protected void OnDrawOrderChanged()
	{
		if (this.DrawOrderChanged != null)
		{
			this.DrawOrderChanged(this, EventArgs.Empty);
		}
	}

	/// <summary>Fires the VisibleChanged event</summary>
	protected void OnVisibleChanged()
	{
		if (this.VisibleChanged != null)
		{
			this.VisibleChanged(this, EventArgs.Empty);
		}
	}

	/// <summary>Retrieves the input service from a service provider</summary>
	/// <param name="serviceProvider">
	///   Service provider the input service is retrieved from
	/// </param>
	/// <returns>The retrieved input service</returns>
	private static IInputService getInputService(IServiceProvider serviceProvider)
	{
		IInputService inputService = (IInputService)serviceProvider.GetService(typeof(IInputService));
		if (inputService == null)
		{
			throw new InvalidOperationException("Using the GUI with the default input capturer requires the IInputService. Please either add the IInputService to Game.Services by using the Nuclex.Input.InputManager in your game or provide a custom IInputCapturer implementation for the GUI and assign it before GuiManager.Initialize() is called.");
		}
		return inputService;
	}

	/// <summary>Retrieves the graphics device service from a service provider</summary>
	/// <param name="serviceProvider">
	///   Service provider the graphics device service is retrieved from
	/// </param>
	/// <returns>The retrieved graphics device service</returns>
	private static IGraphicsDeviceService getGraphicsDeviceService(IServiceProvider serviceProvider)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		IGraphicsDeviceService val = (IGraphicsDeviceService)serviceProvider.GetService(typeof(IGraphicsDeviceService));
		if (val == null)
		{
			throw new InvalidOperationException("Using the GUI with the default visualizer requires the IGraphicsDeviceService. Please either add an IGraphicsDeviceService to Game.Services by using XNA's GraphicsDeviceManager in your game or provide a custom IGuiVisualizer implementation for the GUI and assign it before GuiManager.Initialize() is called.");
		}
		return val;
	}
}
