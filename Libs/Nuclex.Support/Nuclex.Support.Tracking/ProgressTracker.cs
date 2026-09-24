using System;
using System.Collections.Generic;

namespace Nuclex.Support.Tracking;

/// <summary>
///   Helps tracking the progress of one or more background transactions
/// </summary>
/// <remarks>
///   <para>
///     This is useful if you want to display a progress bar for multiple
///     transactions but can not guarantee that no additional transactions
///     will appear inmidst of execution.
///   </para>
///   <para>
///     This class does not implement the <see cref="T:Nuclex.Support.Tracking.Transaction" /> interface itself
///     in order to not violate the design principles of transactions which
///     guarantee that a <see cref="T:Nuclex.Support.Tracking.Transaction" /> will only finish once (whereas the
///     progress tracker might 'finish' any number of times).
///   </para>
/// </remarks>
public class ProgressTracker : IDisposable, IProgressReporter
{
	/// <summary>Matches a direct transaction to a fully wrapped one</summary>
	private class TransactionMatcher
	{
		/// <summary>Transaction this instance compares against</summary>
		private Transaction toMatch;

		/// <summary>
		///   Initializes a new transaction matcher that matches against
		///   the specified transaction
		/// </summary>
		/// <param name="toMatch">Transaction to match against</param>
		public TransactionMatcher(Transaction toMatch)
		{
			this.toMatch = toMatch;
		}

		/// <summary>
		///   Checks whether the provided transaction matches the comparison
		///   transaction of the instance
		/// </summary>
		/// <param name="other">Transaction to match to the comparison transaction</param>
		public bool Matches(ObservedWeightedTransaction<Transaction> other)
		{
			return object.ReferenceEquals(other.WeightedTransaction.Transaction, toMatch);
		}
	}

	/// <summary>Whether the tracker is currently idle</summary>
	private volatile bool idle;

	/// <summary>Current summed progress of the tracked transactions</summary>
	private volatile float progress;

	/// <summary>Total weight of all transactions being tracked</summary>
	private volatile float totalWeight;

	/// <summary>Transactions being tracked by this tracker</summary>
	private List<ObservedWeightedTransaction<Transaction>> trackedTransactions;

	/// <summary>Delegate for the asyncEnded() method</summary>
	private ObservedWeightedTransaction<Transaction>.ReportDelegate asyncEndedDelegate;

	/// <summary>Delegate for the asyncProgressUpdated() method</summary>
	private ObservedWeightedTransaction<Transaction>.ReportDelegate asyncProgressUpdatedDelegate;

	/// <summary>Whether the tracker is currently idle</summary>
	public bool Idle => idle;

	/// <summary>Current summed progress of the tracked transactions</summary>
	public float Progress => progress;

	/// <summary>Triggered when the idle state of the tracker changes</summary>
	/// <remarks>
	///   The tracker is idle when no transactions are being tracked in it. If you're
	///   using this class to feed a progress bar, this would be the event to use for
	///   showing or hiding the progress bar. The tracker starts off as idle because,
	///   upon construction, its list of transactions will be empty.
	/// </remarks>
	public event EventHandler<IdleStateEventArgs> AsyncIdleStateChanged;

	/// <summary>Triggered when the total progress has changed</summary>
	public event EventHandler<ProgressReportEventArgs> AsyncProgressChanged;

	/// <summary>Initializes a new transaction tracker</summary>
	public ProgressTracker()
	{
		trackedTransactions = new List<ObservedWeightedTransaction<Transaction>>();
		idle = true;
		asyncEndedDelegate = asyncEnded;
		asyncProgressUpdatedDelegate = asyncProgressChanged;
	}

	/// <summary>Immediately releases all resources owned by the instance</summary>
	public void Dispose()
	{
		lock (trackedTransactions)
		{
			for (int i = 0; i < trackedTransactions.Count; i++)
			{
				trackedTransactions[i].Dispose();
			}
			trackedTransactions.Clear();
			trackedTransactions = null;
		}
	}

	/// <summary>Begins tracking the specified background transactions</summary>
	/// <param name="transaction">Background transaction to be tracked</param>
	public void Track(Transaction transaction)
	{
		Track(transaction, 1f);
	}

	/// <summary>Begins tracking the specified background transaction</summary>
	/// <param name="transaction">Background transaction to be tracked</param>
	/// <param name="weight">Weight to assign to this background transaction</param>
	public void Track(Transaction transaction, float weight)
	{
		lock (trackedTransactions)
		{
			bool flag = trackedTransactions.Count == 0;
			if (transaction.Ended)
			{
				if (!flag)
				{
					trackedTransactions.Add(new ObservedWeightedTransaction<Transaction>(new WeightedTransaction<Transaction>(transaction, weight), asyncProgressUpdatedDelegate, asyncEndedDelegate));
				}
			}
			else
			{
				ObservedWeightedTransaction<Transaction> item = new ObservedWeightedTransaction<Transaction>(new WeightedTransaction<Transaction>(transaction, weight), asyncProgressUpdatedDelegate, asyncEndedDelegate);
				trackedTransactions.Add(item);
				if (flag)
				{
					setIdle(idle: false);
				}
			}
			totalWeight += weight;
			recalculateProgress();
		}
	}

	/// <summary>Stops tracking the specified background transaction</summary>
	/// <param name="transaction">Background transaction to stop tracking of</param>
	public void Untrack(Transaction transaction)
	{
		lock (trackedTransactions)
		{
			int i;
			for (i = 0; i < trackedTransactions.Count && !object.ReferenceEquals(transaction, trackedTransactions[i].WeightedTransaction.Transaction); i++)
			{
			}
			if (i == trackedTransactions.Count)
			{
				throw new ArgumentException("Specified transaction is not being tracked");
			}
			ObservedWeightedTransaction<Transaction> observedWeightedTransaction = trackedTransactions[i];
			trackedTransactions.RemoveAt(i);
			observedWeightedTransaction.Dispose();
			if (trackedTransactions.Count == 0)
			{
				totalWeight = 0f;
				setIdle(idle: true);
				return;
			}
			float num = 0f;
			for (i = 0; i < trackedTransactions.Count; i++)
			{
				num += trackedTransactions[i].WeightedTransaction.Weight;
			}
			totalWeight = num;
			recalculateProgress();
		}
	}

	/// <summary>Fires the AsyncIdleStateChanged event</summary>
	/// <param name="idle">New idle state to report</param>
	protected virtual void OnAsyncIdleStateChanged(bool idle)
	{
		this.AsyncIdleStateChanged?.Invoke(this, new IdleStateEventArgs(idle));
	}

	/// <summary>Fires the AsyncProgressUpdated event</summary>
	/// <param name="progress">New progress to report</param>
	protected virtual void OnAsyncProgressUpdated(float progress)
	{
		this.AsyncProgressChanged?.Invoke(this, new ProgressReportEventArgs(progress));
	}

	/// <summary>Recalculates the total progress of the tracker</summary>
	private void recalculateProgress()
	{
		bool flag = false;
		lock (trackedTransactions)
		{
			if (totalWeight != 0f)
			{
				float num = 0f;
				for (int i = 0; i < trackedTransactions.Count; i++)
				{
					float weight = trackedTransactions[i].WeightedTransaction.Weight;
					num += trackedTransactions[i].Progress * weight;
				}
				num /= totalWeight;
				if (num != progress)
				{
					progress = num;
					flag = true;
				}
			}
		}
		if (flag)
		{
			OnAsyncProgressUpdated(progress);
		}
	}

	/// <summary>Called when one of the tracked transactions has ended</summary>
	private void asyncEnded()
	{
		lock (trackedTransactions)
		{
			for (int i = 0; i < trackedTransactions.Count; i++)
			{
				if (!trackedTransactions[i].WeightedTransaction.Transaction.Ended)
				{
					return;
				}
			}
			trackedTransactions.Clear();
			totalWeight = 0f;
			setIdle(idle: true);
		}
	}

	/// <summary>Called when one of the tracked transactions has achieved progress</summary>
	private void asyncProgressChanged()
	{
		recalculateProgress();
	}

	/// <summary>Changes the idle state</summary>
	/// <param name="idle">Whether or not the tracker is currently idle</param>
	/// <remarks>
	///   This method expects to be called during a lock() on trackedTransactions!
	/// </remarks>
	private void setIdle(bool idle)
	{
		this.idle = idle;
		OnAsyncIdleStateChanged(idle);
	}
}
