namespace Nuclex.Support.Tracking;

/// <summary>Transaction with an associated weight for the total progress</summary>
public class WeightedTransaction<TransactionType> where TransactionType : Transaction
{
	/// <summary>Transaction whose progress we're tracking</summary>
	private TransactionType transaction;

	/// <summary>Weighting of this transaction in the total progress</summary>
	private float weight;

	/// <summary>Transaction being wrapped by this weighted transaction</summary>
	public TransactionType Transaction => transaction;

	/// <summary>The contribution of this transaction to the total progress</summary>
	public float Weight => weight;

	/// <summary>
	///   Initializes a new weighted transaction with a default weight of 1.0
	/// </summary>
	/// <param name="transaction">Transaction whose progress to monitor</param>
	public WeightedTransaction(TransactionType transaction)
		: this(transaction, 1f)
	{
	}

	/// <summary>Initializes a new weighted transaction</summary>
	/// <param name="transaction">transaction whose progress to monitor</param>
	/// <param name="weight">Weighting of the transaction's progress</param>
	public WeightedTransaction(TransactionType transaction, float weight)
	{
		this.transaction = transaction;
		this.weight = weight;
	}
}
