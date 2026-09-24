using System;

namespace Nuclex.UserInterface;

/// <summary>The control's id has already been taken by another control</summary>
/// <remarks>
///   This exception indicates that you have a name collision between two controls
///   in the same collection. It will either occur when you add a control to a
///   collection that already contains a control with the same name, or when you
///   change the name of a control to that of another control in the same collection.
/// </remarks>
[Serializable]
public class DuplicateNameException : Exception
{
	/// <summary>Initializes the exception</summary>
	public DuplicateNameException()
	{
	}

	/// <summary>Initializes the exception with an error message</summary>
	/// <param name="message">Error message describing the cause of the exception</param>
	public DuplicateNameException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes the exception as a followup exception</summary>
	/// <param name="message">Error message describing the cause of the exception</param>
	/// <param name="inner">Preceding exception that has caused this exception</param>
	public DuplicateNameException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
