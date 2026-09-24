using System;
using System.Collections.Generic;
using System.Threading;

namespace Nuclex.Support.Tracking;

/// <summary>Base class for background processes the user can wait on</summary>
/// <remarks>
///   <para>
///     By encapsulating long-running operations which will ideally be running in
///     a background thread in a class that's derived from <see cref="T:Nuclex.Support.Tracking.Transaction" />
///     you can wait for the completion of the operation and optionally even receive
///     feedback on the achieved progress. This is useful for displaying a progress
///     bar, loading screen or some other means of entertaining the user while he
///     waits for the task to complete.
///   </para>
///   <para>
///     You can register callbacks which will be fired once the <see cref="T:Nuclex.Support.Tracking.Transaction" />
///     task has completed. This class deliberately does not provide an Execute()
///     method or anything similar to clearly seperate the initiation of an operation
///     from just monitoring it. By omitting an Execute() method, it also becomes
///     possible to construct a transaction just-in-time when it is explicitely being
///     asked for.
///   </para>
/// </remarks>
public abstract class Transaction
{
	/// <summary>Dummy transaction which always is in the 'ended' state</summary>
	private class EndedDummyTransaction : Transaction
	{
		/// <summary>Initializes a new ended dummy transaction</summary>
		public EndedDummyTransaction()
		{
			OnAsyncEnded();
		}
	}

	/// <summary>A dummy transaction that's always in the 'ended' state</summary>
	/// <remarks>
	///   Useful if an operation is already complete when it's being asked for or
	///   when a transaction that's lazily created is accessed after the original
	///   operation has ended already.
	/// </remarks>
	public static readonly Transaction EndedDummy = new EndedDummyTransaction();

	/// <summary>Event handlers which have subscribed to the ended event</summary>
	/// <remarks>
	///   Does not need to be volatile since it's only accessed inside 
	/// </remarks>
	protected volatile List<EventHandler> endedEventSubscribers;

	/// <summary>Whether the operation has completed yet</summary>
	protected volatile bool ended;

	/// <summary>Event that will be set when the transaction is completed</summary>
	/// <remarks>
	///   This event is will only be created when it is specifically asked for using
	///   the WaitHandle property.
	/// </remarks>
	protected volatile ManualResetEvent doneEvent;

	/// <summary>Whether the transaction has ended already</summary>
	public virtual bool Ended => ended;

	/// <summary>WaitHandle that can be used to wait for the transaction to end</summary>
	public virtual WaitHandle WaitHandle
	{
		get
		{
			if (doneEvent == null)
			{
				lock (this)
				{
					if (doneEvent == null)
					{
						doneEvent = new ManualResetEvent(ended);
					}
				}
			}
			return doneEvent;
		}
	}

	/// <summary>Will be triggered when the transaction has ended</summary>
	/// <remarks>
	///   If the process is already finished when a client registers to this event,
	///   the registered callback will be invoked synchronously right when the
	///   registration takes place.
	/// </remarks>
	public virtual event EventHandler AsyncEnded
	{
		add
		{
			if (!ended)
			{
				lock (this)
				{
					if (!ended)
					{
						if (object.ReferenceEquals(endedEventSubscribers, null))
						{
							endedEventSubscribers = new List<EventHandler>();
						}
						endedEventSubscribers.Add(value);
						return;
					}
				}
			}
			value(this, EventArgs.Empty);
		}
		remove
		{
			if (ended)
			{
				return;
			}
			lock (this)
			{
				if (!ended && !object.ReferenceEquals(endedEventSubscribers, null))
				{
					int num = endedEventSubscribers.IndexOf(value);
					if (num != -1)
					{
						endedEventSubscribers.RemoveAt(num);
					}
				}
			}
		}
	}

	/// <summary>Waits until the background process finishes</summary>
	public virtual void Wait()
	{
		if (!ended)
		{
			WaitHandle.WaitOne();
		}
	}

	/// <summary>Waits until the background process finishes or a timeout occurs</summary>
	/// <param name="timeout">
	///   Time span after which to stop waiting and return immediately
	/// </param>
	/// <returns>
	///   True if the background process completed, false if the timeout was reached
	/// </returns>
	public virtual bool Wait(TimeSpan timeout)
	{
		if (ended)
		{
			return true;
		}
		return WaitHandle.WaitOne(timeout, exitContext: false);
	}

	/// <summary>Waits until the background process finishes or a timeout occurs</summary>
	/// <param name="timeoutMilliseconds">
	///   Number of milliseconds after which to stop waiting and return immediately
	/// </param>
	/// <returns>
	///   True if the background process completed, false if the timeout was reached
	/// </returns>
	public virtual bool Wait(int timeoutMilliseconds)
	{
		if (ended)
		{
			return true;
		}
		return WaitHandle.WaitOne(timeoutMilliseconds, exitContext: false);
	}

	/// <summary>Fires the AsyncEnded event</summary>
	/// <remarks>
	///   <para>
	///     This event should be fired by the implementing class when its work is completed.
	///     It's of no interest to this class whether the outcome of the process was
	///     successfull or not, the outcome and results of the process taking place both
	///     need to be communicated seperately.
	///   </para>
	///   <para>
	///     Calling this method is mandatory. Implementers need to take care that
	///     the OnAsyncEnded() method is called on any instance of transaction that's
	///     being created. This method also must not be called more than once.
	///   </para>
	/// </remarks>
	protected virtual void OnAsyncEnded()
	{
		lock (this)
		{
			if (ended)
			{
				throw new InvalidOperationException("The transaction has already been ended");
			}
			ended = true;
			if (doneEvent != null)
			{
				doneEvent.Set();
			}
		}
		if (!object.ReferenceEquals(endedEventSubscribers, null))
		{
			for (int i = 0; i < endedEventSubscribers.Count; i++)
			{
				endedEventSubscribers[i](this, EventArgs.Empty);
			}
			endedEventSubscribers = null;
		}
	}
}
