using System;
using Microsoft.Xna.Framework;
using SKAnimation;

namespace Be_Stiff.Characters
{
	public class Arm
	{
		private const float fixedAngleOffset = 1f;

		private ChildBone upArmBone;

		private ChildBone loArmBone;

		private Vector2 cannonPosition;

		private bool ignoreAnimation;

		public ChildBone LoArm => loArmBone;

		public bool HasTempAnimationActive => upArmBone.IgnoreAnimation != IgnoreAnimationType.None;

		public float RelativeRotation => MathHelper.WrapAngle(upArmBone.Angle - 1f);

		public Vector2 ScreenPosition => upArmBone.AbsolutePosition;

		public Vector2 Position => GameElementsControl.ConvertScreenToWorld(upArmBone.AbsolutePosition);

		public Arm()
		{
			upArmBone = null;
			loArmBone = null;
			cannonPosition = Vector2.Zero;
		}

		public Arm(ChildBone upBone, ChildBone loBone)
		{
			upArmBone = upBone;
			loArmBone = loBone;
			cannonPosition = Vector2.Zero;
			ignoreAnimation = false;
		}

		public float GetLength()
		{
			return (upArmBone.AbsolutePosition - loArmBone.GetEndPosition()).Length();
		}

		public Skeleton GetMainSkeleton()
		{
			Bone parent = upArmBone.Parent;
			Bone bone = null;
			while (parent != null)
			{
				bone = parent;
				parent = parent.Parent;
			}
			return (Skeleton)bone;
		}

		public Vector2 HandPosition()
		{
			return loArmBone.GetEndPosition();
		}

		public float GetRotation()
		{
			return MathHelper.WrapAngle(upArmBone.AbsoluteAngle - (float)upArmBone.Side * 1f);
		}

		public void SetRotation(float angle)
		{
			angle = MathHelper.WrapAngle(angle);
			if (ignoreAnimation)
			{
				upArmBone.SetAbsoluteAngleSmooth(angle + (float)upArmBone.Side * 1f, limitByOffset: true, 2f, GameElementsControl.LastFrameTimeInMS);
			}
		}

		public Vector2 CannonPosition(float length)
		{
			Vector2 zero = Vector2.Zero;
			zero.X = length * (float)Math.Cos(GetRotation());
			zero.Y = length * (float)Math.Sin(GetRotation());
			return GameElementsControl.ConvertScreenToWorld(ScreenPosition + zero);
		}

		public void Update()
		{
			if (ignoreAnimation)
			{
				loArmBone.Angle = -1f;
			}
		}

		public void IgnoreAnimation(bool value)
		{
			if (value)
			{
				upArmBone.IgnoreAnimation = IgnoreAnimationType.Angle;
				loArmBone.IgnoreAnimation = IgnoreAnimationType.Both;
			}
			else
			{
				upArmBone.IgnoreAnimation = IgnoreAnimationType.None;
				loArmBone.IgnoreAnimation = IgnoreAnimationType.None;
			}
			ignoreAnimation = value;
		}
	}
}
