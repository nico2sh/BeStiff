namespace Nuclex.Support.Scheduling;

/// <summary>Interface for abortable processes</summary>
public interface IAbortable
{
	/// <summary>Aborts the running process. Can be called from any thread.</summary>
	/// <remarks>
	///   The receive should honor the abort request and stop whatever it is
	///   doing as soon as possible. The method does not impose any requirement
	///   on the timeliness of the reaction of the running process, but implementers
	///   are advised to not ignore the abort request and urged to try and design
	///   their code in such a way that it can be stopped in a reasonable time
	///   (eg. within 1 second of the abort request).
	/// </remarks>
	void AsyncAbort();
}
