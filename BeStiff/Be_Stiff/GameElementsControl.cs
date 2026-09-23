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

namespace Be_Stiff
{
	internal static class GameElementsControl
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

		public static GameSprite GetSprite(string textureName)
		{
			gameSprites.TryGetValue(textureName, out var value);
			return value;
		}

		/// <summary>
		/// Intended world size of object art, keyed by the lower-case base name
		/// of the Ogmo template's texture file. Filled when a level loads.
		/// </summary>
		private static readonly Dictionary<string, Point> templateSizes = new Dictionary<string, Point>();

		private static bool legacyLevel;

		public static void RegisterTemplateSizes(OgmoXNA.OgmoProject project, bool legacy)
		{
			legacyLevel = legacy;
			templateSizes.Clear();
			foreach (var pair in gameSprites)
			{
				pair.Value.Scale = pair.Value.TexturePath != null ? LegacySpriteScale(pair.Value.TexturePath, pair.Value.Texture) : 1f;
			}
			foreach (var template in project.ObjectTemplates)
			{
				if (string.IsNullOrEmpty(template.TextureFile))
				{
					continue;
				}
				string key = System.IO.Path.GetFileNameWithoutExtension(template.TextureFile).ToLowerInvariant();
				templateSizes[key] = new Point(template.Width, template.Height);
			}
		}

		/// <summary>
		/// Some sprites were never redrawn when the game moved to its final
		/// scale and are exactly half the size their Ogmo template expects.
		/// Returns 2 for those, 1 otherwise.
		/// </summary>
		/// <summary>Art that was redrawn at twice its size for the current scale.</summary>
		private static readonly string[] RedrawnSprites =
		{
			"slopeleft", "sloperight", "exitdoor", "barrel1", "barrel2", "barrel3", "barrel4",
			"chairback", "chairleg", "chairseat", "tableleg", "tablesurface",
		};

		public static bool IsLegacyLevel => legacyLevel;

		private static float LegacySpriteScale(string texturePath, Texture2D texture)
		{
			if (legacyLevel)
			{
				// Old-scale level: old art as is, redrawn (2x) art at half.
				string baseName = texturePath.Replace('\\', '/');
				baseName = baseName.Substring(baseName.LastIndexOf('/') + 1).ToLowerInvariant();
				return System.Array.IndexOf(RedrawnSprites, baseName) >= 0 ? 0.5f : 1f;
			}
			string name = texturePath.Replace('\\', '/');
			name = name.Substring(name.LastIndexOf('/') + 1).ToLowerInvariant();
			Point size = Point.Zero;
			if (!templateSizes.TryGetValue(name, out size))
			{
				foreach (var pair in templateSizes)
				{
					if (pair.Key.Length >= 4 && name.StartsWith(pair.Key))
					{
						size = pair.Value;
						break;
					}
				}
			}
			if (size == Point.Zero)
			{
				return 1f;
			}
			float rx = (float)size.X / texture.Width;
			float ry = (float)size.Y / texture.Height;
			if (rx > 1.7f && rx < 2.3f && ry > 1.7f && ry < 2.3f)
			{
				return 2f;
			}
			return 1f;
		}

		public static void LoadSprite(string textureName, string texturePath)
		{
			if (!gameSprites.ContainsKey(textureName))
			{
				Texture2D fromTexture = GameScreen.Content.Load<Texture2D>(texturePath);
				GameSprite value = new GameSprite(fromTexture);
				value.TexturePath = texturePath;
				value.Scale = LegacySpriteScale(texturePath, fromTexture);
				if (value.Scale != 1f && Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null)
				{
					Console.Error.WriteLine($"  sprite {textureName} ({texturePath}) {fromTexture.Width}x{fromTexture.Height} drawn at x{value.Scale}");
				}
				gameSprites.Add(textureName, value);
			}
		}

		public static SpriteFont GetFont(string fontName)
		{
			if (fonts.ContainsKey(fontName))
			{
				return fonts[fontName];
			}
			SpriteFont spriteFont = GameScreen.Content.Load<SpriteFont>(fontName);
			fonts.Add(fontName, spriteFont);
			return spriteFont;
		}

		public static void LoadFont(string fontName, string fontPath)
		{
			if (!fonts.ContainsKey(fontName))
			{
				SpriteFont value = GameScreen.Content.Load<SpriteFont>(fontPath);
				fonts.Add(fontName, value);
			}
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
			if (Environment.GetEnvironmentVariable("BESTIFF_DEATH_LOG") != null && hero != null && hero.IsDead() && physicsFrames % 10 == 0)
				Console.Error.WriteLine($"  dead f{physicsFrames} rot={hero.MainBody.Rotation:F2} angVel={hero.MainBody.AngularVelocity:F2} pos={hero.MainBody.Position} vel={hero.MainBody.LinearVelocity} awake={hero.MainBody.Awake} fixedRot={hero.MainBody.FixedRotation} inertia={hero.MainBody.Inertia:F2}");
			if (int.TryParse(Environment.GetEnvironmentVariable("BESTIFF_JUMP_FRAME"), out int jumpFrame) && physicsFrames == jumpFrame && hero != null)
			{
				Vector2 up = new Vector2(0f, -hero.MainBody.Mass * 12f);
				hero.MainBody.ApplyLinearImpulse(ref up); // debug: scripted jump
			}
			if (Environment.GetEnvironmentVariable("BESTIFF_GIRDER_LOG") != null && physicsFrames % 15 == 0)
			{
				foreach (var wo in worldShadowObjects.Values)
				{
					if (wo is GirderSmall g && g.MainBody != null)
						Console.Error.WriteLine($"  girder f{physicsFrames} pos={g.MainBody.Position} rot={g.MainBody.Rotation:F2} vel={g.MainBody.LinearVelocity} awake={g.MainBody.Awake} repairs={world.NonFiniteRepairs} hero={hero.Position} {g.DebugRopes()}");
				}
			}
			if (int.TryParse(Environment.GetEnvironmentVariable("BESTIFF_KILL_FRAME"), out int killFrame) && physicsFrames == killFrame && hero != null && !hero.IsDead())
			{
				hero.BloodyDie(); // debug: ragdoll test
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

		public static void DebugDrawString(string str, params object[] args)
		{
		}

		public static void DebugDrawPoint(Vector2 p, float size, Color color)
		{
		}

		public static void DebugDrawSegment(Vector2 p1, Vector2 p2, Color color)
		{
		}

		public static void DebugDrawPolygon(ref Vector2[] vertices, Color color)
		{
		}

		public static void DebugDrawAABB(ref AABB aabb, Color color)
		{
		}

		private static void DrawMaskedObjects()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			alphaMaskEffect.Parameters["AlphaMap"].SetValue(krypton.mMap);
			alphaMaskEffect.CurrentTechnique = alphaMaskEffect.Techniques["AlphaMapShader"];
			alphaMaskEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetObjects, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
		}

		private static bool nanReported;
		private static int physicsFrames;

		/// <summary>Debug helper: describes the hero's state.</summary>
		public static string DebugHeroState()
		{
			if (hero == null) return "null";
			return $"disposed={hero.Disposed} dead={hero.IsDead()} pos={hero.Position} camera={Camera.Position}";
		}

		/// <summary>Debug helper: saves the intermediate render targets as PNGs.</summary>
		public static void DebugSaveRenderTargets(string prefix)
		{
			var targets = new (string, Microsoft.Xna.Framework.Graphics.Texture2D)[]
			{
				("lightmap", krypton != null ? krypton.mMap : null),
				("hero", renderTargetHero),
				("objects", renderTargetObjects),
				("background", renderTargetBackground),
				("noises", renderTargetNoises),
			};
			foreach (var (name, tex) in targets)
			{
				if (tex == null) continue;
				using (var fs = System.IO.File.Create(prefix + "_" + name + ".png"))
				{
					tex.SaveAsPng(fs, tex.Width, tex.Height);
				}
			}
		}

		private static void DrawHero()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			heroDrawEffect.Parameters["AlphaMap"].SetValue(renderTargetObjects);
			heroDrawEffect.CurrentTechnique = heroDrawEffect.Techniques["BlackShadowMapShader"];
			heroDrawEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetHero, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
		}

		private static void DrawBackground()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			grayShadowEffect.Parameters["AlphaMap"].SetValue(krypton.mMap);
			grayShadowEffect.CurrentTechnique = grayShadowEffect.Techniques["GrayShadowMapShader"];
			grayShadowEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetBackground, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
		}

		private static void DrawLineOfSightShadows()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			alphaShadowEffect.Parameters["ShadowColor"].SetValue(hero.ShadowColor.ToVector4());
			alphaShadowEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(krypton.mMap, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
		}

		private static void SetRenderTargets()
		{
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetObjects);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			foreach (WorldObject value in worldObjects.Values)
			{
				value.Draw();
			}
			enemiesControl.Draw();
			ScreenManager.SpriteBatch.End();
			particlesmanager.Draw(Camera.View);
			ScreenManager.GraphicsDevice.SetRenderTarget(null);
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetNoises);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			if (!hero.IsDead())
			{
				noiseManager.Draw();
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetHero);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			if (!hero.Disposed)
			{
				hero.Draw();
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetBackground);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			level.DrawBackgroundLayers();
			level.DrawBackground();
			foreach (WorldObject value2 in worldShadowObjects.Values)
			{
				value2.Draw();
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.GraphicsDevice.SetRenderTarget(null);
		}

		public static void Draw()
		{
			krypton.Matrix = Camera.View;
			krypton.LightMapPrepare();
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			SetRenderTargets();
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			DrawBackground();
			DrawLineOfSightShadows();
			DrawMaskedObjects();
			DrawHero();
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			foreach (WorldObject value in worldShadowObjects.Values)
			{
				IShadowCaster shadowCaster = (IShadowCaster)value;
				shadowCaster.DrawHull();
			}
			level.DrawFloor();
			level.DrawGoals();
			hero.DrawCrossHair();
			level.DrawFrames();
			alphaNoisesEffect.Parameters["AlphaMap"].SetValue(krypton.mMap);
			alphaNoisesEffect.Parameters["ObjectsMap"].SetValue(renderTargetObjects);
			alphaNoisesEffect.Parameters["HeroMap"].SetValue(renderTargetHero);
			alphaNoisesEffect.CurrentTechnique = alphaNoisesEffect.Techniques["AlphaNoisesShader"];
			alphaNoisesEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetNoises, Camera.Position, null, Color.White, 0f, Camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
			ScreenManager.SpriteBatch.Begin();
			userInterface.Draw();
			ScreenManager.SpriteBatch.End();
			if (debugViewEnabled)
			{
				Matrix projection = Camera.SimProjection;
				Matrix view = Camera.SimView;
				debugView.RenderDebugData(ref projection, ref view);
			}
		}
	}
}
