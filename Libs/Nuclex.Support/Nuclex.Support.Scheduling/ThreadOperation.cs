using System;
using System.Threading;

namespace Nuclex.Support.Scheduling;

/// <summary>Operation that executes a method in a background thread</summary>
public abstract class ThreadOperation : Operation
{
	/// <summary>Whether to use the ThreadPool for obtaining a background thread</summary>
	private bool useThreadPool;

	/// <summary>Exception that has occured in the background process</summary>
	private volatile Exception exception;

	/// <summary>
	///   Initializes a new threaded operation.
	/// </summary>
	/// <remarks>
	///   Uses a ThreadPool thread to execute the method in a background thread.
	/// </remarks>
	public ThreadOperation()
		: this(useThreadPool: true)
	{
	}

	/// <summary>
	///   Initializes a new threaded operation which optionally uses the ThreadPool.
	/// </summary>
	/// <param name="useThreadPool">Whether to use a ThreadPool thread.</param>
	/// <remarks>
	///   If useThreadPool is false, a new thread will be created. This guarantees
	///   that the method will be executed immediately but has an impact on
	///   performance since the creation of new threads is not a cheap operation.
	/// </remarks>
	public ThreadOperation(bool useThreadPool)
	{
		this.useThreadPool = useThreadPool;
	}

	/// <summary>Launches the background operation</summary>
	public override void Start()
	{
		if (useThreadPool)
		{
			ThreadPool.QueueUserWorkItem(callMethod);
			return;
		}
		Thread thread = new Thread((ThreadStart)callMethod);
		thread.Name = "Nuclex.Support.Scheduling.ThreadOperation";
		thread.IsBackground = true;
		thread.Start();
	}

	/// <summary>Contains the payload to be executed in the background thread</summary>
	protected abstract void Execute();

	/// <summary>Invokes the delegate passed as an argument</summary>
	/// <param name="state">Not used</param>
	private void callMethod(object state)
	{
		callMethod();
	}

	/// <summary>Invokes the delegate passed as an argument</summary>
	private void callMethod()
	{
		try
		{
			Execute();
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		finally
		{
			OnAsyncEnded();
		}
	}

	/// <summary>
	///   Allows the specific request implementation to re-throw an exception if
	///   the background process finished unsuccessfully
	/// </summary>
	protected override void ReraiseExceptions()
	{
		if (exception != null)
		{
			throw exception;
		}
	}
}
