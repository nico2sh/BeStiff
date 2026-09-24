using System;
using System.Collections.Generic;
using System.Threading;

namespace Nuclex.Support.Tracking;

/// <summary>Forms a single transaction from a group of transactions</summary>
/// <typeparam name="TransactionType">Type of transactions to manage as a set</typeparam>
public class TransactionGroup<TransactionType> : Transaction, IDisposable, IProgressReporter where TransactionType : Transaction
{
	/// <summary>Transactions being managed in the set</summary>
	private volatile List<ObservedWeightedTransaction<TransactionType>> children;

	/// <summary>
	///   Wrapper collection for exposing the child transactions under the
	///   WeightedTransaction interface
	/// </summary>
	private volatile WeightedTransactionWrapperCollection<TransactionType> wrapper;

	/// <summary>Summed weight of all transactions in the set</summary>
	private float totalWeight;

	/// <summary>Whether we already called OnAsyncEnded</summary>
	private int endedCalled;

	/// <summary>Childs contained in the transaction set</summary>
	public IList<WeightedTransaction<TransactionType>> Children
	{
		get
		{
			if (wrapper == null)
			{
				wrapper = new WeightedTransactionWrapperCollection<TransactionType>(children);
			}
			return wrapper;
		}
	}

	/// <summary>will be triggered to report when progress has been achieved</summary>
	public event EventHandler<ProgressReportEventArgs> AsyncProgressChanged;

	/// <summary>Initializes a new transaction group</summary>
	/// <param name="children">Transactions to track with this group</param>
	/// <remarks>
	///   Uses a default weighting factor of 1.0 for all transactions.
	/// </remarks>
	public TransactionGroup(IEnumerable<TransactionType> children)
	{
		List<ObservedWeightedTransaction<TransactionType>> list = new List<ObservedWeightedTransaction<TransactionType>>();
		foreach (TransactionType child in children)
		{
			list.Add(new ObservedWeightedTransaction<TransactionType>(new WeightedTransaction<TransactionType>(child), asyncProgressUpdated, asyncChildEnded));
		}
		totalWeight = list.Count;
		this.children = list;
		asyncChildEnded();
	}

	/// <summary>Initializes a new transaction group</summary>
	/// <param name="children">Transactions to track with this group</param>
	public TransactionGroup(IEnumerable<WeightedTransaction<TransactionType>> children)
	{
		List<ObservedWeightedTransaction<TransactionType>> list = new List<ObservedWeightedTransaction<TransactionType>>();
		foreach (WeightedTransaction<TransactionType> child in children)
		{
			list.Add(new ObservedWeightedTransaction<TransactionType>(child, asyncProgressUpdated, asyncChildEnded));
			totalWeight += child.Weight;
		}
		this.children = list;
		asyncChildEnded();
	}

	/// <summary>Immediately releases all resources owned by the object</summary>
	public void Dispose()
	{
		if (children != null)
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].Dispose();
			}
			children = null;
			wrapper = null;
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

	/// <summary>
	///   Called when the progress of one of the observed transactions changes
	/// </summary>
	private void asyncProgressUpdated()
	{
		if (children != null)
		{
			float num = 0f;
			for (int i = 0; i < children.Count; i++)
			{
				num += children[i].Progress * children[i].WeightedTransaction.Weight;
			}
			if (totalWeight > 0f)
			{
				num /= totalWeight;
			}
			OnAsyncProgressChanged(num);
		}
	}

	/// <summary>
	///   Called when an observed transaction ends
	/// </summary>
	private void asyncChildEnded()
	{
		if (children == null)
		{
			return;
		}
		for (int i = 0; i < children.Count; i++)
		{
			TransactionType transaction = children[i].WeightedTransaction.Transaction;
			if (!transaction.Ended)
			{
				return;
			}
		}
		if (Interlocked.Exchange(ref endedCalled, 1) == 0)
		{
			OnAsyncEnded();
		}
	}
}
