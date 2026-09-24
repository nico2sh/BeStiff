using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnimationTest;

/// <summary>
/// Debug helper: when EDITOR_SHOT is set to a .png path, saves the back buffer
/// after EDITOR_SHOT_FRAME frames (default 120).
/// </summary>
internal static class DebugScreenshot
{
	private static readonly string ShotPath = Environment.GetEnvironmentVariable("EDITOR_SHOT");

	private static readonly int ShotFrame = int.TryParse(Environment.GetEnvironmentVariable("EDITOR_SHOT_FRAME"), out int frame) && frame > 0 ? frame : 120;

	private static int frames;

	public static void AfterDraw(GraphicsDevice device)
	{
		frames++;
		if (string.IsNullOrEmpty(ShotPath) || frames != ShotFrame)
		{
			return;
		}
		int width = device.PresentationParameters.BackBufferWidth;
		int height = device.PresentationParameters.BackBufferHeight;
		Color[] data = new Color[width * height];
		device.GetBackBufferData(data);
		using Texture2D texture = new Texture2D(device, width, height);
		texture.SetData(data);
		using FileStream stream = File.Create(ShotPath);
		texture.SaveAsPng(stream, width, height);
		Console.Error.WriteLine("Saved screenshot to " + ShotPath);
	}
}
