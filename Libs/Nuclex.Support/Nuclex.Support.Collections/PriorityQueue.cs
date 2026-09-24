using System;
using System.Collections;
using System.Collections.Generic;

namespace Nuclex.Support.Collections;

/// <summary>Queue that dequeues items in order of their priority</summary>
public class PriorityQueue<ItemType> : ICollection, IEnumerable<ItemType>, IEnumerable
{
	/// <summary>Enumerates all items contained in a priority queue</summary>
	private class Enumerator : IEnumerator<ItemType>, IDisposable, IEnumerator
	{
		/// <summary>Index of the current item in the priority queue</summary>
		private int index;

		/// <summary>The priority queue whose items this instance enumerates</summary>
		private PriorityQueue<ItemType> priorityQueue;

		/// <summary>The current item being enumerated</summary>
		ItemType IEnumerator<ItemType>.Current => priorityQueue.heap[index];

		/// <summary>The current item being enumerated</summary>
		object IEnumerator.Current => priorityQueue.heap[index];

		/// <summary>Initializes a new priority queue enumerator</summary>
		/// <param name="priorityQueue">Priority queue to be enumerated</param>
		public Enumerator(PriorityQueue<ItemType> priorityQueue)
		{
			this.priorityQueue = priorityQueue;
			Reset();
		}

		/// <summary>Resets the enumerator to its initial state</summary>
		public void Reset()
		{
			index = -1;
		}

		/// <summary>Moves to the next item in the priority queue</summary>
		/// <returns>True if a next item was found, false if the end has been reached</returns>
		public bool MoveNext()
		{
			if (index + 1 == priorityQueue.count)
			{
				return false;
			}
			index++;
			return true;
		}

		/// <summary>Releases all resources used by the enumerator</summary>
		public void Dispose()
		{
		}
	}

	/// <summary>Comparer used to order the items in the priority queue</summary>
	private IComparer<ItemType> comparer;

	/// <summary>Total number of items in the priority queue</summary>
	private int count;

	/// <summary>Available space in the priority queue</summary>
	private int capacity;

	/// <summary>Tree containing the items in the priority queue</summary>
	private ItemType[] heap;

	/// <summary>Total number of items in the priority queue</summary>
	public int Count => count;

	/// <summary>
	///   Obtains an object that can be used to synchronize accesses to the priority queue
	///   from different threads
	/// </summary>
	public object SyncRoot => this;

	/// <summary>Whether operations performed on this priority queue are thread safe</summary>
	public bool IsSynchronized => false;

	/// <summary>
	///   Initializes a new priority queue using IComparable for comparing items
	/// </summary>
	public PriorityQueue()
		: this((IComparer<ItemType>)Comparer<ItemType>.Default)
	{
	}

	/// <summary>Initializes a new priority queue</summary>
	/// <param name="comparer">Comparer to use for ordering the items</param>
	public PriorityQueue(IComparer<ItemType> comparer)
	{
		this.comparer = comparer;
		capacity = 15;
		heap = new ItemType[capacity];
	}

	/// <summary>Returns the topmost item in the queue without dequeueing it</summary>
	/// <returns>The topmost item in the queue</returns>
	public ItemType Peek()
	{
		if (count == 0)
		{
			throw new InvalidOperationException("No items queued");
		}
		return heap[0];
	}

	/// <summary>Takes the item with the highest priority off from the queue</summary>
	/// <returns>The item with the highest priority in the list</returns>
	/// <exception cref="T:System.InvalidOperationException">When the queue is empty</exception>
	public ItemType Dequeue()
	{
		if (count == 0)
		{
			throw new InvalidOperationException("No items available to dequeue");
		}
		ItemType result = heap[0];
		count--;
		trickleDown(0, heap[count]);
		return result;
	}

	/// <summary>Puts an item into the priority queue</summary>
	/// <param name="item">Item to be queued</param>
	public void Enqueue(ItemType item)
	{
		if (count == capacity)
		{
			growHeap();
		}
		count++;
		bubbleUp(count - 1, item);
	}

	/// <summary>Removes all items from the priority queue</summary>
	public void Clear()
	{
		count = 0;
	}

	/// <summary>Copies the contents of the priority queue into an array</summary>
	/// <param name="array">Array to copy the priority queue into</param>
	/// <param name="index">Starting index for the destination array</param>
	public void CopyTo(Array array, int index)
	{
		Array.Copy(heap, 0, array, index, count);
	}

	/// <summary>Returns a typesafe enumerator for the priority queue</summary>
	/// <returns>A new enumerator for the priority queue</returns>
	public IEnumerator<ItemType> GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Moves an item upwards in the heap tree</summary>
	/// <param name="index">Index of the item to be moved</param>
	/// <param name="item">Item to be moved</param>
	private void bubbleUp(int index, ItemType item)
	{
		int parent = getParent(index);
		while (index > 0 && comparer.Compare(heap[parent], item) < 0)
		{
			heap[index] = heap[parent];
			index = parent;
			parent = getParent(index);
		}
		heap[index] = item;
	}

	/// <summary>Move the item downwards in the heap tree</summary>
	/// <param name="index">Index of the item to be moved</param>
	/// <param name="item">Item to be moved</param>
	private void trickleDown(int index, ItemType item)
	{
		for (int num = getLeftChild(index); num < count; num = getLeftChild(index))
		{
			if (num + 1 < count && comparer.Compare(heap[num], heap[num + 1]) < 0)
			{
				num++;
			}
			heap[index] = heap[num];
			index = num;
		}
		bubbleUp(index, item);
	}

	/// <summary>Obtains the left child item in the heap tree</summary>
	/// <param name="index">Index of the item whose left child to return</param>
	/// <returns>The left child item of the provided parent item</returns>
	private int getLeftChild(int index)
	{
		return index * 2 + 1;
	}

	/// <summary>Calculates the parent entry of the item on the heap</summary>
	/// <param name="index">Index of the item whose parent to calculate</param>
	/// <returns>The index of the parent to the specified item</returns>
	private int getParent(int index)
	{
		return (index - 1) / 2;
	}

	/// <summary>Increases the size of the priority collection's heap</summary>
	private void growHeap()
	{
		capacity = capacity * 2 + 1;
		ItemType[] destinationArray = new ItemType[capacity];
		Array.Copy(heap, 0, destinationArray, 0, count);
		heap = destinationArray;
	}

	/// <summary>Returns an enumerator for the priority queue</summary>
	/// <returns>A new enumerator for the priority queue</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}
}
