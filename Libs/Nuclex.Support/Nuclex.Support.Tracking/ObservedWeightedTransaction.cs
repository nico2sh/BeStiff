using System;

namespace Nuclex.Support.Tracking;

/// <summary>Transaction being observed by another object</summary>
/// <typeparam name="TransactionType">
///   Type of the transaction that is being observed
/// </typeparam>
internal class ObservedWeightedTransaction<TransactionType> : IDisposable where TransactionType : Transaction
{
	/// <summary>Delegate for reporting progress updates</summary>
	public delegate void ReportDelegate();

	private EventHandler<ProgressReportEventArgs> asyncProgressChangedEventHandler;

	/// <summary>The observed transaction's progress reporting interface</summary>
	private IProgressReporter progressReporter;

	/// <summary>The weighted wable that is being observed</summary>
	private WeightedTransaction<TransactionType> weightedTransaction;

	/// <summary>Callback to invoke when the progress updates</summary>
	private volatile ReportDelegate progressUpdateCallback;

	/// <summary>Callback to invoke when the transaction ends</summary>
	private volatile ReportDelegate endedCallback;

	/// <summary>Progress achieved so far</summary>
	private volatile float progress;

	/// <summary>Weighted transaction being observed</summary>
	public WeightedTransaction<TransactionType> WeightedTransaction => weightedTransaction;

	/// <summary>Amount of progress this transaction has achieved so far</summary>
	public float Progress => progress;

	/// <summary>Initializes a new observed transaction</summary>
	/// <param name="weightedTransaction">Weighted transaction being observed</param>
	/// <param name="progressUpdateCallback">
	///   Callback to invoke when the transaction's progress changes
	/// </param>
	/// <param name="endedCallback">
	///   Callback to invoke when the transaction has ended
	/// </param>
	internal ObservedWeightedTransaction(WeightedTransaction<TransactionType> weightedTransaction, ReportDelegate progressUpdateCallback, ReportDelegate endedCallback)
	{
		this.weightedTransaction = weightedTransaction;
		TransactionType transaction = weightedTransaction.Transaction;
		if (transaction.Ended)
		{
			progress = 1f;
			progressUpdateCallback();
			return;
		}
		this.endedCallback = endedCallback;
		this.progressUpdateCallback = progressUpdateCallback;
		this.weightedTransaction.Transaction.AsyncEnded += asyncEnded;
		progressReporter = this.weightedTransaction.Transaction as IProgressReporter;
		if (progressReporter != null)
		{
			asyncProgressChangedEventHandler = asyncProgressChanged;
			progressReporter.AsyncProgressChanged += asyncProgressChangedEventHandler;
		}
	}

	/// <summary>Immediately releases all resources owned by the object</summary>
	public void Dispose()
	{
		asyncDisconnectEvents();
	}

	/// <summary>Called when the observed transaction has ended</summary>
	/// <param name="sender">Transaction that has ended</param>
	/// <param name="e">Not used</param>
	private void asyncEnded(object sender, EventArgs e)
	{
		ReportDelegate reportDelegate = endedCallback;
		ReportDelegate reportDelegate2 = progressUpdateCallback;
		asyncDisconnectEvents();
		if (progress != 1f)
		{
			progress = 1f;
			reportDelegate2();
		}
		reportDelegate();
	}

	/// <summary>Called when the progress of the observed transaction changes</summary>
	/// <param name="sender">Transaction whose progress has changed</param>
	/// <param name="arguments">Contains the updated progress</param>
	private void asyncProgressChanged(object sender, ProgressReportEventArgs arguments)
	{
		progress = arguments.Progress;
		progressUpdateCallback?.Invoke();
	}

	/// <summary>Unsubscribes from all events of the observed transaction</summary>
	private void asyncDisconnectEvents()
	{
		if (endedCallback == null)
		{
			return;
		}
		lock (this)
		{
			if (endedCallback != null)
			{
				weightedTransaction.Transaction.AsyncEnded -= asyncEnded;
				if (progressReporter != null)
				{
					progressReporter.AsyncProgressChanged -= asyncProgressChangedEventHandler;
					asyncProgressChangedEventHandler = null;
				}
				endedCallback = null;
				progressUpdateCallback = null;
			}
		}
	}
}
