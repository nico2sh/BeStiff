using Nuclex.Support.Tracking;

namespace Nuclex.Support.Scheduling;

/// <summary>Base class for observable operations running in the background</summary>
public abstract class Operation : Request
{
	/// <summary>Launches the background operation</summary>
	public abstract void Start();
}
