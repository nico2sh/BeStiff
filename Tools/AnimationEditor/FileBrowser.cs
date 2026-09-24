using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnimationTest;

/// <summary>
/// Lists folders and project files for a path being typed in a dialog, so
/// the dialog can be used to browse (the original only listed recent projects).
/// </summary>
internal static class FileBrowser
{
	private const int MaxEntries = 300;

	/// <summary>Expands a leading "~" to the home folder.</summary>
	public static string Expand(string path)
	{
		if (!string.IsNullOrEmpty(path) && (path == "~" || path.StartsWith("~/")))
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + path.Substring(1);
		}
		return path;
	}

	/// <summary>
	/// Fills <paramref name="names"/> (what the list shows) and
	/// <paramref name="targets"/> (the full path of each entry) for the folder
	/// the typed path is in, filtered by the partially typed name. Folders end
	/// in '/'; ".." goes up.
	/// </summary>
	public static void List(string typed, IList<string> names, IList<string> targets)
	{
		string path = Expand(typed);
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		string folder;
		string prefix;
		if (path.EndsWith("/") || Directory.Exists(path))
		{
			folder = path;
			prefix = "";
		}
		else
		{
			folder = Path.GetDirectoryName(path);
			prefix = Path.GetFileName(path);
		}
		if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
		{
			return;
		}
		DirectoryInfo parent = Directory.GetParent(Path.GetFullPath(folder).TrimEnd('/'));
		if (parent != null && prefix.Length == 0)
		{
			names.Add("..");
			targets.Add(parent.FullName);
		}
		try
		{
			IEnumerable<string> folders = Directory.EnumerateDirectories(folder).Where(entry => Matches(entry, prefix)).OrderBy(entry => Path.GetFileName(entry), StringComparer.OrdinalIgnoreCase);
			IEnumerable<string> files = Directory.EnumerateFiles(folder, "*.xml").Where(entry => Matches(entry, prefix)).OrderBy(entry => Path.GetFileName(entry), StringComparer.OrdinalIgnoreCase);
			foreach (string entry in folders.Take(MaxEntries))
			{
				names.Add(Path.GetFileName(entry) + "/");
				targets.Add(entry);
			}
			foreach (string entry in files.Take(MaxEntries))
			{
				names.Add(Path.GetFileName(entry));
				targets.Add(entry);
			}
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (IOException)
		{
		}
	}

	private static bool Matches(string entry, string prefix)
	{
		string name = Path.GetFileName(entry);
		return !name.StartsWith(".") && name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
	}
}
