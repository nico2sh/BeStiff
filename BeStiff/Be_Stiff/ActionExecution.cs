using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class ActionExecution
	{
		private bool performingAction;

		private Enemy enemy;

		private Portal portal;

		public bool Performing => performingAction;

		public ActionExecution(Enemy executor)
		{
			enemy = executor;
			performingAction = false;
		}

		public void Update()
		{
			if (!performingAction)
			{
				return;
			}
			switch (portal.ActionToPerform.Action)
			{
			case Actions.Jump:
				if (enemy.Landed())
				{
					performingAction = false;
				}
				break;
			case Actions.JumpRelative:
				if (enemy.Landed())
				{
					performingAction = false;
				}
				break;
			case Actions.JumpDown:
				if (enemy.Landed())
				{
					performingAction = false;
				}
				break;
			case Actions.Walk:
				enemy.WalkTo(portal.ActionToPerform.Side.X, hasToStop: true);
				if ((enemy.SectorPosition == portal.DestinationSector || enemy.SectorPosition != portal.OriginSector) && enemy.TouchingFloor())
				{
					performingAction = false;
				}
				break;
			case Actions.EnterElevator:
				TakeElevator(portal.ActionToPerform.Object as Elevator, portal.ActionToPerform.RefNumber);
				if ((enemy.SectorPosition == portal.DestinationSector || enemy.SectorPosition != portal.OriginSector) && enemy.TouchingFloor())
				{
					performingAction = false;
				}
				break;
			case Actions.ExitElevator:
				ExitElevator(portal.ActionToPerform.Object as Elevator, portal.ActionToPerform.RefNumber, (int)portal.ActionToPerform.Side.X);
				if ((enemy.SectorPosition == portal.DestinationSector || enemy.SectorPosition != portal.OriginSector) && enemy.TouchingFloor())
				{
					performingAction = false;
				}
				break;
			case Actions.EnterPlatform:
				JumpToPlatform(portal.ActionToPerform.Object as MovingPlatformH, portal.ActionToPerform.RefNumber);
				if ((enemy.SectorPosition == portal.DestinationSector || enemy.SectorPosition != portal.OriginSector) && enemy.TouchingFloor())
				{
					performingAction = false;
				}
				break;
			case Actions.ExitPlatform:
				JumpFromPlatform(portal.ActionToPerform.Object as MovingPlatformH, portal.ActionToPerform.RefNumber);
				if ((enemy.SectorPosition == portal.DestinationSector || enemy.SectorPosition != portal.OriginSector) && enemy.TouchingFloor())
				{
					performingAction = false;
				}
				break;
			}
			if (!performingAction)
			{
				enemy.Stop();
			}
		}

		private void JumpToPlatform(MovingPlatformH platform, int fromFloor)
		{
			if (platform.CurrentFloor() == fromFloor)
			{
				Vector2 point = new Vector2(portal.ActionToPerform.Side.X, platform.MainBody.Position.Y);
				enemy.JumpTo(ref point);
			}
			else
			{
				enemy.Stop();
			}
		}

		private void JumpFromPlatform(MovingPlatformH platform, int fromFloor)
		{
			if (platform.CurrentFloor() == fromFloor)
			{
				Vector2 point = portal.ActionToPerform.Side;
				enemy.JumpTo(ref point);
			}
			else
			{
				enemy.Stop();
			}
		}

		private void TakeElevator(Elevator elevator, int fromFloor)
		{
			Vector2 pos = enemy.Position;
			if (elevator.IsInside(ref pos))
			{
				enemy.Stop();
			}
			else if (elevator.CurrentFloor() == fromFloor)
			{
				enemy.WalkTo(elevator.CenterPosition.X, hasToStop: true);
			}
			else if (Math.Abs(enemy.Position.X - portal.Center.X) < 0.1f)
			{
				enemy.Stop();
			}
			else
			{
				enemy.WalkTo(portal.Center.X, hasToStop: true);
			}
		}

		private void ExitElevator(Elevator elevator, int fromFloor, float side)
		{
			if (elevator.CurrentFloor() == fromFloor)
			{
				enemy.WalkTo(side, hasToStop: false);
			}
			else if (Math.Abs(enemy.Position.X - elevator.CenterPosition.X) < 0.1f)
			{
				enemy.Stop();
			}
			else
			{
				enemy.WalkTo(elevator.CenterPosition.X, hasToStop: true);
			}
		}

		public void ExecuteAction(Portal portal)
		{
			if (!performingAction)
			{
				this.portal = portal;
				if (portal.ActionToPerform.Action == Actions.Jump)
				{
					if (enemy.Landed())
					{
						Vector2 point = portal.ActionToPerform.Side;
						enemy.JumpTo(ref point);
						performingAction = true;
					}
				}
				else if (portal.ActionToPerform.Action == Actions.JumpRelative)
				{
					if (enemy.Landed())
					{
						Vector2 point2 = portal.ActionToPerform.Side + enemy.FeetPosition;
						enemy.JumpTo(ref point2);
						performingAction = true;
					}
				}
				else if (portal.ActionToPerform.Action == Actions.JumpDown)
				{
					if (enemy.Landed())
					{
						enemy.JumpingDown = true;
						performingAction = true;
					}
				}
				else
				{
					performingAction = true;
				}
			}
			else
			{
				enemy.JumpingDown = false;
			}
		}
	}
}
