using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Be_Stiff.AI
{
	internal class EnemiesControl
	{
		private List<Enemy> enemies;

		private Stack<EnemySquad> emptySquads;

		private List<EnemySquad> activeSquads;

		private Enemy[] squadCandidates;

		private Vector3[] noises;

		private int noisesNumber;

		public EnemiesControl()
		{
			squadCandidates = new Enemy[EnemySquad.MaxMembers - 1];
			enemies = new List<Enemy>();
			emptySquads = new Stack<EnemySquad>();
			activeSquads = new List<EnemySquad>();
			noises = new Vector3[10];
			noisesNumber = 0;
		}

		public void AddEnemy(Enemy enemy)
		{
			if (GetEnemyByName(enemy.Name) == null)
			{
				enemies.Add(enemy);
				int num = enemies.Count / 3 + 1;
				if (emptySquads.Count < num)
				{
					emptySquads.Push(new EnemySquad());
				}
				return;
			}
			throw new Exception("Enemy already added with this name: " + enemy.Name);
		}

		/// <summary>Debug helper: kills every living enemy.</summary>
		public void DebugKillAll()
		{
			foreach (Enemy enemy in enemies)
			{
				if (!enemy.IsDead())
				{
					enemy.BloodyDie();
				}
			}
		}

		public Enemy GetEnemyByName(string enemyName)
		{
			for (int i = 0; i < enemies.Count; i++)
			{
				Enemy enemy = enemies[i];
				if (enemyName == enemy.Name)
				{
					return enemy;
				}
			}
			return null;
		}

		public void Update()
		{
			for (int num = enemies.Count - 1; num >= 0; num--)
			{
				Enemy leader = enemies[num];
				leader.Update();
				if (!leader.IsDead())
				{
					for (int i = 0; i < noisesNumber; i++)
					{
						Vector2 vector = new Vector2(noises[i].X, noises[i].Y);
						float z = noises[i].Z;
						(vector - leader.Position).Length();
						if ((vector - leader.Position).Length() <= z)
						{
							leader.Brain.HearNoise(vector);
						}
					}
					if (leader.Brain.RequestSquad())
					{
						CreateSquad(ref leader);
					}
				}
				if (leader.Disposed)
				{
					enemies.Remove(leader);
				}
			}
			noisesNumber = 0;
			for (int num2 = activeSquads.Count - 1; num2 >= 0; num2--)
			{
				EnemySquad enemySquad = activeSquads[num2];
				if (!enemySquad.IsDissolved())
				{
					enemySquad.Update();
				}
				else
				{
					emptySquads.Push(enemySquad);
					activeSquads.RemoveAt(num2);
				}
			}
		}

		public void RegisterNoise(Vector3 newNoise)
		{
			if (newNoise.Z > 0f)
			{
				noises[noisesNumber++] = newNoise;
			}
		}

		private void CreateSquad(ref Enemy leader)
		{
			int num = 0;
			for (int i = 0; i < EnemySquad.MaxMembers - 1; i++)
			{
				squadCandidates[i] = null;
			}
			Vector2 position = leader.Position;
			foreach (Enemy enemy in enemies)
			{
				if (num > EnemySquad.MaxMembers - 1)
				{
					break;
				}
				if (enemy.IsDead() || enemy == leader)
				{
					continue;
				}
				float num2 = Vector2.Distance(position, enemy.Position);
				if (!(num2 < 20f) || enemy.Brain.HasSquad())
				{
					continue;
				}
				bool flag = false;
				Sector start = leader.LastKnownSectorPosition;
				Sector end = enemy.LastKnownSectorPosition;
				if (start == end)
				{
					flag = true;
				}
				else
				{
					Stack<Sector> retValue = new Stack<Sector>();
					GameElementsControl.PathFindMap.GetPathFromTo(ref start, ref end, ref retValue);
					if (retValue.Count != 0)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (num < EnemySquad.MaxMembers - 1)
				{
					squadCandidates[num] = enemy;
					num++;
					continue;
				}
				for (int j = 0; j < EnemySquad.MaxMembers - 1; j++)
				{
					float num3 = Vector2.Distance(position, squadCandidates[j].Position);
					if (num2 < num3)
					{
						squadCandidates[j] = enemy;
						break;
					}
				}
			}
			if (num >= 2)
			{
				EnemySquad enemySquad = emptySquads.Pop();
				enemySquad.Create(leader, squadCandidates);
				activeSquads.Add(enemySquad);
			}
		}

		private int debugDrawCount;

		public void Draw()
		{
			bool dbg = Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null && debugDrawCount++ % 120 == 0;
			foreach (Enemy enemy in enemies)
			{
				if (dbg) Console.Error.WriteLine($"  enemy {enemy.GetType().Name} '{enemy.Name}' dead={enemy.IsDead()} disposed={enemy.Disposed} pos={enemy.Position} screen={GameElementsControl.ConvertWorldToScreen(enemy.Position)}");
				enemy.Draw();
			}
		}
	}
}
