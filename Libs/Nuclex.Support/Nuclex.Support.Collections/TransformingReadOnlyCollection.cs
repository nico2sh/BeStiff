using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Nuclex.Support.Collections;

/// <summary>Collection that transforms the contents of another collection.</summary>
/// <typeparam name="ContainedItemType">
///   Type of the items contained in the wrapped collection.
/// </typeparam>
/// <typeparam name="ExposedItemType">
///   Type this collection exposes its items as.
/// </typeparam>
/// <remarks>
///   <para>
///     This collection is useful if you want to expose the objects of an arbitrary
///     collection under a different type. It can be used, for example, to construct
///     wrappers for the items in a collection on-the-fly, eliminating the need to
///     manage the wrappers in parallel to the real items and improving performance
///     by only constructing a wrapper when an item is actually requested.
///   </para>
///   <para>
///     Another common use would be if you have a private collection of a non-public
///     type that's derived from some publicly visible type. By using this collection,
///     you can return the items under the publicly visible type while still having
///     your private collection under the non-public type, eliminating the need to
///     downcast each time you need to access elements of the non-public type.
///   </para>
/// </remarks>
public abstract class TransformingReadOnlyCollection<ContainedItemType, ExposedItemType> : IList<ExposedItemType>, ICollection<ExposedItemType>, IEnumerable<ExposedItemType>, IList, ICollection, IEnumerable
{
	/// <summary>
	///   An enumerator that transforms the items returned by an enumerator of the
	///   wrapped collection into the exposed type on-the-fly.
	/// </summary>
	private class TransformingEnumerator : IEnumerator<ExposedItemType>, IDisposable, IEnumerator
	{
		/// <summary>
		///   Collection that owns this enumerator; required to invoke the item
		///   transformation method.
		/// </summary>
		private TransformingReadOnlyCollection<ContainedItemType, ExposedItemType> transformer;

		/// <summary>An enumerator from the wrapped collection</summary>
		private IEnumerator<ContainedItemType> containedTypeEnumerator;

		/// <summary>
		///   The element in the collection at the current position of the enumerator.
		/// </summary>
		public ExposedItemType Current => transformer.Transform(containedTypeEnumerator.Current);

		/// <summary>The current element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">
		///   The enumerator is positioned before the first element of the collection
		///   or after the last element.
		/// </exception>
		object IEnumerator.Current => Current;

		/// <summary>Initializes a new transforming enumerator</summary>
		/// <param name="transformer">Owner; used to invoke the Transform() method</param>
		/// <param name="containedTypeEnumerator">Enumerator of the wrapped collection</param>
		public TransformingEnumerator(TransformingReadOnlyCollection<ContainedItemType, ExposedItemType> transformer, IEnumerator<ContainedItemType> containedTypeEnumerator)
		{
			this.transformer = transformer;
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

	/// <summary>Items being transformed upon exposure by this collection</summary>
	private IList<ContainedItemType> items;

	/// <summary>Synchronization root for threaded accesses to this collection</summary>
	private object syncRoot;

	/// <summary>
	///   The number of elements contained in the TransformingReadOnlyCollection instance
	/// </summary>
	public int Count => items.Count;

	/// <summary>Gets the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <returns>The element at the specified index.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///    Index is less than zero or index is equal to or greater than
	///    TransformingReadOnlyCollection.Count.
	/// </exception>
	public ExposedItemType this[int index] => Transform(items[index]);

	/// <summary>Whether the List is write-protected</summary>
	public bool IsReadOnly => true;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <returns>The element at the specified index.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the TransformingReadOnlyCollection.
	/// </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   The property is set and the TransformingReadOnlyCollection is read-only
	/// </exception>
	ExposedItemType IList<ExposedItemType>.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new NotSupportedException("The collection is ready-only");
		}
	}

	/// <summary>
	///   A value indicating whether the TransformingReadOnlyCollection has a fixed
	///   size.
	/// </summary>
	bool IList.IsFixedSize => true;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <returns>The element at the specified index</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the TransformingReadOnlyCollection
	/// </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   The property is set and the TransformingReadOnlyCollection is read-only.
	/// </exception>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new NotSupportedException("The collection is ready-only");
		}
	}

	/// <summary>
	///   The number of elements contained in the TransformingReadOnlyCollection.
	/// </summary>
	int ICollection.Count => Count;

	/// <summary>
	///   A value indicating whether access to the TransformingReadOnlyCollection
	///   is synchronized (thread safe).
	/// </summary>
	bool ICollection.IsSynchronized => false;

	/// <summary>
	///   An object that can be used to synchronize access to the
	///   TransformingReadOnlyCollection.
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

	/// <summary>Initializes a new transforming collection wrapper</summary>
	/// <param name="items">
	///   Internal list of items that are transformed into the exposed type when
	///   accessed through the TransformingReadOnlyCollection.
	/// </param>
	public TransformingReadOnlyCollection(IList<ContainedItemType> items)
	{
		this.items = items;
	}

	/// <summary>
	///   Determines whether an element is in the TransformingReadOnlyCollection
	/// </summary>
	/// <param name="item">
	///   The object to locate in the TransformingReadOnlyCollection.
	///   The value can be null for reference types.
	/// </param>
	/// <returns>
	///   True if value is found in the TransformingReadOnlyCollection; otherwise, false.
	/// </returns>
	/// <remarks>
	///   The default implementation of this method is very unoptimized and will
	///   enumerate all the items in the collection, transforming one after another
	///   to check whether the transformed item matches the item the user was
	///   looking for. It is recommended to provide a custom implementation of
	///   this method, if possible.
	/// </remarks>
	public virtual bool Contains(ExposedItemType item)
	{
		return IndexOf(item) != -1;
	}

	/// <summary>
	///   Copies the entire TransformingReadOnlyCollection to a compatible one-dimensional
	///   System.Array, starting at the specified index of the target array.
	/// </summary>
	/// <param name="array">
	///   The one-dimensional System.Array that is the destination of the elements copied
	///   from the TransformingReadOnlyCollection. The System.Array must have
	///   zero-based indexing.
	/// </param>
	/// <param name="index">
	///   The zero-based index in array at which copying begins.
	/// </param>
	/// <exception cref="T:System.ArgumentException">
	///   Index is equal to or greater than the length of array or the number of elements
	///   in the source TransformingReadOnlyCollection is greater than the available space
	///   from index to the end of the destination array.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is less than zero.
	/// </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   Array is null.
	/// </exception>
	public void CopyTo(ExposedItemType[] array, int index)
	{
		if (items.Count > array.Length - index)
		{
			throw new ArgumentException("Array too small to fit the collection items starting at the specified index");
		}
		for (int i = 0; i < items.Count; i++)
		{
			array[i + index] = Transform(items[i]);
		}
	}

	/// <summary>
	///   Returns an enumerator that iterates through the TransformingReadOnlyCollection.
	/// </summary>
	/// <returns>
	///   An enumerator or the TransformingReadOnlyCollection.
	/// </returns>
	public IEnumerator<ExposedItemType> GetEnumerator()
	{
		return new TransformingEnumerator(this, items.GetEnumerator());
	}

	/// <summary>
	///   Searches for the specified object and returns the zero-based index of the
	///   first occurrence within the entire TransformingReadOnlyCollection.
	/// </summary>
	/// <param name="item">
	///   The object to locate in the TransformingReadOnlyCollection. The value can
	///   be null for reference types.
	/// </param>
	/// <returns>
	///   The zero-based index of the first occurrence of item within the entire
	///   TransformingReadOnlyCollection, if found; otherwise, -1.
	/// </returns>
	/// <remarks>
	///   The default implementation of this method is very unoptimized and will
	///   enumerate all the items in the collection, transforming one after another
	///   to check whether the transformed item matches the item the user was
	///   looking for. It is recommended to provide a custom implementation of
	///   this method, if possible.
	/// </remarks>
	public int IndexOf(ExposedItemType item)
	{
		if (item == null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (Transform(items[i]) == null)
				{
					return i;
				}
			}
		}
		else
		{
			EqualityComparer<ExposedItemType> equalityComparer = EqualityComparer<ExposedItemType>.Default;
			for (int j = 0; j < items.Count; j++)
			{
				if (equalityComparer.Equals(Transform(items[j]), item))
				{
					return j;
				}
			}
		}
		return -1;
	}

	/// <summary>Transforms an item into the exposed type</summary>
	/// <param name="item">Item to be transformed</param>
	/// <returns>The transformed item</returns>
	/// <remarks>
	///   This method is used to transform an item in the wrapped collection into
	///   the exposed item type whenever the user accesses an item. Expect it to
	///   be called frequently, because the TransformingReadOnlyCollection does
	///   not cache or otherwise store the transformed items.
	/// </remarks>
	protected abstract ExposedItemType Transform(ContainedItemType item);

	/// <summary>
	///   Inserts an item to the TransformingReadOnlyCollection at the specified index.
	/// </summary>
	/// <param name="index">
	///   The zero-based index at which item should be inserted.
	/// </param>
	/// <param name="item">
	///   The object to insert into the TransformingReadOnlyCollection
	/// </param>
	/// <exception cref="T:System.NotSupportedException">
	///   The TransformingReadOnlyCollection is read-only.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   index is not a valid index in the TransformingReadOnlyCollection.
	/// </exception>
	void IList<ExposedItemType>.Insert(int index, ExposedItemType item)
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>
	///   Removes the TransformingReadOnlyCollection item at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   The TransformingReadOnlyCollection is read-only.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the TransformingReadOnlyCollection.
	/// </exception>
	void IList<ExposedItemType>.RemoveAt(int index)
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>Adds an item to the TransformingReadOnlyCollection.</summary>
	/// <param name="item">The object to add to the TransformingReadOnlyCollection</param>
	/// <exception cref="T:System.NotSupportedException">
	///   The TransformingReadOnlyCollection is read-only.
	/// </exception>
	void ICollection<ExposedItemType>.Add(ExposedItemType item)
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>Removes all items from the TransformingReadOnlyCollection</summary>
	/// <exception cref="T:System.NotSupportedException">
	///    The TransformingReadOnlyCollection is read-only.
	/// </exception>
	void ICollection<ExposedItemType>.Clear()
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>
	///   Removes the first occurrence of a specific object from the
	///   TransformingReadOnlyCollection.
	/// </summary>
	/// <param name="item">
	///   The object to remove from the TransformingReadOnlyCollection
	/// </param>
	/// <returns>
	///   True if item was successfully removed from the TransformingReadOnlyCollection;
	///   otherwise, false. This method also returns false if item is not found in the
	///   original TransformingReadOnlyCollection.
	/// </returns>
	/// <exception cref="T:System.NotSupportedException">
	///   The TransformingReadOnlyCollection is read-only.
	/// </exception>
	bool ICollection<ExposedItemType>.Remove(ExposedItemType item)
	{
		throw new NotSupportedException("The collection is ready-only");
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

	/// <summary>Adds an item to the TransformingReadOnlyCollection.</summary>
	/// <param name="value">
	///   The System.Object to add to the TransformingReadOnlyCollection.
	/// </param>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <exception cref="T:System.NotSupportedException">
	///   The System.Collections.IList is read-only or the TransformingReadOnlyCollection
	///   has a fixed size.
	/// </exception>
	int IList.Add(object value)
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>Removes all items from the TransformingReadOnlyCollection.</summary>
	/// <exception cref="T:System.NotSupportedException">
	///   The TransformingReadOnlyCollection is read-only.
	/// </exception>
	void IList.Clear()
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>
	///   Determines whether the TransformingReadOnlyCollection contains a specific value.
	/// </summary>
	/// <param name="value">
	///   The System.Object to locate in the TransformingReadOnlyCollection.
	/// </param>
	/// <returns>
	///   True if the System.Object is found in the TransformingReadOnlyCollection;
	///   otherwise, false.
	/// </returns>
	bool IList.Contains(object value)
	{
		return Contains((ExposedItemType)value);
	}

	/// <summary>
	///   Determines the index of a specific item in the TransformingReadOnlyCollection.
	/// </summary>
	/// <param name="value">
	///   The System.Object to locate in the TransformingReadOnlyCollection.
	/// </param>
	/// <returns>
	///   The index of value if found in the list; otherwise, -1.
	/// </returns>
	int IList.IndexOf(object value)
	{
		return IndexOf((ExposedItemType)value);
	}

	/// <summary>
	///   Inserts an item to the TransformingReadOnlyCollection at the specified index.
	/// </summary>
	/// <param name="index">
	///   The zero-based index at which value should be inserted.
	/// </param>
	/// <param name="value">
	///   The System.Object to insert into the TransformingReadOnlyCollection.
	/// </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the TransformingReadOnlyCollection.
	/// </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   The System.Collections.IList is read-only or the TransformingReadOnlyCollection
	///   has a fixed size.
	/// </exception>
	/// <exception cref="T:System.NullReferenceException">
	///   Value is null reference in the TransformingReadOnlyCollection.
	/// </exception>
	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>
	///   Removes the first occurrence of a specific object from the
	///   TransformingReadOnlyCollection.
	/// </summary>
	/// <param name="value">
	///   The System.Object to remove from the TransformingReadOnlyCollection.
	/// </param>
	/// <exception cref="T:System.NotSupportedException">
	///   The TransformingReadOnlyCollection is read-only or the
	///   TransformingReadOnlyCollection has a fixed size.
	/// </exception>
	void IList.Remove(object value)
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>
	///   Removes the TransformingReadOnlyCollection item at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Index is not a valid index in the TransformingReadOnlyCollection.
	/// </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   The TransformingReadOnlyCollection is read-only or the
	///   TransformingReadOnlyCollection has a fixed size.
	/// </exception>
	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException("The collection is ready-only");
	}

	/// <summary>
	///   Copies the elements of the TransformingReadOnlyCollection to an System.Array,
	///   starting at a particular System.Array index.
	/// </summary>
	/// <param name="array">
	///   The one-dimensional System.Array that is the destination of the elements
	///   copied from TransformingReadOnlyCollection. The System.Array must have zero-based
	///   indexing.
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
	///   of array or the number of elements in the source TransformingReadOnlyCollection
	///   is greater than the available space from index to the end of the destination
	///   array.
	/// </exception>
	/// <exception cref="T:System.InvalidCastException">
	///   The type of the source TransformingReadOnlyCollection cannot be cast
	///   automatically to the type of the destination array.
	/// </exception>
	void ICollection.CopyTo(Array array, int index)
	{
		CopyTo((ExposedItemType[])array, index);
	}
}
