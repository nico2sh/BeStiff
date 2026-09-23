using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Be_Stiff
{
	/// <summary>
	/// Debug input scripting. BESTIFF_KEYS="100-150:D;200-230:Space,A" holds
	/// the listed keys during the given frame ranges (frames counted from the
	/// first Update). Used to reproduce input sequences unattended.
	/// </summary>
	internal static class DebugKeys
	{
		private static readonly List<(int start, int end, Keys[] keys)> schedule = Load();
		private static int frame;

		private static List<(int, int, Keys[])> Load()
		{
			var list = new List<(int, int, Keys[])>();
			string spec = Environment.GetEnvironmentVariable("BESTIFF_KEYS");
			if (string.IsNullOrEmpty(spec))
			{
				return list;
			}
			foreach (string entry in spec.Split(';'))
			{
				string[] parts = entry.Split(':');
				string[] range = parts[0].Split('-');
				var keys = new List<Keys>();
				foreach (string k in parts[1].Split(','))
				{
					keys.Add((Keys)Enum.Parse(typeof(Keys), k.Trim(), true));
				}
				list.Add((int.Parse(range[0]), int.Parse(range[1]), keys.ToArray()));
			}
			return list;
		}

		public static KeyboardState Merge(KeyboardState real)
		{
			if (schedule.Count == 0)
			{
				return real;
			}
			frame++;
			var pressed = new List<Keys>(real.GetPressedKeys());
			foreach (var (start, end, keys) in schedule)
			{
				if (frame >= start && frame <= end)
				{
					foreach (Keys k in keys)
					{
						if (!pressed.Contains(k)) pressed.Add(k);
					}
				}
			}
			return new KeyboardState(pressed.ToArray());
		}
	}
}
