#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using Microsoft.Xna.Framework.Content;
using Nuclex.Support.Plugins;
using Nuclex.UserInterface.Controls;

namespace Nuclex.UserInterface.Visuals.Flat;

/// <summary>Draws traditional flat GUIs using 2D bitmaps</summary>
public class FlatGuiVisualizer : IGuiVisualizer, IDisposable
{
	/// <summary>Container for a control and its absolute boundaries</summary>
	private struct ControlWithBounds
	{
		/// <summary>Control stored in the container</summary>
		public Control Control;

		/// <summary>Absolute boundaries of the stored control</summary>
		public RectangleF Bounds;

		/// <summary>Initializes a new control and absolute boundary container</summary>
		/// <param name="control">Control being store in the container</param>
		/// <param name="bounds">Absolute boundaries the control lives in</param>
		public ControlWithBounds(Control control, RectangleF bounds)
		{
			Control = control;
			Bounds = bounds;
		}

		/// <summary>
		///   Builds an absolute boundary container from the provided control
		/// </summary>
		/// <param name="control">Control from which a container will be created</param>
		/// <param name="containerBounds">
		///   Absolute boundaries of the control's parent
		/// </param>
		/// <returns>A new container with the control</returns>
		public static ControlWithBounds FromControl(Control control, RectangleF containerBounds)
		{
			containerBounds.X += control.Bounds.Location.X.Fraction * containerBounds.Width;
			containerBounds.X += control.Bounds.Location.X.Offset;
			containerBounds.Y += control.Bounds.Location.Y.Fraction * containerBounds.Height;
			containerBounds.Y += control.Bounds.Location.Y.Offset;
			containerBounds.Width = control.Bounds.Size.X.ToOffset(containerBounds.Width);
			containerBounds.Height = control.Bounds.Size.Y.ToOffset(containerBounds.Height);
			return new ControlWithBounds(control, containerBounds);
		}

		/// <summary>
		///   Builds a control and absolute boundary container from a screen
		/// </summary>
		/// <param name="screen">
		///   Screen whose desktop control and absolute boundaries are used to
		///   construct the container
		/// </param>
		/// <returns>A new container with the screen's desktop control</returns>
		public static ControlWithBounds FromScreen(Screen screen)
		{
			return new ControlWithBounds(screen.Desktop, screen.Desktop.Bounds.ToOffset(screen.Width, screen.Height));
		}
	}

	/// <summary>Interface for a generic (typeless) control renderer</summary>
	internal interface IControlRendererAdapter
	{
		/// <summary>The type of the control renderer being adapted</summary>
		Type AdaptedType { get; }

		/// <summary>
		///   Renders the specified control using the provided graphics interface
		/// </summary>
		/// <param name="controlToRender">Control that will be rendered</param>
		/// <param name="graphics">
		///   Graphics interface that will be used to render the control
		/// </param>
		void Render(Control controlToRender, IFlatGuiGraphics graphics);
	}

	/// <summary>
	///   Adapter that automatically casts a control down to the renderer's supported
	///   control type
	/// </summary>
	/// <typeparam name="ControlType">
	///   Type of control the control renderer casts down to
	/// </typeparam>
	/// <remarks>
	///   This is simply an optimization to avoid invoking the control renderer
	///   by reflection (using the Invoke() method) which would require us to construct
	///   an object[] array on the heap to pass its arguments.
	/// </remarks>
	private class ControlRendererAdapter<ControlType> : IControlRendererAdapter where ControlType : Control
	{
		/// <summary>Control renderer this adapter is performing the downcast for</summary>
		private IFlatControlRenderer<ControlType> controlRenderer;

		/// <summary>The type of the control renderer being adapted</summary>
		public Type AdaptedType => controlRenderer.GetType();

		/// <summary>Initializes a new control renderer adapter</summary>
		/// <param name="controlRenderer">Control renderer the adapter is used for</param>
		public ControlRendererAdapter(IFlatControlRenderer<ControlType> controlRenderer)
		{
			this.controlRenderer = controlRenderer;
		}

		/// <summary>
		///   Renders the specified control using the provided graphics interface
		/// </summary>
		/// <param name="controlToRender">Control that will be rendered</param>
		/// <param name="graphics">
		///   Graphics interface that will be used to render the control
		/// </param>
		public void Render(Control controlToRender, IFlatGuiGraphics graphics)
		{
			controlRenderer.Render((ControlType)controlToRender, graphics);
		}
	}

	/// <summary>
	///   Employs concrete types implementing IFlatGuiControlRenderer&lt;&gt;
	/// </summary>
	/// <remarks>
	///   This employer actually looks for concrete implementations using a variant
	///   of the IFlatGuiControlRenderer&lt;&gt; interface, regardless of the
	///   type it has been realized for.
	/// </remarks>
	internal class ControlRendererEmployer : Employer
	{
		/// <summary>Employed renderers</summary>
		private Dictionary<Type, IControlRendererAdapter> renderers;

		/// <summary>Renderers that were employed to the plugin host</summary>
		public Dictionary<Type, IControlRendererAdapter> Renderers => renderers;

		/// <summary>Initializes a new control renderer employer</summary>
		public ControlRendererEmployer()
		{
			renderers = new Dictionary<Type, IControlRendererAdapter>();
		}

		/// <summary>Determines whether the type suites the employer's requirements</summary>
		/// <param name="type">Type that is checked for employability</param>
		/// <returns>True if the type can be employed</returns>
		public override bool CanEmploy(Type type)
		{
			if (!typeof(IFlatControlRenderer).IsAssignableFrom(type))
			{
				return false;
			}
			if (!PluginHelper.HasDefaultConstructor(type))
			{
				return false;
			}
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (interfaces[i].IsGenericType)
				{
					Type genericTypeDefinition = interfaces[i].GetGenericTypeDefinition();
					if (genericTypeDefinition == typeof(IFlatControlRenderer<>))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>Employs the specified plugin type</summary>
		/// <param name="type">Type to be employed</param>
		public override void Employ(Type type)
		{
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (!interfaces[i].IsGenericType)
				{
					continue;
				}
				Type genericTypeDefinition = interfaces[i].GetGenericTypeDefinition();
				if (genericTypeDefinition == typeof(IFlatControlRenderer<>))
				{
					Type[] genericArguments = interfaces[i].GetGenericArguments();
					if (renderers.ContainsKey(genericArguments[0]))
					{
						string message = $"Warning: Control type '{genericArguments[0].FullName.ToString()}' already using renderer '{renderers[genericArguments[0]].AdaptedType.FullName.ToString()}'.\n         Second renderer '{type.FullName.ToString()}' will be ignored!";
						Trace.WriteLine(message);
						continue;
					}
					Type type2 = typeof(ControlRendererAdapter<>).MakeGenericType(genericArguments[0]);
					ConstructorInfo constructor = type2.GetConstructor(new Type[1] { interfaces[i] });
					object obj = constructor.Invoke(new object[1] { Activator.CreateInstance(type) });
					renderers.Add(genericArguments[0], (IControlRendererAdapter)obj);
				}
			}
		}
	}

	/// <summary>Holds the assemblies we have employed for our cause</summary>
	private PluginHost pluginHost;

	/// <summary>Carries the employed control renderers</summary>
	private ControlRendererEmployer employer;

	/// <summary>Used to draw the individual building elements of the GUI</summary>
	private FlatGuiGraphics flatGuiGraphics;

	/// <summary>Helps draw the GUI controls in the hierarchically correct order</summary>
	/// <remarks>
	///   This is a field and not a local variable because the stack allocates
	///   heap memory and we don't want that to happen in a frame-by-frame basis on
	///   the compact framework. By reusing the same stack over and over, the amount
	///   of heap allocations required will amortize itself.
	/// </remarks>
	private Stack<ControlWithBounds> controlStack;

	/// <summary>
	///   Plugin repository from which renderers for GUI controls are taken
	/// </summary>
	public PluginRepository RendererRepository => pluginHost.Repository;

	/// <summary>Returns the assembly containing the GUI visualizer</summary>
	private static Assembly Self => typeof(FlatGuiVisualizer).Assembly;

	/// <summary>Initializes a new gui visualizer from a skin stored in a file</summary>
	/// <param name="serviceProvider">
	///   Game service provider containing the graphics device service
	/// </param>
	/// <param name="skinPath">
	///   Path to the skin description this GUI visualizer will load
	/// </param>
	public static FlatGuiVisualizer FromFile(IServiceProvider serviceProvider, string skinPath)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		using FileStream skinStream = new FileStream(skinPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		ContentManager val = new ContentManager(serviceProvider, Path.GetDirectoryName(skinPath));
		try
		{
			return new FlatGuiVisualizer(val, skinStream);
		}
		catch (Exception)
		{
			val.Dispose();
			throw;
		}
	}

	/// <summary>Initializes a new gui visualizer from a skin stored as a resource</summary>
	/// <param name="serviceProvider">
	///   Game service provider containing the graphics device service
	/// </param>
	/// <param name="resourceManager">
	///   Resource manager containing the resources used in the skin
	/// </param>
	/// <param name="skinResource">
	///   Name of the resource containing the skin description
	/// </param>
	public static FlatGuiVisualizer FromResource(IServiceProvider serviceProvider, ResourceManager resourceManager, string skinResource)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		byte[] array = (byte[])resourceManager.GetObject(skinResource);
		if (array == null)
		{
			throw new ArgumentException("Resource '" + skinResource + "' not found", "skinResource");
		}
		using MemoryStream skinStream = new MemoryStream(array, writable: false);
		ResourceContentManager val = new ResourceContentManager(serviceProvider, resourceManager);
		try
		{
			return new FlatGuiVisualizer((ContentManager)(object)val, skinStream);
		}
		catch (Exception)
		{
			((ContentManager)val).Dispose();
			throw;
		}
	}

	/// <summary>Initializes a new gui painter for traditional GUIs</summary>
	/// <param name="contentManager">
	///   Content manager that will be used to load the skin resources
	/// </param>
	/// <param name="skinStream">
	///   Stream from which the GUI Visualizer will read the skin description
	/// </param>
	protected FlatGuiVisualizer(ContentManager contentManager, Stream skinStream)
	{
		employer = new ControlRendererEmployer();
		pluginHost = new PluginHost(employer);
		pluginHost.Repository.AddAssembly(Self);
		flatGuiGraphics = new FlatGuiGraphics(contentManager, skinStream);
		controlStack = new Stack<ControlWithBounds>();
	}

	/// <summary>Immediately releases all resources owned by the instance</summary>
	public void Dispose()
	{
		if (flatGuiGraphics != null)
		{
			flatGuiGraphics.Dispose();
			flatGuiGraphics = null;
		}
	}

	/// <summary>Draws an entire GUI hierarchy</summary>
	/// <param name="screen">Screen containing the GUI that will be drawn</param>
	public void Draw(Screen screen)
	{
		flatGuiGraphics.BeginDrawing();
		try
		{
			controlStack.Push(ControlWithBounds.FromScreen(screen));
			while (controlStack.Count > 0)
			{
				ControlWithBounds controlWithBounds = controlStack.Pop();
				Control control = controlWithBounds.Control;
				RectangleF bounds = controlWithBounds.Bounds;
				for (int i = 0; i < control.Children.Count; i++)
				{
					controlStack.Push(ControlWithBounds.FromControl(control.Children[i], bounds));
				}
				renderControl(control);
			}
		}
		finally
		{
			flatGuiGraphics.EndDrawing();
		}
	}

	/// <summary>Renders a single control</summary>
	/// <param name="controlToRender">Control that will be rendered</param>
	private void renderControl(Control controlToRender)
	{
		IControlRendererAdapter value = null;
		Type type = controlToRender.GetType();
		if (!(type == typeof(Control)) && !(type == typeof(DesktopControl)))
		{
			while (type != typeof(object) && !employer.Renderers.TryGetValue(type, out value))
			{
				type = type.BaseType;
			}
			if (value != null)
			{
				value.Render(controlToRender, flatGuiGraphics);
			}
			else
			{
				Trace.WriteLine($"Warning: No renderer found for control '{controlToRender.GetType().FullName.ToString()}' or any of its base classes.\n         Control will not be rendered.");
			}
		}
	}
}
