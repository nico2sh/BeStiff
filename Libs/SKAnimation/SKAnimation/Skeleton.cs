using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SKAnimation
{
	public class Skeleton : Bone
	{
		private const int MAX_BONENUMBER = 20;

		private ChildBone[] drawList;

		private int[] indexList;

		private int bonesNumber;

		private Category baseCollisionCategory;

		private Category baseCollidesWith;

		private Dictionary<string, BoneAnimation> animationsList;

		private BoneAnimation currentAnimation;

		private BoneAnimation tempAnimation;

		private string currentAnimationName;

		public Dictionary<string, BoneAnimation> AnimationsList => animationsList;

		public int CurrenKeyFrame => currentAnimation.CurrentKeyFrame;

		public override float AbsoluteAngle => (float)Side * angle;

		public Skeleton(Vector2 pos, float a, Body mainBody)
		{
			name = "Root";
			position = pos;
			angle = a;
			length = 0f;
			drawOrder = 0;
			childCount = 0;
			child = new ChildBone[Bone.MAX_CHCOUNT];
			animationsList = new Dictionary<string, BoneAnimation>();
			currentAnimation = null;
			tempAnimation = null;
			currentAnimationName = "";
			boneBody = mainBody;
			drawList = new ChildBone[20];
			indexList = new int[20];
			bonesNumber = 0;
		}

		public List<Bone> GetBoneList()
		{
			List<Bone> list = new List<Bone>();
			for (int i = 0; i < bonesNumber; i++)
			{
				list.Add(drawList[i]);
			}
			return list;
		}

		public BoneXMLReadHelper[] GetHelper()
		{
			List<BoneXMLReadHelper> helperFromHere = GetHelperFromHere(1);
			for (int i = 0; i < bonesNumber; i++)
			{
				ChildBone childBone = drawList[i];
				for (int j = 0; j < helperFromHere.Count; j++)
				{
					BoneXMLReadHelper value = helperFromHere[j];
					if (childBone.Name == value.name)
					{
						value.drawOrder = i * 10;
						helperFromHere[j] = value;
						j = helperFromHere.Count;
					}
				}
			}
			return helperFromHere.ToArray();
		}

		public BoneAnimationXMLReadHelper[] GetAnimationHelper()
		{
			BoneAnimationXMLReadHelper[] array = new BoneAnimationXMLReadHelper[animationsList.Count];
			int num = 0;
			foreach (BoneAnimation value in animationsList.Values)
			{
				array[num].name = value.Name;
				array[num].loop = value.Loop;
				array[num].dontInterrupt = value.DontInterrupt;
				KeyFrameAnimationXMLReaderHelper[] array2 = new KeyFrameAnimationXMLReaderHelper[value.KeyFrames.Count];
				int num2 = 0;
				foreach (KeyFrame keyFrame in value.KeyFrames)
				{
					array2[num2].time = (int)keyFrame.Time;
					Dictionary<string, float[]> dictionary = new Dictionary<string, float[]>();
					foreach (string key in keyFrame.KeyFrameInfo.Keys)
					{
						dictionary.Add(key, new float[4]
						{
							keyFrame.KeyFrameInfo[key].Position.X,
							keyFrame.KeyFrameInfo[key].Position.Y,
							keyFrame.KeyFrameInfo[key].Angle,
							keyFrame.KeyFrameInfo[key].SpriteFrame
						});
					}
					array2[num2].values = dictionary;
					num2++;
				}
				array[num].keyframes = array2;
				num++;
			}
			return array;
		}

		public BoneAnimationXMLReadHelper[] GetAnimationHelper(List<string> animationsOrderList)
		{
			BoneAnimationXMLReadHelper[] array = new BoneAnimationXMLReadHelper[animationsOrderList.Count];
			int num = 0;
			foreach (string animationsOrder in animationsOrderList)
			{
				if (animationsList.TryGetValue(animationsOrder, out var value))
				{
					array[num].name = value.Name;
					array[num].loop = value.Loop;
					array[num].dontInterrupt = value.DontInterrupt;
					KeyFrameAnimationXMLReaderHelper[] array2 = new KeyFrameAnimationXMLReaderHelper[value.KeyFrames.Count];
					int num2 = 0;
					foreach (KeyFrame keyFrame in value.KeyFrames)
					{
						array2[num2].time = (int)keyFrame.Time;
						Dictionary<string, float[]> dictionary = new Dictionary<string, float[]>();
						foreach (string key in keyFrame.KeyFrameInfo.Keys)
						{
							dictionary.Add(key, new float[4]
							{
								keyFrame.KeyFrameInfo[key].Position.X,
								keyFrame.KeyFrameInfo[key].Position.Y,
								keyFrame.KeyFrameInfo[key].Angle,
								keyFrame.KeyFrameInfo[key].SpriteFrame
							});
						}
						array2[num2].values = dictionary;
						num2++;
					}
					array[num].keyframes = array2;
					num++;
					continue;
				}
				array[num].name = "Empty-" + num;
				array[num].loop = false;
				array[num].dontInterrupt = false;
				KeyFrameAnimationXMLReaderHelper[] keyframes = new KeyFrameAnimationXMLReaderHelper[1];
				array[num].keyframes = keyframes;
				throw new Exception("Something's wrong, there are animations registered that are not stored in the main dictionnary");
			}
			return array;
		}

		public void AddAnimation(BoneAnimation ani)
		{
			animationsList.Add(ani.Name, ani);
		}

		public bool RemoveAnimation(string animationName)
		{
			if (animationsList.ContainsKey(animationName))
			{
				animationsList.Remove(animationName);
				return true;
			}
			return false;
		}

		public bool AtLastKeyFrame()
		{
			return currentAnimation.AtLastKeyFrame();
		}

		public bool AnimationEnded()
		{
			return currentAnimation.EndedLoop;
		}

		public bool AnimationKeyFrameAt(int pos)
		{
			return currentAnimation.KeyFrameStarted(pos);
		}

		public List<string> GetAnimationListNames()
		{
			return animationsList.Keys.ToList();
		}

		public void ChangeAnimationName(string oldName, string newName)
		{
			if (animationsList.TryGetValue(oldName, out var value))
			{
				value.Name = newName;
				animationsList.Add(newName, value);
				animationsList.Remove(oldName);
			}
		}

		public List<KeyFrame> GetKeyFrames(string animationName)
		{
			return animationsList[animationName].KeyFrames;
		}

		public Bone CreateAndLoadBone(string boneName, string parentBoneName, GraphicsDevice graphicsDevice, string fullPath, World world, float scale)
		{
			ChildBone childBone = null;
			if (parentBoneName == "Root")
			{
				childBone = BoneAddChild(boneName, Vector2.Zero, 0f, 0f, 1f, 1, 0, r: true);
			}
			else
			{
				ChildBone boneByName = GetBoneByName(parentBoneName);
				if (boneByName != null)
				{
					childBone = boneByName.BoneAddChild(boneName, Vector2.Zero, 0f, 0f, 1f, 1, 0, r: true);
				}
			}
			if (childBone != null)
			{
				int num = 10;
				if (bonesNumber > 0)
				{
					num = indexList[bonesNumber - 1];
				}
				childBone.SetDrawOrder(num + 10);
				childBone.Load(graphicsDevice, fullPath, world, scale);
				foreach (string key in animationsList.Keys)
				{
					BoneAnimation boneAnimation = animationsList[key];
					for (int i = 0; i < boneAnimation.KeyFrames.Count; i++)
					{
						KeyFrame keyFrame = boneAnimation.KeyFrames[i];
						keyFrame.KeyFrameInfo.Add(boneName, new KeyFrameInfo(0f, Vector2.Zero));
					}
				}
			}
			return childBone;
		}

		public bool AddEmptyAnimation(string animationName)
		{
			if (animationsList.ContainsKey(animationName))
			{
				return false;
			}
			BoneAnimation boneAnimation = new BoneAnimation(animationName, hasToLoop: false);
			KeyFrame keyFrame = new KeyFrame();
			keyFrame.Time = 10.0;
			Dictionary<string, KeyFrameInfo> dictionary = new Dictionary<string, KeyFrameInfo>();
			List<Bone> boneList = GetBoneList();
			foreach (Bone item in boneList)
			{
				dictionary.Add(value: new KeyFrameInfo(item.BaseAngle, item.OriginalPosition), key: item.Name);
			}
			keyFrame.KeyFrameInfo = dictionary;
			boneAnimation.AddKeyFrame(keyFrame);
			AddAnimation(boneAnimation);
			return true;
		}

		public bool RemoveBone(string boneName)
		{
			ChildBone boneByName = GetBoneByName(boneName);
			Bone parent = boneByName.Parent;
			List<ChildBone> allChilds = boneByName.GetAllChilds();
			allChilds.Add(boneByName);
			List<int> list = new List<int>(allChilds.Count + 1);
			for (int i = 0; i < bonesNumber; i++)
			{
				if (allChilds.Contains(drawList[i]))
				{
					list.Add(i);
				}
			}
			int num = 0;
			foreach (int item in list)
			{
				parent.RemoveChild(boneByName);
				for (int j = item - num; j < bonesNumber - 1; j++)
				{
					drawList[j] = drawList[j + 1];
					indexList[j] = indexList[j + 1];
				}
				drawList[bonesNumber - 1] = null;
				indexList[bonesNumber - 1] = 0;
				num++;
				bonesNumber--;
			}
			foreach (string key in animationsList.Keys)
			{
				BoneAnimation boneAnimation = animationsList[key];
				for (int k = 0; k < boneAnimation.KeyFrames.Count; k++)
				{
					KeyFrame keyFrame = boneAnimation.KeyFrames[k];
					foreach (ChildBone item2 in allChilds)
					{
						keyFrame.KeyFrameInfo.Remove(item2.Name);
					}
				}
			}
			return true;
		}

		public override Vector2 GetEndPosition()
		{
			return base.Position;
		}

		/// <summary>Gives every ragdoll bone body an angular velocity (used to make a corpse slump).</summary>
		public void ApplyRagdollAngularVelocity(float angularVelocity)
		{
			for (int i = 0; i < bonesNumber; i++)
			{
				if (drawList[i] is ChildBone bone && bone.BoneBody != null && bone.BoneBody.Enabled)
				{
					bone.BoneBody.AngularVelocity += angularVelocity;
				}
			}
		}

		public void MakePuppet(bool puppet)
		{
			if (isPuppet != puppet)
			{
				isPuppet = puppet;
				if (!puppet)
				{
					position = new Vector2((int)(boneBody.Position.X * worldScale), (int)(boneBody.Position.Y * worldScale));
				}
				for (int i = 0; i < childCount; i++)
				{
					child[i].MakePuppet(puppet, flip: false);
				}
			}
			if (!puppet && isPuppet && currentAnimation != null)
			{
				currentAnimation.Reset();
			}
		}

		public void DeactivateContact()
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollisionCategory(Category.None);
			}
			for (int j = 0; j < childCount; j++)
			{
				child[j].SetCollidesWith(Category.None);
			}
		}

		public void ReactivateContact()
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollisionCategory(baseCollisionCategory);
			}
			for (int j = 0; j < childCount; j++)
			{
				child[j].SetCollidesWith(baseCollidesWith);
			}
		}

		public override void SetCollisionCategory(Category category)
		{
			baseCollisionCategory = category;
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollisionCategory(category);
			}
		}

		public override void SetCollidesWith(Category category)
		{
			baseCollidesWith = category;
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollidesWith(category);
			}
		}

		public override void SetCollisionGroup(short group)
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollisionGroup(group);
			}
		}

		public void InsertTempAnimation(BoneAnimation animation)
		{
			if (tempAnimation != null)
			{
				tempAnimation.Reset();
			}
			tempAnimation = animation;
		}

		public void CancelTempAnimation()
		{
			if (tempAnimation != null)
			{
				tempAnimation.Reset();
				tempAnimation = null;
			}
		}

		public void Load(ContentManager Content, string spritesSubFolder, World world, float scale)
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].SpriteScale = SpriteScale;
				child[i].SingleFrameSprites = SingleFrameSprites;
			}
			worldScale = scale;
			for (int i = 0; i < childCount; i++)
			{
				child[i].Load(Content, spritesSubFolder, world, worldScale);
			}
		}

		public void Load(GraphicsDevice graphicsDevice, string spritesSubFolder, World world, float scale)
		{
			worldScale = scale;
			for (int i = 0; i < childCount; i++)
			{
				child[i].Load(graphicsDevice, spritesSubFolder, world, worldScale);
			}
		}

		public void ReloadTexture(GraphicsDevice graphicsDevice, string spritesSubFolder)
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].ReloadTexture(graphicsDevice, spritesSubFolder);
			}
		}

		public void Dispose(World world)
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].Dispose(world);
			}
		}

		public void LoadAnimations(BoneAnimation[] animations)
		{
			animationsList.Clear();
			foreach (BoneAnimation ani in animations)
			{
				AddAnimation(ani);
			}
		}

		public void UpdateBoneAnimationByOffset(string boneName, Vector2 offset)
		{
			foreach (string key in animationsList.Keys)
			{
				BoneAnimation boneAnimation = animationsList[key];
				for (int i = 0; i < boneAnimation.KeyFrames.Count; i++)
				{
					_ = boneAnimation.KeyFrames[i];
					if (boneAnimation.KeyFrames[i].KeyFrameInfo.TryGetValue(boneName, out var value))
					{
						Vector2 vector = new Vector2(value.Position.X + offset.X, value.Position.Y + offset.Y);
						value.Position = vector;
						boneAnimation.KeyFrames[i].KeyFrameInfo[boneName] = value;
					}
				}
			}
		}

		public void AddToSortList(int order, ChildBone bone)
		{
			if (!(bone.Name != name))
			{
				return;
			}
			int i;
			for (i = 0; i < bonesNumber; i++)
			{
				int num = indexList[i];
				if (num > order)
				{
					break;
				}
			}
			for (int num2 = bonesNumber - 1; num2 >= i; num2--)
			{
				indexList[num2 + 1] = indexList[num2];
				drawList[num2 + 1] = drawList[num2];
			}
			indexList[i] = order;
			drawList[i] = bone;
			bonesNumber++;
		}

		public bool MoveBoneUpInSortList(int order)
		{
			if (order <= 0 || order >= bonesNumber)
			{
				return false;
			}
			ChildBone childBone = drawList[order];
			int num = indexList[order];
			drawList[order] = drawList[order - 1];
			indexList[order] = indexList[order - 1];
			drawList[order - 1] = childBone;
			indexList[order - 1] = num;
			return true;
		}

		public bool MoveBoneDownInSortList(int order)
		{
			if (order < 0 || order >= bonesNumber - 1)
			{
				return false;
			}
			ChildBone childBone = drawList[order];
			int num = indexList[order];
			drawList[order] = drawList[order + 1];
			indexList[order] = indexList[order + 1];
			drawList[order + 1] = childBone;
			indexList[order + 1] = num;
			return true;
		}

		public void SetAnimation(string animationName)
		{
			if (currentAnimationName != animationName)
			{
				if (currentAnimation != null && currentAnimation.DontInterrupt && !currentAnimation.AtLastKeyFrame())
				{
					return;
				}
				if (animationsList.TryGetValue(animationName, out currentAnimation))
				{
					currentAnimation.Reset();
				}
				else
				{
					currentAnimation = null;
				}
			}
			currentAnimationName = animationName;
		}

		public void SetAnimationFrame(int frame)
		{
			currentAnimation.SetFrame(frame);
		}

		public KeyFrameInfo GetAnimationInfo(string bone, string animationName, int frame)
		{
			if (animationsList.TryGetValue(animationName, out var value) && frame < value.KeyFrames.Count && value.KeyFrames[frame].KeyFrameInfo.TryGetValue(bone, out var value2))
			{
				return value2;
			}
			return default(KeyFrameInfo);
		}

		public void Initialize(GraphicsDevice graphicsDevice)
		{
		}

		public void SwitchSide(int newSide)
		{
			Side = newSide;
			for (int i = 0; i < bonesNumber; i++)
			{
				drawList[i].Side = newSide;
			}
		}

		public Bone getFirstBone()
		{
			if (bonesNumber == 0)
			{
				return this;
			}
			return GetBoneAt(0);
		}

		public int GetBonesNumber()
		{
			return bonesNumber;
		}

		public ChildBone GetBoneByName(string boneName)
		{
			for (int i = 0; i < bonesNumber; i++)
			{
				ChildBone childBone = drawList[i];
				if (childBone.Name == boneName)
				{
					return childBone;
				}
			}
			return null;
		}

		public ChildBone GetBoneAt(int index)
		{
			if (index >= bonesNumber)
			{
				return null;
			}
			return drawList[index];
		}

		public string[] GetData()
		{
			string[] array = new string[GetBonesNumber() + 2];
			int num = 0;
			array[num] = position.X + " " + position.Y + " " + angle + " " + name;
			num++;
			for (int i = 0; i < bonesNumber; i++)
			{
				Bone bone = drawList[i];
				array[num] = bone.Position.X + " " + bone.Position.Y + " " + bone.Angle + " " + bone.Name;
				num++;
			}
			array[num] = "====================";
			return array;
		}

		public void Update(double frameTimeInMs)
		{
			if (tempAnimation == null)
			{
				if (currentAnimation != null)
				{
					currentAnimation.Update(drawList, bonesNumber, frameTimeInMs);
				}
			}
			else if (currentAnimation == null)
			{
				// Only a temporary animation is playing (e.g. a hit reaction on an
				// enemy that never had a base animation set).
				tempAnimation.Update(drawList, bonesNumber, frameTimeInMs);
				if (tempAnimation.AtLastKeyFrame())
				{
					tempAnimation.Reset();
					tempAnimation = null;
				}
			}
			else if (currentAnimation.DontInterrupt && tempAnimation.Overrideable)
			{
				currentAnimation.Update(drawList, bonesNumber, frameTimeInMs);
				tempAnimation.Reset();
				tempAnimation = null;
			}
			else
			{
				currentAnimation.Update(drawList, bonesNumber, frameTimeInMs, ref tempAnimation);
				if (tempAnimation.AtLastKeyFrame())
				{
					tempAnimation.Reset();
					tempAnimation = null;
				}
			}
			Vector2 parentEndPosition = new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length) + position;
			for (int i = 0; i < childCount; i++)
			{
				child[i].Update(frameTimeInMs, parentEndPosition, angle);
			}
		}

		public override void DrawDebugAnimationAtFrame(Vector2 position, float initialAngle, string animationName, int frame, SpriteBatch spriteBatch)
		{
			for (int i = 0; i < bonesNumber; i++)
			{
				drawList[i].DrawDebugAnimationAtFrame(position, initialAngle, animationName, frame, spriteBatch);
			}
		}

		public override void DrawDebug(SpriteBatch spriteBatch)
		{
			Vector2 zero = Vector2.Zero;
			VertexPositionColor[] array = new VertexPositionColor[2];
			array[0].Position = new Vector3(position, 0f);
			array[0].Color = Color.Black;
			zero = new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length) + position;
			array[1].Position = new Vector3(zero, 0f);
			array[1].Color = Color.Black;
			spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, array, 0, 1);
			for (int i = 0; i < bonesNumber; i++)
			{
				drawList[i].DrawDebug(spriteBatch);
			}
		}

		public override void DrawAnimationAtFrame(Vector2 position, float initialAngle, string animationName, int frame, SpriteBatch spriteBatch)
		{
			for (int i = 0; i < bonesNumber; i++)
			{
				drawList[i].DrawAnimationAtFrame(position, initialAngle, animationName, frame, spriteBatch);
			}
		}

		public override void DrawInitialSkeleton(SpriteBatch spriteBatch, float initialAngle)
		{
			for (int i = 0; i < bonesNumber; i++)
			{
				drawList[i].DrawInitialSkeleton(spriteBatch, initialAngle);
			}
		}

		public override void DrawDebugInitialSkeleton(SpriteBatch spriteBatch, float initialAngle)
		{
			Vector2 zero = Vector2.Zero;
			VertexPositionColor[] array = new VertexPositionColor[2];
			array[0].Position = new Vector3(position, 0f);
			array[0].Color = Color.Black;
			zero = new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length) + position;
			array[1].Position = new Vector3(zero, 0f);
			array[1].Color = Color.Black;
			spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, array, 0, 1);
			for (int i = 0; i < bonesNumber; i++)
			{
				drawList[i].DrawDebugInitialSkeleton(spriteBatch, initialAngle);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			for (int i = 0; i < bonesNumber; i++)
			{
				drawList[i].Draw(spriteBatch);
			}
		}
	}
}
