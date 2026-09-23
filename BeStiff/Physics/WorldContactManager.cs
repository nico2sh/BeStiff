using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Physics
{
	public class WorldContactManager
	{
		public bool Enabled { get; set; }

		public WorldContactManager()
		{
			ContactManager contactManager = GameElementsControl.World.ContactManager;
			contactManager.PostSolve = (PostSolveDelegate)Delegate.Combine(contactManager.PostSolve, new PostSolveDelegate(PostSolve));
			ContactManager contactManager2 = GameElementsControl.World.ContactManager;
			contactManager2.PreSolve = (PreSolveDelegate)Delegate.Combine(contactManager2.PreSolve, new PreSolveDelegate(PreSolve));
			ContactManager contactManager3 = GameElementsControl.World.ContactManager;
			contactManager3.BeginContact = (BeginContactDelegate)Delegate.Combine(contactManager3.BeginContact, new BeginContactDelegate(BeginContact));
			ContactManager contactManager4 = GameElementsControl.World.ContactManager;
			contactManager4.EndContact = (EndContactDelegate)Delegate.Combine(contactManager4.EndContact, new EndContactDelegate(EndContact));
		}

		public void Dispose()
		{
			ContactManager contactManager = GameElementsControl.World.ContactManager;
			contactManager.PostSolve = (PostSolveDelegate)Delegate.Remove(contactManager.PostSolve, new PostSolveDelegate(PostSolve));
			ContactManager contactManager2 = GameElementsControl.World.ContactManager;
			contactManager2.PreSolve = (PreSolveDelegate)Delegate.Remove(contactManager2.PreSolve, new PreSolveDelegate(PreSolve));
			ContactManager contactManager3 = GameElementsControl.World.ContactManager;
			contactManager3.BeginContact = (BeginContactDelegate)Delegate.Remove(contactManager3.BeginContact, new BeginContactDelegate(BeginContact));
			ContactManager contactManager4 = GameElementsControl.World.ContactManager;
			contactManager4.EndContact = (EndContactDelegate)Delegate.Remove(contactManager4.EndContact, new EndContactDelegate(EndContact));
		}

		public void Reset()
		{
		}

		private bool BeginContact(Contact contact)
		{
			if (!Enabled)
			{
				return true;
			}
			WorldObjectData worldObjectData = contact.FixtureA.UserData as WorldObjectData;
			WorldObjectData worldObjectData2 = contact.FixtureB.UserData as WorldObjectData;
			if (worldObjectData != null && worldObjectData2 != null)
			{
				if (worldObjectData.Type == WorldObjectType.InteractiveSensor)
				{
					InteractiveWorldObject interactiveWorldObject = worldObjectData.Object as InteractiveWorldObject;
					if (interactiveWorldObject.Interacts(worldObjectData2.Type))
					{
						interactiveWorldObject.StartInteracting(worldObjectData2.Object);
					}
				}
				if (worldObjectData2.Type == WorldObjectType.InteractiveSensor)
				{
					InteractiveWorldObject interactiveWorldObject2 = worldObjectData2.Object as InteractiveWorldObject;
					if (interactiveWorldObject2.Interacts(worldObjectData.Type))
					{
						interactiveWorldObject2.StartInteracting(worldObjectData.Object);
					}
				}
				if (worldObjectData.Type == WorldObjectType.Deadly && worldObjectData2.Object is Human)
				{
					_ = contact.Manifold.Points;
					contact.GetWorldManifold(out var _, out var _);
					((Human)worldObjectData2.Object).BloodyDie();
				}
				if (worldObjectData2.Type == WorldObjectType.Deadly && worldObjectData.Object is Human)
				{
					contact.GetWorldManifold(out var _, out var _);
					((Human)worldObjectData.Object).BloodyDie();
				}
			}
			return true;
		}

		private void EndContact(Contact contact)
		{
			WorldObjectData worldObjectData = contact.FixtureA.UserData as WorldObjectData;
			WorldObjectData worldObjectData2 = contact.FixtureB.UserData as WorldObjectData;
			if (worldObjectData == null || worldObjectData2 == null)
			{
				return;
			}
			if (worldObjectData.Type == WorldObjectType.InteractiveSensor)
			{
				InteractiveWorldObject interactiveWorldObject = worldObjectData.Object as InteractiveWorldObject;
				if (interactiveWorldObject.Interacts(worldObjectData2.Type))
				{
					interactiveWorldObject.StopInteracting(worldObjectData2.Object);
				}
			}
			if (worldObjectData2.Type == WorldObjectType.InteractiveSensor)
			{
				InteractiveWorldObject interactiveWorldObject2 = worldObjectData2.Object as InteractiveWorldObject;
				if (interactiveWorldObject2.Interacts(worldObjectData.Type))
				{
					interactiveWorldObject2.StopInteracting(worldObjectData.Object);
				}
			}
		}

		private void PreSolve(Contact contact, ref Manifold oldManifold)
		{
			if (!Enabled)
			{
				return;
			}
			WorldObjectData worldObjectData = contact.FixtureA.UserData as WorldObjectData;
			WorldObjectData worldObjectData2 = contact.FixtureB.UserData as WorldObjectData;
			if (worldObjectData == null || worldObjectData2 == null)
			{
				return;
			}
			if (worldObjectData.Object is IOneSidedWorldObject oneSidedWorldObject)
			{
				if (oneSidedWorldObject.ActiveOneSide)
				{
					if (worldObjectData2.Object is IOneSidedAffected oneSidedAffected)
					{
						if (oneSidedAffected.OneSidedIgnored == worldObjectData.Object)
						{
							contact.Enabled = false;
						}
						else if (!oneSidedAffected.IgnoreOneSide())
						{
							if (!CheckIfAbove(oneSidedAffected.FloorPosition + new Vector2(0f, 0.2f), oneSidedWorldObject.TheVertices, worldObjectData.Object.MainBody))
							{
								contact.Enabled = false;
							}
						}
						else
						{
							oneSidedAffected.OneSidedIgnore(worldObjectData.Object);
							contact.Enabled = false;
						}
					}
				}
				else if (worldObjectData2.Type != WorldObjectType.CollisionWorldObject)
				{
					contact.Enabled = oneSidedWorldObject.ConditionalContacts(worldObjectData2.Object);
				}
			}
			else if (worldObjectData2.Object is IOneSidedWorldObject oneSidedWorldObject2)
			{
				if (oneSidedWorldObject2.ActiveOneSide)
				{
					if (worldObjectData.Object is IOneSidedAffected oneSidedAffected2)
					{
						if (oneSidedAffected2.OneSidedIgnored == worldObjectData2.Object)
						{
							contact.Enabled = false;
						}
						else if (!oneSidedAffected2.IgnoreOneSide())
						{
							if (!CheckIfAbove(oneSidedAffected2.FloorPosition + new Vector2(0f, 0.2f), oneSidedWorldObject2.TheVertices, worldObjectData2.Object.MainBody))
							{
								contact.Enabled = false;
							}
						}
						else
						{
							oneSidedAffected2.OneSidedIgnore(worldObjectData2.Object);
							contact.Enabled = false;
						}
					}
				}
				else if (worldObjectData.Type != WorldObjectType.CollisionWorldObject)
				{
					contact.Enabled = oneSidedWorldObject2.ConditionalContacts(worldObjectData.Object);
				}
			}
			if (contact.Enabled)
			{
				worldObjectData.Object.CalculateImpactDamage(ref contact, ref oldManifold, worldObjectData2);
				worldObjectData2.Object.CalculateImpactDamage(ref contact, ref oldManifold, worldObjectData);
			}
		}

		private void PostSolve(Contact contact, ContactConstraint constraint)
		{
			if (!Enabled)
			{
				return;
			}
			if (contact.FixtureA == null || contact.FixtureB == null)
			{
				contact.Enabled = false;
			}
			else if (contact.FixtureA.UserData is WorldObjectData && contact.FixtureB.UserData is WorldObjectData)
			{
				float val = 0f;
				contact.GetManifold(out var manifold);
				for (int i = 0; i < manifold.PointCount; i++)
				{
					val = Math.Max(val, manifold.Points[i].NormalImpulse);
				}
			}
		}

		public void ProcessContacts()
		{
		}

		private void HandleCollision(WorldObjectData worldObjectDataA, WorldObjectData worldObjectDataB, Contact contactItem)
		{
			contactItem.GetWorldManifold(out var normal, out var _);
			if (worldObjectDataA.Type == WorldObjectType.HumanFeet)
			{
				Human human = worldObjectDataA.Object as Human;
				human.ComputeFeet(normal);
			}
			if (worldObjectDataB.Type == WorldObjectType.HumanFeet)
			{
				Human human2 = worldObjectDataB.Object as Human;
				human2.ComputeFeet(normal);
			}
		}

		private bool CheckIfAbove(Vector2 pos, Vertices vertices, Body body)
		{
			bool flag = true;
			bool flag2 = false;
			for (int i = 0; i < vertices.Count; i++)
			{
				int num = i + 1;
				if (num == vertices.Count)
				{
					num = 0;
				}
				Vector2 worldPoint = body.GetWorldPoint(vertices[i]);
				Vector2 worldPoint2 = body.GetWorldPoint(vertices[num]);
				if ((pos.X < worldPoint.X && pos.X > worldPoint2.X) || (pos.X > worldPoint.X && pos.X < worldPoint2.X))
				{
					flag2 = true;
					Vector2 vector = worldPoint2 - worldPoint;
					float num2 = vector.Y / vector.X;
					float num3 = worldPoint.Y - num2 * worldPoint.X;
					float num4 = pos.X * num2 + num3;
					flag = pos.Y <= num4 && flag;
				}
			}
			return flag2 && flag;
		}
	}
}
