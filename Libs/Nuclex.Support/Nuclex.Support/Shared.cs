using System.Diagnostics;

namespace Nuclex.Support;

/// <summary>Manages a globally shared instance of the given Type</summary>
/// <typeparam name="SharedType">
///   Type of which a globally shared instance will be provided
/// </typeparam>
public static class Shared<SharedType> where SharedType : new()
{
	/// <summary>Stored the globally shared instance</summary>
	private static readonly SharedType instance = new SharedType();

	/// <summary>Returns the global instance of the class</summary>
	public static SharedType Instance
	{
		[DebuggerStepThrough]
		get
		{
			return instance;
		}
	}
}
