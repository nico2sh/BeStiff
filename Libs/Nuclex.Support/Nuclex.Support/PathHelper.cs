using System;
using System.IO;
using System.Text;

namespace Nuclex.Support;

/// <summary>Utility class for path operations</summary>
public static class PathHelper
{
	/// <summary>Converts an absolute path into a relative one</summary>
	/// <param name="basePath">Base directory the new path should be relative to</param>
	/// <param name="absolutePath">Absolute path that will be made relative</param>
	/// <returns>
	///   A path relative to the indicated base directory that matches the
	///   absolute path given.
	/// </returns>
	public static string MakeRelative(string basePath, string absolutePath)
	{
		string[] array = basePath.Split(new char[1] { Path.DirectorySeparatorChar });
		string[] array2 = absolutePath.Split(new char[1] { Path.DirectorySeparatorChar });
		int num = -1;
		int num2 = Math.Min(array.Length, array2.Length);
		for (int i = 0; i < num2 && array2[i] == array[i]; i++)
		{
			num = i;
		}
		if (num == -1)
		{
			return absolutePath;
		}
		int num3 = (array.Length - (num + 1)) * 3;
		for (int j = num + 1; j < array2.Length; j++)
		{
			num3 += array2[j].Length + 1;
		}
		StringBuilder stringBuilder = new StringBuilder(num3);
		for (int k = num + 1; k < array.Length; k++)
		{
			if (array[k].Length > 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(Path.DirectorySeparatorChar);
				}
				stringBuilder.Append("..");
			}
		}
		for (int l = num + 1; l < array2.Length; l++)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(Path.DirectorySeparatorChar);
			}
			stringBuilder.Append(array2[l]);
		}
		return stringBuilder.ToString();
	}
}
