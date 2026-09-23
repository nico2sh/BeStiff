namespace Be_Stiff
{
	public class GameSettings
	{
		public uint DrawAABBs;

		public uint DrawCOMs;

		public uint DrawContactForces;

		public uint DrawContactNormals;

		public uint DrawContactPoints;

		public uint DrawFrictionForces;

		public uint DrawJoints;

		public uint DrawPairs;

		public uint DrawShapes;

		public uint DrawStats;

		public uint EnableContinuous;

		public uint EnableWarmStarting;

		public float Hz;

		public uint Pause;

		public int PositionIterations;

		public uint SingleStep;

		public int VelocityIterations;

		public GameSettings()
		{
			Hz = 60f;
			VelocityIterations = 8;
			PositionIterations = 3;
			DrawShapes = 1u;
			DrawJoints = 1u;
			EnableWarmStarting = 1u;
			EnableContinuous = 1u;
		}
	}
}
