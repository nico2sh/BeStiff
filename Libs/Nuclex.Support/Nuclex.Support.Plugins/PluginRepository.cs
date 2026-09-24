#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Nuclex.Support.Plugins;

/// <summary>Stores loaded plugins</summary>
/// <remarks>
///   This class manages a set of assemblies that have been dynamically loaded 
///   as plugins. It usually is shared by multiple PluginHosts that handle
///   different interfaces of one plugin type.
/// </remarks>
public class PluginRepository
{
	/// <summary>Default assembly loader used to read assemblies from files</summary>
	public class DefaultAssemblyLoader : IAssemblyLoader
	{
		/// <summary>The only instance of the DefaultAssemblyLoader</summary>
		public static readonly DefaultAssemblyLoader Instance = new DefaultAssemblyLoader();

		/// <summary>Initializes a new default assembly loader</summary>
		/// <remarks>
		///   Made protected to provide users with a small incentive for using
		///   the Instance property instead of creating new instances all around.
		/// </remarks>
		protected DefaultAssemblyLoader()
		{
		}

		/// <summary>Loads an assembly from a file system path</summary>
		/// <param name="path">Path the assembly will be loaded from</param>
		/// <returns>The loaded assembly</returns>
		protected virtual Assembly LoadAssemblyFromFile(string path)
		{
			return Assembly.LoadFrom(path);
		}

		/// <summary>Tries to loads an assembly from a file</summary>
		/// <param name="path">Path to the file that is loaded as an assembly</param>
		/// <param name="loadedAssembly">
		///   Output parameter that receives the loaded assembly or null
		/// </param>
		/// <returns>True if the assembly was loaded successfully, otherwise false</returns>
		public bool TryLoadFile(string path, out Assembly loadedAssembly)
		{
			try
			{
				loadedAssembly = LoadAssemblyFromFile(path);
				return true;
			}
			catch (DllNotFoundException)
			{
				reportError("Assembly '" + path + "' or one of its dependencies is missing");
			}
			catch (UnauthorizedAccessException)
			{
				reportError("Not authorized to load assembly '" + path + "', possible rights problem");
			}
			catch (BadImageFormatException)
			{
				reportError("'" + path + "' is not a .NET assembly, requires a different version of the .NET Runtime or does not support the current instruction set (x86/x64)");
			}
			catch (Exception ex4)
			{
				reportError("Failed to load plugin assembly '" + path + "': " + ex4.Message);
			}
			loadedAssembly = null;
			return false;
		}
	}

	/// <summary>Loaded plugin assemblies</summary>
	private List<Assembly> assemblies;

	/// <summary>Takes care of loading assemblies for the repositories</summary>
	private IAssemblyLoader assemblyLoader;

	/// <summary>List of all loaded plugin assemblies in the repository</summary>
	public List<Assembly> LoadedAssemblies => assemblies;

	/// <summary>Triggered whenever a new assembly is loaded into this repository</summary>
	public event AssemblyLoadEventHandler AssemblyLoaded;

	/// <summary>Initializes a new instance of the plugin repository</summary>
	public PluginRepository()
		: this(DefaultAssemblyLoader.Instance)
	{
	}

	/// <summary>Initializes a new instance of the plugin repository</summary>
	/// <param name="loader">
	///   Loader to use for loading assemblies into this repository
	/// </param>
	public PluginRepository(IAssemblyLoader loader)
	{
		assemblies = new List<Assembly>();
		assemblyLoader = loader;
	}

	/// <summary>Loads all plugins matching a wildcard specification</summary>
	/// <param name="wildcard">Path of one or more plugins via wildcard</param>
	/// <remarks>
	///   This function always assumes that a plugin is optional. This means that
	///   even when you specify a unique file name and a matching file is not found,
	///   no exception will be raised and the error is silently ignored.
	/// </remarks>
	public void AddFiles(string wildcard)
	{
		string text = Path.GetDirectoryName(wildcard);
		string fileName = Path.GetFileName(wildcard);
		if (text == null || text == string.Empty)
		{
			text = ".";
		}
		string[] files = Directory.GetFiles(text, fileName);
		string[] array = files;
		foreach (string path in array)
		{
			if (assemblyLoader.TryLoadFile(path, out var loadedAssembly))
			{
				AddAssembly(loadedAssembly);
			}
		}
	}

	/// <summary>Adds the specified assembly to the repository</summary>
	/// <remarks>
	///   Also used internally, so any assembly that is to be put into the repository,
	///   not matter how, wanders through this method
	/// </remarks>
	public void AddAssembly(Assembly assembly)
	{
		assemblies.Add(assembly);
		if (this.AssemblyLoaded != null)
		{
			this.AssemblyLoaded(this, new AssemblyLoadEventArgs(assembly));
		}
	}

	/// <summary>Reports an error to the debugging console</summary>
	/// <param name="error">Error message that will be reported</param>
	private static void reportError(string error)
	{
		Trace.WriteLine(error);
	}
}
