using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class BrainDead : BrainState
	{
		private double _timeOfDecease;

		public BrainDead(Brain mainBrain)
			: base(mainBrain)
		{
			thisState = 7;
		}

		public override void Init()
		{
			_timeOfDecease = GameElementsControl.CurrentTimeInMS;
		}

		public override int Update()
		{
			double num = GameElementsControl.CurrentTimeInMS - _timeOfDecease;
			_ = 5000.0;
			return thisState;
		}

		public override int Hit(ref Vector2 position, ref HitType hitType)
		{
			return thisState;
		}

		public override int HearNoise(ref Vector2 position)
		{
			return thisState;
		}
	}
}
