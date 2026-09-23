using System;
using System.IO;
using System.Text;
using EasyStorage;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Be_Stiff
{
	internal static class Globals
	{
		public const Category CollisionEnemy = Category.Cat2;

		public const Category CollisionHero = Category.Cat3;

		public const Category CollisionFeetCategory = Category.Cat5;

		public const Category CollisionFragmentCategory = Category.Cat6;

		public const Category CollisionProjectile = Category.Cat7;

		public const Category CollisionIgnoreProjectile = Category.Cat8;

		private const Category _collisionPolygons = Category.Cat10;

		private const Category _collisionElevator = Category.Cat15;

		private const Category _collisionLights = Category.Cat20;

		public const short CollisionHeroGroup = -1;

		private const short _collisionTableGroup = 2;

		private const short _collisionHumanGroup = -3;

		private const short _collisionGlassPieceGroup = 11;

		private const float _floorAngleLimit = 0.6f;

		public static string GameVersion = "Alpha 0.04";

		public static LevelsManager LevelsManager = new LevelsManager();

		public static Vector2 SafeFrame = new Vector2(68f, 38f);

		public static bool OptionFullScreen = false;

		public static int OptionCurrentScreenResolution = 0;

		public static int OptionSoundVolume = 10;

		public static int OptionMusicVolume = 10;

		public static int OptionControl = 0;

		public static int ActiveControl = 0;

		private static char[] numberBuffer = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		public static Keys InputKeyStart = Keys.Enter;

		public static Keys InputKeyBack = Keys.Escape;

		public static Keys InputKeyLeft = Keys.A;

		public static Keys InputKeyRight = Keys.D;

		public static Keys InputKeyUp = Keys.W;

		public static Keys InputKeyDown = Keys.S;

		public static Keys InputKeyJump = Keys.Space;

		public static Keys InputKeyReload = Keys.R;

		public static Keys InputKeyShowMap = Keys.Tab;

		public static Keys InputKeySwitch = Keys.E;

		public static Keys InputKeySlide = Keys.LeftControl;

		public static Keys InputKeySloMo = Keys.F;

		public static Keys InputKeyKick = Keys.None;

		public static Keys InputKeyShoot = Keys.None;

		public static Keys InputKeySecShoot = Keys.LeftShift;

		public static Buttons InputButtonJump = Buttons.A;

		public static Buttons InputButtonReload = Buttons.Y;

		public static Buttons InputButtonShowMap = Buttons.Back;

		public static Buttons InputButtonSwitch = Buttons.LeftShoulder;

		public static Buttons InputButtonSlide = Buttons.B;

		public static Buttons InputButtonSloMo = Buttons.RightStick;

		public static Buttons InputButtonKick = Buttons.X;

		public static Buttons InputButtonShoot = Buttons.RightTrigger;

		public static Buttons InputButtonSecShoot = Buttons.RightShoulder;

		public static Buttons InputButtonRun = Buttons.LeftTrigger;

		public static MouseButtons InputMouseButtonJump = MouseButtons.None;

		public static MouseButtons InputMouseButtonReload = MouseButtons.None;

		public static MouseButtons InputMouseButtonShowMap = MouseButtons.None;

		public static MouseButtons InputMouseButtonSwitch = MouseButtons.None;

		public static MouseButtons InputMouseButtonSlide = MouseButtons.None;

		public static MouseButtons InputMouseButtonSloMo = MouseButtons.None;

		public static MouseButtons InputMouseButtonKick = MouseButtons.RightButton;

		public static MouseButtons InputMouseButtonShoot = MouseButtons.LeftButton;

		public static MouseButtons InputMouseButtonSecShoot = MouseButtons.None;

		public static int ScoreEndLevel = 5000;

		public static int ScoreOpenVaultExplosive = 2000;

		public static int ScoreAchieveVault = 3000;

		public static int ScoreShootEnemy = 1500;

		public static int ScoreHeadShootEnemy = 500;

		public static int ScoreKickEnemy = 3000;

		public static int ScoreKillEnemy = 5000;

		public static int ScoreCrushEnemy = 2000;

		public static int ScoreExplodeEnemy = 30;

		public static int ScoreDestroyCannon = 4000;

		public static int ScoreShootCannon = 500;

		public static int ScoreExplodeCannon = 40;

		public static IAsyncSaveDevice SaveDevice;

		private static string fileName_options = "BeStiff_Options";

		private static string fileName_saves = "BeStiff_Save";

		private static string containerName = "BeStiff_Save";

		public static float FloorAngleLimit => 0.6f;

		public static void Init()
		{
			LoadDefaultLevels();
		}

		private static void LoadDefaultLevels()
		{
			LevelsManager.Clear();
			LevelsManager.AddNewArea("The Training");
			LevelsManager.GetArea("The Training").AddLevel("First Tutorial", "tutorial1", new Vector2(0f, 0f)).Unlock();
			LevelsManager.GetArea("The Training").GetLevel("tutorial1").AddNextLevel("tutorial2");
			LevelsManager.GetArea("The Training").AddLevel("Enemies", "tutorial2", new Vector2(150f, 100f));
			LevelsManager.GetArea("The Training").GetLevel("tutorial2").AddNextLevel("testlevel");
			LevelsManager.GetArea("The Training").AddLevel("Test Level", "testlevel", new Vector2(320f, 0f));
		}

		public static void LoadOptions()
		{
			try
			{
				if (!SaveDevice.IsReady)
				{
					return;
				}
				if (SaveDevice.FileExists(containerName, fileName_options))
				{
					SaveDevice.Load(containerName, fileName_options, delegate(Stream stream)
					{
						using (StreamReader streamReader = new StreamReader(stream))
						{
							string text = streamReader.ReadLine();
							if (text != GameVersion)
							{
								SaveOptions();
							}
							else
							{
								OptionCurrentScreenResolution = int.Parse(streamReader.ReadLine());
								OptionFullScreen = bool.Parse(streamReader.ReadLine());
								OptionSoundVolume = int.Parse(streamReader.ReadLine());
								OptionMusicVolume = int.Parse(streamReader.ReadLine());
								OptionControl = int.Parse(streamReader.ReadLine());
								streamReader.ReadLine();
								InputKeyLeft = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyRight = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyUp = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyDown = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyJump = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyReload = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyShowMap = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeySwitch = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeySlide = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeySloMo = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyKick = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeyShoot = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								InputKeySecShoot = (Keys)Enum.Parse(typeof(Keys), streamReader.ReadLine(), ignoreCase: true);
								streamReader.ReadLine();
								InputButtonJump = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonReload = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonShowMap = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonSwitch = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonSlide = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonSloMo = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonKick = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonShoot = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonSecShoot = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								InputButtonRun = (Buttons)Enum.Parse(typeof(Buttons), streamReader.ReadLine(), ignoreCase: true);
								streamReader.ReadLine();
								InputMouseButtonJump = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonReload = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonShowMap = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonSwitch = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonSlide = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonSloMo = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonKick = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonShoot = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
								InputMouseButtonSecShoot = (MouseButtons)Enum.Parse(typeof(MouseButtons), streamReader.ReadLine(), ignoreCase: true);
							}
						}
					});
				}
				else
				{
					SaveOptions();
				}
			}
			catch (Exception)
			{
			}
		}

		public static void SaveOptions()
		{
			try
			{
				if (!SaveDevice.IsReady)
				{
					return;
				}
				SaveDevice.SaveAsync(containerName, fileName_options, delegate(Stream stream)
				{
					using (StreamWriter streamWriter = new StreamWriter(stream))
					{
						streamWriter.WriteLine(GameVersion);
						streamWriter.WriteLine(OptionCurrentScreenResolution);
						streamWriter.WriteLine(OptionFullScreen);
						streamWriter.WriteLine(OptionSoundVolume);
						streamWriter.WriteLine(OptionMusicVolume);
						streamWriter.WriteLine(OptionControl);
						streamWriter.WriteLine("==KeyboardControls==");
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyLeft));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyRight));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyUp));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyDown));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyJump));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyReload));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyShowMap));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeySwitch));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeySlide));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeySloMo));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyKick));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeyShoot));
						streamWriter.WriteLine(Enum.GetName(typeof(Keys), InputKeySecShoot));
						streamWriter.WriteLine("==GamePadControls==");
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonJump));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonReload));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonShowMap));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonSwitch));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonSlide));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonSloMo));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonKick));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonShoot));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonSecShoot));
						streamWriter.WriteLine(Enum.GetName(typeof(Buttons), InputButtonRun));
						streamWriter.WriteLine("==MouseButtonControls==");
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonJump));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonReload));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonShowMap));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonSwitch));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonSlide));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonSloMo));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonKick));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonShoot));
						streamWriter.WriteLine(Enum.GetName(typeof(MouseButtons), InputMouseButtonSecShoot));
					}
				});
			}
			catch (Exception)
			{
			}
		}

		public static void LoadSaveGames()
		{
			try
			{
				if (!SaveDevice.IsReady)
				{
					return;
				}
				if (SaveDevice.FileExists(containerName, fileName_saves))
				{
					SaveDevice.Load(containerName, fileName_saves, delegate(Stream stream)
					{
						using (StreamReader streamReader = new StreamReader(stream))
						{
							string text = streamReader.ReadLine();
							if (text != GameVersion)
							{
								SaveSaveGames();
							}
							else
							{
								while (!streamReader.EndOfStream)
								{
									string text2 = streamReader.ReadLine();
									int num = text2.IndexOf('|');
									string areaName = text2.Substring(0, num);
									string levelAssetName = text2.Substring(num + 1);
									Area area = LevelsManager.GetArea(areaName);
									if (area == null)
									{
										SaveSaveGames();
										break;
									}
									LevelSelection level = area.GetLevel(levelAssetName);
									if (level == null)
									{
										SaveSaveGames();
										break;
									}
									int score = int.Parse(streamReader.ReadLine());
									int time = int.Parse(streamReader.ReadLine());
									bool unlocked = bool.Parse(streamReader.ReadLine());
									level.LoadValues(score, time, unlocked);
								}
							}
						}
					});
				}
				else
				{
					SaveSaveGames();
				}
			}
			catch (Exception)
			{
			}
		}

		public static void SaveSaveGames()
		{
			try
			{
				if (!SaveDevice.IsReady)
				{
					return;
				}
				SaveDevice.SaveAsync(containerName, fileName_saves, delegate(Stream stream)
				{
					using (StreamWriter streamWriter = new StreamWriter(stream))
					{
						streamWriter.WriteLine(GameVersion);
						foreach (Area area in LevelsManager.Areas)
						{
							foreach (LevelSelection level in area.Levels)
							{
								streamWriter.WriteLine(area.Name + "|" + level.AssetName);
								streamWriter.WriteLine(level.BestScore);
								streamWriter.WriteLine(level.BestTime);
								streamWriter.WriteLine(level.Unlocked);
							}
						}
					}
				});
			}
			catch (Exception)
			{
			}
		}

		public static bool CollidesWithHuman(Fixture fixture)
		{
			if (Collides(fixture))
			{
				if (fixture.UserData is WorldObjectData worldObjectData && worldObjectData.Object is Human)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public static bool Collides(Fixture fixture)
		{
			if (fixture.CollisionCategories != Category.None && fixture.Body.Enabled && !fixture.IsSensor)
			{
				return true;
			}
			return false;
		}

		public static float GetAngle(this Vector2 v)
		{
			return (float)Math.Atan2(v.Y, v.X);
		}

		public static Vector2 CreateVector2(float angle, float length)
		{
			return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
		}

		public static float PerpDot(Vector2 a, Vector2 b)
		{
			return (0f - a.Y) * b.X + a.X * b.Y;
		}

		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
		{
			T result = value;
			if (value.CompareTo(max) > 0)
			{
				result = max;
			}
			if (value.CompareTo(min) < 0)
			{
				result = min;
			}
			return result;
		}

		public static StringBuilder AppendNumber(this StringBuilder stringBuilder, int number, int minDigits)
		{
			if (number < 0)
			{
				stringBuilder.Append('-');
				number = -number;
			}
			int num = 0;
			do
			{
				int num2 = number % 10;
				numberBuffer[num] = (char)(48 + num2);
				number /= 10;
				num++;
			}
			while (number > 0 || num < minDigits);
			for (num--; num >= 0; num--)
			{
				stringBuilder.Append(numberBuffer[num]);
			}
			return stringBuilder;
		}
	}
}
