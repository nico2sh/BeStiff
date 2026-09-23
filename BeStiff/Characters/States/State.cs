namespace Be_Stiff.Characters.States
{
	public abstract class State
	{
		protected StateMachine stateMachine;

		private bool useExitAnimation;

		public Stage Stage { get; private set; }

		public int StateID { get; protected set; }

		public string Name { get; private set; }

		internal abstract void OnEnter();

		protected abstract bool Entering();

		protected abstract void During();

		protected abstract bool Exiting();

		internal abstract void OnExit();

		protected abstract void CheckRules();

		public State(StateMachine baseStateMachine)
		{
			Stage = Stage.Enter;
			useExitAnimation = true;
			stateMachine = baseStateMachine;
		}

		internal bool Update()
		{
			CheckRules();
			switch (Stage)
			{
			case Stage.Enter:
				if (Entering())
				{
					Stage = Stage.During;
					During();
				}
				break;
			case Stage.During:
				During();
				break;
			case Stage.Exit:
				if (useExitAnimation)
				{
					return Exiting();
				}
				return true;
			}
			return false;
		}

		public void SwitchState(int newState, bool useExitTransition)
		{
			Stage = Stage.Exit;
			useExitAnimation = useExitTransition;
			stateMachine.SwitchState(newState);
		}

		internal void ExitState(bool useExit)
		{
			Stage = Stage.Exit;
			useExitAnimation = useExit;
		}

		internal State Initialize()
		{
			Stage = Stage.Enter;
			return this;
		}
	}
}
