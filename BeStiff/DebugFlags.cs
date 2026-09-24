using System;

namespace Be_Stiff
{
	/// <summary>
	/// Debug switches (BESTIFF_* environment variables, documented in the
	/// README), read once at startup so per-frame code can test them cheaply.
	/// </summary>
	internal static class DebugFlags
	{
		public static readonly bool Dump = IsSet("BESTIFF_DUMP");

		public static readonly string Level = Environment.GetEnvironmentVariable("BESTIFF_LEVEL");

		public static readonly string HeroPos = Environment.GetEnvironmentVariable("BESTIFF_HERO_POS");

		public static readonly string Keys = Environment.GetEnvironmentVariable("BESTIFF_KEYS");

		public static readonly string Shot = Environment.GetEnvironmentVariable("BESTIFF_SHOT");

		public static readonly int ShotFrame = GetInt("BESTIFF_SHOT_FRAME") is int frame && frame > 0 ? frame : 180;

		public static readonly bool NoCannon = IsSet("BESTIFF_NO_CANNON");

		public static readonly bool StateLog = IsSet("BESTIFF_STATE_LOG");

		public static readonly bool DeathLog = IsSet("BESTIFF_DEATH_LOG");

		public static readonly bool GirderLog = IsSet("BESTIFF_GIRDER_LOG");

		public static readonly int JumpFrame = GetInt("BESTIFF_JUMP_FRAME") ?? 0;

		public static readonly int KillFrame = GetInt("BESTIFF_KILL_FRAME") ?? 0;

		public static readonly int KillEnemyFrame = GetInt("BESTIFF_KILL_ENEMY_FRAME") ?? 0;

		private static bool IsSet(string name)
		{
			return Environment.GetEnvironmentVariable(name) != null;
		}

		private static int? GetInt(string name)
		{
			return int.TryParse(Environment.GetEnvironmentVariable(name), out int value) ? value : null;
		}
	}
}
