#define TRACE
using System;
using System.Diagnostics;
using System.Reflection;

namespace Nuclex.Support.Plugins;

/// <summary>Integration host for plugins</summary>
/// <remarks>
///   This class is created by the party that is interested in loading plugins,
///   herein referred to as the "plugin user". The plugin host will monitor a
///   repository and react to any assembly being loaded into that repository by
///   iterating over all types (as in classes and structures) found in the
///   assembly and using the employer to do whatever the plugin user intends 
///   to do with the types found in that assembly
/// </remarks>
public class PluginHost
{
	/// <summary>Employs and manages types in the loaded plugin assemblies</summary>
	private Employer employer;

	/// <summary>Repository containing all plugins loaded, shared with other hosts</summary>
	private PluginRepository repository;

	/// <summary>The repository containing all loaded plugins</summary>
	public PluginRepository Repository => repository;

	/// <summary>The employer that is used by this plugin integration host</summary>
	public Employer Employer => employer;

	/// <summary>Initializes a plugin host using a new repository</summary>
	/// <param name="employer">Employer used assess and employ the plugin types</param>
	public PluginHost(Employer employer)
		: this(employer, new PluginRepository())
	{
	}

	/// <summary>Initializes the plugin using an existing repository</summary>
	/// <param name="employer">Employer used assess and employ the plugin types</param>
	/// <param name="repository">Repository in which plugins will be stored</param>
	public PluginHost(Employer employer, PluginRepository repository)
	{
		this.employer = employer;
		this.repository = repository;
		foreach (Assembly loadedAssembly in this.repository.LoadedAssemblies)
		{
			employAssemblyTypes(loadedAssembly);
		}
		this.repository.AssemblyLoaded += assemblyLoadHandler;
	}

	/// <summary>Responds to a new plugin being loaded into the repository</summary>
	/// <param name="sender">Repository into which the assembly was loaded</param>
	/// <param name="arguments">Event arguments; contains the loaded assembly</param>
	private void assemblyLoadHandler(object sender, AssemblyLoadEventArgs arguments)
	{
		employAssemblyTypes(arguments.LoadedAssembly);
	}

	/// <summary>Employs all employable types in an assembly</summary>
	/// <param name="assembly">Assembly whose types to assess and to employ</param>
	private void employAssemblyTypes(Assembly assembly)
	{
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsPublic || type.IsAbstract)
			{
				continue;
			}
			object[] customAttributes = type.GetCustomAttributes(inherit: true);
			if (containsNoPluginAttribute(customAttributes))
			{
				continue;
			}
			try
			{
				if (employer.CanEmploy(type))
				{
					employer.Employ(type);
				}
			}
			catch (Exception ex)
			{
				reportError("Could not employ " + type.ToString() + ": " + ex.Message);
			}
		}
	}

	/// <summary>
	///   Determines whether the specifies list of attributes contains a NoPluginAttribute
	/// </summary>
	/// <param name="attributes">List of attributes to check</param>
	/// <returns>True if the list contained a NoPluginAttribute, false otherwise</returns>
	private static bool containsNoPluginAttribute(object[] attributes)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i] is NoPluginAttribute)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Reports an error to the debugging console</summary>
	/// <param name="error">Error message that will be reported</param>
	private static void reportError(string error)
	{
		Trace.WriteLine(error);
	}
}
