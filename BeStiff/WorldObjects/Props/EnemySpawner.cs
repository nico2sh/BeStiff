using Microsoft.Xna.Framework;

namespace Be_Stiff.WorldObjects.Props
{
	public class EnemySpawner : WorldObject
	{
		private Enemy enemy;

		private bool waitingForSpawning;

		private Vector2 floorPosition;

		public EnemySpawner(Enemy theEnemy, Vector2 position)
		{
			base.Name = "Spawner-" + theEnemy.Name;
			enemy = theEnemy;
			waitingForSpawning = false;
			floorPosition = position;
			GameElementsControl.AddWorldObject(this);
		}

		public override void Update()
		{
			if (!waitingForSpawning)
			{
				if (enemy.IsDead())
				{
					waitingForSpawning = true;
				}
			}
			else if (enemy.TimeOfDeath + 6000.0 <= GameElementsControl.CurrentTimeInMS)
			{
				enemy.MoveToPosition(floorPosition);
				enemy.Resurrect();
				waitingForSpawning = false;
			}
		}
	}
}
