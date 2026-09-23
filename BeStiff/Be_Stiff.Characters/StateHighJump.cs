using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters
{
	public class StateHighJump : HeroState
	{
		protected bool hasStrongLanding;

		private Side sideControlMove;

		public StateHighJump(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 5;
		}

		protected override void CheckRules()
		{
			if (hero.TouchingFloor())
			{
				if (hero.MainBody.LinearVelocity.Y > 5f)
				{
					if (hero.MainBody.LinearVelocity.Y > 10f)
					{
						hero.Skeleton.SetAnimation("LANDSTRONG");
					}
					else
					{
						hero.Skeleton.SetAnimation("LAND");
					}
					hasStrongLanding = true;
				}
				SwitchState(0, useExitTransition: true);
				return;
			}
			if (hero.HookAttached)
			{
				SwitchState(21, useExitTransition: false);
				return;
			}
			if (hero.MustDefend)
			{
				SwitchState(22, useExitTransition: false);
				return;
			}
			if (hero.Input.ControlSlideDown(hero.PlayerIndex))
			{
				SwitchState(9, useExitTransition: true);
				return;
			}
			bool flag = false;
			bool checkForSlide = hero.MainBody.LinearVelocity.Y > -1f;
			if (sideControlMove != Side.None)
			{
				flag = CheckWalls(sideControlMove, checkforHang: true, checkForSlide);
			}
			if (!flag)
			{
				Side side = (Side)Math.Sign(hero.MainBody.LinearVelocity.X);
				if (sideControlMove != side)
				{
					flag = CheckWalls(side, checkforHang: false, checkForSlide);
				}
			}
		}

		internal override void OnEnter()
		{
			sideControlMove = Side.None;
			hero.Skeleton.SetAnimation("JUMPCONTINUE");
		}

		protected override bool Entering()
		{
			hasStrongLanding = false;
			hero.ActionStopWalking();
			return true;
		}

		protected override bool Exiting()
		{
			if (hasStrongLanding)
			{
				hero.ActionStopWalking();
				return hero.Skeleton.AnimationEnded();
			}
			return true;
		}

		internal override void OnExit()
		{
		}

		protected override void During()
		{
			if (hero.Input.ControlLeft(hero.PlayerIndex))
			{
				AirMove(Side.Left);
				sideControlMove = Side.Left;
			}
			else if (hero.Input.ControlRight(hero.PlayerIndex))
			{
				AirMove(Side.Right);
				sideControlMove = Side.Right;
			}
			else
			{
				sideControlMove = Side.None;
				hero.WheelRevoluteJoint.MotorEnabled = false;
			}
		}

		private void AirMove(Side side)
		{
			float value = hero.RunSpeed * (float)side - hero.MainBody.LinearVelocity.X;
			float num = hero.RunSpeed / 2000f * (float)GameElementsControl.LastFrameTimeInMS;
			value = ((side != Side.Left) ? MathHelper.Clamp(value, 0f, num) : MathHelper.Clamp(value, 0f - num, 0f));
			Vector2 impulse = new Vector2(value * hero.Mass, 0f);
			hero.MainBody.ApplyLinearImpulse(ref impulse);
			float num2 = MathHelper.Clamp(Math.Abs(hero.MainBody.LinearVelocity.X / hero.WheelRadius), 5f, 100f);
			hero.WheelRevoluteJoint.MotorSpeed = (float)Math.Sign(hero.MainBody.LinearVelocity.X) * num2;
		}

		private bool CheckWalls(Side side, bool checkforHang, bool checkForSlide)
		{
			if (!checkforHang && !checkForSlide)
			{
				return false;
			}
			float num = 0.2f;
			float num2 = (hero.Height - hero.Width / 2f) / 2f;
			float num3 = ((side == Side.Left) ? (0f - (hero.Width / 2f + num)) : (hero.Width * 0.5f));
			AABB aabb = default(AABB);
			aabb.LowerBound = hero.Position + new Vector2(num3, 0f - num2);
			aabb.UpperBound = hero.Position + new Vector2(num + num3, num2);
			float posX = 0f;
			Fixture wsFixture = null;
			FixtureList fixtureList = RayCastCallBacks.QueryWallSlideAABB(ref aabb);
			Vector2 bodyHangPos = new Vector2(0f, float.MinValue);
			Body retBody = null;
			bool flag = false;
			for (int i = 0; i < fixtureList.CurrentElements; i++)
			{
				WorldObjectData worldObjectData = fixtureList.Elements[i].UserData as WorldObjectData;
				if (checkForSlide && worldObjectData.Object.CanWallSlide(side, ref posX, ref wsFixture))
				{
					_ = GameElementsControl.Hero.Position.X;
					_ = GameElementsControl.Hero.Width / 2f;
					if (CheckWallSlide(side, wsFixture))
					{
						StateWallSliding stateWallSliding = stateMachine.States[13] as StateWallSliding;
						stateWallSliding.SetData(wsFixture, posX, side);
						SwitchState(13, useExitTransition: false);
						return true;
					}
				}
				if (!checkforHang || !worldObjectData.Object.CanHanged())
				{
					continue;
				}
				if (fixtureList.Elements[i].ShapeType == ShapeType.Polygon)
				{
					PolygonShape polygonShape = fixtureList.Elements[i].Shape as PolygonShape;
					for (int j = 0; j < polygonShape.Vertices.Count; j++)
					{
						Vector2 point = fixtureList.Elements[i].Body.GetWorldPoint(polygonShape.Vertices[j]);
						if (aabb.Contains(ref point) && point.Y > bodyHangPos.Y && ((!checkForSlide && StateHanging.WillClimb(point, hero.Position, hero.Height)) || checkForSlide) && CanHang(side, point))
						{
							flag = true;
							bodyHangPos = point;
							retBody = fixtureList.Elements[i].Body;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				StateHanging stateHanging = stateMachine.States[14] as StateHanging;
				stateHanging.SetData(side, retBody, bodyHangPos);
				SwitchState(14, useExitTransition: true);
				return true;
			}
			return false;
		}

		private bool CheckWallSlide(Side side, Fixture wallSlideFixture)
		{
			float x = wallSlideFixture.Body.LinearVelocity.X;
			if (x != 0f)
			{
				if (side == Side.Left && x < 0f)
				{
					return false;
				}
				if (side == Side.Right && x > 0f)
				{
					return false;
				}
			}
			Vector2 zero = Vector2.Zero;
			zero.X = hero.MainBody.Position.X;
			zero.Y = hero.MainBody.Position.Y - hero.Height / 4f;
			Vector2 zero2 = Vector2.Zero;
			float num = hero.Width / 2f + 0.15f;
			zero2.X = zero.X + (float)side * num;
			zero2.Y = zero.Y;
			bool flag = false;
			RayCastInput input = default(RayCastInput);
			input.Point1 = zero;
			input.Point2 = zero2;
			input.MaxFraction = 50f;
			flag = wallSlideFixture.RayCast(out var output, ref input, 0);
			bool flag2 = false;
			zero.Y = hero.MainBody.Position.Y + (hero.Height - hero.Width) / 2f;
			zero2.Y = zero.Y;
			input.Point1 = zero;
			input.Point2 = zero2;
			flag2 = wallSlideFixture.RayCast(out output, ref input, 0);
			if (flag)
			{
				return flag2;
			}
			return false;
		}

		private bool CanHang(Side side, Vector2 pos)
		{
			if (side == Side.None)
			{
				return false;
			}
			Vector2 po = Vector2.Zero;
			Vector2 norm = Vector2.Zero;
			Vector2 zero = Vector2.Zero;
			zero.X = hero.MainBody.Position.X + (float)side * hero.Width / 2f;
			zero.Y = pos.Y - 0.1f;
			Vector2 zero2 = Vector2.Zero;
			zero2.X = zero.X + (float)side * 0.5f;
			zero2.Y = zero.Y;
			float frac;
			bool flag = RayCastCallBacks.RayCastOneNoHuman(zero, zero2, out po, out norm, out frac) != null;
			zero = zero2;
			zero2.Y += 0.2f;
			bool flag2 = RayCastCallBacks.RayCastOneNoHuman(zero, zero2, out po, out norm, out frac) != null;
			return !flag && flag2;
		}

		private bool GetWorldHangPosition(Fixture fixture, Vector2 position, out Vector2 hangPosition)
		{
			if (fixture.ShapeType == ShapeType.Polygon)
			{
				PolygonShape polygonShape = fixture.Shape as PolygonShape;
				foreach (Vector2 vertex in polygonShape.Vertices)
				{
					hangPosition = fixture.Body.GetWorldPoint(vertex);
					(hangPosition - position).Length();
					if ((hangPosition - position).Length() <= 0.4f)
					{
						return true;
					}
				}
				if (fixture.UserData is WorldObjectData worldObjectData && worldObjectData.Object is IOneSidedWorldObject)
				{
					return GethighestOneSidePoint(polygonShape.Vertices, fixture.Body, position, out hangPosition);
				}
			}
			hangPosition = Vector2.Zero;
			return false;
		}

		private bool GethighestOneSidePoint(Vertices vertices, Body body, Vector2 pos, out Vector2 highestPos)
		{
			bool result = false;
			float num = float.MinValue;
			for (int i = 0; i < vertices.Count; i++)
			{
				int num2 = i + 1;
				if (num2 == vertices.Count)
				{
					num2 = 0;
				}
				Vector2 worldPoint = body.GetWorldPoint(vertices[i]);
				Vector2 worldPoint2 = body.GetWorldPoint(vertices[num2]);
				if ((pos.X < worldPoint.X && pos.X > worldPoint2.X) || (pos.X > worldPoint.X && pos.X < worldPoint2.X))
				{
					result = true;
					Vector2 vector = worldPoint2 - worldPoint;
					float num3 = vector.Y / vector.X;
					float num4 = worldPoint.Y - num3 * worldPoint.X;
					float num5 = pos.X * num3 + num4;
					if (num <= num5)
					{
						num = num5;
					}
				}
			}
			highestPos = new Vector2(pos.X, num);
			return result;
		}
	}
}
