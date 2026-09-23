namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// This is an internal structure.
	/// </summary>
	public struct TimeStep
	{
		/// <summary>
		/// Time step (Delta time)
		/// </summary>
		public float dt;

		/// <summary>
		/// dt * inv_dt0
		/// </summary>
		public float dtRatio;

		/// <summary>
		/// Inverse time step (0 if dt == 0).
		/// </summary>
		public float inv_dt;
	}
}
