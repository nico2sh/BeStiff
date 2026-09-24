using System;
using System.Linq;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.Characters
{
	public abstract class Enemy : Human
	{
		private Brain brain;

		private Vector2 worldPosition;

		private Sector sectorPosition;

		private Sector lastKnownSectorPosition;

		private double lastTimeUpdatedPosition;

		private bool hasSpawner;

		private bool pooled;

		public bool JumpingDown;

		public Brain Brain => brain;

		public Sector SectorPosition => sectorPosition;

		public Sector LastKnownSectorPosition => lastKnownSectorPosition;

		public Vector2 ArmScreenPosition => arm.ScreenPosition;

		public Vector2 ArmPosition => arm.Position;

		public float ArmRotation => arm.GetRotation();

		public Enemy()
			: base(new Vector2(0.3f, 1.95f))
		{
			sightAngle = 0f;
		}

		public Enemy(Vector2 size)
			: base(size)
		{
			sightAngle = 0f;
		}

		protected override void LoadCustomStuff(OgmoObject obj)
		{
			base.Name = obj.GetValue<OgmoStringValue>("ID").Value;
			brain = new Brain(this);
			brain.Load();
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			brain.AddPathPoint(worldScaledOgmoObject.Position);
			for (int i = 0; i < worldScaledOgmoObject.Nodes.Count(); i++)
			{
				brain.AddPathPoint(worldScaledOgmoObject.Nodes[i].Position);
			}
			worldPosition = base.Position;
			sectorPosition = GameElementsControl.PathFindMap.GetSectorAt(ref worldPosition);
			if (sectorPosition != null)
			{
				lastKnownSectorPosition = sectorPosition;
			}
			lastTimeUpdatedPosition = GameElementsControl.CurrentTimeInMS;
			if (obj.GetValue<OgmoBooleanValue>("LookingLeft").Value)
			{
				sideLooking = Side.Left;
				sightAngle = (float)Math.PI;
			}
			if (obj.GetValue<OgmoBooleanValue>("Seated").Value)
			{
				brain.SetSittingSpot(0, sideLooking);
			}
			if (obj.GetValue<OgmoBooleanValue>("AutoSpawn").Value)
			{
				new EnemySpawner(this, worldScaledOgmoObject.Position);
				hasSpawner = true;
			}
			else
			{
				hasSpawner = false;
			}
			pooled = false;
			LoadEnemyType();
			GameElementsControl.AddEnemy(this);
		}

		protected virtual void LoadEnemyType()
		{
		}

		private int debugFrames;

		/// <summary>
		/// Enemies still use the pre-state-machine flow: dispatch on the simple
		/// state and apply the animation the state handlers picked (the newer
		/// Human base class only drives the hero's state machine).
		/// </summary>
		protected override void UpdateState()
		{
			switch (state)
			{
			case StateEnum.Walking:
				StateWalking();
				StateAlways();
				break;
			case StateEnum.Jumping:
				StateJumping();
				StateAlways();
				break;
			case StateEnum.Standed:
				StateStanded();
				StateAlways();
				break;
			}
			if (!string.IsNullOrEmpty(currentAnimation))
			{
				skeleton.SetAnimation(currentAnimation);
			}
		}

		public override void Update()
		{
			if (DebugFlags.Dump && base.Name == "Mik7" && debugFrames++ < 8)
				Console.Error.WriteLine($"Mik7 f{debugFrames} pos={Position} feet={FeetPosition} vel={MainBody.LinearVelocity} angVel={MainBody.AngularVelocity} rot={MainBody.Rotation} balanceMotor={balanceRevoluteJoint.MotorSpeed} wheelMotor={wheelRevoluteJoint.MotorSpeed} floorAngle={floorAngle} state={state} frameMs={GameElementsControl.LastFrameTimeInMS}");
			if (!Disposed && (float.IsNaN(Position.X) || float.IsNaN(Position.Y)))
			{
				// Physics body blew up (usually spawned inside level geometry).
				Console.Error.WriteLine("Enemy '" + base.Name + "' has a NaN position, removing it");
				Dispose();
				return;
			}
			if (!IsDead())
			{
				arm.SetRotation(sightAngle);
				base.Update();
				UpdatePosition();
				brain.Update();
			}
			else if (timeOfDeath + 5000.0 <= GameElementsControl.CurrentTimeInMS)
			{
				Dispose();
			}
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			brain.Hit(position, hitType);
			weapon[activeWeapon].Holster();
			switch (hitType)
			{
			case HitType.Blunt:
				currentAnimation = "HIT";
				skeleton.SetAnimation(currentAnimation);
				currentEnergy -= hitDirection.Length() * 4f;
				break;
			case HitType.Kick:
			{
				float num2 = hitDirection.Length();
				currentEnergy -= num2;
				if (state != StateEnum.Dead)
				{
					currentAnimation = "HIT";
					skeleton.SetAnimation(currentAnimation);
					GameElementsControl.Score.AddPoints(Globals.ScoreKickEnemy);
					if (currentEnergy <= 0f)
					{
						bodyRect.ApplyLinearImpulse(hitDirection);
					}
				}
				break;
			}
			case HitType.Pistol:
			{
				float y = bodyRect.GetLocalPoint(position).Y;
				if (y >= physHeight / 2f - physWidth)
				{
					blood.ReleaseQuantity = 50;
					currentEnergy = 0f;
					if (state != StateEnum.Dead)
					{
						GameElementsControl.Score.AddPoints(Globals.ScoreHeadShootEnemy);
					}
				}
				blood.Direction = hitDirection.GetAngle();
				blood.Trigger(GameElementsControl.ConvertWorldToScreen(position));
				currentEnergy -= hitDirection.Length();
				blood.ReleaseQuantity = 10;
				if (state != StateEnum.Dead)
				{
					GameElementsControl.Score.AddPoints(Globals.ScoreShootEnemy);
				}
				break;
			}
			case HitType.Explosion:
				currentEnergy -= hitDirection.Length() * 0.5f;
				if (state != StateEnum.Dead)
				{
					float num = MathHelper.Clamp(hitDirection.Length(), 0f, 100f);
					num *= (float)Globals.ScoreExplodeEnemy;
					GameElementsControl.Score.AddPoints((int)num);
				}
				break;
			}
		}

		protected override void Crush()
		{
			if (!pooled)
			{
				if (state != StateEnum.Dead)
				{
					GameElementsControl.Score.AddPoints(Globals.ScoreCrushEnemy);
				}
				base.Crush();
			}
		}

		protected override void Dispose()
		{
			if (!pooled)
			{
				if (!hasSpawner)
				{
					base.Dispose();
					return;
				}
				DeactivateGeoms();
				skeleton.DeactivateContact();
				pooled = true;
			}
		}

		public override void Draw()
		{
			if (!pooled)
			{
				if (!IsDead())
				{
					weapon[activeWeapon].Draw();
				}
				skeleton.Draw(GameElementsControl.ScreenManager.SpriteBatch);
			}
		}

		protected override void Die()
		{
			if (state != StateEnum.Dead)
			{
				GameElementsControl.Score.AddPoints(Globals.ScoreKillEnemy);
			}
			base.Die();
			brain.Die();
		}

		public override void Resurrect()
		{
			base.Resurrect();
			skeleton.ReactivateContact();
			brain.Reset();
			lastTimeUpdatedPosition = 0.0;
			pooled = false;
		}

		public override bool IgnoreOneSide()
		{
			return JumpingDown;
		}

		public void ForceUpdatePosition()
		{
			worldPosition = base.Position;
			sectorPosition = GameElementsControl.PathFindMap.GetSectorAt(ref worldPosition);
			if (sectorPosition != null)
			{
				lastKnownSectorPosition = sectorPosition;
			}
			lastTimeUpdatedPosition = GameElementsControl.CurrentTimeInMS;
		}

		private void UpdatePosition()
		{
			if ((Math.Abs(base.Position.X - worldPosition.X) > 0.5f || Math.Abs(base.Position.Y - worldPosition.Y) > 0.5f) && lastTimeUpdatedPosition + 100.0 < GameElementsControl.CurrentTimeInMS)
			{
				worldPosition = base.Position;
				sectorPosition = GameElementsControl.PathFindMap.GetSectorAt(ref worldPosition);
				if (sectorPosition != null)
				{
					lastKnownSectorPosition = sectorPosition;
				}
				lastTimeUpdatedPosition = GameElementsControl.CurrentTimeInMS;
			}
			if (brain.HeroDetected)
			{
				Vector2 pos = brain.HeroPosition;
				AimToPoint(ref pos);
			}
			else if (!brain.HeroDetected)
			{
				if (state == StateEnum.Walking || state == StateEnum.Jumping)
				{
					int num = ((!((double)Math.Abs(bodyRect.LinearVelocity.X) < 0.1)) ? Math.Sign(bodyRect.LinearVelocity.X) : ((int)sideLooking));
					AimTo((float)((1 - num) / 2) * (float)Math.PI);
				}
				else
				{
					_ = state;
					_ = 3;
				}
			}
		}

		public override void Shoot()
		{
			base.Shoot();
		}

		public bool AimToPoint(ref Vector2 pos)
		{
			float angle = (pos - arm.Position).GetAngle();
			return AimTo(angle);
		}

		public bool AimToRelativePoint(Vector2 pos, float speed)
		{
			float num = pos.GetAngle();
			if ((double)Math.Abs(MathHelper.WrapAngle(sightAngle - num)) > 0.05)
			{
				float num2 = (float)(Math.PI / (double)speed * GameElementsControl.LastFrameTimeInMS);
				float num3 = MathHelper.Clamp(MathHelper.WrapAngle(sightAngle - num), 0f - num2, num2);
				sightAngle = MathHelper.WrapAngle(sightAngle) - num3;
				return false;
			}
			return true;
		}

		private bool AimTo(float angle)
		{
			float num = MathHelper.WrapAngle(sightAngle);
			if ((double)Math.Abs(num - angle) > 0.05)
			{
				float num2 = (float)(Math.PI / 500.0 * GameElementsControl.LastFrameTimeInMS);
				float num3 = MathHelper.Clamp(MathHelper.WrapAngle(sightAngle - angle), 0f - num2, num2);
				sightAngle = num - num3;
				return false;
			}
			return true;
		}

		public void Stop()
		{
			state = StateEnum.Standed;
			ActionStopWalking();
		}

		public void JumpTo(ref Vector2 point)
		{
			if (Landed())
			{
				Vector2 linearVelocity = bodyRect.LinearVelocity;
				Vector2 vector = point - FloorPosition;
				float num = Math.Min(Math.Abs(vector.X) / 3f, 0.25f);
				float num2 = ((vector.Y < 0f) ? num : (vector.Y + num));
				float num3 = num2 - vector.Y;
				float num4 = (float)Math.Sqrt(2f * (0f - GameElementsControl.Gravity.Y) * num2) - linearVelocity.Y;
				float num5 = Math.Abs(num4 / 10f);
				float num6 = (float)Math.Sqrt(2f * num3 / (0f - GameElementsControl.Gravity.Y));
				float num7 = num5 + num6;
				float y = Mass * num4;
				float x = Mass * (vector.X / num7 - linearVelocity.X);
				Vector2 impulse = new Vector2(x, y);
				bodyRect.ApplyLinearImpulse(ref impulse);
				lastJumpTime = GameElementsControl.CurrentTimeInMS;
			}
		}

		public override void ActionStopWalking()
		{
			base.ActionStopWalking();
		}

		public override void ActionWalk(Side side, float maxSpeed)
		{
			if (Landed())
			{
				if (currentAnimation == "RUN")
				{
					if (skeleton.AnimationKeyFrameAt(0) || skeleton.AnimationKeyFrameAt(2))
					{
						floorWorldObject.StepOver(bodyCircBottom.Position, WorldObjectType.Human, running: true);
					}
				}
				else if (currentAnimation == "WALK")
				{
					if (skeleton.AnimationKeyFrameAt(0) || skeleton.AnimationKeyFrameAt(3))
					{
						floorWorldObject.StepOver(bodyCircBottom.Position, WorldObjectType.Human, running: false);
					}
				}
				else if (currentAnimation == "BACKWALK" && (skeleton.AnimationKeyFrameAt(0) || skeleton.AnimationKeyFrameAt(2)))
				{
					floorWorldObject.StepOver(bodyCircBottom.Position, WorldObjectType.Human, running: false);
				}
			}
			switch (state)
			{
			case StateEnum.Jumping:
			{
				float value = maxSpeed * (float)side - bodyRect.LinearVelocity.X;
				float num = maxSpeed / 2000f * (float)GameElementsControl.LastFrameTimeInMS;
				value = ((side != Side.Left) ? MathHelper.Clamp(value, 0f, num) : MathHelper.Clamp(value, 0f - num, 0f));
				Vector2 impulse = new Vector2(value * mass, 0f);
				bodyRect.ApplyLinearImpulse(ref impulse);
				break;
			}
			case StateEnum.Walking:
				base.ActionWalk(side, maxSpeed);
				break;
			}
		}

		protected override void StateJumping()
		{
			if (TouchingFloor())
			{
				if (bodyRect.LinearVelocity.Y < -5f)
				{
					currentAnimation = "LAND";
				}
			}
			else if (bodyRect.LinearVelocity.Y < -5f)
			{
				currentAnimation = "FALLING";
			}
			else
			{
				currentAnimation = "JUMP";
			}
			base.StateJumping();
		}

		protected override void StateWalking()
		{
			if (Math.Sign(bodyRect.LinearVelocity.X) != (int)sideLooking)
			{
				currentAnimation = "BACKWALK";
			}
			else if (Math.Abs(bodyRect.LinearVelocity.X) < runSpeed / 3f)
			{
				currentAnimation = "WALK";
			}
			else
			{
				currentAnimation = "RUN";
			}
			base.StateWalking();
		}

		protected override void StateStanded()
		{
			if (brain.ShouldSeat())
			{
				if (brain.HasASeat())
				{
					currentAnimation = "SEATED";
				}
				else
				{
					currentAnimation = "FLOORSEATED";
				}
			}
			else
			{
				currentAnimation = "STAND";
			}
			if (!brain.HeroDetected && Brain.NextSectorPath != null)
			{
				Vector2 pos = Brain.NextSectorPath.CenterPosition;
				AimToPoint(ref pos);
			}
			base.StateStanded();
		}

		public bool WalkTo(Portal portal)
		{
			Vector2 pos = base.Position;
			WalkTo(portal.Position.X, hasToStop: true);
			return portal.isInside(ref pos);
		}

		public override bool CanBeHooked()
		{
			return false;
		}

		private void CheckObstacles()
		{
			Side side = (Side)Math.Sign(bodyRect.LinearVelocity.X);
			float num = 1.5f;
			float num2 = (physHeight - physWidth) / 2f;
			float num3 = ((side == Side.Left) ? (0f - (physWidth / 2f + num)) : (physWidth * 0.5f));
			AABB aabb = default(AABB);
			aabb.LowerBound = base.Position + new Vector2(num3, 0f - num2);
			aabb.UpperBound = base.Position + new Vector2(num + num3, num2);
			if (sectorPosition != null)
			{
				if (side == Side.Left)
				{
					aabb.LowerBound.X = MathHelper.Clamp(aabb.LowerBound.X, sectorPosition.UpLeft.X, base.Position.X + num + num3);
				}
				else
				{
					aabb.UpperBound.X = MathHelper.Clamp(aabb.UpperBound.X, base.Position.X + num3, sectorPosition.downRight.X);
				}
			}
			if (RayCastCallBacks.QueryObstacleAABB(ref aabb, out var vertice))
			{
				vertice.Y += 0.2f;
				JumpTo(ref vertice);
			}
		}

		public bool WalkTo(float worldXpoint, bool hasToStop)
		{
			if (Landed() && bodyRect.LinearVelocity.X != 0f && Math.Abs(bodyRect.Rotation) <= 0.1f)
			{
				CheckObstacles();
			}
			float num = worldXpoint - base.Position.X;
			if (TouchingFloor())
			{
				if (num == 0f)
				{
					state = StateEnum.Standed;
					ActionStopWalking();
					if ((double)Math.Abs(bodyRect.LinearVelocity.X) < 0.05)
					{
						return true;
					}
				}
				else
				{
					state = StateEnum.Walking;
					float maxSpeed = (hasToStop ? MathHelper.Clamp(Math.Abs(num * 2f), 1f, runSpeed) : runSpeed);
					if (num < 0f)
					{
						ActionWalk(Side.Left, maxSpeed);
					}
					if (num > 0f)
					{
						ActionWalk(Side.Right, maxSpeed);
					}
				}
			}
			else
			{
				float maxSpeed2 = (hasToStop ? MathHelper.Clamp(Math.Abs(num), 1f, 5f) : 5f);
				if (num < 0f)
				{
					ActionWalk(Side.Left, maxSpeed2);
				}
				if (num > 0f)
				{
					ActionWalk(Side.Right, maxSpeed2);
				}
			}
			return false;
		}

		public bool IsAimingDown()
		{
			return (double)Math.Abs(MathHelper.WrapAngle(ArmRotation + bodyRect.Rotation - (float)Math.PI / 2f)) < 0.1;
		}
	}
}
