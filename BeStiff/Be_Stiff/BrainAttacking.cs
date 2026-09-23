using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BrainAttacking : BrainState
	{
		public BrainAttacking(Brain mainBrain)
			: base(mainBrain)
		{
			thisState = 1;
		}

		public override void Init()
		{
		}

		public override int Update()
		{
			if (mainBrain.HeroDetected)
			{
				float range;
				switch (mainBrain.Stance)
				{
				case Stance.Aggressive:
					range = Math.Min(mainBrain.WeaponRange / 2f, 2f);
					break;
				case Stance.Defensive:
					range = mainBrain.WeaponRange * 0.8f;
					break;
				default:
					range = Math.Min(mainBrain.WeaponRange * 0.7f, 5f);
					break;
				}
				float num = (mainBrain.HeroPosition - mainBrain.OwnerPosition).Length();
				if (num <= mainBrain.WeaponRange && mainBrain.AimingAtHero)
				{
					mainBrain.ShootAtHero();
				}
				mainBrain.MoveToDestinationPoint(mainBrain.HeroPosition, range);
				return thisState;
			}
			mainBrain.ReferencePoint = mainBrain.HeroPosition;
			return 4;
		}

		public override int HearNoise(ref Vector2 position)
		{
			return thisState;
		}

		public override int Hit(ref Vector2 position, ref HitType hitType)
		{
			return thisState;
		}
	}
}
