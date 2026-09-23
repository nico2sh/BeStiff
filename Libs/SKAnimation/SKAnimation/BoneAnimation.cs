using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SKAnimation
{
	public class BoneAnimation
	{
		private static int MAX_KFCOUNT = 20;

		public string Name;

		private List<KeyFrame> keyFrames;

		private int totalKeyFrames;

		private int currentKeyFramePos;

		private int nextKeyFramePos;

		private double time;

		private float timePercentage;

		private bool enteredInAnimation;

		private bool enteredInKeyFrame;

		private bool loop;

		private bool endedLoop;

		private bool dontInterrupt;

		private bool overrideable;

		private Dictionary<string, Vector3> enteringInfo;

		private bool initialized;

		public List<KeyFrame> KeyFrames => keyFrames;

		public Dictionary<string, Vector3> EnteringInfo => enteringInfo;

		public bool EndedLoop => endedLoop;

		public bool DontInterrupt
		{
			get
			{
				return dontInterrupt;
			}
			set
			{
				dontInterrupt = value;
			}
		}

		public bool Loop
		{
			get
			{
				return loop;
			}
			set
			{
				if (totalKeyFrames > 1)
				{
					loop = value;
				}
				else
				{
					loop = false;
				}
			}
		}

		public bool Overrideable
		{
			get
			{
				return overrideable;
			}
			set
			{
				overrideable = value;
			}
		}

		public int CurrentKeyFrame => currentKeyFramePos;

		public BoneAnimation(string name, bool hasToLoop, bool canNotBeInterrupted)
		{
			Name = name;
			keyFrames = new List<KeyFrame>();
			currentKeyFramePos = -1;
			totalKeyFrames = 0;
			time = 0.0;
			loop = hasToLoop;
			dontInterrupt = canNotBeInterrupted;
			overrideable = false;
			enteredInAnimation = false;
			enteredInKeyFrame = false;
			endedLoop = false;
			enteringInfo = new Dictionary<string, Vector3>();
			initialized = false;
		}

		public BoneAnimation(string name, bool hasToLoop)
		{
			Name = name;
			keyFrames = new List<KeyFrame>();
			currentKeyFramePos = -1;
			totalKeyFrames = 0;
			time = 0.0;
			loop = hasToLoop;
			dontInterrupt = false;
			overrideable = false;
			enteredInAnimation = false;
			enteredInKeyFrame = false;
			endedLoop = false;
			enteringInfo = new Dictionary<string, Vector3>();
			initialized = false;
		}

		public void SetFrame(int newFrame)
		{
			if (newFrame == 0)
			{
				Reset();
				return;
			}
			if (newFrame < 0)
			{
				newFrame = 0;
			}
			if (newFrame >= totalKeyFrames)
			{
				newFrame = totalKeyFrames - 1;
			}
			time = 0.0;
			currentKeyFramePos = -1;
			nextKeyFramePos = newFrame;
			endedLoop = false;
			enteredInAnimation = false;
		}

		public bool AtLastKeyFrame()
		{
			return currentKeyFramePos == totalKeyFrames - 1;
		}

		public bool KeyFrameStarted(int pos)
		{
			if (pos == currentKeyFramePos)
			{
				return enteredInKeyFrame;
			}
			return false;
		}

		public void AddKeyFrame(KeyFrame keyFrame)
		{
			totalKeyFrames++;
			keyFrames.Add(keyFrame);
			_ = keyFrame.KeyFrameInfo.Count;
			if (!initialized)
			{
				foreach (string key in keyFrame.KeyFrameInfo.Keys)
				{
					enteringInfo.Add(key, Vector3.Zero);
				}
				initialized = true;
			}
			else
			{
				foreach (string key2 in keyFrame.KeyFrameInfo.Keys)
				{
					if (!enteringInfo.ContainsKey(key2))
					{
						throw new Exception("There is a keyframe containing a missing bone in other keyframes");
					}
				}
			}
			nextKeyFramePos = ((currentKeyFramePos != totalKeyFrames - 1) ? (currentKeyFramePos + 1) : 0);
		}

		public void AddKeyFrameAt(int pos, KeyFrame keyFrame)
		{
			totalKeyFrames++;
			keyFrames.Insert(pos, keyFrame);
			_ = keyFrame.KeyFrameInfo.Count;
			if (!initialized)
			{
				foreach (string key in keyFrame.KeyFrameInfo.Keys)
				{
					enteringInfo.Add(key, Vector3.Zero);
				}
				initialized = true;
			}
			else
			{
				foreach (string key2 in keyFrame.KeyFrameInfo.Keys)
				{
					if (!enteringInfo.ContainsKey(key2))
					{
						throw new Exception("There is a keyframe containing a missing bone in other keyframes");
					}
				}
			}
			nextKeyFramePos = ((currentKeyFramePos != totalKeyFrames - 1) ? (currentKeyFramePos + 1) : 0);
		}

		public void RemoveKeyFrame(KeyFrame keyFrame)
		{
			keyFrames.Remove(keyFrame);
			totalKeyFrames--;
		}

		public void RemoveKeyFrame(int keyFramePos)
		{
			keyFrames.RemoveAt(keyFramePos);
			totalKeyFrames--;
			Reset();
		}

		public bool MoveKeyFrameUp(int kfPos)
		{
			if (kfPos <= 0)
			{
				return false;
			}
			KeyFrame value = keyFrames[kfPos];
			keyFrames[kfPos] = keyFrames[kfPos - 1];
			keyFrames[kfPos - 1] = value;
			return true;
		}

		public bool MoveKeyFrameDown(int kfPos)
		{
			if (kfPos >= totalKeyFrames - 1)
			{
				return false;
			}
			KeyFrame value = keyFrames[kfPos];
			keyFrames[kfPos] = keyFrames[kfPos + 1];
			keyFrames[kfPos + 1] = value;
			return true;
		}

		public void Reset()
		{
			time = 0.0;
			currentKeyFramePos = -1;
			nextKeyFramePos = ((currentKeyFramePos != totalKeyFrames - 1) ? (currentKeyFramePos + 1) : 0);
			endedLoop = false;
			enteredInAnimation = false;
		}

		public void Update(ChildBone[] bones, int bonesNumber, double frameTimeInMs)
		{
			if (totalKeyFrames > 0)
			{
				KeyFrame nextKeyFrame = UpdateKeyFrameNumber(frameTimeInMs);
				for (int i = 0; i < bonesNumber; i++)
				{
					UpdateBone(bones[i], nextKeyFrame);
				}
				if (!enteredInAnimation)
				{
					enteredInAnimation = true;
				}
			}
		}

		public void Update(ChildBone[] bones, int bonesNumber, double frameTimeInMs, ref BoneAnimation partialAnimation)
		{
			partialAnimation.Update(bones, bonesNumber, frameTimeInMs);
			bool[] array = new bool[bonesNumber];
			foreach (string key in partialAnimation.enteringInfo.Keys)
			{
				for (int i = 0; i < bonesNumber; i++)
				{
					if (bones[i].Name == key)
					{
						array[i] = true;
					}
				}
			}
			if (totalKeyFrames <= 0)
			{
				return;
			}
			KeyFrame nextKeyFrame = UpdateKeyFrameNumber(frameTimeInMs);
			for (int j = 0; j < bonesNumber; j++)
			{
				if (!array[j])
				{
					UpdateBone(bones[j], nextKeyFrame);
				}
			}
			if (!enteredInAnimation)
			{
				enteredInAnimation = true;
			}
		}

		private void UpdateBone(ChildBone currentBone, KeyFrame nextKeyFrame)
		{
			if (currentBone.IgnoreAnimation == IgnoreAnimationType.Both || !nextKeyFrame.KeyFrameInfo.TryGetValue(currentBone.Name, out var value))
			{
				return;
			}
			Vector2 position = value.Position;
			float angle = value.Angle;
			int spriteFrame = value.SpriteFrame;
			if (currentKeyFramePos != -1)
			{
				KeyFrame keyFrame = keyFrames[currentKeyFramePos];
				if (keyFrame.KeyFrameInfo.TryGetValue(currentBone.Name, out var value2))
				{
					if (enteredInKeyFrame)
					{
						enteringInfo[currentBone.Name] = new Vector3(value2.Position, value2.Angle);
					}
				}
				else if (enteredInKeyFrame)
				{
					enteringInfo[currentBone.Name] = new Vector3(value2.Position, currentBone.Angle);
				}
			}
			else if (!enteredInAnimation)
			{
				enteringInfo[currentBone.Name] = new Vector3(currentBone.Position, currentBone.Angle);
			}
			Vector3 vector = enteringInfo[currentBone.Name];
			float num = vector.Z;
			Vector2 value3 = new Vector2(vector.X, vector.Y);
			if (currentBone.AngleOffset == 0f || currentBone.AngleOffset == (float)Math.PI)
			{
				angle = num - MathHelper.WrapAngle(num - angle);
			}
			else
			{
				angle = currentBone.BaseAngle - MathHelper.WrapAngle(currentBone.BaseAngle - angle);
				num = currentBone.BaseAngle - MathHelper.WrapAngle(currentBone.BaseAngle - num);
				float num2 = MathHelper.WrapAngle(num - angle);
				if (Math.Round(num2, 3) == 0.0)
				{
					angle = num;
				}
				else
				{
					num2 = (float)Math.Round(num2, 3);
					float num3 = currentBone.BaseAngle - currentBone.AngleOffset;
					float num4 = currentBone.BaseAngle + currentBone.AngleOffset;
					if (num2 < 0f)
					{
						if (num3 > angle)
						{
							angle += (float)Math.PI * 2f;
						}
					}
					else if (num2 > 0f)
					{
						if (num4 < angle)
						{
							angle -= (float)Math.PI * 2f;
						}
					}
					else
					{
						angle = num - MathHelper.WrapAngle(num - angle);
					}
				}
			}
			if (currentBone.IgnoreAnimation != IgnoreAnimationType.Position)
			{
				currentBone.Position = Vector2.Lerp(value3, position, timePercentage);
			}
			if (currentBone.IgnoreAnimation != IgnoreAnimationType.Angle)
			{
				currentBone.Angle = MathHelper.Lerp(num, angle, timePercentage);
			}
			currentBone.SetCurrentKeyFrame(spriteFrame);
		}

		private KeyFrame UpdateKeyFrameNumber(double frameTimeInMs)
		{
			KeyFrame keyFrame = keyFrames[nextKeyFramePos];
			time += frameTimeInMs;
			double num = keyFrame.Time;
			if (time > num)
			{
				if (AtLastKeyFrame())
				{
					endedLoop = true;
				}
				else
				{
					endedLoop = false;
				}
				time -= keyFrame.Time;
				currentKeyFramePos = nextKeyFramePos;
				if (AtLastKeyFrame())
				{
					if (!loop)
					{
						nextKeyFramePos = currentKeyFramePos;
					}
					else if (totalKeyFrames > 1)
					{
						nextKeyFramePos = 1;
					}
					else
					{
						nextKeyFramePos = 0;
					}
				}
				else
				{
					nextKeyFramePos = currentKeyFramePos + 1;
				}
				enteredInKeyFrame = true;
			}
			else
			{
				enteredInKeyFrame = false;
			}
			timePercentage = (float)(time / keyFrames[nextKeyFramePos].Time);
			return keyFrame;
		}
	}
}
