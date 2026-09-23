using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Be_Stiff.AI
{
	public class EnemySquad
	{
		public static int MaxMembers = 4;

		private int currentMembers;

		private Enemy[] enemyList;

		private Orders[] enemyOrders;

		private Vector2 lastHeroPosition;

		private bool updatedInformation;

		private bool recentlyUpdated;

		private double lastUpdateTime;

		private double lastCheckedTime;

		private bool membersAnswered;

		private double creationTime;

		private bool ordersGiven;

		private bool dissolved;

		public EnemySquad()
		{
			enemyList = new Enemy[MaxMembers];
			enemyOrders = new Orders[MaxMembers];
			for (int i = 0; i < MaxMembers; i++)
			{
				enemyList[i] = null;
			}
		}

		public void Create(Enemy leader, Enemy[] members)
		{
			creationTime = GameElementsControl.CurrentTimeInMS;
			lastUpdateTime = creationTime;
			lastCheckedTime = creationTime;
			lastHeroPosition = Vector2.Zero;
			updatedInformation = false;
			currentMembers = 0;
			recentlyUpdated = false;
			ordersGiven = false;
			dissolved = false;
			membersAnswered = false;
			AddLeader(leader);
			int num = 0;
			bool flag = false;
			while (!flag)
			{
				if (currentMembers < MaxMembers)
				{
					Enemy enemy = members[num];
					if (enemy == null)
					{
						flag = true;
						continue;
					}
					AddMember(enemy);
					num++;
				}
				else
				{
					flag = true;
				}
			}
		}

		public bool IsDissolved()
		{
			return dissolved;
		}

		public void UpdateInformation(Vector2 heroPos)
		{
			if (!updatedInformation)
			{
				recentlyUpdated = true;
				lastHeroPosition = heroPos;
				updatedInformation = true;
			}
		}

		public Vector2 GetHeroPos()
		{
			return lastHeroPosition;
		}

		private bool AddLeader(Enemy enemy)
		{
			if (enemyList[0] != null)
			{
				return false;
			}
			enemy.Brain.AddNoiseHelp();
			enemyList[0] = enemy;
			Orders orders = new Orders
			{
				action = SquadAction.Attack,
				position = lastHeroPosition
			};
			enemyOrders[0] = orders;
			lastHeroPosition = enemy.Brain.HeroPosition;
			enemy.Brain.SetSquad(this, 0);
			currentMembers++;
			return true;
		}

		private bool AddMember(Enemy enemy)
		{
			if (enemy == null)
			{
				throw new Exception("Something's wrong here, enemy null");
			}
			int num = currentMembers;
			if (num != 0)
			{
				if (enemyList[num] != null)
				{
					throw new Exception("Something's wrong here, enemy slot already assigned");
				}
				enemyList[num] = enemy;
				Orders orders = new Orders
				{
					action = SquadAction.Attack,
					position = lastHeroPosition
				};
				enemyOrders[num] = orders;
				enemy.Brain.SetSquad(this, num);
				currentMembers++;
				return true;
			}
			return false;
		}

		private void RemoveFromSquad(int enemyPos)
		{
			enemyList[enemyPos].Brain.RemoveFromSquad();
			for (int i = enemyPos; i < currentMembers; i++)
			{
				int num = i + 1;
				if (num != currentMembers)
				{
					enemyList[i] = enemyList[num];
					enemyList[i].Brain.SetNewSquadPos(i);
				}
				else
				{
					enemyList[i] = null;
				}
			}
			currentMembers--;
		}

		public bool GetUpdatedInformation(out Vector2 heroPos)
		{
			heroPos = lastHeroPosition;
			return updatedInformation;
		}

		public void Update()
		{
			if (!ordersGiven && GiveOrders())
			{
				ordersGiven = true;
			}
			if (recentlyUpdated)
			{
				recentlyUpdated = false;
				lastUpdateTime = GameElementsControl.CurrentTimeInMS;
			}
			if (lastUpdateTime + 1000.0 < GameElementsControl.CurrentTimeInMS)
			{
				updatedInformation = false;
			}
			int num = currentMembers - 1;
			while (num >= 0)
			{
				if (enemyList[num] != null)
				{
					if (enemyList[num].IsDead())
					{
						RemoveFromSquad(num);
					}
					num--;
					continue;
				}
				throw new Exception("Something's wrong, a slot not assigned");
			}
			if (lastCheckedTime + 1000.0 < GameElementsControl.CurrentTimeInMS)
			{
				EvaluateSquad();
				lastCheckedTime = GameElementsControl.CurrentTimeInMS;
			}
			if (!membersAnswered && creationTime + 500.0 < GameElementsControl.CurrentTimeInMS)
			{
				for (int i = 1; i < currentMembers; i++)
				{
					enemyList[i].Brain.AddNoiseOK();
					membersAnswered = true;
				}
			}
		}

		private void EvaluateSquad()
		{
			if (currentMembers < 3)
			{
				Dissolve();
				return;
			}
			if (lastUpdateTime + 10000.0 < GameElementsControl.CurrentTimeInMS)
			{
				Dissolve();
				return;
			}
			bool flag = false;
			int num = -1;
			for (int i = 0; i < currentMembers; i++)
			{
				if (enemyList[i] != null)
				{
					num = i;
					if (enemyOrders[i].action == SquadAction.Attack)
					{
						flag = true;
						i = currentMembers;
					}
				}
			}
			if (!flag)
			{
				Orders orders = new Orders
				{
					action = SquadAction.Attack,
					position = lastHeroPosition
				};
				enemyOrders[num] = orders;
			}
		}

		private void Dissolve()
		{
			for (int i = 0; i < currentMembers; i++)
			{
				if (enemyList[i] != null)
				{
					enemyList[i].Brain.RemoveFromSquad();
					enemyList[i] = null;
					continue;
				}
				throw new Exception("Slots not assigned, bad");
			}
			currentMembers = 0;
			dissolved = true;
		}

		public Orders GetOrders(int pos)
		{
			return enemyOrders[pos];
		}

		public bool HasPartnersNearThan(int enemyAskingPos, float meters)
		{
			Vector2 position = enemyList[enemyAskingPos].Position;
			bool result = false;
			for (int i = 0; i < currentMembers; i++)
			{
				if (i != enemyAskingPos && (enemyList[i].Position - position).Length() < meters)
				{
					result = true;
				}
			}
			return result;
		}

		private bool GiveOrders()
		{
			if (updatedInformation)
			{
				Sector sectorAt = GameElementsControl.PathFindMap.GetSectorAt(ref lastHeroPosition);
				List<Portal> portalsTo = GameElementsControl.PathFindMap.GetPortalsTo(sectorAt);
				List<Enemy> list = new List<Enemy>(enemyList);
				foreach (Portal item in portalsTo)
				{
					float num = float.MaxValue;
					Enemy enemy = null;
					int num2 = -1;
					Vector2 center = item.Center;
					for (int num3 = list.Count - 1; num3 >= 0; num3--)
					{
						Enemy enemy2 = list[num3];
						if (enemy2 != null)
						{
							float num4 = (enemy2.Position - center).Length();
							if (num4 < num)
							{
								num = num4;
								enemy = enemy2;
								num2 = num3;
							}
						}
					}
					if (enemy != null)
					{
						list.Remove(enemy);
						Orders orders = new Orders
						{
							action = SquadAction.Defend,
							position = item.Center
						};
						enemyOrders[num2] = orders;
					}
				}
				return true;
			}
			return false;
		}
	}
}
