using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Be_Stiff.AI
{
	public class Brain
	{
		private const double reactionTime = 500.0;

		private BrainState[] brainStates;

		private Enemy owner;

		private Vector2 referencePoint;

		private int currentBrainState;

		private ActionExecution actionExecution;

		private Stance stance;

		private Stack<Sector> destinationPath;

		private Portal nextPortal;

		private Vector2 destinationPoint;

		// Last start/end sector pair with no path between them, so an enemy
		// chasing an unreachable target doesn't rerun the search every frame.
		private Sector unreachableFrom;

		private Sector unreachableTo;

		// Portals can open later (elevators, platforms, broken walls), so the
		// unreachable pair is retried after this much real time.
		private const double UnreachableRetryMs = 250.0;

		private double unreachableTime;

		private Sector destinationSector;

		private bool hasDestination;

		private Vector2 lastHeroPosition;

		private Vector2 relativeVisibleHeroPosition;

		private double lastTimeLookingHero;

		private bool aimingAtHero;

		private double timeAskedSquad;

		private List<Vector2> patrolPath;

		public int CurrentPatrolPoint;

		private List<int> sittingSpot;

		private double timeOfAlert;

		private double timeAlerted;

		private int alertLevel;

		private double shootReactionTime;

		private double shootTime;

		private bool hasToShoot;

		private float safeDirection;

		private double startTimeForSafeDirection;

		private EnemySquad squad;

		private int squadPos;

		private ISeat seat;

		private WorldObjectData[] touchingObjects;

		private int touchingObjectsNum;

		private string[] noiseAlarm;

		private string noiseHelp;

		private string[] noiseOK;

		private string noiseQuestion;

		public bool HeroDetected => alertLevel == 2;

		public bool AimingAtHero => aimingAtHero;

		public float WeaponRange => owner.WeaponRange;

		public Sector NextSectorPath
		{
			get
			{
				if (destinationPath.Count > 0)
				{
					return destinationPath.Peek();
				}
				return null;
			}
		}

		public bool Alerted => alertLevel > 0;

		public Vector2 HeroPosition => lastHeroPosition + relativeVisibleHeroPosition;

		public Vector2 OwnerPosition => owner.Position;

		public Side SideLooking => owner.SideLooking;

		public Stance Stance => stance;

		public Vector2 ReferencePoint
		{
			get
			{
				return referencePoint;
			}
			set
			{
				referencePoint = value;
			}
		}

		public List<Vector2> PatrolPath => patrolPath;

		public List<int> SittingSpots => sittingSpot;

		public Brain(Enemy theOwner)
		{
			owner = theOwner;
			actionExecution = new ActionExecution(owner);
			stance = Stance.Aggressive;
			currentBrainState = 0;
			brainStates = new BrainState[8];
			brainStates[1] = new BrainAttacking(this);
			brainStates[7] = new BrainDead(this);
			brainStates[5] = new BrainFollowingOrders(this);
			brainStates[0] = new BrainIdle(this);
			brainStates[4] = new BrainInvestigating(this);
			brainStates[2] = new BrainPatrolling(this);
			brainStates[3] = new BrainScanning(this);
			brainStates[6] = new BrainSeated(this);
			brainStates[currentBrainState].Init();
			destinationPath = new Stack<Sector>();
			patrolPath = new List<Vector2>();
			sittingSpot = new List<int>();
			CurrentPatrolPoint = 0;
			safeDirection = 0f;
			startTimeForSafeDirection = 0.0;
			shootReactionTime = 300.0;
			squad = null;
			squadPos = -1;
			touchingObjects = new WorldObjectData[10];
			touchingObjectsNum = 0;
			timeAlerted = 0.0;
			alertLevel = 0;
		}

		public void Load()
		{
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyAlarm1", "audio\\noises\\enemy\\enemyAlarm1");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyAlarm2", "audio\\noises\\enemy\\enemyAlarm2");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyAlarm3", "audio\\noises\\enemy\\enemyAlarm3");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyHelp", "audio\\noises\\enemy\\enemyHelp");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyOK1", "audio\\noises\\enemy\\enemyOK1");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyOK2", "audio\\noises\\enemy\\enemyOK2");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyOK3", "audio\\noises\\enemy\\enemyOK3");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyOK4", "audio\\noises\\enemy\\enemyOK4");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("enemyQuestion", "audio\\noises\\enemy\\enemyQuestion");
			GameElementsControl.LoadSprite("noiseWhat", "sprites\\noises\\what");
			GameElementsControl.LoadSprite("noiseAlert", "sprites\\noises\\alert");
			GameElementsControl.LoadSprite("noiseHelp", "sprites\\noises\\help");
			GameElementsControl.LoadSprite("noiseOK", "sprites\\noises\\ok");
			noiseAlarm = new string[3];
			noiseAlarm[0] = "enemyAlarm1";
			GameElementsControl.NoiseManager.LoadNoise("enemyAlarm1", "alert", "enemyAlarm1", 500.0, 0f);
			noiseAlarm[1] = "enemyAlarm2";
			GameElementsControl.NoiseManager.LoadNoise("enemyAlarm2", "alert", "enemyAlarm2", 500.0, 0f);
			noiseAlarm[2] = "enemyAlarm3";
			GameElementsControl.NoiseManager.LoadNoise("enemyAlarm3", "alert", "enemyAlarm3", 500.0, 0f);
			noiseOK = new string[4];
			noiseOK[0] = "enemyOK1";
			GameElementsControl.NoiseManager.LoadNoise("enemyOK1", "ok", "enemyOK1", 500.0, 0f);
			noiseOK[1] = "enemyOK2";
			GameElementsControl.NoiseManager.LoadNoise("enemyOK2", "ok", "enemyOK2", 500.0, 0f);
			noiseOK[2] = "enemyOK3";
			GameElementsControl.NoiseManager.LoadNoise("enemyOK3", "ok", "enemyOK3", 500.0, 0f);
			noiseOK[3] = "enemyOK4";
			GameElementsControl.NoiseManager.LoadNoise("enemyOK4", "ok", "enemyOK4", 500.0, 0f);
			noiseHelp = "enemyHelp";
			GameElementsControl.NoiseManager.LoadNoise("enemyHelp", "help", "enemyHelp", 500.0, 0f);
			noiseQuestion = "enemyQuestion";
			GameElementsControl.NoiseManager.LoadNoise("enemyQuestion", "what", "enemyQuestion", 500.0, 0f);
		}

		public void SetCurrentSeat(ISeat theSeat)
		{
			seat = theSeat;
		}

		public void AddNoiseAlarm()
		{
			int num = GameElementsControl.Random.Next(3);
			GameElementsControl.NoiseManager.AddVisualNoise(noiseAlarm[num], OwnerPosition + new Vector2(0f, 3f));
		}

		public void AddNoiseOK()
		{
			int num = GameElementsControl.Random.Next(4);
			GameElementsControl.NoiseManager.AddVisualNoise(noiseOK[num], OwnerPosition + new Vector2(0f, 3f));
		}

		public void AddNoiseHelp()
		{
			GameElementsControl.NoiseManager.AddVisualNoise(noiseHelp, OwnerPosition + new Vector2(0f, 3f));
		}

		public void Alert()
		{
			if (alertLevel == 0)
			{
				GameElementsControl.NoiseManager.AddVisualNoise(noiseQuestion, OwnerPosition + new Vector2(0f, 3f));
				alertLevel = 1;
			}
			timeOfAlert = GameElementsControl.CurrentTimeInMS;
		}

		public void AimToPoint(ref Vector2 point)
		{
			owner.AimToPoint(ref point);
		}

		public bool AimToRelativePoint(Vector2 point, float speed)
		{
			return owner.AimToRelativePoint(point, speed);
		}

		public void StopWalking()
		{
			owner.Stop();
		}

		public bool RequestSquad()
		{
			if (timeAskedSquad + 1000.0 < GameElementsControl.CurrentTimeInMS && !HasSquad() && HeroDetected)
			{
				timeAskedSquad = GameElementsControl.CurrentTimeInMS;
				return true;
			}
			return false;
		}

		public void SetSquad(EnemySquad newSquad, int pos)
		{
			squad = newSquad;
			squadPos = pos;
			lastHeroPosition = squad.GetHeroPos();
			AssignToSquad();
		}

		public void AddPathPoint(Vector2 point)
		{
			patrolPath.Add(point);
			sittingSpot.Add(0);
		}

		public void SetSittingSpot(int spot, Side side)
		{
			sittingSpot[spot] = (int)side;
		}

		public bool HasToScan()
		{
			if (sittingSpot[CurrentPatrolPoint] == 0)
			{
				return true;
			}
			return false;
		}

		public bool ShouldSeat()
		{
			if (currentBrainState != 6)
			{
				return false;
			}
			float x = (PatrolPath[CurrentPatrolPoint] - owner.Position).X;
			if (Math.Abs(x) < 0.05f)
			{
				return true;
			}
			return false;
		}

		public bool HasASeat()
		{
			if (seat == null)
			{
				return false;
			}
			if (sittingSpot[CurrentPatrolPoint] == (int)seat.Side)
			{
				float x = (seat.ReferencePoint - owner.Position).X;
				if (Math.Abs(x) < 0.1f)
				{
					return true;
				}
			}
			return false;
		}

		public void GetTouchingObjects(out WorldObjectData[] worldObjects, out int number)
		{
			ContactEdge contactEdge = owner.MainBody.ContactList;
			touchingObjectsNum = 0;
			if (contactEdge != null)
			{
				do
				{
					foreach (Fixture fixture in contactEdge.Other.FixtureList)
					{
						if (fixture.UserData is WorldObjectData worldObjectData)
						{
							touchingObjects[touchingObjectsNum] = worldObjectData;
							touchingObjectsNum++;
						}
					}
					contactEdge = contactEdge.Next;
				}
				while (contactEdge != null && touchingObjectsNum < 10);
			}
			worldObjects = touchingObjects;
			number = touchingObjectsNum;
		}

		// Set once the brain has reacted to the hero's death.
		private bool heroDeathHandled;

		/// <summary>
		/// The hero died: drop the alert and any chase, and go back to patrolling
		/// instead of running to (or hitting) the body.
		/// </summary>
		private void ForgetHero()
		{
			alertLevel = 0;
			timeAlerted = 0.0;
			aimingAtHero = false;
			hasToShoot = false;
			relativeVisibleHeroPosition = Vector2.Zero;
			if (currentBrainState == 1 || currentBrainState == 4 || currentBrainState == 5)
			{
				MarkArrived();
				currentBrainState = 2;
				brainStates[currentBrainState].Init();
			}
		}

		public void Update()
		{
			if (GameElementsControl.Hero.IsDead())
			{
				if (!heroDeathHandled)
				{
					heroDeathHandled = true;
					ForgetHero();
				}
			}
			else
			{
				heroDeathHandled = false;
				Vector2 centerPosition = GameElementsControl.Hero.GetCenterPosition();
				if ((centerPosition - owner.Position).Length() < 13f)
				{
					Vector2[] lineOfSight = GameElementsControl.Hero.GetLineOfSight();
					RegisterHeroPosition(centerPosition, lineOfSight);
				}
			}
			if (hasToShoot)
			{
				shootTime -= GameElementsControl.LastFrameTimeInMS;
				if (shootTime <= 0.0)
				{
					// The hero may have moved away or ducked during the
					// reaction time: check again instead of swinging at air.
					if (CanHitHero())
					{
						owner.Shoot();
					}
					hasToShoot = false;
				}
			}
			if (owner.SectorPosition == null && owner.TouchingFloor())
			{
				GoToSafeSector();
			}
			else
			{
				safeDirection = 0f;
				int num = brainStates[currentBrainState].Update();
				if (num != currentBrainState)
				{
					currentBrainState = num;
					brainStates[currentBrainState].Init();
				}
				actionExecution.Update();
			}
			if (!HeroDetected)
			{
				if (squadPos != -1 && squad.GetUpdatedInformation(out lastHeroPosition))
				{
					alertLevel = 2;
				}
			}
			else if (lastTimeLookingHero + 1000.0 <= GameElementsControl.CurrentTimeInMS)
			{
				if (squadPos != -1)
				{
					if (squad.GetUpdatedInformation(out lastHeroPosition))
					{
						alertLevel = 2;
					}
				}
				else
				{
					alertLevel = 1;
					referencePoint = lastHeroPosition;
					timeOfAlert = GameElementsControl.CurrentTimeInMS;
				}
			}
			if (alertLevel == 1 && timeOfAlert + timeAlerted + 500.0 + 1000.0 <= GameElementsControl.CurrentTimeInMS)
			{
				alertLevel = 0;
				timeAlerted = 0.0;
				relativeVisibleHeroPosition = Vector2.Zero;
			}
		}

		private void GoToSafeSector()
		{
			if (safeDirection == 0f)
			{
				owner.ForceUpdatePosition();
				if (owner.SectorPosition != null)
				{
					return;
				}
				float num = float.MaxValue;
				float num2 = float.MaxValue;
				Vector2 p = owner.Position;
				Vector2 p2 = p - new Vector2(100f, 0f);
				float num3 = GameElementsControl.PathFindMap.CheckClosestSector(ref p, ref p2);
				p2 = p - new Vector2(num3, 0f);
				if (CheckIfLineIsClear(p, p2))
				{
					num = num3;
				}
				p2 = p + new Vector2(100f, 0f);
				num3 = GameElementsControl.PathFindMap.CheckClosestSector(ref p, ref p2);
				p2 = p + new Vector2(num3, 0f);
				if (CheckIfLineIsClear(p, p2))
				{
					num2 = num3;
				}
				if (num < num2)
				{
					safeDirection = -100f;
				}
				else if (num > num2)
				{
					safeDirection = 100f;
				}
				else
				{
					safeDirection = (GameElementsControl.Random.Next(2) * 2 - 1) * 100; // Next(2): 0 or 1, i.e. left or right
				}
				startTimeForSafeDirection = GameElementsControl.CurrentTimeInMS;
			}
			else
			{
				owner.WalkTo(owner.Position.X + safeDirection, hasToStop: false);
			}
			if (startTimeForSafeDirection + 10000.0 < GameElementsControl.CurrentTimeInMS)
			{
				safeDirection = 0f;
			}
		}

		private void RegisterHeroPosition(Vector2 heroCenterPos, Vector2[] sightPoints)
		{
			if (alertLevel < 2)
			{
				float num = (owner.ArmPosition - heroCenterPos).GetAngle();
				float num2 = Math.Abs(MathHelper.WrapAngle(num - owner.SightAngle - (float)Math.PI));
				if (!((double)num2 <= Math.PI / 4.0))
				{
					return;
				}
				Vector2 armPosition = owner.ArmPosition;
				for (int i = 0; i < sightPoints.Length; i++)
				{
					Vector2 vector = sightPoints[i];
					float num3 = (vector - armPosition).Length();
					if (CheckIfLineIsClear(armPosition, vector))
					{
						lastHeroPosition = heroCenterPos;
						relativeVisibleHeroPosition = vector - heroCenterPos;
						if (alertLevel == 0)
						{
							referencePoint = heroCenterPos;
							Alert();
						}
						else
						{
							timeAlerted += GameElementsControl.LastFrameTimeInMS;
						}
						if (timeAlerted >= 500.0)
						{
							AddNoiseAlarm();
							lastTimeLookingHero = GameElementsControl.CurrentTimeInMS;
							timeAskedSquad = GameElementsControl.CurrentTimeInMS;
							alertLevel = 2;
							if (owner.WeaponRange >= num3)
							{
								aimingAtHero = true;
							}
							else
							{
								aimingAtHero = false;
							}
							if (squad != null)
							{
								squad.UpdateInformation(lastHeroPosition);
							}
						}
						i = sightPoints.Length;
					}
					else
					{
						aimingAtHero = false;
					}
				}
				return;
			}
			lastHeroPosition = heroCenterPos;
			Vector2 armPosition2 = owner.ArmPosition;
			for (int j = 0; j < sightPoints.Length; j++)
			{
				Vector2 vector2 = sightPoints[j];
				float num4 = (vector2 - armPosition2).Length();
				if (CheckIfLineIsClear(armPosition2, vector2))
				{
					relativeVisibleHeroPosition = vector2 - heroCenterPos;
					lastTimeLookingHero = GameElementsControl.CurrentTimeInMS;
					if (owner.WeaponRange >= num4)
					{
						aimingAtHero = true;
					}
					else
					{
						aimingAtHero = false;
					}
					if (squad != null)
					{
						squad.UpdateInformation(lastHeroPosition);
					}
					j = sightPoints.Length;
				}
				else
				{
					aimingAtHero = false;
				}
			}
		}

		public bool CheckIfLineIsClear(Vector2 p1, Vector2 p2)
		{
			return !RayCastCallBacks.RayCastLineOfSight(p1, p2, alertLevel > 0);
		}

		private bool CanLookThrough(Fixture fixture)
		{
			if (fixture.UserData is WorldObjectData worldObjectData)
			{
				if (worldObjectData.Object.CanSeeThrough(alertLevel > 0))
				{
					return true;
				}
				if (worldObjectData.Object is Human)
				{
					return true;
				}
			}
			return false;
		}

		public void HearNoise(Vector2 position)
		{
			int num = brainStates[currentBrainState].HearNoise(ref position);
			if (num != currentBrainState)
			{
				currentBrainState = num;
				brainStates[currentBrainState].Init();
			}
		}

		public void Hit(Vector2 position, HitType hitType)
		{
			int num = brainStates[currentBrainState].Hit(ref position, ref hitType);
			if (num != currentBrainState)
			{
				currentBrainState = num;
				brainStates[currentBrainState].Init();
			}
			shootTime = shootReactionTime * 1.5;
		}

		private void AssignToSquad()
		{
			currentBrainState = 5;
			brainStates[currentBrainState].Init();
		}

		public void RemoveFromSquad()
		{
			currentBrainState = 1;
			brainStates[currentBrainState].Init();
			squadPos = -1;
		}

		public void SetNewSquadPos(int pos)
		{
			squadPos = pos;
		}

		public bool HasSquad()
		{
			if (squad != null && squad.IsDissolved())
			{
				squad = null;
			}
			return squad != null;
		}

		public bool WaitingSquad()
		{
			return squad.HasPartnersNearThan(squadPos, 5f);
		}

		public Orders GetOrders()
		{
			return squad.GetOrders(squadPos);
		}

		public void Die()
		{
			currentBrainState = 7;
			brainStates[currentBrainState].Init();
		}

		public void Reset()
		{
			unreachableFrom = null;
			unreachableTo = null;
			alertLevel = 0;
			aimingAtHero = false;
			destinationPath.Clear();
			hasToShoot = false;
			timeAlerted = 0.0;
			relativeVisibleHeroPosition = Vector2.Zero;
			lastTimeLookingHero = 0.0;
			CurrentPatrolPoint = 0;
			currentBrainState = 0;
			brainStates[currentBrainState].Init();
		}

		private bool CanHitHero()
		{
			Hero hero = GameElementsControl.Hero;
			return aimingAtHero && !hero.IsDead() && (owner.WeaponCanHitLyingTarget || !hero.IsLied());
		}

		public void ShootAtHero()
		{
			if (!hasToShoot && CanHitHero())
			{
				shootTime = shootReactionTime;
				hasToShoot = true;
			}
		}

		public bool MoveToDestinationPoint(Vector2 point, float range)
		{
			SetDestinationPoint(ref point);
			float x = (point - owner.Position).X;
			if (Math.Abs(x) < 0.05f + range && destinationPath.Count == 0 && owner.SectorPosition == destinationSector)
			{
				MarkArrived();
				return true;
			}
			if (destinationPath.Count == 0)
			{
				Sector start = owner.SectorPosition;
				Sector end = destinationSector;
				if (start != null)
				{
					if (end == null)
					{
						MarkArrived();
						return true;
					}
					if (start != end && (start != unreachableFrom || end != unreachableTo || GameElementsControl.RealTimeInMS - unreachableTime > UnreachableRetryMs))
					{
						GameElementsControl.PathFindMap.GetPathFromTo(ref start, ref end, ref destinationPath);
						if (destinationPath.Count == 0)
						{
							unreachableFrom = start;
							unreachableTo = end;
							unreachableTime = GameElementsControl.RealTimeInMS;
						}
					}
				}
			}
			if (destinationPath.Count != 0)
			{
				if (MoveToSector(destinationPath.Peek()))
				{
					destinationPath.Pop();
				}
			}
			else
			{
				if (destinationSector != owner.SectorPosition)
				{
					MarkArrived();
					return true;
				}
				if (owner.WalkTo(point.X, hasToStop: true))
				{
					MarkArrived();
					return true;
				}
			}
			return false;
		}

		private bool MoveToSector(Sector sect)
		{
			Sector start = owner.SectorPosition;
			if (start != null)
			{
				if (!actionExecution.Performing)
				{
					if (start == sect)
					{
						return true;
					}
					Portal portalFor = start.getPortalFor(sect);
					if (portalFor != null)
					{
						if (owner.WalkTo(portalFor))
						{
							nextPortal = portalFor;
							actionExecution.ExecuteAction(portalFor);
						}
					}
					else
					{
						Sector sector = destinationSector;
						if (sector != null)
						{
							GameElementsControl.PathFindMap.GetPathFromTo(ref start, ref destinationSector, ref destinationPath);
						}
					}
				}
				else
				{
					actionExecution.ExecuteAction(nextPortal);
				}
			}
			return false;
		}

		private void SetDestinationPoint(ref Vector2 newDestinationPoint)
		{
			if ((newDestinationPoint - destinationPoint).Length() > 1f)
			{
				destinationPoint = newDestinationPoint;
				destinationSector = GameElementsControl.PathFindMap.GetSectorAt(ref newDestinationPoint);
				if (destinationPath.Count > 0 && destinationSector != destinationPath.Peek())
				{
					destinationPath.Clear();
				}
				hasDestination = true;
			}
		}

		private void MarkArrived()
		{
			hasDestination = false;
			destinationPath.Clear();
			destinationPoint = Vector2.Zero;
			owner.Stop();
		}
	}
}
