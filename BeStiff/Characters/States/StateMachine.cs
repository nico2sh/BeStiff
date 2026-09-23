using System;
using System.Collections.Generic;

namespace Be_Stiff.Characters.States
{
	public abstract class StateMachine
	{
		private State nextState;

		private Dictionary<int, State> states;

		private int[] currentEvents;

		private int eventsNumber;

		public State CurrentState { get; protected set; }

		public Dictionary<int, State> States => states;

		public StateMachine()
		{
			states = new Dictionary<int, State>();
			nextState = null;
			currentEvents = new int[10];
			eventsNumber = 0;
		}

		// Debug switch: log every state change to stderr.
		private static readonly bool DebugStateLog = Environment.GetEnvironmentVariable("BESTIFF_STATE_LOG") != null;

		protected virtual string DebugInfo()
		{
			return string.Empty;
		}

		public void InsertEvent(int eventID)
		{
			currentEvents[eventsNumber] = eventID;
			eventsNumber++;
		}

		public bool CheckEvent(int eventID)
		{
			for (int i = 0; i < eventsNumber; i++)
			{
				if (currentEvents[i] == eventID)
				{
					return true;
				}
			}
			return false;
		}

		public void AddState(State state)
		{
			if (states.ContainsKey(state.StateID))
			{
				throw new Exception("Duplicated State ID");
			}
			states.Add(state.StateID, state);
			if (nextState == null)
			{
				nextState = state;
			}
		}

		public virtual void Update()
		{
			if (CurrentState.Update() && nextState != null)
			{
				if (DebugStateLog)
				{
					Console.Error.WriteLine($"  state t={GameElementsControl.CurrentTimeInMS:F0} {CurrentState.GetType().Name} -> {nextState.GetType().Name} {DebugInfo()}");
				}
				CurrentState.OnExit();
				CurrentState = nextState.Initialize();
				CurrentState.OnEnter();
			}
			eventsNumber = 0;
		}

		internal void SwitchState(int newState)
		{
			if (nextState.StateID != newState && CurrentState.StateID != newState && !states.TryGetValue(newState, out nextState))
			{
				throw new Exception("State doesn't exist, WTF");
			}
		}
	}
}
