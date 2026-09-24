using System;
using System.Runtime.Serialization;

namespace Nuclex.Support.Scheduling;

/// <summary>Indicates that an operation has been forcefully aborted</summary>
/// <remarks>
///   This exception is the typical result of using AsyncAbort() on a running
///   background process. 
/// </remarks>
[Serializable]
public class AbortedException : Exception
{
	/// <summary>Initializes the exception</summary>
	public AbortedException()
	{
	}

	/// <summary>Initializes the exception with an error message</summary>
	/// <param name="message">Error message describing the cause of the exception</param>
	public AbortedException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes the exception as a followup exception</summary>
	/// <param name="message">Error message describing the cause of the exception</param>
	/// <param name="inner">Preceding exception that has caused this exception</param>
	public AbortedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	/// <summary>Initializes the exception from its serialized state</summary>
	/// <param name="info">Contains the serialized fields of the exception</param>
	/// <param name="context">Additional environmental informations</param>
	protected AbortedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
