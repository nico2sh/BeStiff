using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	public abstract class BrainState
	{
		protected Brain mainBrain;

		protected int thisState;

		public BrainState(Brain brain)
		{
			mainBrain = brain;
		}

		public virtual int Update()
		{
			return thisState;
		}

		public virtual void Init()
		{
		}

		public virtual int HearNoise(ref Vector2 position)
		{
			return thisState;
		}

		public virtual int Hit(ref Vector2 position, ref HitType hitType)
		{
			return thisState;
		}
	}
}
