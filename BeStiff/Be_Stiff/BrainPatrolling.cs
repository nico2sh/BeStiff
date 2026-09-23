using System.Linq;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BrainPatrolling : BrainState
	{
		private const double _timeAtEachPatrolPoint = 1000.0;

		private int nextPatrolPoint;

		private double _timeAtPatrolPoint;

		private bool _reachedNextPoint;

		public BrainPatrolling(Brain mainBrain)
			: base(mainBrain)
		{
			thisState = 2;
			_timeAtPatrolPoint = GameElementsControl.CurrentTimeInMS;
			nextPatrolPoint = 0;
			mainBrain.CurrentPatrolPoint = 0;
		}

		public override void Init()
		{
			_reachedNextPoint = false;
		}

		public override int Update()
		{
			if (mainBrain.HeroDetected)
			{
				return 1;
			}
			if (mainBrain.Alerted)
			{
				Vector2 point = mainBrain.HeroPosition;
				mainBrain.AimToPoint(ref point);
				mainBrain.StopWalking();
			}
			else
			{
				if (_reachedNextPoint)
				{
					if (mainBrain.HasToScan())
					{
						return 3;
					}
					return 6;
				}
				if (mainBrain.MoveToDestinationPoint(mainBrain.PatrolPath[nextPatrolPoint], 0f))
				{
					_reachedNextPoint = true;
					mainBrain.CurrentPatrolPoint = nextPatrolPoint;
					nextPatrolPoint = (nextPatrolPoint + 1) % mainBrain.PatrolPath.Count();
					_timeAtPatrolPoint = GameElementsControl.CurrentTimeInMS;
				}
			}
			return thisState;
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
			int num = thisState;
			HitType hitType2 = hitType;
			if (hitType2 == HitType.Blunt)
			{
				Vector2 referencePoint = Vector2.Normalize(position - mainBrain.OwnerPosition) + mainBrain.OwnerPosition;
				mainBrain.ReferencePoint = referencePoint;
				num = 4;
			}
			else
			{
				num = 4;
				mainBrain.ReferencePoint = GameElementsControl.Hero.Position;
			}
			return num;
		}
	}
}
