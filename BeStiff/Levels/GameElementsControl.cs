using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using Krypton;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OgmoXNA;

namespace Be_Stiff.Levels
{
	internal static partial class GameElementsControl
	{
		private static World world;

		private static WorldContactManager worldContactManager;

		private static GameSettings gameSettings;

		private static GameplayScreen gameScreen;

		private static Camera2D camera;

		private static bool fixedCamera;

		private static Vector2 gravity;

		private static Dictionary<string, WorldObject> worldObjects;

		private static Dictionary<string, WorldObject> worldShadowObjects;

		private static List<WorldObject> worldStaticObjects;

		private static List<string> worldObjectRemoveQueue;

		private static List<string> worldShadowObjectRemoveQueue;

		private static Dictionary<string, GameSprite> gameSprites;

		private static Dictionary<string, SpriteFont> fonts;

		private static PathFindMap pathFindMap;

		private static ParticlesManager particlesmanager;

		private static KryptonEngine krypton;

		private static NoiseManager noiseManager;

		private static UserInterface userInterface;

		private static Color shadowColor;

		private static RenderTarget2D renderTargetObjects;

		private static RenderTarget2D renderTargetBackground;

		private static RenderTarget2D renderTargetNoises;

		private static RenderTarget2D renderTargetHero;

		private static double inGameTimeInMs;

		private static double lastFrameGameTimeInMs;

		private static double realGameTimeInMs;

		private static double realLastFrameGameTimeInMs;

		public static GameTime GameTime;

		private static bool levelFinished;

		private static double timeFinished;

		private static Hero hero;

		private static EnemiesControl enemiesControl;

		private static DebugViewXNA debugView;

		private static bool debugViewEnabled = false;

		private static int textLine;

		private static int tw;

		private static int th;

		private static Level level;

		private static Vector2 maskSpritePos;

		private static Effect alphaMaskEffect;

		private static Effect alphaShadowEffect;

		private static Effect grayShadowEffect;

		private static Effect alphaNoisesEffect;

		private static Effect heroDrawEffect;

		private static PlayerIndex outPlayerIndex;

		private static int counter;

		private static Random rand;

		public static Level Level => level;

		public static World World => world;

		public static Vector2 Gravity => gravity;

		public static ParticlesManager Particlesmanager => particlesmanager;

		public static KryptonEngine Krypton => krypton;

		public static NoiseManager NoiseManager => noiseManager;

		public static Camera2D Camera => camera;

		public static Random Random => rand;

		public static Hero Hero
		{
			get
			{
				return hero;
			}
			set
			{
				hero = value;
			}
		}

		public static InputHelper Input => ScreenManager.InputHelper;

		public static ScoreManager Score => userInterface.Score;

		public static double TotalRealTime
		{
			get
			{
				if (levelFinished)
				{
					return timeFinished;
				}
				return realGameTimeInMs;
			}
		}

		public static Dictionary<string, WorldObject> WorldObjects => worldObjects;

		public static Dictionary<string, WorldObject> WorldShadowObjects => worldShadowObjects;

		public static PathFindMap PathFindMap => pathFindMap;

		public static DebugViewXNA DebugView => debugView;

		public static double CurrentTimeInMS => inGameTimeInMs;

		public static double RealTimeInMS => realGameTimeInMs;

		public static double LastFrameTimeInMS => lastFrameGameTimeInMs;

		public static double RealLastFrameTimeInMS => realLastFrameGameTimeInMs;

		public static ScreenManager ScreenManager => gameScreen.ScreenManager;

		public static GameplayScreen GameScreen => gameScreen;

		public static int Counter => counter++;

		public static Color ShadowColor => shadowColor;

		public static bool BulletTime => Hero.BulletTime;

		public static void Initialize(GameplayScreen gs)
		{
			gameScreen = gs;
			krypton = new KryptonEngine(ScreenManager.Game, "Effects\\Krypton\\KryptonEffect");
			krypton.AmbientColor = new Color(0, 0, 0);
			krypton.SpriteBatchCompatablityEnabled = true;
			krypton.CullMode = CullMode.None;
			krypton.Initialize();
			gameSettings = new GameSettings();
			textLine = 30;
			rand = new Random(69);
			levelFinished = false;
			inGameTimeInMs = 0.0;
			lastFrameGameTimeInMs = 0.0;
			realGameTimeInMs = 0.0;
			realLastFrameGameTimeInMs = 0.0;
			shadowColor = new Color(0, 0, 0, 100);
			maskSpritePos = new Vector2(100f, 800f);
		}

		public static void LoadContent()
		{
			counter = 0;
			gameSprites = new Dictionary<string, GameSprite>();
			fonts = new Dictionary<string, SpriteFont>();
			Settings.ContinuousPhysics = false;
			gravity = new Vector2(0f, 10f);
			world = new World(gravity);
			worldContactManager = new WorldContactManager();
			worldContactManager.Enabled = true;
			particlesmanager = new ParticlesManager();
			particlesmanager.Load();
			PresentationParameters presentationParameters = ScreenManager.GraphicsDevice.PresentationParameters;
			renderTargetObjects = new RenderTarget2D(ScreenManager.GraphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
			renderTargetBackground = new RenderTarget2D(ScreenManager.GraphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
			renderTargetNoises = new RenderTarget2D(ScreenManager.GraphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
			renderTargetHero = new RenderTarget2D(ScreenManager.GraphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
			noiseManager = new NoiseManager(ScreenManager.AudioManager);
			userInterface = new UserInterface();
			userInterface.Load();
			debugView = new DebugViewXNA(World);
			DebugView.LoadContent(ScreenManager.GraphicsDevice, GameScreen.Content);
			uint num = 0u;
			num += gameSettings.DrawShapes;
			num += gameSettings.DrawJoints * 2;
			num += gameSettings.DrawAABBs * 4;
			num += gameSettings.DrawPairs * 8;
			num += gameSettings.DrawCOMs * 16;
			debugView.Flags = (DebugViewFlags)num;
			worldObjects = new Dictionary<string, WorldObject>();
			worldShadowObjects = new Dictionary<string, WorldObject>();
			worldStaticObjects = new List<WorldObject>();
			worldObjectRemoveQueue = new List<string>();
			worldShadowObjectRemoveQueue = new List<string>();
			pathFindMap = new PathFindMap();
			enemiesControl = new EnemiesControl();
			tw = ScreenManager.GraphicsDevice.Viewport.Width;
			th = ScreenManager.GraphicsDevice.Viewport.Height;
			camera = new Camera2D(ScreenManager.GraphicsDevice);
			camera.Position = Vector2.Zero;
			Input.Initialize(new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2));
			level = new Level();
			alphaMaskEffect = GameScreen.Content.Load<Effect>("effects\\alphamap");
			alphaShadowEffect = GameScreen.Content.Load<Effect>("effects\\alphashadow");
			grayShadowEffect = GameScreen.Content.Load<Effect>("effects\\grayshadow");
			alphaNoisesEffect = GameScreen.Content.Load<Effect>("effects\\alphanoises");
			heroDrawEffect = GameScreen.Content.Load<Effect>("effects\\herodraw");
			CacheEffectParameters();
		}

		public static void Unload()
		{
		}

		public static void LoadLevel(string levelName)
		{
			level.Load(GameScreen.Content.Load<OgmoLevel>(levelName));
			camera.RefreshUnits();
			SetCameraZoom(1f);
		}

		public static void SetCameraZoom(float zoom)
		{
			camera.Zoom = zoom;
			Vector2 minPosition = level.UpLeft - Globals.SafeFrame + camera.CurrentSize / (2f * zoom);
			Vector2 maxPosition = level.DownRight + Globals.SafeFrame - camera.CurrentSize / (2f * zoom);
			if (minPosition.X >= maxPosition.X && minPosition.Y >= maxPosition.Y)
			{
				fixedCamera = true;
				camera.MinPosition = (level.DownRight - level.UpLeft) / 2f;
				camera.MaxPosition = camera.MinPosition;
				camera.Position = camera.MinPosition;
				camera.TrackingBody = null;
			}
			else
			{
				fixedCamera = false;
				if (minPosition.X >= maxPosition.X)
				{
					minPosition.X = (level.UpLeft + level.DownRight).X / 2f;
					maxPosition.X = minPosition.X;
				}
				if (minPosition.Y >= maxPosition.Y)
				{
					minPosition.Y = (level.UpLeft + level.DownRight).Y / 2f;
					maxPosition.Y = minPosition.Y;
				}
				camera.MinPosition = minPosition;
				camera.MaxPosition = maxPosition;
				camera.Position = ConvertUnits.ToDisplayUnits(hero.MainBody.Position);
				camera.TrackingBody = hero.MainBody;
			}
			camera.Jump2Target();
			level.SetCameraZoom(zoom);
		}

		public static void AddWorldObject(WorldObject wo)
		{
			if (!worldObjects.ContainsKey(wo.Name))
			{
				worldObjects.Add(wo.Name, wo);
				return;
			}
			throw new Exception("World Object Already Exists");
		}

		public static void AddShadowCasterWorldObject(WorldObject wo)
		{
			if (!worldShadowObjects.ContainsKey(wo.Name))
			{
				worldShadowObjects.Add(wo.Name, wo);
				return;
			}
			throw new Exception("World Object Already Exists");
		}

		public static void AddWorldStaticObject(WorldObject wo)
		{
			worldStaticObjects.Add(wo);
		}

		public static WorldObject GetWorldObjectByName(string name)
		{
			WorldObject value = null;
			if (worldObjects.TryGetValue(name, out value))
			{
				return value;
			}
			if (worldShadowObjects.TryGetValue(name, out value))
			{
				return value;
			}
			return value;
		}

		public static void AddEnemy(Enemy enemy)
		{
			enemiesControl.AddEnemy(enemy);
		}

		public static void RegisterNoise(Vector3 noise)
		{
			enemiesControl.RegisterNoise(noise);
		}

		public static void Update(GameTime gameTime)
		{
			GameTime = gameTime;
			double num = 16.666666666666668;
			double num2 = 1.0;
			NoiseManager.Pitch = -0f;
			if (levelFinished)
			{
				num2 = 0.20000000298023224;
				userInterface.StopWatch.Stop();
				NoiseManager.Pitch = -1f;
			}
			if (hero.IsDead())
			{
				userInterface.StopWatch.Stop();
				num2 = 0.2;
				NoiseManager.Pitch = -1f;
			}
			else if (hero.BulletTime)
			{
				num2 = 0.3;
				NoiseManager.Pitch = -1f;
			}
			realGameTimeInMs += num;
			realLastFrameGameTimeInMs = num;
			lastFrameGameTimeInMs = num * num2;
			inGameTimeInMs += lastFrameGameTimeInMs;
			float dt = (float)lastFrameGameTimeInMs * 0.001f;
			world.Step(dt);
			physicsFrames++;
			if (DebugFlags.DeathLog && hero != null && hero.IsDead() && physicsFrames % 10 == 0)
				Console.Error.WriteLine($"  dead f{physicsFrames} rot={hero.MainBody.Rotation:F2} angVel={hero.MainBody.AngularVelocity:F2} pos={hero.MainBody.Position} vel={hero.MainBody.LinearVelocity} awake={hero.MainBody.Awake} fixedRot={hero.MainBody.FixedRotation} inertia={hero.MainBody.Inertia:F2}");
			if (DebugFlags.JumpFrame > 0 && physicsFrames == DebugFlags.JumpFrame && hero != null)
			{
				Vector2 up = new Vector2(0f, -hero.MainBody.Mass * 12f);
				hero.MainBody.ApplyLinearImpulse(ref up); // debug: scripted jump
			}
			if (DebugFlags.GirderLog && physicsFrames % 15 == 0)
			{
				foreach (var wo in worldShadowObjects.Values)
				{
					if (wo is GirderSmall g && g.MainBody != null)
						Console.Error.WriteLine($"  girder f{physicsFrames} pos={g.MainBody.Position} rot={g.MainBody.Rotation:F2} vel={g.MainBody.LinearVelocity} awake={g.MainBody.Awake} repairs={world.NonFiniteRepairs} hero={hero.Position} {g.DebugRopes()}");
				}
			}
			if (DebugFlags.KillFrame > 0 && physicsFrames == DebugFlags.KillFrame && hero != null && !hero.IsDead())
			{
				hero.BloodyDie(); // debug: ragdoll test
			}
			if (DebugFlags.KillEnemyFrame > 0 && physicsFrames == DebugFlags.KillEnemyFrame)
			{
				enemiesControl.DebugKillAll(); // debug: enemy ragdoll test
			}
			if (!nanReported)
			{
				foreach (var body in world.BodyList)
				{
					if (float.IsNaN(body.Position.X) || float.IsNaN(body.Position.Y) || float.IsNaN(body.Rotation))
					{
						nanReported = true;
						string owner = body.UserData is WorldObjectData wod ? (wod.Object != null ? wod.Object.GetType().Name + " '" + wod.Object.Name + "'" : wod.Type.ToString()) : (body.UserData == null ? "null" : body.UserData.GetType().Name);
						string shapes = "";
						foreach (var f in body.FixtureList) shapes += f.Shape.ShapeType + "(" + (f.UserData == null ? "null" : f.UserData.GetType().Name) + ") ";
						string ownerName = "?";
						foreach (var wo in worldObjects.Values) if (wo.DebugOwnsBody(body)) ownerName = wo.GetType().Name + " '" + wo.Name + "'";
						foreach (var f in body.FixtureList) if (f.UserData is WorldObjectData fwod && fwod.Object != null) ownerName += " fixture:" + fwod.Object.GetType().Name + " '" + fwod.Object.Name + "'";
						Console.Error.WriteLine($"NaN body after step frame={physicsFrames} dt={dt}: owner={owner}/{ownerName} type={body.BodyType} pos={body.Position} rot={body.Rotation} linVel={body.LinearVelocity} angVel={body.AngularVelocity} shapes={shapes}");
						break;
					}
				}
			}
			Level.Update();
			particlesmanager.Update();
			noiseManager.Update();
			userInterface.Update();
			if (Input.ControlKeyboard.IsNewKeyPress(Keys.F1, GameScreen.ControllingPlayer, out outPlayerIndex))
			{
				if (debugViewEnabled)
				{
					debugViewEnabled = false;
				}
				else
				{
					debugViewEnabled = true;
				}
			}
			if (Input.ControlKeyboard.IsNewKeyPress(Globals.InputKeyShowMap, GameScreen.ControllingPlayer, out outPlayerIndex))
			{
				SetCameraZoom(0.5f);
			}
			if (Input.ControlKeyboard.IsReleasedKeyPress(Globals.InputKeyShowMap, GameScreen.ControllingPlayer, out outPlayerIndex))
			{
				SetCameraZoom(1f);
			}
			textLine = 30;
			if (!fixedCamera)
			{
				Vector2 vector = ConvertWorldToScreen(hero.Position);
				vector.X = (int)vector.X;
				vector.Y = (int)vector.Y;
			}
			camera.Update(gameTime);
			foreach (WorldObject value in worldObjects.Values)
			{
				if (value.Enabled)
				{
					value.Update();
				}
				else
				{
					worldObjectRemoveQueue.Add(value.Name);
				}
			}
			foreach (WorldObject value2 in worldShadowObjects.Values)
			{
				if (value2.Enabled)
				{
					value2.Update();
				}
				else
				{
					worldShadowObjectRemoveQueue.Add(value2.Name);
				}
			}
			enemiesControl.Update();
			if (!hero.Disposed)
			{
				hero.Update();
			}
			CleanWorldObjects();
			if (Level.Passed && !levelFinished)
			{
				timeFinished = realGameTimeInMs;
				levelFinished = true;
			}
		}

		public static bool FinishByAchieve(out double time)
		{
			time = 0.0;
			if (levelFinished && timeFinished + 1000.0 <= realGameTimeInMs)
			{
				time = timeFinished;
				return true;
			}
			return false;
		}

		public static bool FinishByFail()
		{
			if (hero.IsDead() && hero.TimeOfDeath + 4000.0 <= CurrentTimeInMS)
			{
				return true;
			}
			return false;
		}

		private static void CleanWorldObjects()
		{
			foreach (string item in worldObjectRemoveQueue)
			{
				worldObjects.Remove(item);
			}
			foreach (string item2 in worldShadowObjectRemoveQueue)
			{
				worldShadowObjects.Remove(item2);
			}
			worldObjectRemoveQueue.Clear();
			worldShadowObjectRemoveQueue.Clear();
		}

		public static Vector2 GetScreenCenter()
		{
			return new Vector2(tw / 2, th / 2);
		}

		public static Vector2 ConvertWorldToScreen(Vector2 pos)
		{
			return ConvertUnits.ToDisplayUnits(pos);
		}

		public static Vector2 ConvertScreenToWorld(Vector2 pos)
		{
			return ConvertUnits.ToSimUnits(pos);
		}
	}
}
