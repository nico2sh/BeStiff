using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BrainIdle : BrainState
	{
		public BrainIdle(Brain mainBrain)
			: base(mainBrain)
		{
		}

		public override void Init()
		{
		}

		public override int Update()
		{
			if (mainBrain.HeroDetected)
			{
				return 1;
			}
			return 2;
		}

		public override int HearNoise(ref Vector2 position)
		{
			mainBrain.Alert();
			mainBrain.ReferencePoint = position;
			return 4;
		}

		public override int Hit(ref Vector2 position, ref HitType hitType)
		{
			mainBrain.Alert();
			return 3;
		}
	}
}
