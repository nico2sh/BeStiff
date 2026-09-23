using System;
using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;

namespace Be_Stiff.Graphics
{
	public class ParticlesManager
	{
		public const int particleGunSmokePos = 0;

		public const int particleBloodPos = 1;

		public const int particleHitSparkPos = 2;

		public const int particleJumpDustPos = 3;

		public const int particleCannonShootSmokePos = 4;

		public const int particleBulletTrailPos = 5;

		public const int particleGrenadePos = 6;

		public const int particleBloodBlowPos = 7;

		public const int particleHitDustPos = 8;

		public const int particleSmokeBarrelPos = 9;

		public const int particleLaserSightPos = 10;

		public const int particleSmokeCannonPos = 11;

		public const int particleMisileTrailPos = 12;

		public const int particleKickSmokePos = 13;

		public const int particleGoalSparklePos = 14;

		private SpriteBatchRenderer renderer;

		private Emitter[] emitters;

		public ParticlesManager()
		{
			emitters = new Emitter[15];
			renderer = new SpriteBatchRenderer();
		}

		private void addEmitter(Emitter emitter, int pos)
		{
			emitters[pos] = emitter;
		}

		public Emitter getGunSmokeEmitter()
		{
			return emitters[0];
		}

		public Emitter getCannonShootSmokeEmmiter()
		{
			return emitters[4];
		}

		public Emitter getBloodEmitter()
		{
			return emitters[1];
		}

		public Emitter getBloodBlowEmitter()
		{
			return emitters[7];
		}

		public Emitter getHitSparkEmitter()
		{
			return emitters[2];
		}

		public Emitter getJumpDustEmitter()
		{
			return emitters[3];
		}

		public Emitter getBulletTrailEmitter()
		{
			return emitters[5];
		}

		public Emitter getGrenadeEmitter()
		{
			return emitters[6];
		}

		public Emitter getHitDustEmitter()
		{
			return emitters[8];
		}

		public Emitter getSmokeBarrelEmitter()
		{
			return emitters[9];
		}

		public Emitter getLaserSightEmitter()
		{
			return emitters[10];
		}

		public Emitter getSmokeCannonEmitter()
		{
			return emitters[11];
		}

		public Emitter getMisileTrailEmitter()
		{
			return emitters[12];
		}

		public Emitter getKickSmokeEmitter()
		{
			return emitters[13];
		}

		public Emitter getGoalSparkleEmitter()
		{
			return emitters[14];
		}

		public void Load()
		{
			renderer.GraphicsDeviceService = GameElementsControl.ScreenManager.GraphicsDeviceService;
			renderer.LoadContent(GameElementsControl.GameScreen.Content);
			GameElementsControl.LoadSprite("particleSmoke", "sprites\\Particles\\smoke");
			GameElementsControl.LoadSprite("particleBlood", "sprites\\Particles\\blood");
			GameElementsControl.LoadSprite("particleSpark", "sprites\\Particles\\spark");
			GameElementsControl.LoadSprite("particleWhitedot", "sprites\\Particles\\whiteDot");
			addGunSmoke();
			addBlood();
			addHitSpark();
			addJumpDust();
			addCannonShootSmoke();
			addBulletTrail();
			addGrenadeEmitter();
			addBloodBlow();
			addHitDust();
			addBarrelSmoke();
			addLaserSight();
			addCannonSmoke();
			addMisileTrail();
			addKickSmoke();
			addGoalSparkles();
		}

		public void Update()
		{
			float deltaSeconds = (float)GameElementsControl.LastFrameTimeInMS / 1000f;
			Emitter[] array = emitters;
			foreach (Emitter emitter in array)
			{
				emitter.Update(deltaSeconds);
			}
		}

		public void Draw(Matrix matrixTransform)
		{
			Emitter[] array = emitters;
			foreach (Emitter emitter in array)
			{
				renderer.RenderEmitter(emitter, ref matrixTransform);
			}
		}

		private void addBlood()
		{
			ConeEmitter coneEmitter = new ConeEmitter();
			coneEmitter.Name = "Blood";
			coneEmitter.ParticleTexture = GameElementsControl.GetSprite("particleBlood").Texture;
			coneEmitter.Budget = 2000;
			coneEmitter.ConeAngle = (float)Math.PI / 16f;
			coneEmitter.ReleaseQuantity = 20;
			coneEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 0.5f
			};
			coneEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 200f,
				Variation = 100f
			};
			coneEmitter.ReleaseColour = Color.White.ToVector3();
			coneEmitter.Term = 1f;
			coneEmitter.ReleaseOpacity = 1f;
			coneEmitter.BlendMode = EmitterBlendMode.Alpha;
			coneEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				},
				new LinearGravityModifier
				{
					Gravity = new Vector2(0f, 200f)
				}
			};
			Emitter emitter = coneEmitter;
			emitter.Initialise();
			addEmitter(emitter, 1);
		}

		private void addBloodBlow()
		{
			LineEmitter lineEmitter = new LineEmitter();
			lineEmitter.Name = "BloodBlow";
			lineEmitter.ParticleTexture = GameElementsControl.GetSprite("particleBlood").Texture;
			lineEmitter.Budget = 6000;
			lineEmitter.Angle = 0f;
			lineEmitter.Length = 32f;
			lineEmitter.ReleaseQuantity = 250;
			lineEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 0.5f
			};
			lineEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 50f,
				Variation = 25f
			};
			lineEmitter.ReleaseColour = Color.White.ToVector3();
			lineEmitter.Term = 1f;
			lineEmitter.BlendMode = EmitterBlendMode.Alpha;
			lineEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				},
				new LinearGravityModifier
				{
					Gravity = new Vector2(0f, 200f)
				}
			};
			Emitter emitter = lineEmitter;
			emitter.Initialise();
			addEmitter(emitter, 7);
		}

		private void addGunSmoke()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "GunSmoke";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			circleEmitter.Budget = 2000;
			circleEmitter.Radius = 5f;
			circleEmitter.Radiate = false;
			circleEmitter.Ring = false;
			circleEmitter.ReleaseQuantity = 25;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 3f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 30f,
				Variation = 5f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.ReleaseColour = Color.White.ToVector3();
			circleEmitter.Term = 1f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 0.5f,
					Ultimate = 0f
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 0);
		}

		private void addCannonShootSmoke()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "CannonShootSmoke";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			circleEmitter.Budget = 200;
			circleEmitter.Radius = 15f;
			circleEmitter.Radiate = false;
			circleEmitter.Ring = false;
			circleEmitter.ReleaseQuantity = 20;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 10f,
				Variation = 10f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 10f,
				Variation = 10f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.ReleaseColour = Color.White.ToVector3();
			circleEmitter.Term = 1.5f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 4);
		}

		private void addHitSpark()
		{
			ConeEmitter coneEmitter = new ConeEmitter();
			coneEmitter.Name = "HitSpark";
			coneEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSpark").Texture;
			coneEmitter.Budget = 2000;
			coneEmitter.ConeAngle = (float)Math.PI / 4f;
			coneEmitter.ReleaseQuantity = 10;
			coneEmitter.ReleaseScale = new VariableFloat
			{
				Value = 10f,
				Variation = 5f
			};
			coneEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 50f,
				Variation = 50f
			};
			coneEmitter.ReleaseOpacity = 1f;
			coneEmitter.ReleaseColour = Color.White.ToVector3();
			coneEmitter.Term = 0.5f;
			coneEmitter.BlendMode = EmitterBlendMode.Alpha;
			coneEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			Emitter emitter = coneEmitter;
			emitter.Initialise();
			addEmitter(emitter, 2);
		}

		private void addJumpDust()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "JumpDust";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			circleEmitter.Budget = 200;
			circleEmitter.Radius = 5f;
			circleEmitter.Radiate = true;
			circleEmitter.Ring = false;
			circleEmitter.ReleaseQuantity = 10;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 5f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 10f,
				Variation = 10f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.ReleaseColour = Color.Gray.ToVector3();
			circleEmitter.Term = 1.5f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 3);
		}

		private void addBulletTrail()
		{
			LineEmitter lineEmitter = new LineEmitter();
			lineEmitter.Name = "BulletTrail";
			lineEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			lineEmitter.Budget = 2000;
			lineEmitter.ReleaseQuantity = 10;
			lineEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 0.5f
			};
			lineEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 10f,
				Variation = 0.5f
			};
			lineEmitter.ReleaseOpacity = 1f;
			lineEmitter.ReleaseColour = Color.White.ToVector3();
			lineEmitter.Term = 0.8f;
			lineEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			LineEmitter lineEmitter2 = lineEmitter;
			lineEmitter2.Initialise();
			addEmitter(lineEmitter2, 5);
		}

		private void addHitDust()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "HitDust";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			circleEmitter.Budget = 200;
			circleEmitter.Radius = 5f;
			circleEmitter.Radiate = true;
			circleEmitter.Ring = false;
			circleEmitter.ReleaseQuantity = 50;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 5f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 10f,
				Variation = 10f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.ReleaseColour = Color.Gray.ToVector3();
			circleEmitter.Term = 1.5f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 8);
		}

		private void addGrenadeEmitter()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "GreandeSmoke";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			circleEmitter.Budget = 3000;
			circleEmitter.Radius = 15f;
			circleEmitter.Radiate = true;
			circleEmitter.Ring = false;
			circleEmitter.ReleaseQuantity = 300;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 20f,
				Variation = 15f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 100f,
				Variation = 100f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.ReleaseColour = Color.White.ToVector3();
			circleEmitter.Term = 1.5f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 6);
		}

		private void addBarrelSmoke()
		{
			ConeEmitter coneEmitter = new ConeEmitter();
			coneEmitter.Name = "BarrelSmoke";
			coneEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			coneEmitter.Budget = 2000;
			coneEmitter.ConeAngle = (float)Math.PI / 8f;
			coneEmitter.ReleaseQuantity = 1;
			coneEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 3f
			};
			coneEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 50f,
				Variation = 0.5f
			};
			coneEmitter.ReleaseOpacity = 1f;
			coneEmitter.ReleaseColour = Color.White.ToVector3();
			coneEmitter.Term = 0.5f;
			coneEmitter.BlendMode = EmitterBlendMode.Alpha;
			coneEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				},
				new LinearGravityModifier
				{
					Gravity = new Vector2(0f, -500f)
				},
				new ColourModifier
				{
					InitialColour = Color.White.ToVector3(),
					UltimateColour = Color.Black.ToVector3()
				}
			};
			Emitter emitter = coneEmitter;
			emitter.Initialise();
			addEmitter(emitter, 9);
		}

		private void addLaserSight()
		{
			LineEmitter lineEmitter = new LineEmitter();
			lineEmitter.Name = "LaserSight";
			lineEmitter.ParticleTexture = GameElementsControl.GetSprite("particleWhitedot").Texture;
			lineEmitter.Budget = 10000;
			lineEmitter.ReleaseQuantity = 100;
			lineEmitter.ReleaseScale = 1f;
			lineEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 2f,
				Variation = 0.5f
			};
			lineEmitter.ReleaseOpacity = 0.5f;
			lineEmitter.BlendMode = EmitterBlendMode.Alpha;
			lineEmitter.ReleaseColour = Color.Red.ToVector3();
			lineEmitter.Term = 0.05f;
			LineEmitter lineEmitter2 = lineEmitter;
			lineEmitter2.Initialise();
			addEmitter(lineEmitter2, 10);
		}

		private void addCannonSmoke()
		{
			ConeEmitter coneEmitter = new ConeEmitter();
			coneEmitter.Name = "CannonSmoke";
			coneEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			coneEmitter.Budget = 2000;
			coneEmitter.ConeAngle = (float)Math.PI / 8f;
			coneEmitter.ReleaseQuantity = 10;
			coneEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 1f
			};
			coneEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 30f,
				Variation = 10f
			};
			coneEmitter.ReleaseOpacity = 1f;
			coneEmitter.ReleaseColour = Color.White.ToVector3();
			coneEmitter.Term = 1f;
			coneEmitter.BlendMode = EmitterBlendMode.Alpha;
			coneEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				},
				new LinearGravityModifier
				{
					Gravity = new Vector2(0f, -50f)
				},
				new ColourModifier
				{
					InitialColour = Color.White.ToVector3(),
					UltimateColour = Color.Black.ToVector3()
				}
			};
			Emitter emitter = coneEmitter;
			emitter.Initialise();
			addEmitter(emitter, 11);
		}

		private void addMisileTrail()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "MisileTrail";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			circleEmitter.Budget = 2000;
			circleEmitter.Radius = 4f;
			circleEmitter.Radiate = true;
			circleEmitter.Ring = false;
			circleEmitter.ReleaseQuantity = 50;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 5f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 5f,
				Variation = 3f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.ReleaseColour = Color.Gray.ToVector3();
			circleEmitter.Term = 1.5f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 12);
		}

		private void addKickSmoke()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "KickSmoke";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleSmoke").Texture;
			circleEmitter.Budget = 2000;
			circleEmitter.Radius = 6f;
			circleEmitter.Radiate = true;
			circleEmitter.Ring = false;
			circleEmitter.ReleaseQuantity = 30;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 5f,
				Variation = 4f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 10f,
				Variation = 5f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.ReleaseColour = Color.White.ToVector3();
			circleEmitter.Term = 1f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 13);
		}

		private void addGoalSparkles()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Name = "GoalSparkle";
			circleEmitter.ParticleTexture = GameElementsControl.GetSprite("particleWhitedot").Texture;
			circleEmitter.Budget = 2000;
			circleEmitter.Radius = 1f;
			circleEmitter.Radiate = true;
			circleEmitter.Ring = true;
			circleEmitter.ReleaseQuantity = 300;
			circleEmitter.ReleaseScale = new VariableFloat
			{
				Value = 2f,
				Variation = 1f
			};
			circleEmitter.ReleaseSpeed = new VariableFloat
			{
				Value = 200f,
				Variation = 20f
			};
			circleEmitter.BlendMode = EmitterBlendMode.Alpha;
			circleEmitter.Term = 1f;
			circleEmitter.Modifiers = new ModifierCollection
			{
				new OpacityModifier
				{
					Initial = 1f,
					Ultimate = 0f
				},
				new ColourModifier
				{
					InitialColour = Color.DarkRed.ToVector3(),
					UltimateColour = Color.Yellow.ToVector3()
				}
			};
			CircleEmitter circleEmitter2 = circleEmitter;
			circleEmitter2.Initialise();
			addEmitter(circleEmitter2, 14);
		}
	}
}
