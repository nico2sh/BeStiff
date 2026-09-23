using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.AI
{
	internal class BrainFollowingOrders : BrainState
	{
		private Side _sideScanning;

		public BrainFollowingOrders(Brain mainBrain)
			: base(mainBrain)
		{
			thisState = 5;
		}

		public override void Init()
		{
		}

		public override int Update()
		{
			if (!mainBrain.HasSquad())
			{
				return 4;
			}
			Orders orders = mainBrain.GetOrders();
			switch (orders.action)
			{
			case SquadAction.Defend:
				OrderDefend(ref orders.position);
				break;
			case SquadAction.Attack:
				OrderAttack();
				break;
			default:
				OrderAttack();
				break;
			}
			return thisState;
		}

		public override int HearNoise(ref Vector2 position)
		{
			return thisState;
		}

		public override int Hit(ref Vector2 position, ref HitType hitType)
		{
			return thisState;
		}

		private void OrderDefend(ref Vector2 pos)
		{
			if (mainBrain.WaitingSquad())
			{
				OrderAttack();
				return;
			}
			float weaponRange = mainBrain.WeaponRange;
			mainBrain.MoveToDestinationPoint(pos, weaponRange);
			if (mainBrain.AimingAtHero)
			{
				mainBrain.ShootAtHero();
				return;
			}
			Vector2 point = mainBrain.HeroPosition;
			mainBrain.AimToPoint(ref point);
		}

		private void ScanSurroundings(ref Enemy owner)
		{
			bool flag = owner.IsAimingDown();
			if (_sideScanning == Side.Left)
			{
				if (owner.SideLooking == Side.Right && flag)
				{
					_sideScanning = Side.Right;
				}
			}
			else if (owner.SideLooking == Side.Left && flag)
			{
				_sideScanning = Side.Left;
			}
		}

		private void OrderAttack()
		{
			mainBrain.MoveToDestinationPoint(mainBrain.HeroPosition, Math.Min(mainBrain.WeaponRange, 5f));
			Vector2 point = mainBrain.HeroPosition;
			mainBrain.AimToPoint(ref point);
			if (mainBrain.AimingAtHero)
			{
				mainBrain.ShootAtHero();
			}
		}
	}
}
