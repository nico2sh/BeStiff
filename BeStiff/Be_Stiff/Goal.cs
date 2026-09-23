using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;

namespace Be_Stiff
{
	public abstract class Goal : InteractiveWorldObject
	{
		protected bool achieved;

		protected double timeForAchieving;

		private GameSprite arrow;

		private GameSpriteVariables arrowVariables;

		private GameSprite progressBar;

		private GameSpriteVariables progressBarVariables;

		private GameSpriteVariables progressBarBackgroundVariables;

		private bool growing;

		private Vector2 growingRate;

		private double elapsed;

		private CircleEmitter achievedEmitter;

		public override Body MainBody => mainBody;

		public Goal()
		{
			GameElementsControl.LoadSprite("arrow", "sprites\\goals\\arrow");
			GameElementsControl.LoadSprite("whitePixel", "sprites\\whitepixel");
			arrow = GameElementsControl.GetSprite("arrow");
			arrowVariables = GameSprite.GetDefaultVariables();
			growingRate = new Vector2(0.001f, 0.001f);
			elapsed = 0.0;
			progressBar = GameElementsControl.GetSprite("whitePixel");
			timeForAchieving = 1000.0;
			progressBarVariables = GameSprite.GetDefaultVariables();
			progressBarVariables.scale = new Vector2(0f, 5f);
			progressBarVariables.color = Color.Gray;
			progressBarBackgroundVariables = GameSprite.GetDefaultVariables();
			progressBarBackgroundVariables.scale = new Vector2(30f, 5f);
			progressBarBackgroundVariables.color = Color.DarkGray;
			achievedEmitter = (CircleEmitter)GameElementsControl.Particlesmanager.getGoalSparkleEmitter();
			GameElementsControl.ScreenManager.AudioManager.LoadSound("chan", "audio\\noises\\chan");
		}

		public override void Update()
		{
			if (!Active)
			{
				return;
			}
			elapsed += GameElementsControl.LastFrameTimeInMS;
			if (elapsed >= 500.0)
			{
				elapsed -= 500.0;
				growing = !growing;
			}
			if (growing)
			{
				arrowVariables.scale += growingRate * (float)GameElementsControl.LastFrameTimeInMS;
			}
			else
			{
				arrowVariables.scale -= growingRate * (float)GameElementsControl.LastFrameTimeInMS;
			}
			if (achieved)
			{
				return;
			}
			if (interacting)
			{
				interactionTime += GameElementsControl.LastFrameTimeInMS;
				if (interactionTime >= timeForAchieving)
				{
					Achieve();
				}
				progressBarVariables.scale.X = (int)MathHelper.Clamp((float)(interactionTime / timeForAchieving) * 30f, 0f, 30f);
			}
			else
			{
				progressBarVariables.scale.X = 0f;
			}
		}

		public override bool Interacts(WorldObjectType type)
		{
			return type == WorldObjectType.Hero;
		}

		protected virtual void Achieve()
		{
			achievedEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
			achieved = true;
			GameElementsControl.NoiseManager.AddNoise("chan", mainBody.Position);
			GameElementsControl.Level.AchieveGoal(this);
		}

		public virtual void DrawGoal()
		{
			if (!achieved && Active)
			{
				if (!interacting)
				{
					arrow.Draw(GameElementsControl.ConvertWorldToScreen(MainBody.Position) - new Vector2(0f, 32f), arrowVariables);
					return;
				}
				progressBar.Draw(GameElementsControl.ConvertWorldToScreen(MainBody.Position) - new Vector2(15f, 32f), progressBarBackgroundVariables);
				progressBar.Draw(GameElementsControl.ConvertWorldToScreen(MainBody.Position) - new Vector2(15f, 32f), progressBarVariables);
			}
		}
	}
}
