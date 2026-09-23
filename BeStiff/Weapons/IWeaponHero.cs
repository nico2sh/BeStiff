using Microsoft.Xna.Framework;

namespace Be_Stiff.Weapons
{
	public interface IWeaponHero
	{
		GameSprite UISprite { get; }

		int InfoNumber { get; }

		int SecondaryInfoNumber { get; }

		void HandleInput(InputHelper input, PlayerIndex? playerIndex);
	}
}
