using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	public abstract class Projectile : WorldObject
	{
		protected bool active;

		protected double lastShotTime;

		public ProjectileType ProjectileType;

		public bool Active => active;

		public Projectile()
		{
			active = false;
			lastShotTime = 0.0;
		}

		public virtual bool Shoot(Vector2 position, float angle, Vector2 pretendedPosition)
		{
			if (!active)
			{
				active = true;
				lastShotTime = GameElementsControl.CurrentTimeInMS;
				return true;
			}
			return false;
		}
	}
}
