using Microsoft.Xna.Framework;
using SKAnimation;

namespace Be_Stiff.Weapons
{
	internal class HeroNoWeapon : NoWeapon, IWeaponHero
	{
		private GameSprite uiSprite;

		private bool chainTwoPunches;

		private bool inSecondPunch;

		public GameSprite UISprite => uiSprite;

		public int InfoNumber => -1;

		public int SecondaryInfoNumber => -1;

		public HeroNoWeapon(Arm arm, ChildBone otherArm)
			: base(arm, otherArm)
		{
			minDelayBetweenPunch = 350.0;
		}

		public override void Load()
		{
			GameElementsControl.LoadSprite("fistUI", "sprites\\ui\\fistui");
			uiSprite = GameElementsControl.GetSprite("fistUI");
			base.Load();
		}

		public void HandleInput(InputHelper input, PlayerIndex? playerIndex)
		{
			if (input.ControlSecondaryShoot(playerIndex))
			{
				GameElementsControl.Hero.MustDefend = true;
			}
			else
			{
				GameElementsControl.Hero.MustDefend = false;
			}
		}
	}
}
