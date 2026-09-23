using System;
using System.Collections.Generic;
using System.IO;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SKAnimation
{
	public class ChildBone : Bone
	{
		private Bone parent;

		private Texture2D texture;

		private Vector2 textureOrigin;

		private SpriteEffects se;

		private Fixture fix;

		private int frames;

		private int frameWidth;

		private int currentFrame;

		private Rectangle spriteRectangle;

		private int defaultFrame;

		private bool isInRagdoll;

		private IgnoreAnimationType ignoreAnimation;

		public override Bone Parent => parent;

		public int SpriteFrames => frames;

		public int DefaultFrame => defaultFrame;

		public bool IsInRagdoll
		{
			get
			{
				return isInRagdoll;
			}
			set
			{
				isInRagdoll = value;
			}
		}

		public IgnoreAnimationType IgnoreAnimation
		{
			get
			{
				return ignoreAnimation;
			}
			set
			{
				ignoreAnimation = value;
			}
		}

		public ChildBone(Bone parentBone, string boneName, Vector2 pos, float a, float o, float l, int f, int df, bool r)
		{
			name = boneName;
			position = pos;
			angle = a;
			baseAngle = a;
			if (baseAngle == -(float)Math.PI)
			{
				baseAngle = (float)Math.PI;
			}
			originalPosition = pos;
			angleOffset = o;
			length = l;
			frames = f;
			defaultFrame = (int)MathHelper.Clamp(df, 0f, frames - 1);
			isInRagdoll = r;
			drawOrder = 0;
			childCount = 0;
			child = new ChildBone[Bone.MAX_CHCOUNT];
			parent = parentBone;
			isPuppet = false;
			ignoreAnimation = IgnoreAnimationType.None;
			currentFrame = 0;
		}

		public void SetCurrentKeyFrame(int newKeyFrame)
		{
			if (SingleFrameSprites)
			{
				newKeyFrame = 0;
			}
			if (currentFrame != newKeyFrame)
			{
				if (newKeyFrame > frames - 1)
				{
					newKeyFrame = frames % newKeyFrame;
				}
				currentFrame = newKeyFrame;
				spriteRectangle.X = currentFrame * frameWidth;
			}
		}

		private void CreateBody(World physWorld, float scale)
		{
			world = physWorld;
			worldScale = scale;
			boneBody = BodyFactory.CreateBody(world);
			fix = FixtureFactory.AttachRectangle(length / worldScale, 0.2f, 10f, new Vector2(length / (2f * worldScale), 0f), boneBody);
			boneBody.BodyType = BodyType.Dynamic;
			boneBody.Position = new Vector2(base.Position.X / worldScale, (0f - base.Position.Y) / worldScale);
			Vector2 endPosition = parent.GetEndPosition();
			endPosition = new Vector2(endPosition.X / worldScale, (0f - endPosition.Y) / worldScale);
			boneJoint = new RevoluteJoint(parent.BoneBody, boneBody, parent.BoneBody.GetLocalPoint(endPosition), Vector2.Zero);
			boneJoint.LimitEnabled = true;
			boneJoint.CollideConnected = false;
			boneBody.Enabled = false;
		}

		public void Load(ContentManager Content, string Subfolder, World physWorld, float scale)
		{
			try
			{
				texture = Content.Load<Texture2D>(Subfolder + name);
			}
			catch (ContentLoadException)
			{
				// Bone without art in this sprite set (e.g. enemies lack the hero's
				// extra torso layers). The bone still exists for the hierarchy but
				// is not drawn.
				texture = null;
			}
			if (SingleFrameSprites)
			{
				frames = 1;
				defaultFrame = 0;
			}
			if (texture != null)
			{
				if (System.Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null)
					System.Console.Error.WriteLine($"  bone {name} tex={texture.Width}x{texture.Height} frames={frames} defaultFrame={defaultFrame} len={length} pos={base.Position} angle={angle} drawOrder={drawOrder} ragdoll={isInRagdoll}");
				frameWidth = texture.Width / frames;
				textureOrigin = new Vector2(frameWidth / 2, texture.Height / 2);
				spriteRectangle = new Rectangle(0, 0, frameWidth, texture.Height);
			}
			CreateBody(physWorld, scale);
			for (int i = 0; i < childCount; i++)
			{
				child[i].SpriteScale = SpriteScale;
				child[i].SingleFrameSprites = SingleFrameSprites;
				child[i].Load(Content, Subfolder, world, worldScale);
			}
		}

		public void Load(GraphicsDevice graphicsDevice, string fullPath, World physWorld, float scale)
		{
			try
			{
				using (Stream stream = File.OpenRead(fullPath + "//" + name + ".png"))
				{
					texture = Texture2D.FromStream(graphicsDevice, stream);
				}
			}
			catch
			{
				texture = new Texture2D(graphicsDevice, 1, 1);
			}
			frameWidth = texture.Width / frames;
			textureOrigin = new Vector2(frameWidth / 2, texture.Height / 2);
			spriteRectangle = new Rectangle(0, 0, frameWidth, texture.Height);
			CreateBody(physWorld, scale);
			for (int i = 0; i < childCount; i++)
			{
				child[i].Load(graphicsDevice, fullPath, world, worldScale);
			}
		}

		public void ReloadTexture(GraphicsDevice graphicsDevice, string fullPath)
		{
			try
			{
				using (Stream stream = File.OpenRead(fullPath + "//" + name + ".png"))
				{
					texture = Texture2D.FromStream(graphicsDevice, stream);
				}
			}
			catch
			{
				texture = new Texture2D(graphicsDevice, 1, 1);
			}
			frameWidth = texture.Width / frames;
			textureOrigin = new Vector2(frameWidth / 2, texture.Height / 2);
			spriteRectangle = new Rectangle(0, 0, frameWidth, texture.Height);
			for (int i = 0; i < childCount; i++)
			{
				child[i].ReloadTexture(graphicsDevice, fullPath);
			}
		}

		public void SetData(string newName, Vector2 newPosition, float newAngle, float newLength, float newAngleOffset, int newFrames, int newDefaultFrame, Bone newParent)
		{
			string text = name;
			name = newName;
			originalPosition = newPosition;
			baseAngle = MathHelper.WrapAngle(newAngle);
			if (baseAngle == -(float)Math.PI)
			{
				baseAngle = (float)Math.PI;
			}
			angleOffset = MathHelper.WrapAngle(newAngleOffset);
			length = newLength;
			frames = newFrames;
			frameWidth = texture.Width / frames;
			textureOrigin = new Vector2(frameWidth / 2, texture.Height / 2);
			spriteRectangle = new Rectangle(0, 0, frameWidth, texture.Height);
			defaultFrame = newDefaultFrame;
			if (!parent.RemoveChild(this))
			{
				throw new Exception("Child Inconsistency Error");
			}
			if (!newParent.AddChild(this))
			{
				throw new Exception("Child Inconsistency Error");
			}
			if (text != name)
			{
				Skeleton skeleton = FindRootBone() as Skeleton;
				foreach (string key in skeleton.AnimationsList.Keys)
				{
					BoneAnimation boneAnimation = skeleton.AnimationsList[key];
					for (int i = 0; i < boneAnimation.KeyFrames.Count; i++)
					{
						KeyFrame keyFrame = boneAnimation.KeyFrames[i];
						KeyFrameInfo value = keyFrame.KeyFrameInfo[text];
						keyFrame.KeyFrameInfo.Remove(text);
						keyFrame.KeyFrameInfo.Add(name, value);
					}
				}
			}
			parent = newParent;
		}

		public void SetData(float newAngle, float newAngleOffset)
		{
			baseAngle = MathHelper.WrapAngle(newAngle);
			if (baseAngle == -(float)Math.PI)
			{
				baseAngle = (float)Math.PI;
			}
			angleOffset = MathHelper.WrapAngle(newAngleOffset);
		}

		public void SetData(float newAngle)
		{
			baseAngle = MathHelper.WrapAngle(newAngle);
			if (baseAngle == -(float)Math.PI)
			{
				baseAngle = (float)Math.PI;
			}
		}

		public override void SetDrawOrder(int order)
		{
			drawOrder = order;
			Skeleton skeleton = (Skeleton)FindRootBone();
			skeleton.AddToSortList(order, this);
		}

		public override void SetUserData(object userData)
		{
			fix.UserData = userData;
			base.SetUserData(userData);
		}

		public void Dispose(World world)
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].Dispose(world);
			}
			world.RemoveBody(boneBody);
		}

		public void LooseJoints()
		{
			for (int i = 0; i < childCount; i++)
			{
				child[i].LooseJoints();
			}
		}

		public void MakePuppet(bool puppet, bool flip)
		{
			if (isInRagdoll)
			{
				if (puppet && !isPuppet)
				{
					absolutePosition = Vector2.Transform(originalPosition, Matrix.CreateRotationZ(parent.AbsoluteAngle));
					absolutePosition += parent.GetEndPosition();
					if (base.Name == "leftLoLeg")
					{
						int num = 0;
						num++;
					}
					// Centre of the joint's limit window: the bone's rest angle relative
					// to the physics body it hangs from (its parent bone's body, or the
					// character's main body for bones attached to the root). The old
					// expression mirrored this for root-attached bones.
					float num2 = MathHelper.WrapAngle(parent.AbsoluteAngle + (float)Side * baseAngle - parent.BoneBody.Rotation);
					// A bone without an angle range (the torso, locked to the
					// character's capsule while alive) gets a wide one in ragdoll
					// mode so the body can slump over.
					float ragdollOffset = angleOffset > 0f ? angleOffset : MathHelper.PiOver2;
					float num3 = num2 + ragdollOffset;
					float num4 = num2 - ragdollOffset;
					float num5 = AbsoluteAngle;
					float num6 = num5 - parent.BoneBody.Rotation;
					num6 = MathHelper.WrapAngle(num6 - num2) + num2;
					if (num6 < num4 || num6 > num3)
					{
						int num7 = 0;
						num7++;
					}
					num5 = num6 + parent.BoneBody.Rotation;
					boneBody.Position = new Vector2(AbsolutePosition.X / worldScale, AbsolutePosition.Y / worldScale);
					boneBody.Rotation = num5;
					boneBody.LinearVelocity = Vector2.Zero;
					boneBody.AngularVelocity = 0f;
					boneBody.Enabled = true;
					Vector2 worldPoint = new Vector2(AbsolutePosition.X / worldScale, AbsolutePosition.Y / worldScale);
					boneJoint.LocalAnchorA = parent.BoneBody.GetLocalPoint(worldPoint);
					boneJoint.LocalAnchorB = boneBody.GetLocalPoint(boneBody.Position);
					world.AddJoint(boneJoint);
					boneJoint.UpperLimit = num3;
					boneJoint.LowerLimit = num4;
					if (System.Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null)
						System.Console.Error.WriteLine($"  ragdoll {name} side={Side} base={baseAngle:F2} off={angleOffset:F2} parentAbs={parent.AbsoluteAngle:F2} parentBodyRot={parent.BoneBody.Rotation:F2} center={num2:F2} window=[{num4:F2},{num3:F2}] jointAngle={boneBody.Rotation - parent.BoneBody.Rotation:F2} flip={flip}");
				}
				else if (!puppet && isPuppet)
				{
					Vector2 vector = new Vector2((int)(boneBody.Position.X * worldScale), (int)(boneBody.Position.Y * worldScale));
					absolutePosition = vector;
					float newAngle = ((Side != -1) ? MathHelper.WrapAngle(0f - boneBody.Rotation) : (flip ? MathHelper.WrapAngle(0f - boneBody.Rotation) : MathHelper.WrapAngle(boneBody.Rotation)));
					SetAbsoluteAngle(newAngle, limitByOffset: true);
					world.RemoveJoint(boneJoint);
					boneBody.Enabled = false;
				}
			}
			isPuppet = puppet;
			for (int i = 0; i < childCount; i++)
			{
				child[i].MakePuppet(puppet, flip: true);
			}
		}

		public void Update(double frameTimeInMs, Vector2 parentEndPosition, float parentAngle)
		{
			float num = 0f;
			if (!isPuppet)
			{
				absolutePosition = Vector2.Transform(position, Matrix.CreateRotationZ(parentAngle));
				num = MathHelper.WrapAngle(angle + parentAngle);
				if (Side == -1)
				{
					se = SpriteEffects.FlipVertically;
					absolutePosition.X = 0f - absolutePosition.X;
					absoluteAngle = MathHelper.WrapAngle((float)Math.PI - num);
				}
				else
				{
					se = SpriteEffects.None;
					absoluteAngle = num;
				}
				absolutePosition += parent.GetEndPosition();
			}
			else if (!isInRagdoll)
			{
				num = MathHelper.WrapAngle(baseAngle + parent.BoneBody.Rotation);
				Vector2 vector;
				if (Side == -1)
				{
					vector = new Vector2(originalPosition.X, 0f - originalPosition.Y);
					se = SpriteEffects.FlipVertically;
				}
				else
				{
					vector = originalPosition;
					se = SpriteEffects.None;
				}
				absolutePosition = Vector2.Transform(vector, Matrix.CreateRotationZ(parent.BoneBody.Rotation));
				absoluteAngle = num;
				Vector2 worldPoint = parent.BoneBody.GetWorldPoint(new Vector2(parent.Length / worldScale, 0f));
				worldPoint = new Vector2((int)(worldPoint.X * worldScale), (int)(worldPoint.Y * worldScale));
				absolutePosition += worldPoint;
			}
			for (int i = 0; i < childCount; i++)
			{
				child[i].Update(frameTimeInMs, GetEndPosition(), num);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (texture == null)
			{
				return;
			}
			if (!isPuppet)
			{
				spriteBatch.Draw(texture, (absolutePosition + GetEndPosition()) / 2f, spriteRectangle, Color.White, absoluteAngle, textureOrigin, SpriteScale, se, 0f);
				return;
			}
			Rectangle value = spriteRectangle;
			value.X = defaultFrame * frameWidth;
			Vector2 vector;
			float rotation;
			if (isInRagdoll)
			{
				Vector2 worldPoint = boneBody.GetWorldPoint(new Vector2(length / (2f * worldScale), 0f));
				vector = new Vector2((int)(worldPoint.X * worldScale), (int)(worldPoint.Y * worldScale));
				rotation = boneBody.Rotation;
			}
			else
			{
				rotation = absoluteAngle;
				vector = (absolutePosition + GetEndPosition()) / 2f;
			}
			spriteBatch.Draw(texture, vector, value, Color.White, rotation, textureOrigin, SpriteScale, se, 0f);
		}

		public override void DrawAnimationAtFrame(Vector2 initDrawPosition, float initialAngle, string animationName, int frame, SpriteBatch spriteBatch)
		{
			if (texture == null)
			{
				return;
			}
			Skeleton skeleton = FindRootBone() as Skeleton;
			KeyFrameInfo animationInfo = skeleton.GetAnimationInfo(name, animationName, frame);
			Vector2 vector = initDrawPosition;
			float num = initialAngle;
			List<Bone> list = new List<Bone>();
			for (Bone bone = parent; bone != null; bone = bone.Parent)
			{
				list.Add(bone);
			}
			Vector2 vector2 = Vector2.Zero + vector;
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Bone bone2 = list[num2];
				KeyFrameInfo animationInfo2 = skeleton.GetAnimationInfo(bone2.Name, animationName, frame);
				vector = Vector2.Transform(animationInfo2.Position, Matrix.CreateRotationZ(num)) + vector2;
				num = MathHelper.WrapAngle(animationInfo2.Angle + num);
				vector2 = new Vector2((float)Math.Cos(num) * bone2.Length, (float)Math.Sin(num) * bone2.Length) + vector;
			}
			vector = Vector2.Transform(animationInfo.Position, Matrix.CreateRotationZ(num)) + vector2;
			num = MathHelper.WrapAngle(animationInfo.Angle + num);
			Vector2 vector3 = new Vector2((float)Math.Cos(num) * length, (float)Math.Sin(num) * length) + vector;
			Rectangle value = spriteRectangle;
			value.X = animationInfo.SpriteFrame * frameWidth;
			spriteBatch.Draw(texture, (vector + vector3) / 2f, value, Color.White, num, textureOrigin, 1f, SpriteEffects.None, 0f);
		}

		public override void DrawInitialSkeleton(SpriteBatch spriteBatch, float initialAngle)
		{
			if (texture == null)
			{
				return;
			}
			Vector2 zero = Vector2.Zero;
			float num = initialAngle;
			List<Bone> list = new List<Bone>();
			for (Bone bone = parent; bone != null; bone = bone.Parent)
			{
				list.Add(bone);
			}
			Vector2 vector = Vector2.Zero + zero;
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Bone bone2 = list[num2];
				zero = Vector2.Transform(bone2.OriginalPosition, Matrix.CreateRotationZ(num)) + vector;
				num = MathHelper.WrapAngle(bone2.BaseAngle + num);
				vector = new Vector2((float)Math.Cos(num) * bone2.Length, (float)Math.Sin(num) * bone2.Length) + zero;
			}
			zero = Vector2.Transform(originalPosition, Matrix.CreateRotationZ(num)) + vector;
			num = MathHelper.WrapAngle(baseAngle + num);
			Vector2 vector2 = new Vector2((float)Math.Cos(num) * length, (float)Math.Sin(num) * length) + zero;
			Rectangle value = spriteRectangle;
			value.X = defaultFrame * frameWidth;
			spriteBatch.Draw(texture, (zero + vector2) / 2f, value, Color.White, num, textureOrigin, 1f, SpriteEffects.None, 0f);
		}

		public override void DrawDebug(SpriteBatch spriteBatch)
		{
			Vector2 zero = Vector2.Zero;
			VertexPositionColor[] array = new VertexPositionColor[2];
			array[0].Position = new Vector3(absolutePosition, 0f);
			array[0].Color = Color.Black;
			zero = new Vector2((float)Math.Cos(absoluteAngle) * length, (float)Math.Sin(absoluteAngle) * length) + absolutePosition;
			array[1].Position = new Vector3(zero, 0f);
			array[1].Color = Color.Black;
			spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, array, 0, 1);
			for (int i = 0; i < childCount; i++)
			{
				child[i].DrawDebug(spriteBatch);
			}
		}

		public override void DrawDebugAnimationAtFrame(Vector2 initDrawPosition, float initialAngle, string animationName, int frame, SpriteBatch spriteBatch)
		{
			Skeleton skeleton = FindRootBone() as Skeleton;
			KeyFrameInfo animationInfo = skeleton.GetAnimationInfo(name, animationName, frame);
			Vector2 vector = initDrawPosition;
			float num = initialAngle;
			List<Bone> list = new List<Bone>();
			for (Bone bone = parent; bone != null; bone = bone.Parent)
			{
				list.Add(bone);
			}
			Vector2 vector2 = Vector2.Zero + vector;
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Bone bone2 = list[num2];
				KeyFrameInfo animationInfo2 = skeleton.GetAnimationInfo(bone2.Name, animationName, frame);
				vector = Vector2.Transform(animationInfo2.Position, Matrix.CreateRotationZ(num)) + vector2;
				num = MathHelper.WrapAngle(animationInfo2.Angle + num);
				vector2 = new Vector2((float)Math.Cos(num) * bone2.Length, (float)Math.Sin(num) * bone2.Length) + vector;
			}
			vector = Vector2.Transform(animationInfo.Position, Matrix.CreateRotationZ(num)) + vector2;
			num = MathHelper.WrapAngle(animationInfo.Angle + num);
			Vector2 value = new Vector2((float)Math.Cos(num) * length, (float)Math.Sin(num) * length) + vector;
			VertexPositionColor[] array = new VertexPositionColor[2];
			array[0].Position = new Vector3(vector, 0f);
			array[0].Color = Color.Black;
			array[1].Position = new Vector3(value, 0f);
			array[1].Color = Color.Black;
			spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, array, 0, 1);
		}

		public override void DrawDebugInitialSkeleton(SpriteBatch spriteBatch, float initialAngle)
		{
			Vector2 zero = Vector2.Zero;
			float num = initialAngle;
			List<Bone> list = new List<Bone>();
			for (Bone bone = parent; bone != null; bone = bone.Parent)
			{
				list.Add(bone);
			}
			Vector2 vector = Vector2.Zero + zero;
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Bone bone2 = list[num2];
				zero = Vector2.Transform(bone2.OriginalPosition, Matrix.CreateRotationZ(num)) + vector;
				num = MathHelper.WrapAngle(bone2.BaseAngle + num);
				vector = new Vector2((float)Math.Cos(num) * bone2.Length, (float)Math.Sin(num) * bone2.Length) + zero;
			}
			zero = Vector2.Transform(originalPosition, Matrix.CreateRotationZ(num)) + vector;
			num = MathHelper.WrapAngle(baseAngle + num);
			Vector2 value = new Vector2((float)Math.Cos(num) * length, (float)Math.Sin(num) * length) + zero;
			VertexPositionColor[] array = new VertexPositionColor[2];
			array[0].Position = new Vector3(zero, 0f);
			array[0].Color = Color.Black;
			array[1].Position = new Vector3(value, 0f);
			array[1].Color = Color.Black;
			spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, array, 0, 1);
		}
	}
}
