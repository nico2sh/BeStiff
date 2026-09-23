using System.Collections.Generic;

namespace Be_Stiff.Physics
{
	internal class rayDataComparer : IComparer<rayData>
	{
		int IComparer<rayData>.Compare(rayData a, rayData b)
		{
			float num = a.angle - b.angle;
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
