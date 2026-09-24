using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Nuclex.Support.Collections;

/// <summary>Collection of weakly referenced objects</summary>
/// <remarks>
///   This collection tries to expose the interface of a normal collection, but stores
///   objects as weak references. When an object is accessed, it can return null.
///   when the collection detects that one of its items was garbage collected, it
///   will silently remove that item.
/// </remarks>
public class WeakCollection<ItemType> : IList<ItemType>, ICollection<ItemType>, IEnumerable<ItemType>, IList, ICollection, IEnumerable where ItemType : class
{
	/// <summary>
	///   An enumerator that unpacks the items returned by an enumerator of the
	///   weak reference collection into the actual item type on-the-fly.
	/// </summary>
	private class UnpackingEnumerator : IEnumerator<ItemType>, IDisposable, IEnumerator
	{
		/// <summary>An enumerator from the wrapped collection</summary>
		private IEnumerator<WeakReference<ItemType>> containedTypeEnumerator;

		/// <summary>
		///   The element in the collection at the current position of the enumerator.
		/// </summary>
		public ItemType Current => containedTypeEnumerator.Current.Target;

		/// <summary>The current element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">
		///   The enumerator is positioned before the first element of the collection
		///   or after the last element.
		/// </exception>
		object IEnumerator.Current => Current;

		/// <summary>Initializes a new unpacking enumerator</summary>
		/// <param name="containedTypeEnumerator">
		///   Enumerator of the weak reference collection
		/// </param>
		public UnpackingEnumerator(IEnumerator<WeakReference<ItemType>> containedTypeEnumerator)
		{
			this.containedTypeEnumerator = containedTypeEnumerator;
		}

		/// <summary>Immediately releases all resources used by the instance</summary>
		public void Dispose()
		{
			containedTypeEnumerator.Dispose();
		}

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">
		///   The enumerator is positioned before the first element of the collection
		///   or after the last element.
		/// </exception>
		public bool MoveNext()
		{
			return containedTypeEnumerator.MoveNext();
		}

		/// <summary>
		///   Sets the enumerator to its initial position, which is before the first element
		///   in the collection.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">
		///   The collection was modified after the enumerator was created.
		/// </exception>
		public void Reset()
		{
			containedTypeEnumerator.Reset();
		}
	}

	/// <summary>Weak references to the items contained in the collection</summary>
	private IList<WeakReference<ItemType>> items;

	/// <summary>Used to identify and compare items in the collection</summary>
	private IEqualityComparer<ItemType> comparer;

	/// <summary>Synchronization root for threaded accesses to this collection</summary>
	private object syncRoot;

	/// <summary>
	///   The number of elements contained in the WeakCollection instance
	/// </summary>
	public int Count => items.Count;

	/// <summary>Gets the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <returns>The element at the specified index.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///    Index is less than zero or index is equal to or greater than
	///    WeakCollection.Count.
	/// </exception>
	public ItemType this[int index]
	{
		get
		{
			return items[index].Target;
		}
		set
		{
			items[index] = new WeakReference<ItemType>(value);
		}
	}

	/// <summary>Whether the List is write-protected</summary>
	public bool IsReadOnly => items.IsReadOnly;

	/// <summary>
	///   A value indicating whether the WeakCollection has a fixed size.
	/// </summary>
	bool IList.IsFixedSize => (items as IList).IsFixedSize;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <returns>The element at the specified index</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the WeakCollection
	/// </exception>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			ItemType value2 = downcastToItemType(value);
			this[index] = value2;
		}
	}

	/// <summary>
	///   A value indicating whether access to the WeakCollection is
	///   synchronized (thread safe).
	/// </summary>
	bool ICollection.IsSynchronized => false;

	/// <summary>
	///   An object that can be used to synchronize access to the WeakCollection.
	/// </summary>
	object ICollection.SyncRoot
	{
		get
		{
			if (syncRoot == null)
			{
				if (items is ICollection collection)
				{
					syncRoot = collection.SyncRoot;
				}
				else
				{
					Interlocked.CompareExchange(ref syncRoot, new object(), null);
				}
			}
			return syncRoot;
		}
	}

	/// <summary>Initializes a new weak reference collection</summary>
	/// <param name="items">
	///   Internal list of weak references that are unpacking when accessed through
	///   the WeakCollection's interface.
	/// </param>
	public WeakCollection(IList<WeakReference<ItemType>> items)
		: this(items, (IEqualityComparer<ItemType>)EqualityComparer<ItemType>.Default)
	{
	}

	/// <summary>Initializes a new weak reference collection</summary>
	/// <param name="items">
	///   Internal list of weak references that are unpacking when accessed through
	///   the WeakCollection's interface.
	/// </param>
	/// <param name="comparer">
	///   Comparer used to identify and compare items to each other
	/// </param>
	public WeakCollection(IList<WeakReference<ItemType>> items, IEqualityComparer<ItemType> comparer)
	{
		this.items = items;
		this.comparer = comparer;
	}

	/// <summary>
	///   Determines whether an element is in the WeakCollection
	/// </summary>
	/// <param name="item">
	///   The object to locate in the WeakCollection. The value can be null.
	/// </param>
	/// <returns>
	///   True if value is found in the WeakCollection; otherwise, false.
	/// </returns>
	/// <remarks>
	///   The default implementation of this method is very unoptimized and will
	///   enumerate all the items in the collection, transforming one after another
	///   to check whether the transformed item matches the item the user was
	///   looking for. It is recommended to provide a custom implementation of
	///   this method, if possible.
	/// </remarks>
	public virtual bool Contains(ItemType item)
	{
		return IndexOf(item) != -1;
	}

	/// <summary>
	///   Copies the entire WeakCollection to a compatible one-dimensional
	///   System.Array, starting at the specified index of the target array.
	/// </summary>
	/// <param name="array">
	///   The one-dimensional System.Array that is the destination of the elements copied
	///   from the WeakCollection. The System.Array must have zero-based indexing.
	/// </param>
	/// <param name="index">
	///   The zero-based index in array at which copying begins.
	/// </param>
	/// <exception cref="T:System.ArgumentException">
	///   Index is equal to or greater than the length of array or the number of elements
	///   in the source WeakCollection is greater than the available space from index to
	///   the end of the destination array.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is less than zero.
	/// </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   Array is null.
	/// </exception>
	public void CopyTo(ItemType[] array, int index)
	{
		if (items.Count > array.Length - index)
		{
			throw new ArgumentException("Array too small to fit the collection items starting at the specified index");
		}
		for (int i = 0; i < items.Count; i++)
		{
			array[i + index] = items[i].Target;
		}
	}

	/// <summary>Removes all items from the WeakCollection</summary>
	public void Clear()
	{
		items.Clear();
	}

	/// <summary>
	///   Returns an enumerator that iterates through the WeakCollection.
	/// </summary>
	/// <returns>An enumerator or the WeakCollection.</returns>
	public IEnumerator<ItemType> GetEnumerator()
	{
		return new UnpackingEnumerator(items.GetEnumerator());
	}

	/// <summary>
	///   Searches for the specified object and returns the zero-based index of the
	///   first occurrence within the entire WeakCollection.
	/// </summary>
	/// <param name="item">
	///   The object to locate in the WeakCollection. The value can
	///   be null for reference types.
	/// </param>
	/// <returns>
	///   The zero-based index of the first occurrence of item within the entire
	///   WeakCollection, if found; otherwise, -1.
	/// </returns>
	/// <remarks>
	///   The default implementation of this method is very unoptimized and will
	///   enumerate all the items in the collection, transforming one after another
	///   to check whether the transformed item matches the item the user was
	///   looking for. It is recommended to provide a custom implementation of
	///   this method, if possible.
	/// </remarks>
	public int IndexOf(ItemType item)
	{
		for (int i = 0; i < items.Count; i++)
		{
			ItemType target = items[i].Target;
			if (target == null || item == null)
			{
				if (object.ReferenceEquals(item, target))
				{
					return i;
				}
			}
			else if (comparer.Equals(target, item))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	///   Removes the first occurrence of a specific object from the WeakCollection.
	/// </summary>
	/// <param name="item">The object to remove from the WeakCollection</param>
	/// <returns>
	///   True if item was successfully removed from the WeakCollection; otherwise, false.
	/// </returns>
	public bool Remove(ItemType item)
	{
		for (int i = 0; i < items.Count; i++)
		{
			ItemType target = items[i].Target;
			if (target == null || item == null)
			{
				if (object.ReferenceEquals(item, target))
				{
					items.RemoveAt(i);
					return true;
				}
			}
			else if (comparer.Equals(item, target))
			{
				items.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	/// <summary>Adds an item to the WeakCollection.</summary>
	/// <param name="item">The object to add to the WeakCollection</param>
	public void Add(ItemType item)
	{
		items.Add(new WeakReference<ItemType>(item));
	}

	/// <summary>Inserts an item to the WeakCollection at the specified index.</summary>
	/// <param name="index">
	///   The zero-based index at which item should be inserted.
	/// </param>
	/// <param name="item">The object to insert into the WeakCollection</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   index is not a valid index in the WeakCollection.
	/// </exception>
	public void Insert(int index, ItemType item)
	{
		items.Insert(index, new WeakReference<ItemType>(item));
	}

	/// <summary>
	///   Removes the WeakCollection item at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the WeakCollection.
	/// </exception>
	public void RemoveAt(int index)
	{
		items.RemoveAt(index);
	}

	/// <summary>
	///   Removes the items that have been garbage collected from the collection
	/// </summary>
	public void RemoveDeadItems()
	{
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].IsAlive)
			{
				items[num] = items[i];
				num++;
			}
		}
		while (items.Count > num)
		{
			items.RemoveAt(items.Count - 1);
		}
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>
	///   A System.Collections.IEnumerator object that can be used to iterate through
	///   the collection.
	/// </returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Adds an item to the WeakCollection.</summary>
	/// <param name="value">The System.Object to add to the WeakCollection.</param>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <exception cref="T:System.NotSupportedException">
	///   The System.Collections.IList is read-only or the WeakCollection has a fixed size.
	/// </exception>
	int IList.Add(object value)
	{
		ItemType target = downcastToItemType(value);
		return (items as IList).Add(new WeakReference<ItemType>(target));
	}

	/// <summary>
	///   Determines whether the WeakCollection contains a specific value.
	/// </summary>
	/// <param name="value">The System.Object to locate in the WeakCollection.</param>
	/// <returns>
	///   True if the System.Object is found in the WeakCollection; otherwise, false.
	/// </returns>
	bool IList.Contains(object value)
	{
		ItemType item = downcastToItemType(value);
		return Contains(item);
	}

	/// <summary>Determines the index of a specific item in the WeakCollection.</summary>
	/// <param name="value">The System.Object to locate in the WeakCollection.</param>
	/// <returns>
	///   The index of value if found in the list; otherwise, -1.
	/// </returns>
	int IList.IndexOf(object value)
	{
		ItemType item = downcastToItemType(value);
		return IndexOf(item);
	}

	/// <summary>
	///   Inserts an item to the WeakCollection at the specified index.
	/// </summary>
	/// <param name="index">
	///   The zero-based index at which value should be inserted.
	/// </param>
	/// <param name="value">The System.Object to insert into the WeakCollection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the TransformingReadOnlyCollection.
	/// </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   The System.Collections.IList is read-only or the WeakCollection has a fixed size.
	/// </exception>
	/// <exception cref="T:System.NullReferenceException">
	///   Value is null reference in the WeakCollection.
	/// </exception>
	void IList.Insert(int index, object value)
	{
		ItemType item = downcastToItemType(value);
		Insert(index, item);
	}

	/// <summary>
	///   Removes the first occurrence of a specific object from the WeakCollection.
	/// </summary>
	/// <param name="value">The System.Object to remove from the WeakCollection.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   The WeakCollection is read-only or the WeakCollection has a fixed size.
	/// </exception>
	void IList.Remove(object value)
	{
		ItemType item = downcastToItemType(value);
		Remove(item);
	}

	/// <summary>
	///   Copies the elements of the WeakCollection to an System.Array, starting at
	///   a particular System.Array index.
	/// </summary>
	/// <param name="array">
	///   The one-dimensional System.Array that is the destination of the elements
	///   copied from WeakCollection. The System.Array must have zero-based indexing.
	/// </param>
	/// <param name="index">The zero-based index in array at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   Array is null.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is less than zero.
	/// </exception>
	/// <exception cref="T:System.ArgumentException">
	///   Array is multidimensional or index is equal to or greater than the length
	///   of array or the number of elements in the source WeakCollection is greater than
	///   the available space from index to the end of the destination array.
	/// </exception>
	/// <exception cref="T:System.InvalidCastException">
	///   The type of the source WeakCollection cannot be cast automatically to the type of
	///   the destination array.
	/// </exception>
	void ICollection.CopyTo(Array array, int index)
	{
		CopyTo((ItemType[])array, index);
	}

	/// <summary>
	///   Downcasts an object reference to a reference to the collection's item type
	/// </summary>
	/// <param name="value">Object reference that will be downcast</param>
	/// <returns>
	///   The specified object referecne as a reference to the collection's item type
	/// </returns>
	private static ItemType downcastToItemType(object value)
	{
		ItemType val = value as ItemType;
		if (!object.ReferenceEquals(value, null) && val == null)
		{
			throw new ArgumentException("Object is not of a compatible type", "value");
		}
		return val;
	}
}
