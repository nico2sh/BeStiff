using System.Collections.Generic;
using Nuclex.Support.Collections;

namespace Nuclex.Support.Tracking;

/// <summary>Collection of transactions with a weighting value</summary>
/// <typeparam name="TransactionType">Type of transactions to manage</typeparam>
/// <remarks>
///   <para>
///     This collection is exposed as a read-only collection to the user that
///     stores WeightedTransactions. Internally, it merely wraps a collection of
///     an internal type used to keep track of the individual transaction's
///     progress in the TransactionGroup and OperationQueue classes.
///   </para>
///   <para>
///     It is read-only because the design requires a transaction to only ever finish
///     once. If it was possible eg. to add items after a TransactionGroup had signalled
///     itself as being finished, it would be moved into an unfinished state again.
///     Also, an empty TransactionGroup is, by definition, finished (simply because
///     there is no work to do) - unless the contents of the group are passed to the
///     TransactionGroup's constructor and never modified at all, the design would be
///     violated as soon as an instance of the TransactionGroup or OperationQueue
///     classes was created.
///   </para>
/// </remarks>
internal class WeightedTransactionWrapperCollection<TransactionType> : TransformingReadOnlyCollection<ObservedWeightedTransaction<TransactionType>, WeightedTransaction<TransactionType>> where TransactionType : Transaction
{
	/// <summary>Initializes a new weighted transaction collection wrapper</summary>
	/// <param name="items">Items to be exposed as weighted transactions</param>
	internal WeightedTransactionWrapperCollection(IList<ObservedWeightedTransaction<TransactionType>> items)
		: base(items)
	{
	}

	/// <summary>Transforms an item into the exposed type</summary>
	/// <param name="item">Item to be transformed</param>
	/// <returns>The transformed item</returns>
	/// <remarks>
	///   This method is used to transform an item in the wrapped collection into
	///   the exposed item type whenever the user accesses an item. Expect it to
	///   be called frequently, because the TransformingReadOnlyCollection does
	///   not cache otherwise store the transformed items.
	/// </remarks>
	protected override WeightedTransaction<TransactionType> Transform(ObservedWeightedTransaction<TransactionType> item)
	{
		return item.WeightedTransaction;
	}
}
