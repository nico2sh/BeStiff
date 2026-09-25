using System;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Weapons
{
	public class Pistol : Weapon
	{
		protected const float pistolLength = 30f;

		private GameSprite gunSprite;

		private GameSpriteVariables gunSpriteVariables;

		protected AmmoClip ammoClip;

		private double startReloadTime;

		private bool reloading;

		private double accuracy;

		protected Human owner;

		public override double NextEnemyAttackInterval()
		{
			return 800.0 + GameElementsControl.Random.NextDouble() * 400.0;
		}

		public Pistol(Arm arm, double acc)
			: base(arm)
		{
			base.WeaponType = WeaponType.Pistol;
			startReloadTime = 0.0;
			reloading = false;
			weaponRange = 15f;
			accuracy = acc;
		}

		public void Init(Human own)
		{
			owner = own;
			ammoClip = new AmmoClip(6, ProjectileType.Bullet, owner);
		}

		public override void Load()
		{
			GameElementsControl.LoadSprite("pistol", "sprites\\guns\\pistol\\pistol");
			gunSprite = GameElementsControl.GetSprite("pistol");
			gunSpriteVariables = GameSprite.GetDefaultVariables();
			GameElementsControl.ScreenManager.AudioManager.LoadSound("reload", "audio\\noises\\reload");
		}

		public override void Update()
		{
			if (reloading && GameElementsControl.CurrentTimeInMS > startReloadTime + 1000.0)
			{
				reloading = false;
				ownerArm.IgnoreAnimation(value: true);
			}
		}

		public override bool ManualAim(out float angle)
		{
			angle = 0f;
			return true;
		}

		public override void UpdateDrawSide(Side side)
		{
			if (side == Side.Left)
			{
				gunSpriteVariables.spriteEffects = SpriteEffects.FlipVertically;
			}
			else
			{
				gunSpriteVariables.spriteEffects = SpriteEffects.None;
			}
			gunSpriteVariables.rotation = ownerArm.GetRotation();
		}

		public override void Holster()
		{
			ownerArm.IgnoreAnimation(value: true);
		}

		public override void SetArmAnimation()
		{
			ownerArm.IgnoreAnimation(value: true);
		}

		public override void Shoot()
		{
			if (!reloading)
			{
				double num = (0.5 - GameElementsControl.Random.NextDouble()) * Math.PI / 4.0 * (1.0 - accuracy);
				ammoClip.Shoot(ownerArm.Position, ownerArm.GetRotation() + (float)num, ownerArm.CannonPosition(30f));
				if (ammoClip.RemainingClipAmmo() == 0)
				{
					owner.Reload();
				}
			}
		}

		public override bool Reload()
		{
			if (!reloading)
			{
				if (ammoClip.Reload())
				{
					ownerArm.IgnoreAnimation(value: false);
					startReloadTime = GameElementsControl.CurrentTimeInMS;
					reloading = true;
					GameElementsControl.NoiseManager.AddNoise("reload", ownerArm.Position);
					return true;
				}
				return false;
			}
			return false;
		}

		public override void Draw()
		{
			gunSprite.Draw(ownerArm.HandPosition(), gunSpriteVariables);
		}
	}
}
