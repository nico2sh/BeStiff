using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using SKAnimation;

namespace AnimationTest;

/// <summary>
/// Turns a character compiled into the game's content (.xnb skeleton,
/// animations and bone sprites) back into an editor project:
/// skeleton/animation XML, one PNG per bone and a project file.
/// </summary>
internal static class ContentImporter
{
	public const string Usage = "AnimationEditor --import <content folder> <skeleton asset> <animations asset> <sprites asset folder> <output folder>";

	public static void Run(IServiceProvider services, GraphicsDevice device, string[] args)
	{
		if (args.Length != 5)
		{
			throw new ArgumentException("Usage: " + Usage);
		}
		string contentRoot = args[0];
		string skeletonAsset = args[1];
		string animationsAsset = args[2];
		string spritesAsset = args[3];
		string output = args[4];
		Directory.CreateDirectory(Path.Combine(output, "sprites"));
		ContentManager content = new ContentManager(services, contentRoot);

		BoneXMLReadHelper[] bones = LoadSkeleton(content, skeletonAsset);
		Save(Path.Combine(output, "skeleton.xml"), bones);
		BoneAnimationXMLReadHelper[] animations = content.Load<BoneAnimationXMLReadHelper[]>(animationsAsset);
		Save(Path.Combine(output, "animations.xml"), animations);

		int sprites = 0;
		foreach (string bone in bones.Select(b => b.name).Distinct())
		{
			string asset = spritesAsset.TrimEnd('/') + "/" + bone;
			if (!File.Exists(Path.Combine(contentRoot, asset + ".xnb")))
			{
				Console.Error.WriteLine($"  no sprite for bone {bone}");
				continue;
			}
			Texture2D texture = content.Load<Texture2D>(asset);
			using FileStream stream = File.Create(Path.Combine(output, "sprites", bone + ".png"));
			texture.SaveAsPng(stream, texture.Width, texture.Height);
			sprites++;
		}

		Save(Path.Combine(output, "project.xml"), new ProjectData
		{
			// Empty: the editor then uses the project file's own folder.
			BasePath = "",
			SpritesSubPath = "sprites",
			SkeletonFile = "skeleton.xml",
			AnimationsFile = "animations.xml",
			BoundingBox = new Vector2(48f, 96f)
		});
		Console.Error.WriteLine($"Imported {bones.Length} bones, {animations.Length} animations, {sprites} sprites into {output}");
	}

	private static BoneXMLReadHelper[] LoadSkeleton(ContentManager content, string asset)
	{
		object loaded = content.Load<object>(asset);
		BoneXMLReadHelper[] bones = loaded as BoneXMLReadHelper[];
		if (bones == null && loaded is LegacyBoneXMLReadHelper[] legacy)
		{
			bones = legacy.Select(bone => bone.ToCurrent()).ToArray();
		}
		if (bones != null)
		{
			// The old format lists the skeleton itself as a "Root" bone at level 0;
			// the editor's files (and BoneReader) leave it out.
			return bones.Where(bone => bone.name != "Root").ToArray();
		}
		throw new InvalidDataException("Not a skeleton: " + asset);
	}

	private static void Save<T>(string path, T value)
	{
		XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
		using XmlWriter writer = XmlWriter.Create(path, settings);
		IntermediateSerializer.Serialize(writer, value, null);
	}
}
