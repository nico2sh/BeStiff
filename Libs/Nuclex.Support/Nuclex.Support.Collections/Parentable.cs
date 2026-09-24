namespace Nuclex.Support.Collections;

/// <summary>Base class for objects that can be parented to an owner</summary>
/// <typeparam name="ParentType">Type of the parent object</typeparam>
public class Parentable<ParentType>
{
	/// <summary>Current parent of this object</summary>
	private ParentType parent;

	/// <summary>The parent object that owns this instance</summary>
	protected ParentType Parent => parent;

	/// <summary>Invoked whenever the instance's owner changes</summary>
	/// <remarks>
	///   When items are parented for the first time, the oldParent argument will
	///   be null. Also, if the element is removed from the collection, the
	///   current parent will be null.
	/// </remarks>
	/// <param name="oldParent">Previous owner of the instance</param>
	protected virtual void OnParentChanged(ParentType oldParent)
	{
	}

	/// <summary>Assigns a new parent to this instance</summary>
	internal void SetParent(ParentType parent)
	{
		ParentType oldParent = this.parent;
		this.parent = parent;
		OnParentChanged(oldParent);
	}
}
