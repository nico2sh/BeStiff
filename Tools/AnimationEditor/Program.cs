using System;
using Microsoft.Xna.Framework;

namespace AnimationTest;

internal static class Program
{
	private static void Main(string[] args)
	{
		Game1 game = new Game1();
		if (args.Length > 0)
		{
			game.StartupProject = args[0];
		}
		try
		{
			((Game)game).Run();
		}
		finally
		{
			((IDisposable)game)?.Dispose();
		}
	}
}
