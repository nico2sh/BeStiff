using System;
using System.Collections;
using System.Collections.Generic;

namespace Nuclex.Support.Collections;

/// <summary>Queue that dequeues items in order of their priority</summary>
/// <remarks>
///   This variant of the priority queue uses an external priority value. If the
///   priority data type implements the IComparable interface, the user does not
///   even
/// </remarks>
public class PairPriorityQueue<PriorityType, ItemType> : ICollection, IEnumerable<PriorityItemPair<PriorityType, ItemType>>, IEnumerable
{
	/// <summary>Compares two priority queue entries based on their priority</summary>
	private class PairComparer : IComparer<PriorityItemPair<PriorityType, ItemType>>
	{
		/// <summary>Comparer used to compare the priorities of the entries</summary>
		private IComparer<PriorityType> priorityComparer;

		/// <summary>Initializes a new entry comparer</summary>
		/// <param name="priorityComparer">Comparer used to compare entry priorities</param>
		public PairComparer(IComparer<PriorityType> priorityComparer)
		{
			this.priorityComparer = priorityComparer;
		}

		/// <summary>Compares the left entry to the right entry</summary>
		/// <param name="left">Entry on the left side</param>
		/// <param name="right">Entry on the right side</param>
		/// <returns>The relationship of the two entries</returns>
		public int Compare(PriorityItemPair<PriorityType, ItemType> left, PriorityItemPair<PriorityType, ItemType> right)
		{
			return priorityComparer.Compare(left.Priority, right.Priority);
		}
	}

	/// <summary>Intrusive priority queue being wrapped by this class</summary>
	private PriorityQueue<PriorityItemPair<PriorityType, ItemType>> internalQueue;

	/// <summary>Total number of items in the priority queue</summary>
	public int Count => internalQueue.Count;

	/// <summary>
	///   Obtains an object that can be used to synchronize accesses to the priority queue
	///   from different threads
	/// </summary>
	public object SyncRoot => internalQueue.SyncRoot;

	/// <summary>Whether operations performed on this priority queue are thread safe</summary>
	public bool IsSynchronized => internalQueue.IsSynchronized;

	/// <summary>Initializes a new non-intrusive priority queue</summary>
	public PairPriorityQueue()
		: this((IComparer<PriorityType>)Comparer<PriorityType>.Default)
	{
	}

	/// <summary>Initializes a new non-intrusive priority queue</summary>
	/// <param name="priorityComparer">Comparer used to compare the item priorities</param>
	public PairPriorityQueue(IComparer<PriorityType> priorityComparer)
	{
		internalQueue = new PriorityQueue<PriorityItemPair<PriorityType, ItemType>>(new PairComparer(priorityComparer));
	}

	/// <summary>Returns the topmost item in the queue without dequeueing it</summary>
	/// <returns>The topmost item in the queue</returns>
	public PriorityItemPair<PriorityType, ItemType> Peek()
	{
		return internalQueue.Peek();
	}

	/// <summary>Takes the item with the highest priority off from the queue</summary>
	/// <returns>The item with the highest priority in the list</returns>
	public PriorityItemPair<PriorityType, ItemType> Dequeue()
	{
		return internalQueue.Dequeue();
	}

	/// <summary>Puts an item into the priority queue</summary>
	/// <param name="priority">Priority of the item to be queued</param>
	/// <param name="item">Item to be queued</param>
	public void Enqueue(PriorityType priority, ItemType item)
	{
		internalQueue.Enqueue(new PriorityItemPair<PriorityType, ItemType>(priority, item));
	}

	/// <summary>Removes all items from the priority queue</summary>
	public void Clear()
	{
		internalQueue.Clear();
	}

	/// <summary>Copies the contents of the priority queue into an array</summary>
	/// <param name="array">Array to copy the priority queue into</param>
	/// <param name="index">Starting index for the destination array</param>
	public void CopyTo(Array array, int index)
	{
		internalQueue.CopyTo(array, index);
	}

	/// <summary>Returns a typesafe enumerator for the priority queue</summary>
	/// <returns>A new enumerator for the priority queue</returns>
	public IEnumerator<PriorityItemPair<PriorityType, ItemType>> GetEnumerator()
	{
		return internalQueue.GetEnumerator();
	}

	/// <summary>Returns an enumerator for the priority queue</summary>
	/// <returns>A new enumerator for the priority queue</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return internalQueue.GetEnumerator();
	}
}
