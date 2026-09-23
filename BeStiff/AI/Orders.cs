using Microsoft.Xna.Framework;

namespace Be_Stiff.AI
{
	public struct Orders
	{
		public Vector2 position;

		public SquadAction action;

		public Orders(Vector2 pos, SquadAction act)
		{
			position = pos;
			action = act;
		}
	}
}
