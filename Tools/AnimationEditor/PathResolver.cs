using System;
using System.IO;
using System.Linq;

namespace AnimationTest;

/// <summary>
/// Finds files regardless of name case. Projects saved on Windows refer to
/// files with whatever case was typed (e.g. "humanskeleton.xml" for
/// "humanSkeleton.xml"), which only matches on case-insensitive filesystems.
/// </summary>
internal static class PathResolver
{
	/// <summary>
	/// Returns the existing path whose components match <paramref name="path"/>
	/// ignoring case, or <paramref name="path"/> unchanged if none does (so new
	/// files are created where requested).
	/// </summary>
	public static string Resolve(string path)
	{
		if (string.IsNullOrEmpty(path) || File.Exists(path) || Directory.Exists(path))
		{
			return path;
		}
		string full = Path.GetFullPath(path);
		string root = Path.GetPathRoot(full);
		string current = root;
		string[] parts = full.Substring(root.Length).Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < parts.Length; i++)
		{
			string exact = Path.Combine(current, parts[i]);
			if (File.Exists(exact) || Directory.Exists(exact))
			{
				current = exact;
				continue;
			}
			string match = Directory.Exists(current)
				? Directory.EnumerateFileSystemEntries(current).FirstOrDefault(entry => string.Equals(Path.GetFileName(entry), parts[i], StringComparison.OrdinalIgnoreCase))
				: null;
			if (match == null)
			{
				return path;
			}
			current = match;
		}
		return current;
	}
}
