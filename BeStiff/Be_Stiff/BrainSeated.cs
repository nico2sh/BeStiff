using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BrainSeated : BrainState
	{
		private double startTime;

		private Vector2 pointToAim;

		private WorldObjectData[] touchingWorldObjects;

		private int touchingWorldObjectsNum;

		public BrainSeated(Brain mainBrain)
			: base(mainBrain)
		{
			thisState = 6;
		}

		public override void Init()
		{
			pointToAim = new Vector2(mainBrain.SittingSpots[mainBrain.CurrentPatrolPoint], 0f);
			startTime = GameElementsControl.CurrentTimeInMS;
			bool flag = false;
			mainBrain.GetTouchingObjects(out touchingWorldObjects, out touchingWorldObjectsNum);
			for (int i = 0; i < touchingWorldObjectsNum; i++)
			{
				if (touchingWorldObjects[i].Object is ISeat currentSeat)
				{
					mainBrain.SetCurrentSeat(currentSeat);
					flag = true;
					i = touchingWorldObjectsNum;
				}
			}
			if (!flag)
			{
				mainBrain.SetCurrentSeat(null);
			}
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
				return 4;
			}
			if (mainBrain.AimToRelativePoint(pointToAim, 100f) && startTime + 10000.0 <= GameElementsControl.CurrentTimeInMS)
			{
				return 2;
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
			return thisState;
		}
	}
}
