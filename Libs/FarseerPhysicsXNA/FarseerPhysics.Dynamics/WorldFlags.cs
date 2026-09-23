using System;

namespace FarseerPhysics.Dynamics
{
	[Flags]
	public enum WorldFlags
	{
		/// <summary>
		/// Flag that indicates a new fixture has been added to the world.
		/// </summary>
		NewFixture = 1,
		/// <summary>
		/// Flag that clear the forces after each time step.
		/// </summary>
		ClearForces = 4,
		SubStepping = 0x10
	}
}
