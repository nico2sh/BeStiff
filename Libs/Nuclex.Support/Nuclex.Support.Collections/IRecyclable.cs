namespace Nuclex.Support.Collections;

/// <summary>Allows an object to be returned to its initial state</summary>
/// <remarks>
///   <para>
///     This interface is typically implemented by objects which can be recycled
///     in order to avoid the construction overhead of a heavyweight class and to
///     eliminate garbage by reusing instances.
///   </para>
///   <para>
///     Recyclable objects should have a parameterless constructor and calling
///     their Recycle() method should bring them back into the state they were
///     in right after they had been constructed.
///   </para>
/// </remarks>
public interface IRecyclable
{
	/// <summary>Returns the object to its initial state</summary>
	void Recycle();
}
