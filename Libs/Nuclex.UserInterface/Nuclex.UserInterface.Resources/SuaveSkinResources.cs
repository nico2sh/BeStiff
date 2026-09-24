using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Nuclex.UserInterface.Resources;

/// <summary>
///   A strongly-typed resource class, for looking up localized strings, etc.
/// </summary>
[DebuggerNonUserCode]
[CompilerGenerated]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
internal class SuaveSkinResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	/// <summary>
	///   Returns the cached ResourceManager instance used by this class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = new ResourceManager("Nuclex.UserInterface.Resources.SuaveSkinResources", typeof(SuaveSkinResources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	/// <summary>
	///   Overrides the current thread's CurrentUICulture property for all
	///   resource lookups using this strongly typed resource class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static byte[] DefaultFont
	{
		get
		{
			object obj = ResourceManager.GetObject("DefaultFont", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static byte[] SuaveSheet
	{
		get
		{
			object obj = ResourceManager.GetObject("SuaveSheet", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static byte[] SuaveSkin
	{
		get
		{
			object obj = ResourceManager.GetObject("SuaveSkin", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static byte[] TitleFont
	{
		get
		{
			object obj = ResourceManager.GetObject("TitleFont", resourceCulture);
			return (byte[])obj;
		}
	}

	internal SuaveSkinResources()
	{
	}
}
