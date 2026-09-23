using System.Collections.Generic;

namespace SKAnimation
{
	public class KeyFrame
	{
		public double Time;

		public Dictionary<string, KeyFrameInfo> KeyFrameInfo = new Dictionary<string, KeyFrameInfo>();
	}
}
