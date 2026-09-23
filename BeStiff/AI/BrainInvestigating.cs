using Microsoft.Xna.Framework;

namespace Be_Stiff.AI
{
	internal class BrainInvestigating : BrainState
	{
		public BrainInvestigating(Brain mainBrain)
			: base(mainBrain)
		{
			thisState = 4;
		}

		public override void Init()
		{
		}

		public override int Update()
		{
			if (mainBrain.Alerted)
			{
				Vector2 point = mainBrain.ReferencePoint;
				mainBrain.AimToPoint(ref point);
				if (mainBrain.MoveToDestinationPoint(mainBrain.ReferencePoint, 0f))
				{
					return 3;
				}
			}
			else if (mainBrain.MoveToDestinationPoint(mainBrain.ReferencePoint, 0f))
			{
				return 3;
			}
			if (mainBrain.HeroDetected)
			{
				return 1;
			}
			return thisState;
		}

		public override int HearNoise(ref Vector2 position)
		{
			mainBrain.Alert();
			return thisState;
		}

		public override int Hit(ref Vector2 position, ref HitType hitType)
		{
			mainBrain.Alert();
			return thisState;
		}
	}
}
