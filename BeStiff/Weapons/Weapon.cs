using Microsoft.Xna.Framework;

namespace Be_Stiff.Weapons
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

		/// <summary>
		/// False for weapons that swing over a lying or sliding target (the
		/// target dodges by ducking), so the AI does not attack one.
		/// </summary>
		public virtual bool CanHitLyingTarget => true;

		/// <summary>
		/// Minimum time in ms between two attacks by an enemy holding this
		/// weapon, so the hero has room to fight back.
		/// </summary>
		public virtual double EnemyAttackInterval => 1000.0;

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
