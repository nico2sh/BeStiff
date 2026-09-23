using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class PlayerIndexEventArgs : EventArgs
	{
		private PlayerIndex playerIndex;

		public PlayerIndex PlayerIndex => playerIndex;

		public PlayerIndexEventArgs(PlayerIndex playerIndex)
		{
			this.playerIndex = playerIndex;
		}
	}
}
