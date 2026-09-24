using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nuclex.Support;

/// <summary>Alternative Thread pool providing one thread for each core</summary>
/// <remarks>
///   <para>
///     Unlike the normal thread pool, the affine thread pool provides only as many
///     threads as there are CPU cores available on the current platform. This makes
///     it more suitable for tasks you want to spread across all available cpu cores
///     explicitly.
///   </para>
///   <para>
///     However, it's not a good match if you want to run blocking or waiting tasks
///     inside the thread pool because the limited available threads will become
///     congested quickly. It is encouraged to use this class in parallel with
///     .NET's own thread pool, putting tasks that can block into the .NET thread
///     pool and task that perform pure processing into the affine thread pool.
///   </para>
///   <para>
///     Implementation based on original code provided by Stephen Toub
///     (stoub at microsoft ignorethis dot com)
///   </para>
/// </remarks>
public static class AffineThreadPool
{
	/// <summary>Delegate used by the thread pool to report unhandled exceptions</summary>
	/// <param name="exception">Exception that has not been handled</param>
	public delegate void ExceptionDelegate(Exception exception);

	/// <summary>Used to hold a callback delegate and the state for that delegate.</summary>
	private struct UserWorkItem
	{
		/// <summary>Callback delegate for the callback.</summary>
		public WaitCallback Callback;

		/// <summary>State with which to call the callback delegate.</summary>
		public object State;

		/// <summary>Initialize the callback holding object.</summary>
		/// <param name="callback">Callback delegate for the callback.</param>
		/// <param name="state">State with which to call the callback delegate.</param>
		public UserWorkItem(WaitCallback callback, object state)
		{
			Callback = callback;
			State = state;
		}
	}

	/// <summary>Number of CPU cores available on the system</summary>
	public static readonly int Processors;

	/// <summary>Delegate used to handle assertion checks in the code</summary>
	public static volatile ExceptionDelegate ExceptionHandler;

	/// <summary>Available hardware threads the thread pool threads pick from</summary>
	private static Queue<int> hardwareThreads;

	/// <summary>Queue of all the callbacks waiting to be executed.</summary>
	private static Queue<UserWorkItem> userWorkItems;

	/// <summary>
	///   Used to let the threads in the thread pool wait for new work to appear.
	/// </summary>
	private static Semaphore workAvailable;

	/// <summary>List of all worker threads at the disposal of the thread pool.</summary>
	private static List<Thread> workerThreads;

	/// <summary>Number of threads currently active.</summary>
	private static int inUseThreads;

	/// <summary>Gets the number of threads at the disposal of the thread pool</summary>
	public static int MaxThreads => Processors;

	/// <summary>Gets the number of currently active threads in the thread pool</summary>
	public static int ActiveThreads => inUseThreads;

	/// <summary>
	///   Gets the number of callback delegates currently waiting in the thread pool
	/// </summary>
	public static int WaitingWorkItems
	{
		get
		{
			lock (userWorkItems)
			{
				return userWorkItems.Count;
			}
		}
	}

	/// <summary>Initializes the thread pool</summary>
	static AffineThreadPool()
	{
		Processors = Environment.ProcessorCount;
		ExceptionHandler = DefaultExceptionHandler;
		workAvailable = new Semaphore();
		userWorkItems = new Queue<UserWorkItem>(Processors * 4);
		workerThreads = new List<Thread>(Processors);
		inUseThreads = 0;
		hardwareThreads = new Queue<int>(Processors);
		for (int num = Processors; num >= 1; num--)
		{
			hardwareThreads.Enqueue(num);
		}
		for (int i = 0; i < Processors; i++)
		{
			Thread thread = new Thread(ProcessQueuedItems);
			workerThreads.Add(thread);
			thread.Name = "Nuclex.Support.AffineThreadPool Thread #" + i;
			thread.IsBackground = true;
			thread.Start();
		}
	}

	/// <summary>Queues a user work item to the thread pool</summary>
	/// <param name="callback">
	///   A WaitCallback representing the delegate to invoke when a thread in the 
	///   thread pool picks up the work item
	/// </param>
	public static void QueueUserWorkItem(WaitCallback callback)
	{
		QueueUserWorkItem(callback, null);
	}

	/// <summary>Queues a user work item to the thread pool.</summary>
	/// <param name="callback">
	///   A WaitCallback representing the delegate to invoke when a thread in the 
	///   thread pool picks up the work item
	/// </param>
	/// <param name="state">
	///   The object that is passed to the delegate when serviced from the thread pool
	/// </param>
	public static void QueueUserWorkItem(WaitCallback callback, object state)
	{
		UserWorkItem item = new UserWorkItem(callback, state);
		lock (userWorkItems)
		{
			userWorkItems.Enqueue(item);
		}
		workAvailable.Release();
	}

	/// <summary>
	///   Default handler used to respond to unhandled exceptions in ThreadPool threads
	/// </summary>
	/// <param name="exception">Exception that has occurred</param>
	internal static void DefaultExceptionHandler(Exception exception)
	{
		throw exception;
	}

	/// <summary>Retrieves the ProcessThread for the calling thread</summary>
	/// <returns>The ProcessThread for the calling thread</returns>
	internal static ProcessThread GetProcessThread(int threadId)
	{
		ProcessThreadCollection threads = Process.GetCurrentProcess().Threads;
		for (int i = 0; i < threads.Count; i++)
		{
			if (threads[i].Id == threadId)
			{
				return threads[i];
			}
		}
		return null;
	}

	/// <summary>A thread worker function that processes items from the work queue</summary>
	private static void ProcessQueuedItems()
	{
		int idealProcessor;
		lock (hardwareThreads)
		{
			idealProcessor = hardwareThreads.Dequeue();
		}
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			Thread.BeginThreadAffinity();
			int currentThreadId = GetCurrentThreadId();
			ProcessThread processThread = GetProcessThread(currentThreadId);
			if (processThread != null)
			{
				processThread.IdealProcessor = idealProcessor;
			}
		}
		while (true)
		{
			UserWorkItem nextWorkItem = getNextWorkItem();
			Interlocked.Increment(ref inUseThreads);
			try
			{
				nextWorkItem.Callback(nextWorkItem.State);
			}
			catch (Exception exception)
			{
				ExceptionHandler?.Invoke(exception);
			}
			finally
			{
				Interlocked.Decrement(ref inUseThreads);
			}
		}
	}

	/// <summary>Obtains the next work item from the queue</summary>
	/// <returns>The next work item in the queue</returns>
	/// <remarks>
	///   If the queue is empty, the call will block until an item is added to
	///   the queue and the calling thread was the one picking it up.
	/// </remarks>
	private static UserWorkItem getNextWorkItem()
	{
		while (true)
		{
			lock (userWorkItems)
			{
				if (userWorkItems.Count > 0)
				{
					return userWorkItems.Dequeue();
				}
			}
			workAvailable.WaitOne();
		}
	}

	/// <summary>Retrieves the calling thread's thread id</summary>
	/// <returns>The thread is of the calling thread</returns>
	[DllImport("kernel32.dll")]
	internal static extern int GetCurrentThreadId();
}
