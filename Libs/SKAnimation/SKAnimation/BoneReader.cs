using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SKAnimation
{
	public class BoneReader
	{
		private Skeleton ReadXML(string fileName, ContentManager content, Body mainBody)
		{
			object asset = content.Load<object>(fileName);
			BoneXMLReadHelper[] bxrhl = asset as BoneXMLReadHelper[];
			if (bxrhl == null && asset is LegacyBoneXMLReadHelper[] legacy)
			{
				// Skeleton written by the older pipeline (fewer fields per bone).
				bxrhl = new BoneXMLReadHelper[legacy.Length];
				for (int i = 0; i < legacy.Length; i++)
				{
					bxrhl[i] = legacy[i].ToCurrent();
				}
			}
			if (bxrhl == null)
			{
				throw new Microsoft.Xna.Framework.Content.ContentLoadException("Unsupported skeleton asset: " + fileName);
			}
			return ReadFromHelper(bxrhl, mainBody);
		}

		/// <summary>
		/// Multiplier applied to bone positions, lengths and animation offsets
		/// while reading. Used for skeletons authored at the old half-size scale.
		/// </summary>
		private float boneScale = 1f;

		public Skeleton ReadFromHelper(BoneXMLReadHelper[] bxrhl, Body mainBody)
		{
			Skeleton skeleton = new Skeleton(Vector2.Zero, 0f, mainBody);
			Bone bone = skeleton;
			int num = 1;
			for (int i = 0; i < bxrhl.Count(); i++)
			{
				BoneXMLReadHelper boneXMLReadHelper = bxrhl[i];
				if (!(boneXMLReadHelper.name == "Root"))
				{
					while (num > boneXMLReadHelper.level)
					{
						bone = bone.Parent;
						num--;
					}
					int num2 = boneXMLReadHelper.frames;
					if (num2 <= 0)
					{
						num2 = 1;
					}
					bone = bone.BoneAddChild(boneXMLReadHelper.name, boneXMLReadHelper.position * boneScale, MathHelper.WrapAngle(boneXMLReadHelper.angle), boneXMLReadHelper.angleOffset, boneXMLReadHelper.length * boneScale, num2, boneXMLReadHelper.defaultFrame, boneXMLReadHelper.ragdoll);
					bone.SetDrawOrder(boneXMLReadHelper.drawOrder);
					num++;
				}
			}
			return skeleton;
		}

		public Skeleton CreateFromFile(ContentManager Content, string bones, string animations, string spritesSubFolder, World world, float scale, Body mainBody)
		{
			return CreateFromFile(Content, bones, animations, spritesSubFolder, world, scale, mainBody, 1f, false);
		}

		/// <summary>
		/// Creates a skeleton whose sprites are drawn at <paramref name="spriteScale"/>
		/// and, when <paramref name="singleFrameSprites"/> is set, treated as single
		/// images. Used for characters whose art was left at the old half-size scale.
		/// </summary>
		public Skeleton CreateFromFile(ContentManager Content, string bones, string animations, string spritesSubFolder, World world, float scale, Body mainBody, float spriteScale, bool singleFrameSprites)
		{
			return CreateFromFile(Content, bones, animations, spritesSubFolder, world, scale, mainBody, spriteScale, singleFrameSprites, 1f);
		}

		/// <summary>
		/// As above, additionally scaling the skeleton geometry itself by
		/// <paramref name="skeletonScale"/> (for skeleton files authored at the old scale).
		/// </summary>
		public Skeleton CreateFromFile(ContentManager Content, string bones, string animations, string spritesSubFolder, World world, float scale, Body mainBody, float spriteScale, bool singleFrameSprites, float skeletonScale)
		{
			boneScale = skeletonScale;
			Skeleton skeleton = ReadXML(bones, Content, mainBody);
			skeleton.SpriteScale = spriteScale;
			skeleton.SingleFrameSprites = singleFrameSprites;
			skeleton.Load(Content, spritesSubFolder, world, scale);
			List<Bone> boneList = skeleton.GetBoneList();
			BoneAnimation[] animations2 = ReadAnimationFromXML(animations, Content, boneList);
			skeleton.LoadAnimations(animations2);
			skeleton.SetAnimation("NONE");
			boneScale = 1f;
			return skeleton;
		}

		public Skeleton CreateFromHelpers(ContentManager Content, BoneXMLReadHelper[] bxrhl, BoneAnimationXMLReadHelper[] bshl, string spritesSubFolder, World world, float scale, Body mainBody)
		{
			Skeleton skeleton = ReadFromHelper(bxrhl, mainBody);
			skeleton.Load(Content, spritesSubFolder, world, scale);
			List<Bone> boneList = skeleton.GetBoneList();
			BoneAnimation[] animations = ReadAnimationFromHelper(bshl, boneList);
			skeleton.LoadAnimations(animations);
			skeleton.SetAnimation("NONE");
			return skeleton;
		}

		public BoneAnimation[] ReadAnimationFromXML(string fileName, ContentManager content, List<Bone> boneList)
		{
			BoneAnimationXMLReadHelper[] bshl = content.Load<BoneAnimationXMLReadHelper[]>(fileName);
			return ReadAnimationFromHelper(bshl, boneList);
		}

		public BoneAnimation[] ReadAnimationFromHelper(BoneAnimationXMLReadHelper[] bshl, List<Bone> boneList)
		{
			BoneAnimation[] array = new BoneAnimation[bshl.Count()];
			int num = 0;
			for (int i = 0; i < bshl.Count(); i++)
			{
				new List<BoneAnimation>();
				BoneAnimationXMLReadHelper boneAnimationXMLReadHelper = bshl[i];
				BoneAnimation boneAnimation = new BoneAnimation(boneAnimationXMLReadHelper.name, boneAnimationXMLReadHelper.loop, boneAnimationXMLReadHelper.dontInterrupt);
				KeyFrameAnimationXMLReaderHelper[] keyframes = boneAnimationXMLReadHelper.keyframes;
				for (int j = 0; j < keyframes.Count(); j++)
				{
					float num2 = keyframes[j].time;
					KeyFrame keyFrame = new KeyFrame();
					keyFrame.Time = num2;
					foreach (Bone bone in boneList)
					{
						string name = bone.Name;
						float angle;
						Vector2 position;
						int spriteFrame;
						if (keyframes[j].values.ContainsKey(name))
						{
							angle = keyframes[j].values[name][2];
							position = new Vector2(keyframes[j].values[name][0], keyframes[j].values[name][1]) * boneScale;
							spriteFrame = (int)keyframes[j].values[name][3];
						}
						else
						{
							angle = bone.Angle;
							position = bone.Position;
							spriteFrame = 0;
						}
						KeyFrameInfo value = new KeyFrameInfo
						{
							Angle = MathHelper.WrapAngle(angle),
							Position = position,
							SpriteFrame = spriteFrame
						};
						keyFrame.KeyFrameInfo.Add(name, value);
					}
					boneAnimation.AddKeyFrame(keyFrame);
				}
				array[num] = boneAnimation;
				num++;
			}
			return array;
		}
	}
}
