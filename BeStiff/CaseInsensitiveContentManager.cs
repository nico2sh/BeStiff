using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Be_Stiff
{
	/// <summary>
	/// ContentManager that resolves asset names case-insensitively and accepts
	/// Windows-style backslash separators. The original XNA code was written
	/// against a case-insensitive file system, and its asset names do not match
	/// the on-disk casing of the compiled content.
	/// </summary>
	public class CaseInsensitiveContentManager : ContentManager
	{
		private static readonly Dictionary<string, string> Cache =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public CaseInsensitiveContentManager(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public CaseInsensitiveContentManager(IServiceProvider serviceProvider, string rootDirectory)
			: base(serviceProvider, rootDirectory)
		{
		}

		/// <summary>
		/// Loads the asset under its on-disk name, so readers that derive
		/// sibling file paths from the asset name (e.g. SongReader) see the
		/// correct casing.
		/// </summary>
		public override T Load<T>(string assetName)
		{
			return base.Load<T>(Canonical(assetName));
		}

		protected override Stream OpenStream(string assetName)
		{
			if (DebugFlags.Perf)
			{
				// Content read from disk; mid-level loads show up as hitches.
				System.Console.Error.WriteLine($"perf load frame {FramePerf.Frame}: {assetName}");
			}
			string relative = Path.Combine(RootDirectory, assetName.Replace('\\', '/')) + ".xnb";
			string resolved = Resolve(relative);
			if (resolved == null)
			{
				throw new ContentLoadException("Could not find asset: " + assetName);
			}
			return File.OpenRead(resolved);
		}

		private string Canonical(string assetName)
		{
			string normalized = assetName.Replace('\\', '/');
			string relative = Path.Combine(RootDirectory, normalized) + ".xnb";
			string resolved = Resolve(relative);
			if (resolved == null)
			{
				return normalized;
			}
			string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, RootDirectory));
			string rel = Path.GetRelativePath(root, resolved).Replace('\\', '/');
			return rel.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase) ? rel.Substring(0, rel.Length - 4) : rel;
		}

		private static string Resolve(string relative)
		{
			lock (Cache)
			{
				if (Cache.TryGetValue(relative, out string hit))
				{
					return hit;
				}
			}

			string current = AppContext.BaseDirectory;
			foreach (string part in relative.Split('/', StringSplitOptions.RemoveEmptyEntries))
			{
				if (part == ".")
				{
					continue;
				}
				string exact = Path.Combine(current, part);
				if (File.Exists(exact) || Directory.Exists(exact))
				{
					current = exact;
					continue;
				}
				string match = null;
				if (Directory.Exists(current))
				{
					foreach (string entry in Directory.EnumerateFileSystemEntries(current))
					{
						if (string.Equals(Path.GetFileName(entry), part, StringComparison.OrdinalIgnoreCase))
						{
							match = entry;
							break;
						}
					}
				}
				if (match == null)
				{
					return null;
				}
				current = match;
			}

			lock (Cache)
			{
				Cache[relative] = current;
			}
			return current;
		}
	}
}
