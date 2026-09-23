using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	public abstract class Weapon
	{
		private WeaponType name;

		protected Arm ownerArm;

		protected float weaponRange;

		public WeaponType WeaponType
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public float WeaponRange
		{
			get
			{
				return weaponRange;
			}
			set
			{
				weaponRange = value;
			}
		}

		protected Weapon(Arm arm)
		{
			ownerArm = arm;
			weaponRange = 0f;
		}

		public virtual StateEnum GetNewState(StateEnum curState)
		{
			return curState;
		}

		public virtual void Holster()
		{
		}

		public virtual void SetArmAnimation()
		{
		}

		public virtual bool Reload()
		{
			return false;
		}

		public virtual void Shoot()
		{
		}

		public virtual void SecondaryShoot(bool active)
		{
		}

		public virtual void Load()
		{
		}

		public virtual bool ManualAim(out float angle)
		{
			angle = 0f;
			return true;
		}

		public virtual void Update()
		{
		}

		public virtual Vector2 getOrigin()
		{
			return Vector2.Zero;
		}

		public virtual void Draw()
		{
		}

		public virtual void UpdateDrawSide(Side side)
		{
		}
	}
}
