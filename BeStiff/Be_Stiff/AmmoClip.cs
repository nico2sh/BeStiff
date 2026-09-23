using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	public class AmmoClip
	{
		private Projectile[] ammoClip;

		private int clipPos;

		private int clipSize;

		private Human owner;

		private bool canReload;

		private int totalAmmo;

		public AmmoClip(int size, ProjectileType type, Human theOwner)
		{
			clipSize = size;
			ammoClip = new Projectile[clipSize];
			clipPos = 0;
			owner = theOwner;
			switch (type)
			{
			case ProjectileType.Bullet:
			{
				for (int j = 0; j < clipSize; j++)
				{
					ammoClip[j] = new Bullet(owner.Name);
					ammoClip[j].Load();
					totalAmmo = -1;
					canReload = true;
				}
				break;
			}
			case ProjectileType.Grenade:
			{
				for (int i = 0; i < clipSize; i++)
				{
					ammoClip[i] = new Grenade(owner.Name);
					ammoClip[i].Load();
					canReload = false;
				}
				break;
			}
			}
		}

		public void SetTotalAmmo(int total)
		{
			if (total > clipSize)
			{
				totalAmmo = total - clipSize;
				return;
			}
			clipPos = clipSize - total;
			totalAmmo = 0;
		}

		public void Shoot(Vector2 position, float rotation, Vector2 pretendedPosition)
		{
			if (clipPos < clipSize && ammoClip[clipPos].Shoot(position, rotation, pretendedPosition))
			{
				clipPos++;
			}
		}

		public Projectile GetNextProjectile()
		{
			if (clipPos < clipSize)
			{
				return ammoClip[clipPos];
			}
			return null;
		}

		private bool CanReload()
		{
			if (canReload)
			{
				return totalAmmo != 0;
			}
			return false;
		}

		public bool Reload()
		{
			if (CanReload())
			{
				if (totalAmmo != -1)
				{
					int num = Globals.Clamp(totalAmmo - clipPos, 0, totalAmmo);
					int num2 = totalAmmo - num;
					totalAmmo = num;
					clipPos -= num2;
				}
				else
				{
					clipPos = 0;
				}
				return true;
			}
			return false;
		}

		public int RemainingClipAmmo()
		{
			return clipSize - clipPos;
		}

		public int RemainingTotalAmmo()
		{
			if (totalAmmo == -1)
			{
				return 0;
			}
			return totalAmmo;
		}

		public int Refill(int number)
		{
			if (canReload)
			{
				totalAmmo += number;
				return 0;
			}
			int num = Globals.Clamp(number - clipPos, 0, number);
			int num2 = number - num;
			clipPos -= num2;
			return num;
		}
	}
}
