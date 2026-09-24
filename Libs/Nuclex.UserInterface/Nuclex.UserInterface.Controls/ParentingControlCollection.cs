using System;
using System.Collections.ObjectModel;

namespace Nuclex.UserInterface.Controls;

/// <summary>Collection of GUI controls</summary>
/// <remarks>
///   This class is for internal use only. Do not expose it to the user. If it was
///   exposed, the user might decide to use it for storing his own controls, causing
///   exceptions because the collection tries to parent the controls which are already
///   belonging to another collection.
/// </remarks>
internal class ParentingControlCollection : Collection<Control>
{
	/// <summary>GUI this control is currently assigned to. Can be null.</summary>
	private Screen screen;

	/// <summary>Parent control to assign to all controls in this collection.</summary>
	private Control parent;

	/// <summary>Initializes a new parenting control collection</summary>
	/// <param name="parent">Parent control to assign to all children</param>
	public ParentingControlCollection(Control parent)
	{
		this.parent = parent;
	}

	/// <summary>Clears all elements from the collection</summary>
	protected override void ClearItems()
	{
		for (int i = 0; i < base.Count; i++)
		{
			unassignParent(base[i]);
		}
		base.ClearItems();
	}

	/// <summary>Inserts a new element into the collection</summary>
	/// <param name="index">Index at which to insert the element</param>
	/// <param name="item">Item to be inserted</param>
	protected override void InsertItem(int index, Control item)
	{
		ensureIntegrity(item);
		base.InsertItem(index, item);
		assignParent(item);
	}

	/// <summary>Removes an element from the collection</summary>
	/// <param name="index">Index of the element to remove</param>
	protected override void RemoveItem(int index)
	{
		unassignParent(base[index]);
		base.RemoveItem(index);
	}

	/// <summary>Takes over a new element that is directly assigned</summary>
	/// <param name="index">Index of the element that was assigned</param>
	/// <param name="item">New item</param>
	protected override void SetItem(int index, Control item)
	{
		ensureIntegrity(item);
		unassignParent(base[index]);
		assignParent(item);
	}

	/// <summary>Switches the control to a specific GUI</summary>
	/// <param name="screen">Screen that owns the control from now on</param>
	internal void SetScreen(Screen screen)
	{
		this.screen = screen;
		for (int i = 0; i < base.Count; i++)
		{
			base[i].SetScreen(screen);
		}
	}

	/// <summary>
	///   Checks whether the provided name is already taken by a control
	/// </summary>
	/// <param name="name">Id that will be checked</param>
	/// <returns>True if the id is already taken, false otherwise</returns>
	internal bool IsNameTaken(string name)
	{
		if (name == null)
		{
			return false;
		}
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].Name == name)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Moves the specified control to the start of the list</summary>
	/// <param name="controlIndex">
	///   Index of the control that will be moved to the start of the list
	/// </param>
	internal void MoveToStart(int controlIndex)
	{
		Control item = base[controlIndex];
		RemoveAt(controlIndex);
		Insert(0, item);
	}

	/// <summary>Ensures the integrity of the parent/child relationships</summary>
	/// <param name="proposedChild">Control that is to become one of our childs</param>
	private void ensureIntegrity(Control proposedChild)
	{
		if (!object.ReferenceEquals(proposedChild.Parent, null))
		{
			throw new InvalidOperationException("Control already is the child of another control");
		}
		if (object.ReferenceEquals(parent, proposedChild))
		{
			throw new InvalidOperationException("Attempt to instate control as its own parent");
		}
		if (isParent(proposedChild))
		{
			throw new InvalidOperationException("Attempt to instate one of the control's parents as its child");
		}
		if (IsNameTaken(proposedChild.Name))
		{
			throw new DuplicateNameException("The name of the added control has already been taken by another child");
		}
	}

	/// <summary>
	///   Determines whether the provided control is a parent of this control.
	/// </summary>
	/// <param name="control">Control to check for parentage</param>
	/// <returns>True if the control is one of our parents, otherwise false</returns>
	/// <remarks>
	///   This method takes into account all ancestors up to the tree's root.
	/// </remarks>
	private bool isParent(Control control)
	{
		for (Control control2 = parent; control2 != null; control2 = control2.Parent)
		{
			if (object.ReferenceEquals(control2, control))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Gives up the parentage on the item provided</summary>
	/// <param name="item">Item to be unparented</param>
	private void unassignParent(Control item)
	{
		item.SetParent(null);
	}

	/// <summary>Sets up the parentage on the specified item</summary>
	/// <param name="item">Item to be parented</param>
	private void assignParent(Control item)
	{
		item.SetParent(parent);
	}
}
