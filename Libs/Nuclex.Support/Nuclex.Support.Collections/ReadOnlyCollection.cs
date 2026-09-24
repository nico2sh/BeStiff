using System;
using System.Collections;
using System.Collections.Generic;

namespace Nuclex.Support.Collections;

/// <summary>Wraps a Collection and prevents users from modifying it</summary>
/// <typeparam name="ItemType">Type of items to manage in the Collection</typeparam>
public class ReadOnlyCollection<ItemType> : ICollection<ItemType>, IEnumerable<ItemType>, ICollection, IEnumerable
{
	/// <summary>The wrapped Collection under its type-safe interface</summary>
	private ICollection<ItemType> typedCollection;

	/// <summary>The wrapped Collection under its object interface</summary>
	private ICollection objectCollection;

	/// <summary>The number of items current contained in the List</summary>
	public int Count => typedCollection.Count;

	/// <summary>Whether the List is write-protected</summary>
	public bool IsReadOnly => true;

	/// <summary>Whether the List is synchronized for multi-threaded usage</summary>
	bool ICollection.IsSynchronized => objectCollection.IsSynchronized;

	/// <summary>Synchronization root on which the List locks</summary>
	object ICollection.SyncRoot => objectCollection.SyncRoot;

	/// <summary>Initializes a new read-only Collection wrapper</summary>
	/// <param name="collection">Collection that will be wrapped</param>
	public ReadOnlyCollection(ICollection<ItemType> collection)
	{
		typedCollection = collection;
		objectCollection = collection as ICollection;
	}

	/// <summary>Determines whether the List contains the specified item</summary>
	/// <param name="item">Item that will be checked for</param>
	/// <returns>True if the specified item is contained in the List</returns>
	public bool Contains(ItemType item)
	{
		return typedCollection.Contains(item);
	}

	/// <summary>Copies the contents of the List into an array</summary>
	/// <param name="array">Array the List will be copied into</param>
	/// <param name="arrayIndex">
	///   Starting index at which to begin filling the destination array
	/// </param>
	public void CopyTo(ItemType[] array, int arrayIndex)
	{
		typedCollection.CopyTo(array, arrayIndex);
	}

	/// <summary>Returns a new enumerator over the contents of the List</summary>
	/// <returns>The new List contents enumerator</returns>
	public IEnumerator<ItemType> GetEnumerator()
	{
		return typedCollection.GetEnumerator();
	}

	/// <summary>Adds an item to the end of the List</summary>
	/// <param name="item">Item that will be added to the List</param>
	void ICollection<ItemType>.Add(ItemType item)
	{
		throw new NotSupportedException("Adding items is not supported by the read-only List");
	}

	/// <summary>Removes all items from the List</summary>
	void ICollection<ItemType>.Clear()
	{
		throw new NotSupportedException("Clearing is not supported by the read-only List");
	}

	/// <summary>Removes the specified item from the List</summary>
	/// <param name="item">Item that will be removed from the List</param>
	/// <returns>True of the specified item was found in the List and removed</returns>
	bool ICollection<ItemType>.Remove(ItemType item)
	{
		throw new NotSupportedException("Removing items is not supported by the read-only List");
	}

	/// <summary>Returns a new enumerator over the contents of the List</summary>
	/// <returns>The new List contents enumerator</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return objectCollection.GetEnumerator();
	}

	/// <summary>Copies the contents of the List into an array</summary>
	/// <param name="array">Array the List will be copied into</param>
	/// <param name="index">
	///   Starting index at which to begin filling the destination array
	/// </param>
	void ICollection.CopyTo(Array array, int index)
	{
		objectCollection.CopyTo(array, index);
	}
}
