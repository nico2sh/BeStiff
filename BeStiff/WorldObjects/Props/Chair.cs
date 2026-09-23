using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;
using ProjectMercury.Emitters;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Chair : WorldObject, IOneSidedWorldObject, ISeat
	{
		private const float width = 0.75f;

		private const float height = 1f;

		private const float surfaceThick = 0.15f;

		private const float legThick = 0.1f;

		private Body leg1Body;

		private Body leg2Body;

		private Side sideLooking;

		private WeldJoint wj1;

		private WeldJoint wj2;

		private bool breakIt;

		private bool broken;

		private float strength = 100f;

		private double breakTime;

		private GameSprite spriteChairLeg1;

		private GameSpriteVariables spriteVariableChairLeg1;

		private GameSprite spriteChairLeg2;

		private GameSpriteVariables spriteVariableChairLeg2;

		private GameSprite spriteChairSurface;

		private GameSpriteVariables spriteVariableChairSurface;

		private bool projectile;

		private bool setUpAsProjectile;

		private List<WorldObject> safeWorldObjects;

		private CircleEmitter hitSmoke;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => !projectile;

		public bool IsProjectile => projectile;

		public override float Mass => mainBody.Mass + leg1Body.Mass + leg2Body.Mass;

		public Side Side => sideLooking;

		public Vector2 ReferencePoint => MainBody.Position;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("ChairLeft") && !obj.Name.Equals("ChairRight"))
			{
				return false;
			}
			base.Name = "Chair-" + GameElementsControl.Counter;
			if (obj.Name.Equals("ChairLeft"))
			{
				sideLooking = Side.Left;
			}
			if (obj.Name.Equals("ChairRight"))
			{
				sideLooking = Side.Right;
			}
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			breakIt = false;
			broken = false;
			breakTime = 0.0;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position + new Vector2(0f, 0.15f);
			TheVertices = new Vertices();
			TheVertices.Add(new Vector2(-0.375f, -0.075f));
			TheVertices.Add(new Vector2(0.375f, -0.075f));
			TheVertices.Add(new Vector2(0.375f, 0.075f));
			TheVertices.Add(new Vector2(-0.375f, 0.075f));
			PolygonShape shape = new PolygonShape(TheVertices, 11f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			fixture.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture.AfterCollision, new AfterCollisionEventHandler(AfterTableCollision));
			GameElementsControl.LoadSprite("chairBack", "sprites\\objects\\chairback");
			GameElementsControl.LoadSprite("chairLeg", "sprites\\objects\\chairleg");
			GameElementsControl.LoadSprite("chairSeat", "sprites\\objects\\chairseat");
			spriteChairSurface = GameElementsControl.GetSprite("chairSeat");
			Vertices vertices = new Vertices();
			leg1Body = BodyFactory.CreateBody(GameElementsControl.World);
			leg1Body.BodyType = BodyType.Dynamic;
			if (sideLooking == Side.Right)
			{
				spriteChairLeg1 = GameElementsControl.GetSprite("chairBack");
				spriteChairLeg2 = GameElementsControl.GetSprite("chairLeg");
				leg1Body.Position = worldScaledOgmoObject.Position + new Vector2(-0.325f, 0f);
				vertices.Add(new Vector2(-0.05f, -0.5f));
				vertices.Add(new Vector2(0.05f, -0.5f));
				vertices.Add(new Vector2(0.05f, 0.5f));
				vertices.Add(new Vector2(-0.05f, 0.5f));
				shape = new PolygonShape(vertices, 11f);
			}
			else
			{
				spriteChairLeg1 = GameElementsControl.GetSprite("chairLeg");
				spriteChairLeg2 = GameElementsControl.GetSprite("chairBack");
				leg1Body.Position = worldScaledOgmoObject.Position + new Vector2(-0.275f, 0.325f);
				vertices.Add(new Vector2(-0.05f, -0.175f));
				vertices.Add(new Vector2(0.05f, -0.175f));
				vertices.Add(new Vector2(0.05f, 0.175f));
				vertices.Add(new Vector2(-0.05f, 0.175f));
				shape = new PolygonShape(vertices, 11f);
			}
			Fixture fixture2 = leg1Body.CreateFixture(shape);
			fixture2.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			fixture2.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture2.AfterCollision, new AfterCollisionEventHandler(AfterTableCollision));
			wj1 = new WeldJoint(mainBody, leg1Body, new Vector2(-0.375f, 0.075f), leg1Body.GetLocalPoint(mainBody.GetWorldPoint(new Vector2(-0.375f, 0.075f))));
			GameElementsControl.World.AddJoint(wj1);
			leg2Body = BodyFactory.CreateBody(GameElementsControl.World);
			leg2Body.BodyType = BodyType.Dynamic;
			vertices = new Vertices();
			if (sideLooking == Side.Right)
			{
				leg2Body.Position = worldScaledOgmoObject.Position + new Vector2(0.275f, 0.325f);
				vertices.Add(new Vector2(-0.05f, 0.175f));
				vertices.Add(new Vector2(-0.05f, -0.175f));
				vertices.Add(new Vector2(0.05f, -0.175f));
				vertices.Add(new Vector2(0.05f, 0.175f));
				shape = new PolygonShape(vertices, 11f);
			}
			else
			{
				leg2Body.Position = worldScaledOgmoObject.Position + new Vector2(0.325f, 0f);
				vertices.Add(new Vector2(-0.05f, 0.5f));
				vertices.Add(new Vector2(-0.05f, -0.5f));
				vertices.Add(new Vector2(0.05f, -0.5f));
				vertices.Add(new Vector2(0.05f, 0.5f));
				shape = new PolygonShape(vertices, 11f);
			}
			Fixture fixture3 = leg2Body.CreateFixture(shape);
			fixture3.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			fixture3.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture3.AfterCollision, new AfterCollisionEventHandler(AfterTableCollision));
			wj2 = new WeldJoint(mainBody, leg2Body, new Vector2(0.375f, 0.075f), leg2Body.GetLocalPoint(mainBody.GetWorldPoint(new Vector2(0.375f, 0.075f))));
			GameElementsControl.World.AddJoint(wj2);
			spriteVariableChairLeg1 = GameSprite.GetDefaultVariables();
			spriteVariableChairLeg2 = GameSprite.GetDefaultVariables();
			spriteVariableChairSurface = GameSprite.GetDefaultVariables();
			safeWorldObjects = new List<WorldObject>(8);
			hitSmoke = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
			GameElementsControl.ScreenManager.AudioManager.LoadSound("tableHit", "audio\\noises\\tableHit");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("tableBreak", "audio\\noises\\tableBreak");
			GameElementsControl.AddWorldObject(this);
			return true;
		}

		public override void Update()
		{
			spriteVariableChairLeg1.rotation = leg1Body.Rotation;
			spriteVariableChairLeg2.rotation = leg2Body.Rotation;
			spriteVariableChairSurface.rotation = mainBody.Rotation;
			if (!broken)
			{
				if (strength <= 0f)
				{
					strength = 0f;
					breakIt = true;
				}
				if (breakIt)
				{
					Break();
				}
				if (setUpAsProjectile)
				{
					ContactEdge contactEdge = mainBody.ContactList;
					if (contactEdge != null)
					{
						do
						{
							foreach (Fixture fixture in contactEdge.Other.FixtureList)
							{
								if (fixture.UserData is WorldObjectData worldObjectData && worldObjectData.Type != WorldObjectType.CollisionWorldObject && !safeWorldObjects.Contains(worldObjectData.Object))
								{
									safeWorldObjects.Add(worldObjectData.Object);
								}
							}
							contactEdge = contactEdge.Next;
						}
						while (contactEdge != null);
					}
					contactEdge = leg1Body.ContactList;
					if (contactEdge != null)
					{
						do
						{
							foreach (Fixture fixture2 in contactEdge.Other.FixtureList)
							{
								if (fixture2.UserData is WorldObjectData worldObjectData2 && worldObjectData2.Type != WorldObjectType.CollisionWorldObject && !safeWorldObjects.Contains(worldObjectData2.Object))
								{
									safeWorldObjects.Add(worldObjectData2.Object);
								}
							}
							contactEdge = contactEdge.Next;
						}
						while (contactEdge != null);
					}
					contactEdge = leg2Body.ContactList;
					if (contactEdge != null)
					{
						do
						{
							foreach (Fixture fixture3 in contactEdge.Other.FixtureList)
							{
								if (fixture3.UserData is WorldObjectData worldObjectData3 && worldObjectData3.Type != WorldObjectType.CollisionWorldObject && !safeWorldObjects.Contains(worldObjectData3.Object))
								{
									safeWorldObjects.Add(worldObjectData3.Object);
								}
							}
							contactEdge = contactEdge.Next;
						}
						while (contactEdge != null);
					}
					setUpAsProjectile = false;
					projectile = true;
				}
				else if (!projectile && mainBody.LinearVelocity.Length() > 2f)
				{
					setUpAsProjectile = true;
				}
				if (projectile && mainBody.LinearVelocity.Length() < 2f)
				{
					projectile = false;
					safeWorldObjects.Clear();
				}
			}
			else if (breakTime + 2500.0 <= GameElementsControl.CurrentTimeInMS)
			{
				Deactivate();
			}
		}

		public bool ConditionalContacts(WorldObject wo)
		{
			if (projectile)
			{
				if (safeWorldObjects.Contains(wo))
				{
					return false;
				}
				return true;
			}
			return true;
		}

		private void Deactivate()
		{
			GameElementsControl.World.RemoveBody(mainBody);
			GameElementsControl.World.RemoveBody(leg1Body);
			GameElementsControl.World.RemoveBody(leg2Body);
			base.Enabled = false;
		}

		public override void Hit(Vector2 sidePower, Vector2 position, HitType hitType)
		{
			if (!broken)
			{
				switch (hitType)
				{
				case HitType.Kick:
					strength -= 25f;
					break;
				case HitType.Pistol:
					strength -= 30f;
					break;
				case HitType.Explosion:
					strength -= sidePower.Length();
					break;
				case HitType.Blunt:
					break;
				}
			}
		}

		public override bool CanSeeThrough(bool alert)
		{
			return alert;
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			Vector2 impactPoint;
			Vector2 impactNormal;
			float num = CalculateRelativeSpeedFromContact(ref contact, ref oldManifold, out impactPoint, out impactNormal);
			if (num > 1f)
			{
				GameElementsControl.NoiseManager.AddNoise("tableHit", mainBody.Position);
				if (num > 15f)
				{
					strength -= (num - 15f) * 5f;
				}
				if (projectile)
				{
					wod.Object.Hit(mainBody.LinearVelocity / 2f, mainBody.GetWorldPoint(contact.Manifold.LocalPoint), HitType.Blunt);
					hitSmoke.Trigger(GameElementsControl.ConvertWorldToScreen(impactPoint));
				}
			}
		}

		private void AfterTableCollision(Fixture f1, Fixture f2, Contact contact)
		{
			float num = CalculateMaxImpulse(contact);
			if (num > 5f && num > 700f)
			{
				strength = 0f;
			}
			if (!broken && num > 40f)
			{
				strength -= num / 4f;
			}
		}

		public void Break()
		{
			broken = true;
			breakTime = GameElementsControl.CurrentTimeInMS;
			GameElementsControl.World.RemoveJoint(wj1);
			GameElementsControl.World.RemoveJoint(wj2);
			GameElementsControl.NoiseManager.AddNoise("tableBreak", mainBody.Position);
		}

		public override void Draw()
		{
			if (broken)
			{
				spriteChairLeg1.Draw(GameElementsControl.ConvertWorldToScreen(leg1Body.Position), spriteVariableChairLeg1, breakTime + 2000.0, 500.0);
				spriteChairLeg2.Draw(GameElementsControl.ConvertWorldToScreen(leg2Body.Position), spriteVariableChairLeg2, breakTime + 2000.0, 500.0);
				spriteChairSurface.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), spriteVariableChairSurface, breakTime + 2000.0, 500.0);
			}
			else
			{
				spriteChairLeg1.Draw(GameElementsControl.ConvertWorldToScreen(leg1Body.Position), spriteVariableChairLeg1);
				spriteChairLeg2.Draw(GameElementsControl.ConvertWorldToScreen(leg2Body.Position), spriteVariableChairLeg2);
				spriteChairSurface.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), spriteVariableChairSurface);
			}
		}
	}
}
