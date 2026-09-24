using System;
using System.Collections.Generic;

namespace Nuclex.Support.Plugins;

/// <summary>Employer that directly creates instances of the types in a plugin</summary>
/// <typeparam name="T">Interface or base class required for the employed types</typeparam>
/// <remarks>
///   <para>
///     This employer directly creates an instance of any type in a plugin assembly that
///     implements or is derived from the type the generic InstanceEmployer is instanced
///     to. This is useful when the plugin user already has a special plugin interface
///     through which additional informations about a plugin type can be queried or
///     when actually exactly one instance per plugin type is wanted (think of the
///     prototype pattern for example)
///   </para>
///   <para>
///     Because this employer blindly creates an instance of any compatible type found
///     in a plugin assembly it should be used with care. If big types with high
///     construction time or huge memory requirements are loaded this can become
///     a real resource hog. The intention of this employer was to let the plugin user
///     define his own factory interface which possibly provides further details about
///     the type the factory is reponsible for (like a description field). This
///     factory would then be implemented on the plugin side.
///   </para>
/// </remarks>
public class InstanceEmployer<T> : Employer
{
	/// <summary>All instances employed by the instance employer</summary>
	private List<T> employedInstances;

	/// <summary>All instances that have been employed</summary>
	public List<T> Instances => employedInstances;

	/// <summary>Initializes a new instance employer</summary>
	public InstanceEmployer()
	{
		employedInstances = new List<T>();
	}

	/// <summary>Determines whether the type suites the employer's requirements</summary>
	/// <param name="type">Type that is checked for employability</param>
	/// <returns>True if the type can be employed</returns>
	public override bool CanEmploy(Type type)
	{
		if (PluginHelper.HasDefaultConstructor(type) && typeof(T).IsAssignableFrom(type))
		{
			return !type.ContainsGenericParameters;
		}
		return false;
	}

	/// <summary>Employs the specified plugin type</summary>
	/// <param name="type">Type to be employed</param>
	public override void Employ(Type type)
	{
		employedInstances.Add((T)Activator.CreateInstance(type));
	}
}
