using System;

namespace FarseerPhysics.Dynamics.Contacts
{
	[Flags]
	public enum ContactFlags
	{
		None = 0,
		/// <summary>
		/// Used when crawling contact graph when forming islands.
		/// </summary>
		Island = 1,
		/// <summary>
		/// Set when the shapes are touching.
		/// </summary>
		Touching = 2,
		/// <summary>
		/// This contact can be disabled (by user)
		/// </summary>
		Enabled = 4,
		/// <summary>
		/// This contact needs filtering because a fixture filter was changed.
		/// </summary>
		Filter = 8,
		/// <summary>
		/// This bullet contact had a TOI event
		/// </summary>
		BulletHit = 0x10,
		/// <summary>
		/// This contact has a valid TOI i the field TOI
		/// </summary>
		TOI = 0x20
	}
}
