using System;
using System.Collections.Generic;
using System.Threading;
using Nuclex.Support.Tracking;

namespace Nuclex.Support.Scheduling;

/// <summary>Operation that sequentially executes a series of operations</summary>
/// <typeparam name="OperationType">
///   Type of the child operations the QueueOperation will contain
/// </typeparam>
public class OperationQueue<OperationType> : Operation, IProgressReporter where OperationType : Operation
{
	/// <summary>Delegate to the asyncOperationEnded() method</summary>
	private EventHandler asyncOperationEndedDelegate;

	/// <summary>Delegate to the asyncOperationProgressUpdated() method</summary>
	private EventHandler<ProgressReportEventArgs> asyncOperationProgressChangedDelegate;

	/// <summary>Operations being managed in the queue</summary>
	private List<WeightedTransaction<OperationType>> children;

	/// <summary>Summed weight of all operations in the queue</summary>
	private float totalWeight;

	/// <summary>Accumulated weight of the operations already completed</summary>
	private float completedWeight;

	/// <summary>Index of the operation currently executing</summary>
	private int currentOperationIndex;

	/// <summary>Used to detect when an operation completes synchronously</summary>
	private int completionStatus;

	/// <summary>Exception that has occured in the background process</summary>
	private volatile Exception exception;

	/// <summary>Provides access to the child operations of this queue</summary>
	public IList<WeightedTransaction<OperationType>> Children => children;

	/// <summary>will be triggered to report when progress has been achieved</summary>
	public event EventHandler<ProgressReportEventArgs> AsyncProgressChanged;

	/// <summary>Initializes a new queue operation with default weights</summary>
	/// <param name="childs">Child operations to execute in this operation</param>
	/// <remarks>
	///   All child operations will have a default weight of 1.0
	/// </remarks>
	public OperationQueue(IEnumerable<OperationType> childs)
		: this()
	{
		foreach (OperationType child in childs)
		{
			children.Add(new WeightedTransaction<OperationType>(child));
		}
		totalWeight = children.Count;
	}

	/// <summary>Initializes a new queue operation with custom weights</summary>
	/// <param name="childs">Child operations to execute in this operation</param>
	public OperationQueue(IEnumerable<WeightedTransaction<OperationType>> childs)
		: this()
	{
		foreach (WeightedTransaction<OperationType> child in childs)
		{
			children.Add(child);
			totalWeight += child.Weight;
		}
	}

	/// <summary>Initializes a new queue operation</summary>
	private OperationQueue()
	{
		asyncOperationEndedDelegate = asyncOperationEnded;
		asyncOperationProgressChangedDelegate = asyncOperationProgressChanged;
		children = new List<WeightedTransaction<OperationType>>();
	}

	/// <summary>Launches the background operation</summary>
	public override void Start()
	{
		startCurrentOperation();
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

	/// <summary>Fires the progress update event</summary>
	/// <param name="progress">Progress to report (ranging from 0.0 to 1.0)</param>
	/// <remarks>
	///   Informs the observers of this transaction about the achieved progress.
	/// </remarks>
	protected virtual void OnAsyncProgressChanged(float progress)
	{
		OnAsyncProgressChanged(new ProgressReportEventArgs(progress));
	}

	/// <summary>Fires the progress update event</summary>
	/// <param name="eventArguments">Progress to report (ranging from 0.0 to 1.0)</param>
	/// <remarks>
	///   Informs the observers of this transaction about the achieved progress.
	///   Allows for classes derived from the transaction class to easily provide
	///   a custom event arguments class that has been derived from the
	///   transaction's ProgressUpdateEventArgs class.
	/// </remarks>
	protected virtual void OnAsyncProgressChanged(ProgressReportEventArgs eventArguments)
	{
		this.AsyncProgressChanged?.Invoke(this, eventArguments);
	}

	/// <summary>Prepares the current operation and calls its Start() method</summary>
	/// <remarks>
	///   This subscribes the queue to the events of to the current operation
	///   and launches the operation by calling its Start() method.
	/// </remarks>
	private void startCurrentOperation()
	{
		do
		{
			Thread.MemoryBarrier();
			OperationType transaction = children[currentOperationIndex].Transaction;
			transaction.AsyncEnded += asyncOperationEndedDelegate;
			if (transaction is IProgressReporter progressReporter)
			{
				progressReporter.AsyncProgressChanged += asyncOperationProgressChangedDelegate;
			}
			Interlocked.Exchange(ref completionStatus, 1);
			transaction.Start();
		}
		while (Interlocked.Decrement(ref completionStatus) > 0);
	}

	/// <summary>Disconnects from the current operation and calls its End() method</summary>
	/// <remarks>
	///   This unsubscribes the queue from the current operation's events, calls End()
	///   on the operation and, if the operation didn't have an exception to report,
	///   counts up the accumulated progress of th  e queue.
	/// </remarks>
	private void endCurrentOperation()
	{
		Thread.MemoryBarrier();
		OperationType transaction = children[currentOperationIndex].Transaction;
		transaction.AsyncEnded -= asyncOperationEndedDelegate;
		if (transaction is IProgressReporter progressReporter)
		{
			progressReporter.AsyncProgressChanged -= asyncOperationProgressChangedDelegate;
		}
		try
		{
			transaction.Join();
			completedWeight += children[currentOperationIndex].Weight;
			OnAsyncProgressChanged(completedWeight / totalWeight);
		}
		catch (Exception ex)
		{
			exception = ex;
		}
	}

	/// <summary>Called when the current executing operation ends</summary>
	/// <param name="sender">Operation that ended</param>
	/// <param name="arguments">Not used</param>
	private void asyncOperationEnded(object sender, EventArgs arguments)
	{
		endCurrentOperation();
		if (exception == null)
		{
			int num = Interlocked.Increment(ref currentOperationIndex);
			Thread.MemoryBarrier();
			if (num < children.Count)
			{
				if (Interlocked.Increment(ref completionStatus) == 1)
				{
					startCurrentOperation();
				}
				return;
			}
		}
		OnAsyncEnded();
	}

	/// <summary>Called when currently executing operation makes progress</summary>
	/// <param name="sender">Operation that has achieved progress</param>
	/// <param name="arguments">Not used</param>
	private void asyncOperationProgressChanged(object sender, ProgressReportEventArgs arguments)
	{
		float weight = children[currentOperationIndex].Weight;
		float num = arguments.Progress * weight;
		float progress = (completedWeight + num) / totalWeight;
		OnAsyncProgressChanged(progress);
	}
}
