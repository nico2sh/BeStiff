using System;
using Microsoft.Xna.Framework;

namespace EasyStorage
{
	public sealed class SaveDeviceEventArgs : EventArgs
	{
		public SaveDeviceEventResponse Response { get; set; }

		public PlayerIndex? PlayerToPrompt { get; set; }
	}
}
