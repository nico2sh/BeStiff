using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Layers;
using OgmoXNA.Layers.Settings;
using OgmoXNA.Values;

namespace Be_Stiff
{
	public class Level
	{
		public string Name;

		private string startText;

		private string finishText;

		private List<Goal> goals;

		private Goal finalGoal;

		private bool passed;

		private bool alwaysRun;

		private TileGrid tilesBackground;

		private bool hasTilesBackground;

		private TextureRectangles tilesFloor;

		private Decals decals;

		private bool loaded;

		private Background background;

		private Vector2 upLeft;

		private Vector2 downRight;

		private GameSprite frameSprite;

		private Vector2 verticalFrame;

		private Vector2 horizontalFrame;

		private Vector2[] framesPosition;

		private int th;

		private int tw;

		private int width;

		private int height;

		private float worldWidth;

		private float worldHeight;

		public bool AlwaysRun => alwaysRun;

		public Vector2 UpLeft => upLeft;

		public Vector2 DownRight => downRight;

		public Color BackGroundColor => background.Color;

		public int RemainingGoals => goals.Count;

		public bool Passed => passed;

		public string StartText => startText;

		public string FinishText => finishText;

		public Level()
		{
			loaded = false;
			goals = new List<Goal>();
			finalGoal = null;
			passed = false;
			alwaysRun = false;
		}

		public void Load(OgmoLevel ogmoLevel)
		{
			Name = ogmoLevel.GetValue<OgmoStringValue>("title").Value;
			if (Globals.OptionControl == 0)
			{
				string value = ogmoLevel.GetValue<OgmoStringValue>("startTextAlternative").Value;
				if (value != "")
				{
					startText = value;
				}
				else
				{
					startText = ogmoLevel.GetValue<OgmoStringValue>("startText").Value;
				}
			}
			else
			{
				startText = ogmoLevel.GetValue<OgmoStringValue>("startText").Value;
			}
			finishText = ogmoLevel.GetValue<OgmoStringValue>("finishText").Value;
			GameElementsControl.LoadSprite("blackPixel", "sprites\\blackpixel");
			GameElementsControl.NoiseManager.LoadNoise("stepWalking", "step", "step", 300.0, 0f);
			GameElementsControl.NoiseManager.LoadNoise("stepWalkingHero", "step", "step", 300.0, 5f);
			GameElementsControl.ScreenManager.AudioManager.LoadSound("step", "audio\\noises\\step");
			hasTilesBackground = false;
			upLeft = new Vector2(0f, 0f);
			downRight = new Vector2(ogmoLevel.Width, ogmoLevel.Height);
			if (ogmoLevel.GetLayer<OgmoTileLayer>("tiles_bg") != null)
			{
				tilesBackground = new TileGrid(ogmoLevel.GetLayer<OgmoTileLayer>("tiles_bg"), ogmoLevel.Width, ogmoLevel.Height);
				hasTilesBackground = true;
			}
			if (ogmoLevel.GetLayer<OgmoTileLayer>("tiles_floors") != null)
			{
				tilesFloor = new TextureRectangles(ogmoLevel.GetLayer<OgmoTileLayer>("tiles_floors"));
			}
			decals = new Decals();
			if (ogmoLevel.GetLayer<OgmoObjectLayer>("decals") != null)
			{
				decals.Load(ogmoLevel.GetLayer<OgmoObjectLayer>("decals"));
			}
			Dictionary<WorldObject, OgmoObject> dictionary = new Dictionary<WorldObject, OgmoObject>();
			// Old-scale levels were built for 24 display pixels per simulation
			// unit (all hard-coded body sizes match their art at that ratio);
			// the current project uses 48.
			ConvertUnits.SetDisplayUnitToSimUnitRatio(ogmoLevel.LegacyScale > 1 ? 24f : 48f);
			GameElementsControl.RegisterTemplateSizes(ogmoLevel.Project, ogmoLevel.LegacyScale > 1);
			OgmoObject[] objects = ogmoLevel.GetLayer<OgmoObjectLayer>("objects").Objects;
			if (Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null)
			{
				Console.Error.WriteLine($"Level {ogmoLevel.Width}x{ogmoLevel.Height}");
				foreach (var t in ogmoLevel.Project.ObjectTemplates)
					Console.Error.WriteLine($"  template {t.Name} {t.Width}x{t.Height} origin={t.Origin} resizable={t.IsResizableX}/{t.IsResizableY} tiled={t.IsTiled} tex={(t.Texture == null ? "null" : t.Texture.Width + "x" + t.Texture.Height)}");
				foreach (var ls in ogmoLevel.Project.LayerSettings)
					Console.Error.WriteLine($"  layer {ls.Name} grid={ls.GridSize}");
				foreach (OgmoObject o in objects)
				{
					string vals = "";
					foreach (OgmoValue v in o.Values) vals += " " + v.Name + "=" + (v is OgmoNumberValue nv ? nv.Value.ToString() : v is OgmoIntegerValue iv ? iv.Value.ToString() : v is OgmoBooleanValue bv ? bv.Value.ToString() : v is OgmoStringValue sv ? "'" + sv.Value + "'" : v.ToString());
					Console.Error.WriteLine($"  {o.Name} pos={o.Position} size={o.Width}x{o.Height} rot={o.Rotation} origin={o.Origin} tiled={o.IsTiled} tex={(o.Texture == null ? "null" : o.Texture.Width + "x" + o.Texture.Height)} src={o.Source} nodes={o.Nodes.Length}{string.Join("", System.Linq.Enumerable.Select(o.Nodes, n => " node=" + n.Position))}{vals}");
				}
			}
			foreach (OgmoObject ogmoObject in objects)
			{
				if (ogmoObject.Name.Equals("Hero"))
				{
					Vector2 vector = ConvertUnits.ToSimUnits(ogmoObject.Position);
					vector.Y -= 2f;
					GameElementsControl.Hero = new Hero();
					GameElementsControl.Hero.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Elevator"))
				{
					Elevator elevator = new Elevator();
					elevator.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("MovingPlatformH"))
				{
					MovingPlatformH movingPlatformH = new MovingPlatformH();
					movingPlatformH.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("MovingPlatformV"))
				{
					MovingPlatformV movingPlatformV = new MovingPlatformV();
					movingPlatformV.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("SlidingDoor"))
				{
					SlidingDoor slidingDoor = new SlidingDoor();
					slidingDoor.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Glass"))
				{
					Glass glass = new Glass();
					glass.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("BWall"))
				{
					BreakableWall breakableWall = new BreakableWall();
					breakableWall.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Table"))
				{
					Table table = new Table();
					table.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.StartsWith("Chair"))
				{
					Chair chair = new Chair();
					chair.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("EnemyPistol"))
				{
					EnemyPistol enemyPistol = new EnemyPistol();
					enemyPistol.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("EnemyUnarmed"))
				{
					EnemyUnarmed enemyUnarmed = new EnemyUnarmed();
					enemyUnarmed.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("EnemyFatUnarmed"))
				{
					EnemyFatUnarmed enemyFatUnarmed = new EnemyFatUnarmed();
					enemyFatUnarmed.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("EnemyStick"))
				{
					EnemyStick enemyStick = new EnemyStick();
					enemyStick.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Cannon") && Environment.GetEnvironmentVariable("BESTIFF_NO_CANNON") == null)
				{
					WallCannon wallCannon = new WallCannon();
					wallCannon.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Turret"))
				{
					Turret turret = new Turret();
					turret.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Barrel"))
				{
					Barrel barrel = new Barrel();
					barrel.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Crate"))
				{
					Crate crate = new Crate();
					crate.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("CrateSmall"))
				{
					CrateSmall crateSmall = new CrateSmall();
					crateSmall.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.StartsWith("Slope"))
				{
					Slope slope = new Slope();
					slope.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.StartsWith("LargeSlope"))
				{
					LargeSlope largeSlope = new LargeSlope();
					largeSlope.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("GirderSmall"))
				{
					GirderSmall girderSmall = new GirderSmall();
					if (!girderSmall.LoadFromOgmo(ogmoObject))
					{
						dictionary.Add(girderSmall, ogmoObject);
					}
				}
				if (ogmoObject.Name.Equals("OneSidedPlatform"))
				{
					OneSidedPlatform oneSidedPlatform = new OneSidedPlatform();
					oneSidedPlatform.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("OneSidedInvisiblePlatform"))
				{
					OneSidedInvisiblePlatform oneSidedInvisiblePlatform = new OneSidedInvisiblePlatform();
					oneSidedInvisiblePlatform.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Lamp"))
				{
					Lamp lamp = new Lamp();
					if (!lamp.LoadFromOgmo(ogmoObject))
					{
						dictionary.Add(lamp, ogmoObject);
					}
				}
				if (ogmoObject.Name.Equals("Spikes"))
				{
					Spikes spikes = new Spikes();
					if (!spikes.LoadFromOgmo(ogmoObject))
					{
						dictionary.Add(spikes, ogmoObject);
					}
				}
				if (ogmoObject.Name.Equals("Info"))
				{
					InfoSign infoSign = new InfoSign();
					infoSign.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("GunChest"))
				{
					GunChest gunChest = new GunChest();
					gunChest.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("HookChest"))
				{
					HookChest hookChest = new HookChest();
					hookChest.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("BulletChest"))
				{
					BulletChest bulletChest = new BulletChest();
					bulletChest.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("GrenadeChest"))
				{
					GrenadeChest grenadeChest = new GrenadeChest();
					grenadeChest.LoadFromOgmo(ogmoObject);
				}
				if (ogmoObject.Name.Equals("Vault"))
				{
					Vault vault = new Vault();
					vault.LoadFromOgmo(ogmoObject);
					goals.Add(vault);
				}
				if (ogmoObject.Name.Equals("Exit"))
				{
					Exit exit = new Exit();
					exit.LoadFromOgmo(ogmoObject);
					finalGoal = exit;
				}
				ogmoObject.Name.Equals("moving_platform");
				ogmoObject.Name.StartsWith("spike");
			}
			foreach (WorldObject key in dictionary.Keys)
			{
				key.LoadFromOgmo(dictionary[key]);
			}
			if (ogmoLevel.GetLayer<OgmoObjectLayer>("sectors") != null)
			{
				objects = ogmoLevel.GetLayer<OgmoObjectLayer>("sectors").Objects;
				foreach (OgmoObject ogmoObject2 in objects)
				{
					if (ogmoObject2.Name.Equals("Sector"))
					{
						Sector sector = new Sector();
						sector.LoadFromOgmo(ogmoObject2);
					}
				}
			}
			if (ogmoLevel.GetLayer<OgmoObjectLayer>("portals") != null)
			{
				objects = ogmoLevel.GetLayer<OgmoObjectLayer>("portals").Objects;
				foreach (OgmoObject ogmoObject3 in objects)
				{
					if (ogmoObject3.Name.Substring(0, 6).Equals("Portal"))
					{
						Portal portal = new Portal();
						portal.LoadFromOgmo(ogmoObject3);
					}
				}
			}
			int[,] rawData = ogmoLevel.GetLayer<OgmoGridLayer>("floors").RawData;
			if (Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null)
			{
				int gs = ogmoLevel.GetLayer<OgmoGridLayer>("floors").EffectiveGridSize;
				Console.Error.WriteLine($"GridLayer floors: {rawData.GetLength(0)}x{rawData.GetLength(1)} gridSize={gs}");
				for (int y = 0; y < rawData.GetLength(1); y++)
				{
					string row = "";
					for (int x = 0; x < rawData.GetLength(0); x++) row += rawData[x, y] != 0 ? "#" : ".";
					Console.Error.WriteLine("  grid " + row);
				}
			}
			int gridSize = ogmoLevel.GetLayer<OgmoGridLayer>("floors").EffectiveGridSize;
			Rectangle[] array = ConvertTilesToVerticalRectangles(rawData, gridSize);
			Rectangle[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Rectangle rec = array2[i];
				ConvertUnits.ToSimUnits(rec.Width);
				ConvertUnits.ToSimUnits(rec.Height);
				ConvertUnits.ToSimUnits(rec.X, rec.Y);
				CollisionWorldObject collisionWorldObject = new CollisionWorldObject();
				collisionWorldObject.LoadFromOgmo(rec);
			}
			frameSprite = GameElementsControl.GetSprite("blackPixel");
			tw = GameElementsControl.ScreenManager.GraphicsDevice.Viewport.Width;
			th = GameElementsControl.ScreenManager.GraphicsDevice.Viewport.Height;
			width = ogmoLevel.Width;
			height = ogmoLevel.Height;
			worldWidth = ConvertUnits.ToSimUnits(width);
			worldHeight = ConvertUnits.ToSimUnits(height);
			if (ogmoLevel.GetLayer<OgmoObjectLayer>("background") != null)
			{
				OgmoObject ogmoObject4 = ogmoLevel.GetLayer<OgmoObjectLayer>("background").Objects[0];
				if (ogmoObject4.Name.Equals("Skyline"))
				{
					Vector2 position = ogmoObject4.Position;
					int value2 = ogmoObject4.GetValue<OgmoIntegerValue>("R").Value;
					int value3 = ogmoObject4.GetValue<OgmoIntegerValue>("G").Value;
					int value4 = ogmoObject4.GetValue<OgmoIntegerValue>("B").Value;
					background = new BackgroundSkyline();
					background.Load(position, new Color(value2, value3, value4));
				}
				else if (ogmoObject4.Name.Equals("Clouds"))
				{
					Vector2 position2 = ogmoObject4.Position;
					int value5 = ogmoObject4.GetValue<OgmoIntegerValue>("R").Value;
					int value6 = ogmoObject4.GetValue<OgmoIntegerValue>("G").Value;
					int value7 = ogmoObject4.GetValue<OgmoIntegerValue>("B").Value;
					background = new BackgroundClouds();
					background.Load(position2, new Color(value5, value6, value7));
				}
				else if (ogmoObject4.Name.Equals("Cushions"))
				{
					Vector2 position3 = ogmoObject4.Position;
					int value8 = ogmoObject4.GetValue<OgmoIntegerValue>("R").Value;
					int value9 = ogmoObject4.GetValue<OgmoIntegerValue>("G").Value;
					int value10 = ogmoObject4.GetValue<OgmoIntegerValue>("B").Value;
					background = new BackgroundCushions();
					background.Load(position3, new Color(value8, value9, value10));
				}
			}
			SetCameraZoom(GameElementsControl.Camera.Zoom);
			if (goals.Count == 0 && finalGoal != null)
			{
				finalGoal.Active = true;
			}
			loaded = true;
		}

		public Rectangle[] ConvertTilesToVerticalRectangles(int[,] rawData, int gridSize)
		{
			List<Rectangle>[] array = new List<Rectangle>[rawData.GetLength(0)];
			bool[,] array2 = new bool[rawData.GetLength(0), rawData.GetLength(1)];
			for (int i = 0; i < rawData.GetLength(0); i++)
			{
				array[i] = new List<Rectangle>();
				for (int j = 0; j < rawData.GetLength(1); j++)
				{
					if (rawData[i, j] == 1 && !array2[i, j])
					{
						array2[i, j] = true;
						Rectangle item = new Rectangle(i * gridSize, j * gridSize, gridSize, gridSize);
						int num = 1;
						for (int k = j + 1; k < rawData.GetLength(1) && rawData[i, k] == rawData[i, j] && !array2[i, k]; k++)
						{
							item.Height += gridSize;
							array2[i, k] = true;
							num++;
						}
						array[i].Add(item);
					}
				}
			}
			for (int num2 = array.Length - 1; num2 > 0; num2--)
			{
				List<Rectangle> list = array[num2 - 1];
				List<Rectangle> list2 = array[num2];
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					Rectangle value = list[num3];
					for (int num4 = list2.Count - 1; num4 >= 0; num4--)
					{
						Rectangle rectangle = list2[num4];
						if (value.Height == rectangle.Height && value.Right == rectangle.Left && value.Y == rectangle.Y)
						{
							value.Width += rectangle.Width;
							list2.RemoveAt(num4);
							list[num3] = value;
						}
					}
				}
			}
			List<Rectangle> list3 = new List<Rectangle>();
			List<Rectangle>[] array3 = array;
			foreach (List<Rectangle> collection in array3)
			{
				list3.AddRange(collection);
			}
			return list3.ToArray();
		}

		public Rectangle[] ConvertTilesToRectangles(int[,] rawData, int gridSize)
		{
			List<Rectangle> list = new List<Rectangle>();
			bool[,] array = new bool[rawData.GetLength(0), rawData.GetLength(1)];
			for (int i = 0; i < rawData.GetLength(0); i++)
			{
				for (int j = 0; j < rawData.GetLength(1); j++)
				{
					if (rawData[i, j] != 1 || array[i, j])
					{
						continue;
					}
					array[i, j] = true;
					Rectangle item = new Rectangle(i * gridSize, j * gridSize, gridSize, gridSize);
					int num = 1;
					int num2 = 1;
					for (int k = j + 1; k < rawData.GetLength(1) && rawData[i, k] == rawData[i, j] && !array[i, k]; k++)
					{
						item.Height += gridSize;
						array[i, k] = true;
						num2++;
					}
					bool flag = true;
					while (flag && i + num < rawData.GetLength(0))
					{
						for (int k = j; k < j + num2; k++)
						{
							if (rawData[i, k] != rawData[i + num, k])
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							item.Width += gridSize;
							for (int k = j; k < j + num2; k++)
							{
								array[i + num, k] = true;
							}
							num++;
						}
					}
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		public bool IsInside(Vector2 position)
		{
			if (position.X > 0f && position.X < worldWidth && position.Y < 0f)
			{
				return position.Y > 0f - worldHeight;
			}
			return false;
		}

		public void AchieveGoal(Goal goal)
		{
			if (goal == finalGoal)
			{
				passed = true;
				return;
			}
			goals.Remove(goal);
			if (goals.Count == 0)
			{
				if (finalGoal != null)
				{
					finalGoal.Active = true;
				}
				else
				{
					passed = true;
				}
			}
		}

		public void SetCameraZoom(float zoom)
		{
			float num = (float)width * zoom;
			float num2 = (float)height * zoom;
			if ((float)tw > num)
			{
				verticalFrame = new Vector2(((float)tw - num) / (2f * zoom), num2 / zoom);
			}
			else
			{
				verticalFrame = new Vector2(0f, num2 / zoom + Globals.SafeFrame.Y * 2f);
			}
			if ((float)th > num2)
			{
				horizontalFrame = new Vector2((num + verticalFrame.X * 2f) / zoom, ((float)th - num2) / (2f * zoom));
			}
			else
			{
				horizontalFrame = new Vector2((num + verticalFrame.X * 2f) / zoom + Globals.SafeFrame.X * 2f, 0f);
			}
			verticalFrame += new Vector2(Globals.SafeFrame.X, 0f);
			horizontalFrame += new Vector2(0f, Globals.SafeFrame.Y);
			framesPosition = new Vector2[4];
			ref Vector2 reference = ref framesPosition[0];
			reference = new Vector2(UpLeft.X - verticalFrame.X, UpLeft.Y - horizontalFrame.Y);
			ref Vector2 reference2 = ref framesPosition[1];
			reference2 = new Vector2(DownRight.X, UpLeft.Y);
			ref Vector2 reference3 = ref framesPosition[2];
			reference3 = new Vector2(UpLeft.X - verticalFrame.X, DownRight.Y);
			ref Vector2 reference4 = ref framesPosition[3];
			reference4 = new Vector2(UpLeft.X - verticalFrame.X, UpLeft.Y);
			background.SetCameraZoom(zoom);
		}

		public void Update()
		{
			if (!loaded)
			{
				throw new Exception("Level is not loaded");
			}
			background.Update();
		}

		public void DrawBackgroundLayers()
		{
			background.Draw();
		}

		public void DrawBackground()
		{
			if (hasTilesBackground)
			{
				tilesBackground.Draw();
			}
			decals.Draw();
		}

		public void DrawFloor()
		{
			tilesFloor.Draw();
		}

		public void DrawGoals()
		{
			foreach (Goal goal in goals)
			{
				goal.DrawGoal();
			}
			if (finalGoal != null)
			{
				finalGoal.DrawGoal();
			}
		}

		public void DrawFrames()
		{
			frameSprite.Draw(framesPosition[0], horizontalFrame);
			frameSprite.Draw(framesPosition[1], verticalFrame);
			frameSprite.Draw(framesPosition[2], horizontalFrame);
			frameSprite.Draw(framesPosition[3], verticalFrame);
		}

		public void AddWorldObject(WorldObject wo)
		{
		}
	}
}
