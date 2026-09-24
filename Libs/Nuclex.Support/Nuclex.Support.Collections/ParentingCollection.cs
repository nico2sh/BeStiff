using System;
using System.Collections.ObjectModel;

namespace Nuclex.Support.Collections;

/// <summary>Collection that automatically assigns an owner to all its elements</summary>
/// <remarks>
///   This collection automatically assigns a parent object to elements that
///   are managed in it. The elements have to derive from the Parentable&lt;&gt;
///   base class.
/// </remarks>
/// <typeparam name="ParentType">Type of the parent object to assign to items</typeparam>
/// <typeparam name="ItemType">Type of the items being managed in the collection</typeparam>
public class ParentingCollection<ParentType, ItemType> : Collection<ItemType> where ItemType : Parentable<ParentType>
{
	/// <summary>Parent this collection currently belongs to</summary>
	private ParentType parent;

	/// <summary>Reparents all elements in the collection</summary>
	/// <param name="parent">New parent to take ownership of the items</param>
	protected void Reparent(ParentType parent)
	{
		this.parent = parent;
		for (int i = 0; i < base.Count; i++)
		{
			ItemType val = base[i];
			val.SetParent(parent);
		}
	}

	/// <summary>Clears all elements from the collection</summary>
	protected override void ClearItems()
	{
		for (int i = 0; i < base.Count; i++)
		{
			ItemType val = base[i];
			val.SetParent(default(ParentType));
		}
		base.ClearItems();
	}

	/// <summary>Inserts a new element into the collection</summary>
	/// <param name="index">Index at which to insert the element</param>
	/// <param name="item">Item to be inserted</param>
	protected override void InsertItem(int index, ItemType item)
	{
		base.InsertItem(index, item);
		item.SetParent(parent);
	}

	/// <summary>Removes an element from the collection</summary>
	/// <param name="index">Index of the element to remove</param>
	protected override void RemoveItem(int index)
	{
		ItemType val = base[index];
		val.SetParent(default(ParentType));
		base.RemoveItem(index);
	}

	/// <summary>Takes over a new element that is directly assigned</summary>
	/// <param name="index">Index of the element that was assigned</param>
	/// <param name="item">New item</param>
	protected override void SetItem(int index, ItemType item)
	{
		ItemType val = base[index];
		val.SetParent(default(ParentType));
		base.SetItem(index, item);
		item.SetParent(parent);
	}

	/// <summary>Disposes all items contained in the collection</summary>
	/// <remarks>
	///   <para>
	///     This method is intended to support collections that need to dispose their
	///     items. It will unparent all of the collection's items and call Dispose()
	///     on any item that implements IDisposable.
	///   </para>
	///   <para>
	///     Do not call this method from your destructor as it will access the
	///     contained items in order to unparent and to Dispose() them, which leads
	///     to undefined behavior since the object might have already been collected
	///     by the GC. Call it only if your object is being manually disposed.
	///   </para>
	/// </remarks>
	protected void DisposeItems()
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			if (base[num] is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		base.ClearItems();
	}
}
