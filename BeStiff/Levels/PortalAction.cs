using Microsoft.Xna.Framework;

namespace Be_Stiff.Levels
{
	public class PortalAction
	{
		private Actions _actionToPerform;

		private Vector2 _side;

		private int _refNumber;

		private WorldObject _worldObject;

		public Actions Action => _actionToPerform;

		public Vector2 Side => _side;

		public int RefNumber
		{
			get
			{
				return _refNumber;
			}
			set
			{
				_refNumber = value;
			}
		}

		public WorldObject Object
		{
			get
			{
				return _worldObject;
			}
			set
			{
				_worldObject = value;
			}
		}

		public PortalAction(string action, Vector2 side)
		{
			switch (action)
			{
			case "Jump":
				_actionToPerform = Actions.Jump;
				break;
			case "Walk":
				_actionToPerform = Actions.Walk;
				break;
			}
			_worldObject = null;
			_side = side;
		}

		public PortalAction(string action, Vector2 side, WorldObject wo)
		{
			switch (action)
			{
			case "Jump":
				_actionToPerform = Actions.Jump;
				break;
			case "JumpRelative":
				_actionToPerform = Actions.JumpRelative;
				break;
			case "JumpDown":
				_actionToPerform = Actions.JumpDown;
				break;
			case "Walk":
				_actionToPerform = Actions.Walk;
				break;
			case "Elevator":
				_actionToPerform = Actions.EnterElevator;
				break;
			case "ExitElevator":
				_actionToPerform = Actions.ExitElevator;
				break;
			case "Platform":
				_actionToPerform = Actions.EnterPlatform;
				break;
			case "ExitPlatform":
				_actionToPerform = Actions.ExitPlatform;
				break;
			}
			_worldObject = wo;
			_side = side;
		}

		public bool IsFeasible()
		{
			if (_worldObject != null)
			{
				if (_actionToPerform == Actions.Walk || _actionToPerform == Actions.Jump || _actionToPerform == Actions.JumpRelative || _actionToPerform == Actions.JumpDown)
				{
					if (_worldObject.Enabled)
					{
						return false;
					}
					_worldObject = null;
				}
				return true;
			}
			return true;
		}
	}
}
