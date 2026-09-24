using System.Threading;

namespace Nuclex.Support.Scheduling;

/// <summary>Operation that executes a method in a background thread</summary>
public class ThreadCallbackOperation : ThreadOperation
{
	/// <summary>Method to be invoked in a background thread</summary>
	private ThreadStart method;

	/// <summary>
	///   Initializes a new threaded method operation that will call back a
	///   parameterless method from the background thread.
	/// </summary>
	/// <param name="method">Method to be invoked in a background thread</param>
	/// <remarks>
	///   Uses a ThreadPool thread to execute the method in
	/// </remarks>
	public ThreadCallbackOperation(ThreadStart method)
		: this(method, useThreadPool: true)
	{
	}

	/// <summary>
	///   Initializes a new threaded method operation that will call back a
	///   parameterless method from the background thread and use the
	///   thread pool optionally.
	/// </summary>
	/// <param name="method">Method to be invoked in a background thread</param>
	/// <param name="useThreadPool">Whether to use a ThreadPool thread</param>
	/// <remarks>
	///   If useThreadPool is false, a new thread will be created. This guarantees
	///   that the method will be executed immediately but has an impact on
	///   performance since the creation of new threads is not a cheap operation.
	/// </remarks>
	public ThreadCallbackOperation(ThreadStart method, bool useThreadPool)
		: base(useThreadPool)
	{
		this.method = method;
	}

	/// <summary>Executes the thread callback in the background thread</summary>
	protected override void Execute()
	{
		method();
	}
}
