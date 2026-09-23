using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SKAnimation
{
	public abstract class Bone
	{
		/// <summary>
		/// Scale applied when drawing bone sprites. Used for characters whose
		/// sprite art was left at the old half-size scale.
		/// </summary>
		public float SpriteScale = 1f;

		/// <summary>
		/// When true, every bone sprite is treated as a single frame regardless
		/// of the frame count in the skeleton file (old-scale art has no strips).
		/// </summary>
		public bool SingleFrameSprites;

		protected static int MAX_CHCOUNT = 8;

		protected string name;

		protected Vector2 position;

		protected float angle;

		protected float length;

		protected int drawOrder;

		protected int childCount;

		protected ChildBone[] child;

		protected float worldScale;

		protected bool isPuppet;

		protected Body boneBody;

		protected RevoluteJoint boneJoint;

		protected World world;

		protected Vector2 absolutePosition;

		protected float absoluteAngle;

		protected float baseAngle;

		protected float angleOffset;

		protected Vector2 originalPosition;

		public int Side = 1;

		public string Name => name;

		public virtual Bone Parent => null;

		public Vector2 Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
			}
		}

		public float Angle
		{
			get
			{
				return MathHelper.WrapAngle(angle);
			}
			set
			{
				angle = MathHelper.WrapAngle(value);
			}
		}

		public float BaseAngle => MathHelper.WrapAngle(baseAngle);

		public float AngleOffset => MathHelper.WrapAngle(angleOffset);

		public Vector2 OriginalPosition => originalPosition;

		public virtual float AbsoluteAngle => absoluteAngle;

		public virtual Vector2 AbsolutePosition => absolutePosition;

		public virtual float Length => length;

		public Body BoneBody => boneBody;

		public virtual void SetUserData(object userData)
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetUserData(userData);
			}
		}

		public virtual void SetCollisionCategory(Category category)
		{
			boneBody.CollisionCategories = category;
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollisionCategory(category);
			}
		}

		public virtual void SetCollidesWith(Category category)
		{
			BoneBody.CollidesWith = category;
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollidesWith(category);
			}
		}

		public virtual void SetCollisionGroup(short group)
		{
			BoneBody.CollisionGroup = group;
			for (int i = 0; i < childCount; i++)
			{
				child[i].SetCollisionGroup(group);
			}
		}

		public Vector2 GetPositionAtPercentage(float percentage)
		{
			if (!isPuppet)
			{
				float num = percentage * length;
				return new Vector2((float)Math.Cos(absoluteAngle) * num, (float)Math.Sin(absoluteAngle) * num) + absolutePosition;
			}
			return new Vector2(boneBody.Position.X * worldScale, (0f - boneBody.Position.Y) * worldScale);
		}

		public virtual Vector2 GetEndPosition()
		{
			Vector2 zero = Vector2.Zero;
			return new Vector2((float)Math.Cos(absoluteAngle) * length, (float)Math.Sin(absoluteAngle) * length) + absolutePosition;
		}

		protected Bone FindRootBone()
		{
			if (Parent == null)
			{
				return this;
			}
			return Parent.FindRootBone();
		}

		public List<Bone> GetParentsList()
		{
			Bone parent = Parent;
			List<Bone> list = new List<Bone>();
			while (parent != null)
			{
				list.Add(parent);
				parent = parent.Parent;
			}
			return list;
		}

		public List<ChildBone> GetAllChilds()
		{
			List<ChildBone> list = new List<ChildBone>();
			for (int i = 0; i < childCount; i++)
			{
				list.Add(child[i]);
				list.AddRange(child[i].GetAllChilds());
			}
			return list;
		}

		protected List<BoneXMLReadHelper> GetHelperFromHere(int currentLevel)
		{
			List<BoneXMLReadHelper> list = new List<BoneXMLReadHelper>();
			for (int i = 0; i < childCount; i++)
			{
				list.Add(new BoneXMLReadHelper
				{
					angle = child[i].BaseAngle,
					name = child[i].Name,
					position = child[i].OriginalPosition,
					length = child[i].Length,
					angleOffset = child[i].AngleOffset,
					frames = child[i].SpriteFrames,
					level = currentLevel,
					defaultFrame = child[i].DefaultFrame,
					ragdoll = child[i].IsInRagdoll
				});
				list.AddRange(child[i].GetHelperFromHere(currentLevel + 1));
			}
			return list;
		}

		public void SetAbsoluteAngle(float newAngle, bool limitByOffset)
		{
			float num = Parent.AbsoluteAngle;
			if (Parent.Name == "Root" && Side == -1)
			{
				num = 0f - num - (float)Math.PI;
			}
			angle = (float)Side * MathHelper.WrapAngle(newAngle - num);
			if (limitByOffset)
			{
				float max = baseAngle + angleOffset;
				float min = baseAngle - angleOffset;
				angle = MathHelper.Clamp(angle, min, max);
			}
			float num2 = MathHelper.WrapAngle(angle + num);
			if (Side == -1)
			{
				absoluteAngle = MathHelper.WrapAngle((float)Math.PI - num2);
			}
			else
			{
				absoluteAngle = num2;
			}
		}

		public void SetAbsoluteAngleSmooth(float newAngle, bool limitByOffset, float speed, double timeFrame)
		{
			float num = Parent.AbsoluteAngle;
			if (Parent.Name == "Root" && Side == -1)
			{
				num = 0f - num - (float)Math.PI;
			}
			float num2 = baseAngle - MathHelper.WrapAngle(baseAngle - (float)Side * (newAngle - num));
			if (limitByOffset)
			{
				float num3 = baseAngle + angleOffset;
				float num4 = baseAngle - angleOffset;
				if (num2 < num4 || num2 > num3)
				{
					float num5 = Math.Abs(num2 - num3);
					float num6 = Math.Abs(num2 - num4);
					num2 = ((!(num5 < num6)) ? num4 : num3);
				}
			}
			float num7 = angle - num2;
			float num8 = MathHelper.Clamp((float)Math.PI * (speed * (float)timeFrame / 1000f), 0f, Math.Abs(num7));
			if (num7 < 0f)
			{
				angle += num8;
			}
			else if (num7 > 0f)
			{
				angle -= num8;
			}
			float num9 = MathHelper.WrapAngle(angle + num);
			if (Side == -1)
			{
				absoluteAngle = MathHelper.WrapAngle((float)Math.PI - num9);
			}
			else
			{
				absoluteAngle = num9;
			}
		}

		public virtual void SetDrawOrder(int order)
		{
		}

		protected Bone FindBoneByName(string boneName)
		{
			if (boneName == name)
			{
				return this;
			}
			for (int i = 0; i < childCount; i++)
			{
				Bone bone = child[i].FindBoneByName(boneName);
				if (bone != null)
				{
					return bone;
				}
			}
			return null;
		}

		public ChildBone BoneAddChild(string boneName, Vector2 pos, float a, float o, float l, int f, int df, bool r)
		{
			ChildBone childBone = new ChildBone(this, boneName, pos, a, o, l, f, df, r);
			child[childCount++] = childBone;
			return childBone;
		}

		public bool RemoveChild(ChildBone childBone)
		{
			int num = -1;
			for (int i = 0; i < childCount; i++)
			{
				if (child[i] == childBone)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				for (int j = num; j < childCount - 1; j++)
				{
					child[j] = child[j + 1];
				}
				child[childCount - 1] = null;
				childCount--;
			}
			return num != -1;
		}

		public bool AddChild(ChildBone childBone)
		{
			bool flag = false;
			for (int i = 0; i < childCount; i++)
			{
				if (child[i] == childBone)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				child[childCount++] = childBone;
			}
			return !flag;
		}

		public virtual void DrawDebug(SpriteBatch spriteBatch)
		{
		}

		public virtual void DrawDebugInitialSkeleton(SpriteBatch spriteBatch, float initialAngle)
		{
		}

		public virtual void Draw(SpriteBatch spriteBatch)
		{
		}

		public virtual void DrawInitialSkeleton(SpriteBatch spriteBatch, float initialAngle)
		{
		}

		public virtual void DrawAnimationAtFrame(Vector2 position, float initialAngle, string animationName, int frame, SpriteBatch spriteBatch)
		{
		}

		public virtual void DrawDebugAnimationAtFrame(Vector2 position, float initialAngle, string animationName, int frame, SpriteBatch spriteBatch)
		{
		}
	}
}
