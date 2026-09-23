using System.Linq;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BrainScanning : BrainState
	{
		private double _startTime;

		private double _stopTime;

		private bool paused;

		private Vector2[] pointScanning;

		private int posPointScanning;

		private int clockwise;

		private int countTurns;

		public BrainScanning(Brain mainBrain)
			: base(mainBrain)
		{
			thisState = 3;
			pointScanning = new Vector2[5];
			ref Vector2 reference = ref pointScanning[0];
			reference = new Vector2(1f, -1f);
			ref Vector2 reference2 = ref pointScanning[1];
			reference2 = new Vector2(1f, 0f);
			ref Vector2 reference3 = ref pointScanning[2];
			reference3 = new Vector2(0f, 1f);
			ref Vector2 reference4 = ref pointScanning[3];
			reference4 = new Vector2(-1f, 0f);
			ref Vector2 reference5 = ref pointScanning[4];
			reference5 = new Vector2(-1f, -1f);
		}

		public override void Init()
		{
			mainBrain.StopWalking();
			if (mainBrain.SideLooking == Side.Left)
			{
				posPointScanning = 4;
				clockwise = -1;
			}
			else
			{
				posPointScanning = 0;
				clockwise = 1;
			}
			countTurns = 0;
			_startTime = GameElementsControl.CurrentTimeInMS;
			paused = false;
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
			}
			else if (mainBrain.AimToRelativePoint(pointScanning[posPointScanning], 1000f))
			{
				if (paused)
				{
					if (_stopTime + 500.0 <= GameElementsControl.CurrentTimeInMS)
					{
						paused = false;
					}
				}
				else
				{
					if (posPointScanning + clockwise >= pointScanning.Count() || posPointScanning + clockwise < 0)
					{
						clockwise = -clockwise;
						countTurns++;
					}
					posPointScanning += clockwise;
					if (HasToStopForASecond())
					{
						paused = true;
						_stopTime = GameElementsControl.CurrentTimeInMS;
					}
				}
			}
			if (countTurns >= 2)
			{
				return 2;
			}
			return thisState;
		}

		private bool HasToStopForASecond()
		{
			if (posPointScanning != 1)
			{
				return posPointScanning == 3;
			}
			return true;
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
