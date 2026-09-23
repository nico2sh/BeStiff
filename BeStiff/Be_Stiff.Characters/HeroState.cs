namespace Be_Stiff.Characters
{
	public abstract class HeroState : State
	{
		protected Hero hero;

		public HeroState(StateMachine sm, Hero h)
			: base(sm)
		{
			hero = h;
		}

		protected override bool Entering()
		{
			return true;
		}

		protected override bool Exiting()
		{
			return true;
		}
	}
}
