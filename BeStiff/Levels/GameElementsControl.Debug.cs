using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using Krypton;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OgmoXNA;

namespace Be_Stiff.Levels
{
	internal static partial class GameElementsControl
	{
		private static bool nanReported;

		// Debug switches, read once instead of on every physics frame.
		private static readonly bool DebugDeathLog = Environment.GetEnvironmentVariable("BESTIFF_DEATH_LOG") != null;

		private static readonly bool DebugGirderLog = Environment.GetEnvironmentVariable("BESTIFF_GIRDER_LOG") != null;

		private static readonly int DebugJumpFrame = int.TryParse(Environment.GetEnvironmentVariable("BESTIFF_JUMP_FRAME"), out int f) ? f : 0;

		private static readonly int DebugKillFrame = int.TryParse(Environment.GetEnvironmentVariable("BESTIFF_KILL_FRAME"), out int f) ? f : 0;

		private static readonly int DebugKillEnemyFrame = int.TryParse(Environment.GetEnvironmentVariable("BESTIFF_KILL_ENEMY_FRAME"), out int f) ? f : 0;
		private static int physicsFrames;

		/// <summary>Debug helper: describes the hero's state.</summary>
		public static string DebugHeroState()
		{
			if (hero == null) return "null";
			return $"disposed={hero.Disposed} dead={hero.IsDead()} pos={hero.Position} camera={Camera.Position}";
		}

		/// <summary>Debug helper: saves the intermediate render targets as PNGs.</summary>
		public static void DebugSaveRenderTargets(string prefix)
		{
			var targets = new (string, Microsoft.Xna.Framework.Graphics.Texture2D)[]
			{
				("lightmap", krypton != null ? krypton.mMap : null),
				("hero", renderTargetHero),
				("objects", renderTargetObjects),
				("background", renderTargetBackground),
				("noises", renderTargetNoises),
			};
			foreach (var (name, tex) in targets)
			{
				if (tex == null) continue;
				using (var fs = System.IO.File.Create(prefix + "_" + name + ".png"))
				{
					tex.SaveAsPng(fs, tex.Width, tex.Height);
				}
			}
		}
	}
}
