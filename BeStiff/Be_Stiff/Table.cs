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

namespace Be_Stiff
{
	internal class Table : WorldObject, IOneSidedWorldObject
	{
		private const float width = 1.5f;

		private const float height = 1f;

		private const float surfaceThick = 0.2f;

		private const float legThick = 0.25f;

		private Body leg1Body;

		private Body leg2Body;

		private WeldJoint wj1;

		private WeldJoint wj2;

		private bool breakIt;

		private bool broken;

		private float strength = 100f;

		private double breakTime;

		private GameSprite spriteTableLeg1;

		private GameSpriteVariables spriteVariableTableLeg1;

		private GameSprite spriteTableLeg2;

		private GameSpriteVariables spriteVariableTableLeg2;

		private GameSprite spriteTableSurface;

		private GameSpriteVariables spriteVariableTableSurface;

		private bool projectile;

		private bool setUpAsProjectile;

		private List<WorldObject> safeWorldObjects;

		private CircleEmitter hitSmoke;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => !projectile;

		public bool IsProjectile => projectile;

		public override float Mass => mainBody.Mass + leg1Body.Mass + leg2Body.Mass;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Table"))
			{
				return false;
			}
			base.Name = "Table-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			breakIt = false;
			broken = false;
			breakTime = 0.0;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position + new Vector2(0f, -0.4f);
			TheVertices = new Vertices();
			TheVertices.Add(new Vector2(-0.75f, -0.1f));
			TheVertices.Add(new Vector2(0.75f, -0.1f));
			TheVertices.Add(new Vector2(0.75f, 0.1f));
			TheVertices.Add(new Vector2(-0.75f, 0.1f));
			PolygonShape shape = new PolygonShape(TheVertices, 10f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			fixture.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture.AfterCollision, new AfterCollisionEventHandler(AfterTableCollision));
			leg1Body = BodyFactory.CreateBody(GameElementsControl.World);
			leg1Body.BodyType = BodyType.Dynamic;
			leg1Body.Position = worldScaledOgmoObject.Position + new Vector2(-0.5f, 0.1f);
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(-0.125f, -0.4f));
			vertices.Add(new Vector2(0.125f, -0.4f));
			vertices.Add(new Vector2(0.125f, 0.4f));
			vertices.Add(new Vector2(-0.125f, 0.4f));
			shape = new PolygonShape(vertices, 10f);
			Fixture fixture2 = leg1Body.CreateFixture(shape);
			fixture2.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			fixture2.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture2.AfterCollision, new AfterCollisionEventHandler(AfterTableCollision));
			wj1 = new WeldJoint(leg1Body, mainBody, new Vector2(0f, -0.4f), mainBody.GetLocalPoint(leg1Body.GetWorldPoint(new Vector2(0f, -0.4f))));
			GameElementsControl.World.AddJoint(wj1);
			leg2Body = BodyFactory.CreateBody(GameElementsControl.World);
			leg2Body.BodyType = BodyType.Dynamic;
			leg2Body.Position = worldScaledOgmoObject.Position + new Vector2(0.5f, 0.1f);
			vertices = new Vertices();
			vertices.Add(new Vector2(-0.125f, -0.4f));
			vertices.Add(new Vector2(0.125f, -0.4f));
			vertices.Add(new Vector2(0.125f, 0.4f));
			vertices.Add(new Vector2(-0.125f, 0.4f));
			shape = new PolygonShape(vertices, 10f);
			Fixture fixture3 = leg2Body.CreateFixture(shape);
			fixture3.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			fixture3.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture3.AfterCollision, new AfterCollisionEventHandler(AfterTableCollision));
			wj2 = new WeldJoint(leg2Body, mainBody, new Vector2(0f, -0.4f), mainBody.GetLocalPoint(leg2Body.GetWorldPoint(new Vector2(0f, -0.4f))));
			GameElementsControl.World.AddJoint(wj2);
			GameElementsControl.LoadSprite("tableLeg", "sprites\\objects\\tableleg");
			GameElementsControl.LoadSprite("tableSurface", "sprites\\objects\\tablesurface");
			spriteTableLeg1 = GameElementsControl.GetSprite("tableLeg");
			spriteTableLeg2 = GameElementsControl.GetSprite("tableLeg");
			spriteTableSurface = GameElementsControl.GetSprite("tableSurface");
			spriteVariableTableLeg1 = GameSprite.GetDefaultVariables();
			spriteVariableTableLeg2 = GameSprite.GetDefaultVariables();
			spriteVariableTableSurface = GameSprite.GetDefaultVariables();
			safeWorldObjects = new List<WorldObject>(8);
			hitSmoke = (CircleEmitter)GameElementsControl.Particlesmanager.getKickSmokeEmitter();
			GameElementsControl.ScreenManager.AudioManager.LoadSound("tableHit", "audio\\noises\\tableHit");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("tableBreak", "audio\\noises\\tableBreak");
			GameElementsControl.AddWorldObject(this);
			return true;
		}

		public override void Update()
		{
			spriteVariableTableLeg1.rotation = leg1Body.Rotation;
			spriteVariableTableLeg2.rotation = leg2Body.Rotation;
			spriteVariableTableSurface.rotation = mainBody.Rotation;
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
					setUpAsProjectile = false;
					projectile = true;
				}
				else if (!projectile && mainBody.LinearVelocity.Length() > 8f)
				{
					setUpAsProjectile = true;
				}
				if (projectile && mainBody.LinearVelocity.Length() < 8f)
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
			if (CalculateRelativeSpeedFromContact(ref contact, ref oldManifold, out var impactPoint, out var _) > 1f)
			{
				GameElementsControl.NoiseManager.AddNoise("tableHit", mainBody.Position);
				if (projectile)
				{
					wod.Object.Hit(mainBody.LinearVelocity, mainBody.GetWorldPoint(contact.Manifold.LocalPoint), HitType.Blunt);
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
				spriteTableLeg1.Draw(GameElementsControl.ConvertWorldToScreen(leg1Body.Position), spriteVariableTableLeg1, breakTime + 2000.0, 500.0);
				spriteTableLeg2.Draw(GameElementsControl.ConvertWorldToScreen(leg2Body.Position), spriteVariableTableLeg2, breakTime + 2000.0, 500.0);
				spriteTableSurface.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), spriteVariableTableSurface, breakTime + 2000.0, 500.0);
			}
			else
			{
				spriteTableLeg1.Draw(GameElementsControl.ConvertWorldToScreen(leg1Body.Position), spriteVariableTableLeg1);
				spriteTableLeg2.Draw(GameElementsControl.ConvertWorldToScreen(leg2Body.Position), spriteVariableTableLeg2);
				spriteTableSurface.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), spriteVariableTableSurface);
			}
		}
	}
}
