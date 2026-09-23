using System.Collections.Generic;

namespace FarseerPhysics.Common.PhysicsLogic
{
	/// <summary>
	/// This is a comprarer used for 
	/// detecting angle difference between rays
	/// </summary>
	internal class RayDataComparer : IComparer<float>
	{
		int IComparer<float>.Compare(float a, float b)
		{
			float num = a - b;
			if (num > 0f)
			{
				return 1;
			}
			if (num < 0f)
			{
				return -1;
			}
			return 0;
		}
	}
}
