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
		/// Time in ms an enemy holding this weapon waits after an attack
		/// before the next one, so the hero has room to fight back. Called
		/// once per attack, so it may vary.
		/// </summary>
		public virtual double NextEnemyAttackInterval()
		{
			return 1000.0;
		}

		/// <summary>
		/// Chance (0-1) that an enemy's attack is a double one: a second
		/// Shoot() while the first is under way, for weapons that chain.
		/// </summary>
		public virtual double EnemyDoubleAttackChance => 0.0;

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
