using System;
using System.Collections;
using System.Collections.Generic;

namespace Nuclex.Support.Collections;

/// <summary>A double-ended queue that allocates memory in blocks</summary>
/// <typeparam name="ItemType">Type of the items being stored in the queue</typeparam>
/// <remarks>
///   <para>
///     The double-ended queue allows items to be appended to either side of the queue
///     without a hefty toll on performance. Like its namesake in C++, it is implemented
///     using multiple arrays.
///   </para>
///   <para>
///     Therefore, it's not only good at coping with lists that are modified at their
///     beginning, but also at handling huge data sets since enlarging the deque doesn't
///     require items to be copied around and still can be accessed by index.
///   </para>
/// </remarks>
public class Deque<ItemType> : IList<ItemType>, ICollection<ItemType>, IEnumerable<ItemType>, IList, ICollection, IEnumerable
{
	/// <summary>Enumerates over the items in a deque</summary>
	private class Enumerator : IEnumerator<ItemType>, IDisposable, IEnumerator
	{
		/// <summary>Deque the enumerator belongs to</summary>
		private Deque<ItemType> deque;

		/// <summary>Size of the blocks in the deque</summary>
		private int blockSize;

		/// <summary>Index of the last block in the deque</summary>
		private int lastBlock;

		/// <summary>End index of the items in the deque's last block</summary>
		private int lastBlockEndIndex;

		/// <summary>Index of the block the enumerator currently is in</summary>
		private int currentBlockIndex;

		/// <summary>Reference to the block being enumerated</summary>
		private ItemType[] currentBlock;

		/// <summary>Index in the current block</summary>
		private int subIndex;

		/// <summary>The item at the enumerator's current position</summary>
		public ItemType Current
		{
			get
			{
				if (currentBlock == null)
				{
					throw new InvalidOperationException("Enumerator is not on a valid position");
				}
				return currentBlock[subIndex];
			}
		}

		/// <summary>The item at the enumerator's current position</summary>
		object IEnumerator.Current => Current;

		/// <summary>Initializes a new deque enumerator</summary>
		/// <param name="deque">Deque whose items will be enumerated</param>
		public Enumerator(Deque<ItemType> deque)
		{
			this.deque = deque;
			blockSize = this.deque.blockSize;
			lastBlock = this.deque.blocks.Count - 1;
			lastBlockEndIndex = this.deque.lastBlockEndIndex - 1;
			Reset();
		}

		/// <summary>Immediately releases all resources owned by the instance</summary>
		public void Dispose()
		{
			deque = null;
			currentBlock = null;
		}

		/// <summary>Advances the enumerator to the next item</summary>
		/// <returns>True if there was a next item</returns>
		public bool MoveNext()
		{
			if (currentBlockIndex < lastBlock)
			{
				subIndex++;
				if (subIndex >= blockSize)
				{
					currentBlockIndex++;
					currentBlock = deque.blocks[currentBlockIndex];
					if (currentBlockIndex == 0)
					{
						subIndex = deque.firstBlockStartIndex;
					}
					else
					{
						subIndex = 0;
					}
				}
				return true;
			}
			if (subIndex < lastBlockEndIndex)
			{
				subIndex++;
				return true;
			}
			currentBlock = null;
			return false;
		}

		/// <summary>Resets the enumerator to its initial position</summary>
		public void Reset()
		{
			currentBlock = null;
			currentBlockIndex = -1;
			subIndex = deque.blockSize - 1;
		}
	}

	/// <summary>Number if items currently stored in the deque</summary>
	private int count;

	/// <summary>Size of a single deque block</summary>
	private int blockSize;

	/// <summary>Memory blocks being used to store the deque's data</summary>
	private List<ItemType[]> blocks;

	/// <summary>Starting index of data in the first block</summary>
	private int firstBlockStartIndex;

	/// <summary>End index of data in the last block</summary>
	private int lastBlockEndIndex;

	/// <summary>Number of items contained in the double ended queue</summary>
	public int Count => count;

	/// <summary>Accesses an item by its index</summary>
	/// <param name="index">Index of the item that will be accessed</param>
	/// <returns>The item at the specified index</returns>
	public ItemType this[int index]
	{
		get
		{
			findIndex(index, out var blockIndex, out var subIndex);
			return blocks[blockIndex][subIndex];
		}
		set
		{
			findIndex(index, out var blockIndex, out var subIndex);
			blocks[blockIndex][subIndex] = value;
		}
	}

	/// <summary>The first item in the double-ended queue</summary>
	public ItemType First
	{
		get
		{
			if (count == 0)
			{
				throw new InvalidOperationException("The deque is empty");
			}
			return blocks[0][firstBlockStartIndex];
		}
	}

	/// <summary>The last item in the double-ended queue</summary>
	public ItemType Last
	{
		get
		{
			if (count == 0)
			{
				throw new InvalidOperationException("The deque is empty");
			}
			return blocks[blocks.Count - 1][lastBlockEndIndex - 1];
		}
	}

	/// <summary>Whether the deque has a fixed size</summary>
	bool IList.IsFixedSize => false;

	/// <summary>Whether the deque is read-only</summary>
	bool IList.IsReadOnly => false;

	/// <summary>Accesses an item in the deque by its index</summary>
	/// <param name="index">Index of the item that will be accessed</param>
	/// <returns>The item at the specified index</returns>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			verifyCompatibleObject(value);
			this[index] = (ItemType)value;
		}
	}

	/// <summary>Whether the collection is read-only</summary>
	bool ICollection<ItemType>.IsReadOnly => false;

	/// <summary>Whether the deque is thread-synchronized</summary>
	bool ICollection.IsSynchronized => false;

	/// <summary>Synchronization root of the instance</summary>
	object ICollection.SyncRoot => this;

	/// <summary>Initializes a new deque</summary>
	public Deque()
		: this(512)
	{
	}

	/// <summary>Initializes a new deque using the specified block size</summary>
	/// <param name="blockSize">Size of the individual memory blocks used</param>
	public Deque(int blockSize)
	{
		this.blockSize = blockSize;
		blocks = new List<ItemType[]>();
		blocks.Add(new ItemType[this.blockSize]);
	}

	/// <summary>Determines whether the deque contains the specified item</summary>
	/// <param name="item">Item the deque will be scanned for</param>
	/// <returns>True if the deque contains the item, false otherwise</returns>
	public bool Contains(ItemType item)
	{
		return IndexOf(item) != -1;
	}

	/// <summary>Copies the contents of the deque into an array</summary>
	/// <param name="array">Array the contents of the deque will be copied into</param>
	/// <param name="arrayIndex">Array index the deque contents will begin at</param>
	public void CopyTo(ItemType[] array, int arrayIndex)
	{
		if (count > array.Length - arrayIndex)
		{
			throw new ArgumentException("Array too small to hold the collection items starting at the specified index");
		}
		if (blocks.Count == 1)
		{
			Array.Copy(blocks[0], firstBlockStartIndex, array, arrayIndex, lastBlockEndIndex - firstBlockStartIndex);
			return;
		}
		int num = blockSize - firstBlockStartIndex;
		Array.Copy(blocks[0], firstBlockStartIndex, array, arrayIndex, num);
		arrayIndex += num;
		int num2 = blocks.Count - 1;
		for (int i = 1; i < num2; i++)
		{
			Array.Copy(blocks[i], 0, array, arrayIndex, blockSize);
			arrayIndex += blockSize;
		}
		Array.Copy(blocks[num2], 0, array, arrayIndex, lastBlockEndIndex);
	}

	/// <summary>Obtains a new enumerator for the contents of the deque</summary>
	/// <returns>The new enumerator</returns>
	public IEnumerator<ItemType> GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Calculates the block index and local sub index of an entry</summary>
	/// <param name="index">Index of the entry that will be located</param>
	/// <param name="blockIndex">Index of the block the entry is contained in</param>
	/// <param name="subIndex">Local sub index of the entry within the block</param>
	private void findIndex(int index, out int blockIndex, out int subIndex)
	{
		if (index < 0 || index >= count)
		{
			throw new ArgumentOutOfRangeException("Index out of range", "index");
		}
		index += firstBlockStartIndex;
		blockIndex = Math.DivRem(index, blockSize, out subIndex);
	}

	/// <summary>
	///   Determines whether the provided object can be placed in the deque
	/// </summary>
	/// <param name="value">Value that will be checked for compatibility</param>
	/// <returns>True if the value can be placed in the deque</returns>
	private static bool isCompatibleObject(object value)
	{
		if (!(value is ItemType))
		{
			if (value == null)
			{
				return !typeof(ItemType).IsValueType;
			}
			return false;
		}
		return true;
	}

	/// <summary>Verifies that the provided object matches the deque's type</summary>
	/// <param name="value">Value that will be checked for compatibility</param>
	private static void verifyCompatibleObject(object value)
	{
		if (!isCompatibleObject(value))
		{
			throw new ArgumentException("Value does not match the deque's type", "value");
		}
	}

	/// <summary>Inserts an item at the beginning of the double-ended queue</summary>
	/// <param name="item">Item that will be inserted into the queue</param>
	public void AddFirst(ItemType item)
	{
		if (firstBlockStartIndex > 0)
		{
			firstBlockStartIndex--;
		}
		else
		{
			blocks.Insert(0, new ItemType[blockSize]);
			firstBlockStartIndex = blockSize - 1;
		}
		blocks[0][firstBlockStartIndex] = item;
		count++;
	}

	/// <summary>Appends an item to the end of the double-ended queue</summary>
	/// <param name="item">Item that will be appended to the queue</param>
	public void AddLast(ItemType item)
	{
		if (lastBlockEndIndex < blockSize)
		{
			lastBlockEndIndex++;
		}
		else
		{
			blocks.Add(new ItemType[blockSize]);
			lastBlockEndIndex = 1;
		}
		blocks[blocks.Count - 1][lastBlockEndIndex - 1] = item;
		count++;
	}

	/// <summary>Inserts the item at the specified index</summary>
	/// <param name="index">Index the item will be inserted at</param>
	/// <param name="item">Item that will be inserted</param>
	public void Insert(int index, ItemType item)
	{
		int num = count - index;
		if (index < num)
		{
			shiftLeftAndInsert(index, item);
		}
		else
		{
			shiftRightAndInsert(index, item);
		}
	}

	/// <summary>
	///   Shifts all items before the insertion point to the left and inserts
	///   the item at the specified index
	/// </summary>
	/// <param name="index">Index the item will be inserted at</param>
	/// <param name="item">Item that will be inserted</param>
	private void shiftLeftAndInsert(int index, ItemType item)
	{
		if (index == 0)
		{
			AddFirst(item);
			return;
		}
		findIndex(index, out var blockIndex, out var subIndex);
		int num = 0;
		int num2;
		if (firstBlockStartIndex == 0)
		{
			blocks.Insert(0, new ItemType[blockSize]);
			blocks[0][blockSize - 1] = blocks[1][0];
			firstBlockStartIndex = blockSize - 1;
			num2 = 1;
			subIndex--;
			if (subIndex < 0)
			{
				subIndex = blockSize - 1;
			}
			else
			{
				blockIndex++;
			}
			num++;
		}
		else
		{
			num2 = firstBlockStartIndex;
			firstBlockStartIndex--;
			subIndex--;
			if (subIndex < 0)
			{
				subIndex = blockSize - 1;
				blockIndex--;
			}
		}
		if (blockIndex != num)
		{
			Array.Copy(blocks[num], num2, blocks[num], num2 - 1, blockSize - num2);
			blocks[num][blockSize - 1] = blocks[num + 1][0];
			for (int i = num + 1; i < blockIndex; i++)
			{
				Array.Copy(blocks[i], 1, blocks[i], 0, blockSize - 1);
				blocks[i][blockSize - 1] = blocks[i + 1][0];
			}
			num2 = 1;
		}
		Array.Copy(blocks[blockIndex], num2, blocks[blockIndex], num2 - 1, subIndex - num2 + 1);
		blocks[blockIndex][subIndex] = item;
		count++;
	}

	/// <summary>
	///   Shifts all items after the insertion point to the right and inserts
	///   the item at the specified index
	/// </summary>
	/// <param name="index">Index the item will be inserted at</param>
	/// <param name="item">Item that will be inserted</param>
	private void shiftRightAndInsert(int index, ItemType item)
	{
		if (index == count)
		{
			AddLast(item);
			return;
		}
		findIndex(index, out var blockIndex, out var subIndex);
		int num = blocks.Count - 1;
		int num2;
		if (lastBlockEndIndex == blockSize)
		{
			blocks.Add(new ItemType[blockSize]);
			blocks[num + 1][0] = blocks[num][blockSize - 1];
			lastBlockEndIndex = 1;
			num2 = blockSize - 1;
		}
		else
		{
			num2 = lastBlockEndIndex;
			lastBlockEndIndex++;
		}
		if (blockIndex != num)
		{
			Array.Copy(blocks[num], 0, blocks[num], 1, num2);
			blocks[num][0] = blocks[num - 1][blockSize - 1];
			for (int num3 = num - 1; num3 > blockIndex; num3--)
			{
				Array.Copy(blocks[num3], 0, blocks[num3], 1, blockSize - 1);
				blocks[num3][0] = blocks[num3 - 1][blockSize - 1];
			}
			num2 = blockSize - 1;
		}
		Array.Copy(blocks[blockIndex], subIndex, blocks[blockIndex], subIndex + 1, num2 - subIndex);
		blocks[blockIndex][subIndex] = item;
		count++;
	}

	/// <summary>Obtains a new enumerator for the contents of the deque</summary>
	/// <returns>The new enumerator</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Adds an item to the deque</summary>
	/// <param name="value">Item that will be added to the deque</param>
	/// <returns>The index at which the new item was added</returns>
	int IList.Add(object value)
	{
		verifyCompatibleObject(value);
		AddLast((ItemType)value);
		return count - 1;
	}

	/// <summary>Checks whether the deque contains the specified item</summary>
	/// <param name="value">Item the deque will be scanned for</param>
	/// <returns>True if the deque contained the specified item</returns>
	bool IList.Contains(object value)
	{
		if (isCompatibleObject(value))
		{
			return Contains((ItemType)value);
		}
		return false;
	}

	/// <summary>Determines the index of the item in the deque</summary>
	/// <param name="value">Item whose index will be determined</param>
	/// <returns>The index of the specified item in the deque</returns>
	int IList.IndexOf(object value)
	{
		if (isCompatibleObject(value))
		{
			return IndexOf((ItemType)value);
		}
		return -1;
	}

	/// <summary>Inserts an item into the deque at the specified location</summary>
	/// <param name="index">Index at which the item will be inserted</param>
	/// <param name="value">Item that will be inserted</param>
	void IList.Insert(int index, object value)
	{
		verifyCompatibleObject(value);
		Insert(index, (ItemType)value);
	}

	/// <summary>Removes the specified item from the deque</summary>
	/// <param name="value">Item that will be removed from the deque</param>
	void IList.Remove(object value)
	{
		if (isCompatibleObject(value))
		{
			Remove((ItemType)value);
		}
	}

	/// <summary>Adds an item into the deque</summary>
	/// <param name="item">Item that will be added to the deque</param>
	void ICollection<ItemType>.Add(ItemType item)
	{
		AddLast(item);
	}

	/// <summary>Copies the contents of the deque into an array</summary>
	/// <param name="array">Array the contents of the deque will be copied into</param>
	/// <param name="index">Index at which writing into the array will begin</param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (!(array is ItemType[]))
		{
			throw new ArgumentException("Incompatible array type", "array");
		}
		CopyTo((ItemType[])array, index);
	}

	/// <summary>Removes all items from the deque</summary>
	public void Clear()
	{
		if (blocks.Count > 1)
		{
			for (int i = firstBlockStartIndex; i < blockSize; i++)
			{
				blocks[0][i] = default(ItemType);
			}
			blocks.RemoveRange(1, blocks.Count - 1);
		}
		else
		{
			for (int j = firstBlockStartIndex; j < lastBlockEndIndex; j++)
			{
				blocks[0][j] = default(ItemType);
			}
		}
		firstBlockStartIndex = 0;
		lastBlockEndIndex = 0;
		count = 0;
	}

	/// <summary>Removes the specified item from the deque</summary>
	/// <param name="item">Item that will be removed from the deque</param>
	/// <returns>True if the item was found and removed</returns>
	public bool Remove(ItemType item)
	{
		int num = IndexOf(item);
		if (num == -1)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	/// <summary>Removes the first item in the double-ended queue</summary>
	public void RemoveFirst()
	{
		if (count == 0)
		{
			throw new InvalidOperationException("Cannot remove items from empty deque");
		}
		blocks[0][firstBlockStartIndex] = default(ItemType);
		firstBlockStartIndex++;
		if (firstBlockStartIndex >= blockSize)
		{
			if (count > 1)
			{
				blocks.RemoveAt(0);
				firstBlockStartIndex = 0;
			}
			else
			{
				firstBlockStartIndex = 0;
				lastBlockEndIndex = 0;
			}
		}
		count--;
	}

	/// <summary>Removes the last item in the double-ended queue</summary>
	public void RemoveLast()
	{
		if (count == 0)
		{
			throw new InvalidOperationException("Cannot remove items from empty deque");
		}
		int index = blocks.Count - 1;
		blocks[index][lastBlockEndIndex - 1] = default(ItemType);
		lastBlockEndIndex--;
		if (lastBlockEndIndex == 0)
		{
			if (count > 1)
			{
				blocks.RemoveAt(index);
				lastBlockEndIndex = blockSize;
			}
			else
			{
				firstBlockStartIndex = 0;
				lastBlockEndIndex = 0;
			}
		}
		count--;
	}

	/// <summary>Removes the item at the specified index</summary>
	/// <param name="index">Index of the item that will be removed</param>
	public void RemoveAt(int index)
	{
		int num = count - index;
		if (index < num)
		{
			removeFromLeft(index);
		}
		else
		{
			removeFromRight(index);
		}
	}

	/// <summary>
	///   Removes an item from the left side of the queue by shifting all items that
	///   come before it to the right by one
	/// </summary>
	/// <param name="index">Index of the item that will be removed</param>
	private void removeFromLeft(int index)
	{
		if (index == 0)
		{
			RemoveFirst();
			return;
		}
		findIndex(index, out var blockIndex, out var subIndex);
		int num = 0;
		int num3;
		if (blockIndex > num)
		{
			Array.Copy(blocks[blockIndex], 0, blocks[blockIndex], 1, subIndex);
			blocks[blockIndex][0] = blocks[blockIndex - 1][blockSize - 1];
			for (int num2 = blockIndex - 1; num2 > num; num2--)
			{
				Array.Copy(blocks[num2], 0, blocks[num2], 1, blockSize - 1);
				blocks[num2][0] = blocks[num2 - 1][blockSize - 1];
			}
			num3 = blockSize - 1;
		}
		else
		{
			num3 = subIndex;
		}
		Array.Copy(blocks[num], firstBlockStartIndex, blocks[num], firstBlockStartIndex + 1, num3 - firstBlockStartIndex);
		if (firstBlockStartIndex == blockSize - 1)
		{
			blocks.RemoveAt(0);
			firstBlockStartIndex = 0;
		}
		else
		{
			blocks[0][firstBlockStartIndex] = default(ItemType);
			firstBlockStartIndex++;
		}
		count--;
	}

	/// <summary>
	///   Removes an item from the right side of the queue by shifting all items that
	///   come after it to the left by one
	/// </summary>
	/// <param name="index">Index of the item that will be removed</param>
	private void removeFromRight(int index)
	{
		if (index == count - 1)
		{
			RemoveLast();
			return;
		}
		findIndex(index, out var blockIndex, out var subIndex);
		int num = blocks.Count - 1;
		int num2;
		if (blockIndex < num)
		{
			Array.Copy(blocks[blockIndex], subIndex + 1, blocks[blockIndex], subIndex, blockSize - subIndex - 1);
			blocks[blockIndex][blockSize - 1] = blocks[blockIndex + 1][0];
			for (int i = blockIndex + 1; i < num; i++)
			{
				Array.Copy(blocks[i], 1, blocks[i], 0, blockSize - 1);
				blocks[i][blockSize - 1] = blocks[i + 1][0];
			}
			num2 = 0;
		}
		else
		{
			num2 = subIndex;
		}
		Array.Copy(blocks[num], num2 + 1, blocks[num], num2, lastBlockEndIndex - num2 - 1);
		if (lastBlockEndIndex == 1)
		{
			blocks.RemoveAt(num);
			lastBlockEndIndex = blockSize;
		}
		else
		{
			blocks[num][lastBlockEndIndex - 1] = default(ItemType);
			lastBlockEndIndex--;
		}
		count--;
	}

	/// <summary>
	///   Determines the index of the first occurence of the specified item in the deque
	/// </summary>
	/// <param name="item">Item that will be located in the deque</param>
	/// <returns>The index of the item or -1 if it wasn't found</returns>
	public int IndexOf(ItemType item)
	{
		if (blocks.Count == 1)
		{
			int num = lastBlockEndIndex - firstBlockStartIndex;
			int num2 = Array.IndexOf(blocks[0], item, firstBlockStartIndex, num);
			if (num2 != -1)
			{
				return num2 - firstBlockStartIndex;
			}
			return -1;
		}
		int num3 = blockSize - firstBlockStartIndex;
		int num4 = Array.IndexOf(blocks[0], item, firstBlockStartIndex, num3);
		if (num4 != -1)
		{
			return num4 - firstBlockStartIndex;
		}
		int num5 = blocks.Count - 1;
		for (int i = 1; i < num5; i++)
		{
			num4 = Array.IndexOf(blocks[i], item, 0, blockSize);
			if (num4 != -1)
			{
				return num4 - firstBlockStartIndex + i * blockSize;
			}
		}
		num4 = Array.IndexOf(blocks[num5], item, 0, lastBlockEndIndex);
		if (num4 == -1)
		{
			return -1;
		}
		return num4 - firstBlockStartIndex + num5 * blockSize;
	}
}
