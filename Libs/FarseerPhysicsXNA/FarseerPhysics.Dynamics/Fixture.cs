using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// A fixture is used to attach a Shape to a body for collision detection. A fixture
	/// inherits its transform from its parent. Fixtures hold additional non-geometric data
	/// such as friction, collision filters, etc.
	/// Fixtures are created via Body.CreateFixture.
	/// Warning: You cannot reuse fixtures.
	/// </summary>
	public class Fixture : IDisposable
	{
		private static int _fixtureIdCounter;

		/// <summary>
		/// Fires after two shapes has collided and are solved. This gives you a chance to get the impact force.
		/// </summary>
		public AfterCollisionEventHandler AfterCollision;

		/// <summary>
		/// Fires when two fixtures are close to each other.
		/// Due to how the broadphase works, this can be quite inaccurate as shapes are approximated using AABBs.
		/// </summary>
		public BeforeCollisionEventHandler BeforeCollision;

		/// <summary>
		/// Fires when two shapes collide and a contact is created between them.
		/// Note that the first fixture argument is always the fixture that the delegate is subscribed to.
		/// </summary>
		public OnCollisionEventHandler OnCollision;

		/// <summary>
		/// Fires when two shapes separate and a contact is removed between them.
		/// Note that the first fixture argument is always the fixture that the delegate is subscribed to.
		/// </summary>
		public OnSeparationEventHandler OnSeparation;

		public FixtureProxy[] Proxies;

		public int ProxyCount;

		internal Category _collidesWith;

		internal Category _collisionCategories;

		internal short _collisionGroup;

		internal Dictionary<int, bool> _collisionIgnores;

		private float _friction;

		private float _restitution;

		/// <summary>
		/// Defaults to 0
		///
		/// If Settings.UseFPECollisionCategories is set to false:
		/// Collision groups allow a certain group of objects to never collide (negative)
		/// or always collide (positive). Zero means no collision group. Non-zero group
		/// filtering always wins against the mask bits.
		///
		/// If Settings.UseFPECollisionCategories is set to true:
		/// If 2 fixtures are in the same collision group, they will not collide.
		/// </summary>
		public short CollisionGroup
		{
			get
			{
				return _collisionGroup;
			}
			set
			{
				if (_collisionGroup != value)
				{
					_collisionGroup = value;
					Refilter();
				}
			}
		}

		/// <summary>
		/// Defaults to Category.All
		///
		/// The collision mask bits. This states the categories that this
		/// fixture would accept for collision.
		/// Use Settings.UseFPECollisionCategories to change the behavior.
		/// </summary>
		public Category CollidesWith
		{
			get
			{
				return _collidesWith;
			}
			set
			{
				if (_collidesWith != value)
				{
					_collidesWith = value;
					Refilter();
				}
			}
		}

		/// <summary>
		/// The collision categories this fixture is a part of.
		///
		/// If Settings.UseFPECollisionCategories is set to false:
		/// Defaults to Category.Cat1
		///
		/// If Settings.UseFPECollisionCategories is set to true:
		/// Defaults to Category.All
		/// </summary>
		public Category CollisionCategories
		{
			get
			{
				return _collisionCategories;
			}
			set
			{
				if (_collisionCategories != value)
				{
					_collisionCategories = value;
					Refilter();
				}
			}
		}

		/// <summary>
		/// Get the type of the child Shape. You can use this to down cast to the concrete Shape.
		/// </summary>
		/// <value>The type of the shape.</value>
		public ShapeType ShapeType => Shape.ShapeType;

		/// <summary>
		/// Get the child Shape. You can modify the child Shape, however you should not change the
		/// number of vertices because this will crash some collision caching mechanisms.
		/// </summary>
		/// <value>The shape.</value>
		public Shape Shape { get; internal set; }

		/// <summary>
		/// Gets or sets a value indicating whether this fixture is a sensor.
		/// </summary>
		/// <value><c>true</c> if this instance is a sensor; otherwise, <c>false</c>.</value>
		public bool IsSensor { get; set; }

		/// <summary>
		/// Get the parent body of this fixture. This is null if the fixture is not attached.
		/// </summary>
		/// <value>The body.</value>
		public Body Body { get; internal set; }

		/// <summary>
		/// Set the user data. Use this to store your application specific data.
		/// </summary>
		/// <value>The user data.</value>
		public object UserData { get; set; }

		/// <summary>
		/// Get or set the coefficient of friction.
		/// </summary>
		/// <value>The friction.</value>
		public float Friction
		{
			get
			{
				return _friction;
			}
			set
			{
				_friction = value;
			}
		}

		/// <summary>
		/// Get or set the coefficient of restitution.
		/// </summary>
		/// <value>The restitution.</value>
		public float Restitution
		{
			get
			{
				return _restitution;
			}
			set
			{
				_restitution = value;
			}
		}

		/// <summary>
		/// Gets a unique ID for this fixture.
		/// </summary>
		/// <value>The fixture id.</value>
		public int FixtureId { get; private set; }

		public bool IsDisposed { get; set; }

		internal Fixture()
		{
		}

		public Fixture(Body body, Shape shape)
			: this(body, shape, null)
		{
		}

		public Fixture(Body body, Shape shape, object userData)
		{
			if (Settings.UseFPECollisionCategories)
			{
				_collisionCategories = Category.All;
			}
			else
			{
				_collisionCategories = Category.Cat1;
			}
			_collidesWith = Category.All;
			_collisionGroup = 0;
			Friction = 0.2f;
			Restitution = 0f;
			IsSensor = false;
			Body = body;
			UserData = userData;
			Shape = shape.Clone();
			RegisterFixture();
		}

		public void Dispose()
		{
			if (!IsDisposed)
			{
				Body.DestroyFixture(this);
				IsDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// Restores collisions between this fixture and the provided fixture.
		/// </summary>
		/// <param name="fixture">The fixture.</param>
		public void RestoreCollisionWith(Fixture fixture)
		{
			if (_collisionIgnores != null && _collisionIgnores.ContainsKey(fixture.FixtureId))
			{
				_collisionIgnores[fixture.FixtureId] = false;
				Refilter();
			}
		}

		/// <summary>
		/// Ignores collisions between this fixture and the provided fixture.
		/// </summary>
		/// <param name="fixture">The fixture.</param>
		public void IgnoreCollisionWith(Fixture fixture)
		{
			if (_collisionIgnores == null)
			{
				_collisionIgnores = new Dictionary<int, bool>();
			}
			if (_collisionIgnores.ContainsKey(fixture.FixtureId))
			{
				_collisionIgnores[fixture.FixtureId] = true;
			}
			else
			{
				_collisionIgnores.Add(fixture.FixtureId, value: true);
			}
			Refilter();
		}

		/// <summary>
		/// Determines whether collisions are ignored between this fixture and the provided fixture.
		/// </summary>
		/// <param name="fixture">The fixture.</param>
		/// <returns>
		/// 	<c>true</c> if the fixture is ignored; otherwise, <c>false</c>.
		/// </returns>
		public bool IsFixtureIgnored(Fixture fixture)
		{
			if (_collisionIgnores == null)
			{
				return false;
			}
			if (_collisionIgnores.ContainsKey(fixture.FixtureId))
			{
				return _collisionIgnores[fixture.FixtureId];
			}
			return false;
		}

		/// <summary>
		/// Contacts are persistant and will keep being persistant unless they are
		/// flagged for filtering.
		/// This methods flags all contacts associated with the body for filtering.
		/// </summary>
		internal void Refilter()
		{
			for (ContactEdge contactEdge = Body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
			{
				Contact contact = contactEdge.Contact;
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				if (fixtureA == this || fixtureB == this)
				{
					contact.FlagForFiltering();
				}
			}
			World world = Body.World;
			if (world != null)
			{
				IBroadPhase broadPhase = world.ContactManager.BroadPhase;
				for (int i = 0; i < ProxyCount; i++)
				{
					broadPhase.TouchProxy(Proxies[i].ProxyId);
				}
			}
		}

		private void RegisterFixture()
		{
			Proxies = new FixtureProxy[Shape.ChildCount];
			ProxyCount = 0;
			FixtureId = _fixtureIdCounter++;
			if ((Body.Flags & BodyFlags.Enabled) == BodyFlags.Enabled)
			{
				IBroadPhase broadPhase = Body.World.ContactManager.BroadPhase;
				CreateProxies(broadPhase, ref Body.Xf);
			}
			Body.FixtureList.Add(this);
			if (Shape._density > 0f)
			{
				Body.ResetMassData();
			}
			Body.World.Flags |= WorldFlags.NewFixture;
			if (Body.World.FixtureAdded != null)
			{
				Body.World.FixtureAdded(this);
			}
		}

		/// <summary>
		/// Test a point for containment in this fixture.
		/// </summary>
		/// <param name="point">A point in world coordinates.</param>
		/// <returns></returns>
		public bool TestPoint(ref Vector2 point)
		{
			return Shape.TestPoint(ref Body.Xf, ref point);
		}

		/// <summary>
		/// Cast a ray against this Shape.
		/// </summary>
		/// <param name="output">The ray-cast results.</param>
		/// <param name="input">The ray-cast input parameters.</param>
		/// <param name="childIndex">Index of the child.</param>
		/// <returns></returns>
		public bool RayCast(out RayCastOutput output, ref RayCastInput input, int childIndex)
		{
			return Shape.RayCast(out output, ref input, ref Body.Xf, childIndex);
		}

		/// <summary>
		/// Get the fixture's AABB. This AABB may be enlarge and/or stale.
		/// If you need a more accurate AABB, compute it using the Shape and
		/// the body transform.
		/// </summary>
		/// <param name="aabb">The aabb.</param>
		/// <param name="childIndex">Index of the child.</param>
		public void GetAABB(out AABB aabb, int childIndex)
		{
			aabb = Proxies[childIndex].AABB;
		}

		public Fixture Clone(Body body)
		{
			Fixture fixture = new Fixture();
			fixture.Body = body;
			fixture.Shape = Shape.Clone();
			fixture.UserData = UserData;
			fixture.Restitution = Restitution;
			fixture.Friction = Friction;
			fixture.IsSensor = IsSensor;
			fixture._collisionGroup = CollisionGroup;
			fixture._collisionCategories = CollisionCategories;
			fixture._collidesWith = CollidesWith;
			if (_collisionIgnores != null)
			{
				fixture._collisionIgnores = new Dictionary<int, bool>();
				foreach (KeyValuePair<int, bool> collisionIgnore in _collisionIgnores)
				{
					fixture._collisionIgnores.Add(collisionIgnore.Key, collisionIgnore.Value);
				}
			}
			fixture.RegisterFixture();
			return fixture;
		}

		public Fixture DeepClone()
		{
			return Clone(Body.Clone());
		}

		internal void Destroy()
		{
			Proxies = null;
			Shape = null;
			BeforeCollision = null;
			OnCollision = null;
			OnSeparation = null;
			AfterCollision = null;
			if (Body.World.FixtureRemoved != null)
			{
				Body.World.FixtureRemoved(this);
			}
			Body.World.FixtureAdded = null;
			Body.World.FixtureRemoved = null;
			OnSeparation = null;
			OnCollision = null;
		}

		internal void CreateProxies(IBroadPhase broadPhase, ref Transform xf)
		{
			ProxyCount = Shape.ChildCount;
			for (int i = 0; i < ProxyCount; i++)
			{
				FixtureProxy proxy = default(FixtureProxy);
				Shape.ComputeAABB(out proxy.AABB, ref xf, i);
				proxy.Fixture = this;
				proxy.ChildIndex = i;
				proxy.ProxyId = broadPhase.AddProxy(ref proxy);
				Proxies[i] = proxy;
			}
		}

		internal void DestroyProxies(IBroadPhase broadPhase)
		{
			for (int i = 0; i < ProxyCount; i++)
			{
				broadPhase.RemoveProxy(Proxies[i].ProxyId);
				Proxies[i].ProxyId = -1;
			}
			ProxyCount = 0;
		}

		internal void Synchronize(IBroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
		{
			if (ProxyCount != 0)
			{
				for (int i = 0; i < ProxyCount; i++)
				{
					FixtureProxy fixtureProxy = Proxies[i];
					Shape.ComputeAABB(out var aabb, ref transform1, fixtureProxy.ChildIndex);
					Shape.ComputeAABB(out var aabb2, ref transform2, fixtureProxy.ChildIndex);
					fixtureProxy.AABB.Combine(ref aabb, ref aabb2);
					Vector2 displacement = transform2.Position - transform1.Position;
					broadPhase.MoveProxy(fixtureProxy.ProxyId, ref fixtureProxy.AABB, displacement);
				}
			}
		}

		internal bool CompareTo(Fixture fixture)
		{
			if (CollidesWith == fixture.CollidesWith && CollisionCategories == fixture.CollisionCategories && CollisionGroup == fixture.CollisionGroup && Friction == fixture.Friction && IsSensor == fixture.IsSensor && Restitution == fixture.Restitution && Shape.CompareTo(fixture.Shape))
			{
				return UserData == fixture.UserData;
			}
			return false;
		}
	}
}
