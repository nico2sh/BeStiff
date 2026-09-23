using Microsoft.Xna.Framework;

namespace SKAnimation
{
	/// <summary>
	/// Bone record as written by the older content pipeline (before the
	/// defaultFrame and ragdoll fields were added). Used to read skeleton
	/// files that shipped in that format.
	/// </summary>
	public struct LegacyBoneXMLReadHelper
	{
		public string name;

		public int drawOrder;

		public int level;

		public Vector2 position;

		public float angle;

		public float angleOffset;

		public float length;

		public int frames;

		public BoneXMLReadHelper ToCurrent()
		{
			return new BoneXMLReadHelper
			{
				name = name,
				drawOrder = drawOrder,
				level = level,
				position = position,
				angle = angle,
				angleOffset = angleOffset,
				length = length,
				frames = frames,
				defaultFrame = 0,
				ragdoll = false
			};
		}
	}
}
