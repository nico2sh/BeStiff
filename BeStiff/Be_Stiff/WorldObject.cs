using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff
{
	public abstract class WorldObject
	{
		/// <summary>Debug helper: true if this object's main body is the given body.</summary>
		public bool DebugOwnsBody(FarseerPhysics.Dynamics.Body body)
		{
			return mainBody == body;
		}

		private string name;

		private bool _enabled = true;

		protected Body mainBody;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public virtual Body MainBody => mainBody;

		public virtual float Mass => mainBody.Mass;

		public virtual bool LoadFromOgmo(OgmoObject obj)
		{
			return false;
		}

		public virtual void Draw()
		{
		}

		public virtual void Update()
		{
		}

		public virtual void Load()
		{
			throw new NotImplementedException("This needs to be implemented");
		}

		public virtual void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
		}

		public virtual bool CollidesWithObject(Fixture f)
		{
			return Globals.Collides(f);
		}

		public virtual bool CanBeHooked()
		{
			return true;
		}

		public virtual bool CanBeSensed()
		{
			return true;
		}

		public virtual bool CanStopBullets()
		{
			return true;
		}

		public virtual bool CanSeeThrough(bool alerted)
		{
			return false;
		}

		public virtual bool CanWallSlide(Side side, ref float posX, ref Fixture wsFixture)
		{
			return false;
		}

		public virtual bool CanHanged()
		{
			return false;
		}

		protected float CalculateMaxImpulse(Contact contact)
		{
			float num = 0f;
			contact.GetManifold(out var manifold);
			for (int i = 0; i < manifold.PointCount; i++)
			{
				num = Math.Max(num, manifold.Points[i].NormalImpulse);
			}
			return num;
		}

		public virtual void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
		}

		protected float CalculateRelativeSpeedFromContact(ref Contact contact, ref Manifold oldManifold)
		{
			float result = 0f;
			contact.GetWorldManifold(out var normal, out var points);
			Manifold manifold = contact.Manifold;
			Collision.GetPointStates(out var _, out var state2, ref oldManifold, ref manifold);
			if (state2[0] == PointState.Add)
			{
				Vector2 linearVelocityFromWorldPoint = contact.FixtureA.Body.GetLinearVelocityFromWorldPoint(points[0]);
				Vector2 linearVelocityFromWorldPoint2 = contact.FixtureB.Body.GetLinearVelocityFromWorldPoint(points[0]);
				Vector2 value = linearVelocityFromWorldPoint - linearVelocityFromWorldPoint2;
				result = Vector2.Dot(value, normal);
			}
			return result;
		}

		protected float CalculateRelativeSpeedFromContact(ref Contact contact, ref Manifold oldManifold, out Vector2 impactPoint, out Vector2 impactNormal)
		{
			impactPoint = Vector2.Zero;
			impactNormal = Vector2.Zero;
			float result = 0f;
			contact.GetWorldManifold(out var normal, out var points);
			Manifold manifold = contact.Manifold;
			Collision.GetPointStates(out var _, out var state2, ref oldManifold, ref manifold);
			if (state2[0] == PointState.Add)
			{
				Vector2 linearVelocityFromWorldPoint = contact.FixtureA.Body.GetLinearVelocityFromWorldPoint(points[0]);
				Vector2 linearVelocityFromWorldPoint2 = contact.FixtureB.Body.GetLinearVelocityFromWorldPoint(points[0]);
				Vector2 value = linearVelocityFromWorldPoint - linearVelocityFromWorldPoint2;
				result = Vector2.Dot(value, normal);
				impactPoint = points[0];
				impactNormal = normal;
			}
			return result;
		}

		public virtual void StepOver(Vector2 position, WorldObjectType type, bool running)
		{
			switch (type)
			{
			case WorldObjectType.Human:
				GameElementsControl.NoiseManager.AddVisualNoise("stepWalking", position);
				break;
			case WorldObjectType.Hero:
				if (running)
				{
					GameElementsControl.NoiseManager.AddVisualNoise("stepWalkingHero", position);
				}
				else
				{
					GameElementsControl.NoiseManager.AddNoise("step", position);
				}
				break;
			}
		}
	}
}
