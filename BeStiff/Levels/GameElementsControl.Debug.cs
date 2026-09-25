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
				("sightmap", krypton != null ? krypton.SightMap : null),
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
