using System;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;

namespace Be_Stiff.Weapons
{
	public class HeroPistol : Pistol, IWeaponHero
	{
		public static float GrenadeLaunchSpeed = 10f;

		protected GameSprite uiSprite;

		private GameSprite redDot;

		private LineEmitter laserSight;

		private AmmoClip secondaryClip;

		private bool secondaryActive;

		private Vector2 targetPosition;

		public AmmoClip AmmoClip => ammoClip;

		public AmmoClip SecondaryAmmoClip => secondaryClip;

		public GameSprite UISprite => uiSprite;

		public int InfoNumber
		{
			get
			{
				if (secondaryActive)
				{
					return secondaryClip.RemainingClipAmmo();
				}
				return ammoClip.RemainingClipAmmo();
			}
		}

		public int SecondaryInfoNumber => ammoClip.RemainingTotalAmmo();

		public HeroPistol(Arm arm)
			: base(arm, 1.0)
		{
		}

		public override void Load()
		{
			base.Load();
			GameElementsControl.LoadSprite("pistolUI", "sprites\\ui\\pistolui");
			GameElementsControl.LoadSprite("redDot", "sprites\\guns\\pistol\\reddot");
			uiSprite = GameElementsControl.GetSprite("pistolUI");
			redDot = GameElementsControl.GetSprite("redDot");
			laserSight = (LineEmitter)GameElementsControl.Particlesmanager.getLaserSightEmitter();
			targetPosition = Vector2.Zero;
			ammoClip.SetTotalAmmo(10);
			secondaryClip = new AmmoClip(5, ProjectileType.Grenade, owner);
			secondaryClip.SetTotalAmmo(0);
		}

		public void RefillFromPistol(HeroPistol pistol)
		{
			ammoClip.Refill(pistol.AmmoClip.RemainingClipAmmo() + pistol.AmmoClip.RemainingTotalAmmo());
			secondaryClip.Refill(pistol.SecondaryAmmoClip.RemainingClipAmmo() + pistol.SecondaryAmmoClip.RemainingTotalAmmo());
			if (ammoClip.RemainingClipAmmo() == 0)
			{
				owner.Reload();
			}
		}

		public void RefillBullets(int bullets)
		{
			ammoClip.Refill(bullets);
			if (ammoClip.RemainingClipAmmo() == 0)
			{
				owner.Reload();
			}
		}

		public int RefillGrenades(int grenades)
		{
			int result = secondaryClip.Refill(grenades);
			if (ammoClip.RemainingClipAmmo() == 0)
			{
				owner.Reload();
			}
			return result;
		}

		public override void Update()
		{
			base.Update();
			if (!secondaryActive)
			{
				SetCrossHairPosition();
				Vector2 vector = targetPosition - ownerArm.CannonPosition(30f);
				float num = GameElementsControl.ConvertWorldToScreen(vector).Length();
				laserSight.Length = num;
				laserSight.ReleaseQuantity = ((int)(num / 5f) + 1) * 3;
				laserSight.Angle = vector.GetAngle();
				laserSight.Trigger(GameElementsControl.ConvertWorldToScreen((targetPosition + ownerArm.CannonPosition(30f)) / 2f));
			}
		}

		private void SetCrossHairPosition()
		{
			float rotation = ownerArm.GetRotation();
			Vector2 point = (targetPosition = new Vector2(ownerArm.Position.X + weaponRange * (float)Math.Cos(rotation), ownerArm.Position.Y + weaponRange * (float)Math.Sin(rotation)));
			if (!RayCastCallBacks.RayCastPistolCrossHair(ownerArm.Position, point, owner.Name, out targetPosition))
			{
				targetPosition = point;
			}
		}

		public override void Shoot()
		{
			if (secondaryActive)
			{
				secondaryClip.Shoot(ownerArm.Position, ownerArm.GetRotation(), ownerArm.CannonPosition(30f));
			}
			else
			{
				base.Shoot();
			}
		}

		public override void SecondaryShoot(bool active)
		{
			secondaryActive = active;
		}

		public void HandleInput(InputHelper input, PlayerIndex? playerIndex)
		{
			if (input.ControlSecondaryShoot(playerIndex))
			{
				SecondaryShoot(active: true);
			}
			else
			{
				SecondaryShoot(active: false);
			}
			if (input.ControlShoot(playerIndex))
			{
				Shoot();
			}
			else if (input.ControlReload(playerIndex))
			{
				owner.Reload();
			}
		}

		public override void Draw()
		{
			base.Draw();
			if (secondaryActive)
			{
				float num = (float)Math.Cos(0.0 - (double)ownerArm.GetRotation()) * GrenadeLaunchSpeed;
				float num2 = (float)Math.Sin(0.0 - (double)ownerArm.GetRotation()) * GrenadeLaunchSpeed;
				Vector2 zero = Vector2.Zero;
				for (int i = 0; i < 5; i++)
				{
					float num3 = (float)i / 6f;
					zero.X = num3 * num;
					zero.Y = num3 * num2 + (float)Math.Pow(num3, 2.0) * GameElementsControl.Gravity.Y / 2f;
					redDot.Draw(GameElementsControl.ConvertWorldToScreen(ownerArm.Position + zero));
				}
			}
		}
	}
}
