using System;
using System.Threading;

namespace Nuclex.Support;

/// <summary>A reverse counting semaphore</summary>
/// <remarks>
///   <para>
///     This semaphore counts in reverse, which means you can Release() the semaphore
///     as often as you'd like a thread calling WaitOne() to be let through. You
///     can use it in the traditional sense and have any Thread calling WaitOne()
///     make sure to call Release() afterwards, or you can, for example, Release() it
///     whenever work becomes available and let threads take work from the Semaphore
///     by calling WaitOne() alone.
///   </para>
///   <para>
///     Implementation notes (ignore this if you just want to use the Semaphore)
///   </para>
///   <para>
///     We could design a semaphore that uses an auto reset event, where the thread
///     that gets to pass immediately sets the event again if the semaphore isn't full
///     yet to let another thread pass.
///   </para>
///   <para>
///     However, this would mean that when a semaphore receives a large number of
///     wait requests, assuming it would allow, for example, 25 users at once, the
///     thread scheduler would see only 1 thread become eligible for execution. Then
///     that thread would unlock the next and so on. In short, we wait 25 times
///     for the thread scheduler to wake up a thread until all users get through.
///   </para>
///   <para>
///     So we chose a ManualResetEvent, which will wake up more threads than
///     neccessary and possibly cause a period of intense competition for getting
///     a lock on the resource, but will make the thread scheduler see all threads
///     become eligible for execution.
///   </para>
/// </remarks>
public class Semaphore : WaitHandle
{
	/// <summary>Event used to make threads wait if the semaphore is full</summary>
	private ManualResetEvent manualResetEvent;

	/// <summary>Number of users currently accessing the resource</summary>
	/// <remarks>
	///   Since this is a reverse counting semaphore, it will be negative if
	///   the resource is available and 0 if the semaphore is full.
	/// </remarks>
	private int free;

	/// <summary>Initializes a new semaphore</summary>
	public Semaphore()
	{
		createEvent();
	}

	/// <summary>Initializes a new semaphore</summary>
	/// <param name="count">
	///   Number of users that can access the resource at the same time
	/// </param>
	public Semaphore(int count)
	{
		free = count;
		createEvent();
	}

	/// <summary>Initializes a new semaphore</summary>
	/// <param name="initialCount">
	///   Initial number of users accessing the resource 
	/// </param>
	/// <param name="maximumCount">
	///   Maximum numbr of users that can access the resource at the same time
	/// </param>
	public Semaphore(int initialCount, int maximumCount)
	{
		if (initialCount > maximumCount)
		{
			throw new ArgumentOutOfRangeException("initialCount", "Initial count must not be larger than the maximum count");
		}
		free = maximumCount - initialCount;
		createEvent();
	}

	/// <summary>Immediately releases all resources owned by the instance</summary>
	/// <param name="explicitDisposing">
	///   Whether Dispose() has been called explictly
	/// </param>
	protected override void Dispose(bool explicitDisposing)
	{
		if (manualResetEvent != null)
		{
			base.SafeWaitHandle = null;
			manualResetEvent.Close();
			manualResetEvent = null;
		}
		base.Dispose(explicitDisposing);
	}

	/// <summary>
	///   Waits for the resource to become available and locks it
	/// </summary>
	/// <param name="millisecondsTimeout">
	///   Number of milliseconds to wait at most before giving up
	/// </param>
	/// <param name="exitContext">
	///   True to exit the synchronization domain for the context before the wait (if
	///   in a synchronized context), and reacquire it afterward; otherwise, false.
	/// </param>
	/// <returns>
	///   True if the resource was available and is now locked, false if
	///   the timeout has been reached.
	/// </returns>
	public override bool WaitOne(int millisecondsTimeout, bool exitContext)
	{
		int num;
		do
		{
			num = Interlocked.Decrement(ref free);
			if (num >= 0)
			{
				if (num > 0)
				{
					manualResetEvent.Set();
				}
				return true;
			}
			manualResetEvent.Reset();
			Thread.MemoryBarrier();
			num = Interlocked.Increment(ref free);
		}
		while (num < 0 || manualResetEvent.WaitOne(millisecondsTimeout, exitContext));
		return false;
	}

	/// <summary>
	///   Waits for the resource to become available and locks it
	/// </summary>
	/// <returns>
	///   True if the resource was available and is now locked, false if
	///   the timeout has been reached.
	/// </returns>
	public override bool WaitOne()
	{
		return WaitOne(-1, exitContext: false);
	}

	/// <summary>
	///   Waits for the resource to become available and locks it
	/// </summary>
	/// <param name="timeout">
	///   Time span to wait for the lock before giving up
	/// </param>
	/// <param name="exitContext">
	///   True to exit the synchronization domain for the context before the wait (if
	///   in a synchronized context), and reacquire it afterward; otherwise, false.
	/// </param>
	/// <returns>
	///   True if the resource was available and is now locked, false if
	///   the timeout has been reached.
	/// </returns>
	public override bool WaitOne(TimeSpan timeout, bool exitContext)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", "Timeout must be either -1 or positive and less than 2^31");
		}
		return WaitOne((int)num, exitContext);
	}

	/// <summary>
	///   Releases a lock on the resource. Note that for a reverse counting semaphore,
	///   it is legal to Release() the resource before locking it.
	/// </summary>
	public void Release()
	{
		Interlocked.Increment(ref free);
		manualResetEvent.Set();
	}

	/// <summary>Creates the event used to make threads wait for the resource</summary>
	private void createEvent()
	{
		manualResetEvent = new ManualResetEvent(initialState: false);
		base.SafeWaitHandle = manualResetEvent.SafeWaitHandle;
	}
}
