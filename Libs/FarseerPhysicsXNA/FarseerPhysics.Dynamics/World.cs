using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// The world class manages all physics entities, dynamic simulation,
	/// and asynchronous queries.
	/// </summary>
	public class World
	{
		/// <summary>
		/// Fires whenever a body has been added
		/// </summary>
		public BodyDelegate BodyAdded;

		/// <summary>
		/// Fires whenever a body has been removed
		/// </summary>
		public BodyDelegate BodyRemoved;

		internal Queue<Contact> ContactPool = new Queue<Contact>(256);

		/// <summary>
		/// Fires whenever a fixture has been added
		/// </summary>
		public FixtureDelegate FixtureAdded;

		/// <summary>
		/// Fires whenever a fixture has been removed
		/// </summary>
		public FixtureDelegate FixtureRemoved;

		internal WorldFlags Flags;

		/// <summary>
		/// Fires whenever a joint has been added
		/// </summary>
		public JointDelegate JointAdded;

		/// <summary>
		/// Fires whenever a joint has been removed
		/// </summary>
		public JointDelegate JointRemoved;

		public ControllerDelegate ControllerAdded;

		public ControllerDelegate ControllerRemoved;

		private float _invDt0;

		public Island Island = new Island();

		private Body[] _stack = new Body[64];

		private bool _stepComplete;

		private HashSet<Body> _bodyAddList = new HashSet<Body>();

		private HashSet<Body> _bodyRemoveList = new HashSet<Body>();

		private HashSet<Joint> _jointAddList = new HashSet<Joint>();

		private HashSet<Joint> _jointRemoveList = new HashSet<Joint>();

		private TOIInput _input = new TOIInput();

		/// <summary>
		/// If false, the whole simulation stops. It still processes added and removed geometries.
		/// </summary>
		public bool Enabled = true;

		private Stopwatch _watch = new Stopwatch();

		/// <summary>
		/// Change the global gravity vector.
		/// </summary>
		/// <value>The gravity.</value>
		public Vector2 Gravity;

		public List<Controller> ControllerList { get; private set; }

		public List<BreakableBody> BreakableBodyList { get; private set; }

		public float UpdateTime { get; private set; }

		public float ContinuousPhysicsTime { get; private set; }

		public float ControllersUpdateTime { get; private set; }

		public float AddRemoveTime { get; private set; }

		public float ContactsUpdateTime { get; private set; }

		public float SolveUpdateTime { get; private set; }

		/// <summary>
		/// Get the number of broad-phase proxies.
		/// </summary>
		/// <value>The proxy count.</value>
		public int ProxyCount => ContactManager.BroadPhase.ProxyCount;

		/// <summary>
		/// Set flag to control automatic clearing of forces after each time step.
		/// </summary>
		/// <value><c>true</c> if it should auto clear forces; otherwise, <c>false</c>.</value>
		public bool AutoClearForces
		{
			get
			{
				return (Flags & WorldFlags.ClearForces) == WorldFlags.ClearForces;
			}
			set
			{
				if (value)
				{
					Flags |= WorldFlags.ClearForces;
				}
				else
				{
					Flags &= ~WorldFlags.ClearForces;
				}
			}
		}

		/// <summary>
		/// Get the contact manager for testing.
		/// </summary>
		/// <value>The contact manager.</value>
		public ContactManager ContactManager { get; private set; }

		/// <summary>
		/// Get the world body list.
		/// </summary>
		/// <value>Thehead of the world body list.</value>
		public List<Body> BodyList { get; private set; }

		/// <summary>
		/// Get the world joint list. 
		/// </summary>
		/// <value>The joint list.</value>
		public List<Joint> JointList { get; private set; }

		/// <summary>
		/// Get the world contact list. With the returned contact, use Contact.GetNext to get
		/// the next contact in the world list. A null contact indicates the end of the list.
		/// </summary>
		/// <value>The head of the world contact list.</value>
		public List<Contact> ContactList => ContactManager.ContactList;

		/// <summary>
		/// Enable/disable single stepped continuous physics. For testing.
		/// </summary>
		public bool EnableSubStepping
		{
			get
			{
				return (Flags & WorldFlags.SubStepping) == WorldFlags.SubStepping;
			}
			set
			{
				if (value)
				{
					Flags |= WorldFlags.SubStepping;
				}
				else
				{
					Flags &= ~WorldFlags.SubStepping;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FarseerPhysics.Dynamics.World" /> class.
		/// </summary>
		private World()
		{
			Flags = WorldFlags.ClearForces;
			ControllerList = new List<Controller>();
			BreakableBodyList = new List<BreakableBody>();
			BodyList = new List<Body>(32);
			JointList = new List<Joint>(32);
		}

		public World(Vector2 gravity, AABB span)
			: this()
		{
			Gravity = gravity;
			ContactManager = new ContactManager(new QuadTreeBroadPhase(span));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FarseerPhysics.Dynamics.World" /> class.
		/// </summary>
		/// <param name="gravity">The gravity.</param>
		public World(Vector2 gravity)
			: this()
		{
			ContactManager = new ContactManager(new DynamicTreeBroadPhase());
			Gravity = gravity;
		}

		/// <summary>
		/// Add a rigid body.
		/// </summary>
		/// <returns></returns>
		internal void AddBody(Body body)
		{
			if (!_bodyAddList.Contains(body))
			{
				_bodyAddList.Add(body);
			}
		}

		/// <summary>
		/// Destroy a rigid body.
		/// Warning: This automatically deletes all associated shapes and joints.
		/// </summary>
		/// <param name="body">The body.</param>
		public void RemoveBody(Body body)
		{
			if (!_bodyRemoveList.Contains(body))
			{
				_bodyRemoveList.Add(body);
			}
		}

		/// <summary>
		/// Create a joint to constrain bodies together. This may cause the connected bodies to cease colliding.
		/// </summary>
		/// <param name="joint">The joint.</param>
		public void AddJoint(Joint joint)
		{
			if (!_jointAddList.Contains(joint))
			{
				_jointAddList.Add(joint);
			}
		}

		private void RemoveJoint(Joint joint, bool doCheck)
		{
			if (!_jointRemoveList.Contains(joint))
			{
				_jointRemoveList.Add(joint);
			}
		}

		/// <summary>
		/// Destroy a joint. This may cause the connected bodies to begin colliding.
		/// </summary>
		/// <param name="joint">The joint.</param>
		public void RemoveJoint(Joint joint)
		{
			RemoveJoint(joint, doCheck: true);
		}

		/// <summary>
		/// All adds and removes are cached by the World duing a World step.
		/// To process the changes before the world updates again, call this method.
		/// </summary>
		public void ProcessChanges()
		{
			ProcessAddedBodies();
			// Removals first: a joint queued for removal and re-added in the same
			// step (e.g. the hero's grab joint moving from the hanging to the
			// climbing state) must end up present, and never linked twice.
			ProcessRemovedJoints();
			ProcessAddedJoints();
			ProcessRemovedBodies();
			ProcessRemovedJoints();
		}

		private void ProcessRemovedJoints()
		{
			if (_jointRemoveList.Count <= 0)
			{
				return;
			}
			foreach (Joint jointRemove in _jointRemoveList)
			{
				bool collideConnected = jointRemove.CollideConnected;
				if (!JointList.Remove(jointRemove))
				{
					// Not in the world (never added, or already removed): its edges
					// are not linked into any body, so unlinking would corrupt lists.
					continue;
				}
				Body bodyA = jointRemove.BodyA;
				Body bodyB = jointRemove.BodyB;
				bodyA.Awake = true;
				if (!jointRemove.IsFixedType())
				{
					bodyB.Awake = true;
				}
				if (jointRemove.EdgeA.Prev != null)
				{
					jointRemove.EdgeA.Prev.Next = jointRemove.EdgeA.Next;
				}
				if (jointRemove.EdgeA.Next != null)
				{
					jointRemove.EdgeA.Next.Prev = jointRemove.EdgeA.Prev;
				}
				if (jointRemove.EdgeA == bodyA.JointList)
				{
					bodyA.JointList = jointRemove.EdgeA.Next;
				}
				jointRemove.EdgeA.Prev = null;
				jointRemove.EdgeA.Next = null;
				if (!jointRemove.IsFixedType())
				{
					if (jointRemove.EdgeB.Prev != null)
					{
						jointRemove.EdgeB.Prev.Next = jointRemove.EdgeB.Next;
					}
					if (jointRemove.EdgeB.Next != null)
					{
						jointRemove.EdgeB.Next.Prev = jointRemove.EdgeB.Prev;
					}
					if (jointRemove.EdgeB == bodyB.JointList)
					{
						bodyB.JointList = jointRemove.EdgeB.Next;
					}
					jointRemove.EdgeB.Prev = null;
					jointRemove.EdgeB.Next = null;
				}
				if (!jointRemove.IsFixedType() && !collideConnected)
				{
					for (ContactEdge contactEdge = bodyB.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
					{
						if (contactEdge.Other == bodyA)
						{
							contactEdge.Contact.FlagForFiltering();
						}
					}
				}
				if (JointRemoved != null)
				{
					JointRemoved(jointRemove);
				}
			}
			_jointRemoveList.Clear();
		}

		private void ProcessAddedJoints()
		{
			if (_jointAddList.Count <= 0)
			{
				return;
			}
			foreach (Joint jointAdd in _jointAddList)
			{
				if (JointList.Contains(jointAdd))
				{
					// Already linked; linking its edges again would corrupt the lists.
					continue;
				}
				JointList.Add(jointAdd);
				jointAdd.EdgeA.Joint = jointAdd;
				jointAdd.EdgeA.Other = jointAdd.BodyB;
				jointAdd.EdgeA.Prev = null;
				jointAdd.EdgeA.Next = jointAdd.BodyA.JointList;
				if (jointAdd.BodyA.JointList != null)
				{
					jointAdd.BodyA.JointList.Prev = jointAdd.EdgeA;
				}
				jointAdd.BodyA.JointList = jointAdd.EdgeA;
				if (!jointAdd.IsFixedType())
				{
					jointAdd.EdgeB.Joint = jointAdd;
					jointAdd.EdgeB.Other = jointAdd.BodyA;
					jointAdd.EdgeB.Prev = null;
					jointAdd.EdgeB.Next = jointAdd.BodyB.JointList;
					if (jointAdd.BodyB.JointList != null)
					{
						jointAdd.BodyB.JointList.Prev = jointAdd.EdgeB;
					}
					jointAdd.BodyB.JointList = jointAdd.EdgeB;
					Body bodyA = jointAdd.BodyA;
					Body bodyB = jointAdd.BodyB;
					if (!jointAdd.CollideConnected)
					{
						for (ContactEdge contactEdge = bodyB.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
						{
							if (contactEdge.Other == bodyA)
							{
								contactEdge.Contact.FlagForFiltering();
							}
						}
					}
				}
				if (JointAdded != null)
				{
					JointAdded(jointAdd);
				}
			}
			_jointAddList.Clear();
		}

		private void ProcessAddedBodies()
		{
			if (_bodyAddList.Count <= 0)
			{
				return;
			}
			foreach (Body bodyAdd in _bodyAddList)
			{
				BodyList.Add(bodyAdd);
				if (BodyAdded != null)
				{
					BodyAdded(bodyAdd);
				}
			}
			_bodyAddList.Clear();
		}

		private void ProcessRemovedBodies()
		{
			if (_bodyRemoveList.Count <= 0)
			{
				return;
			}
			foreach (Body bodyRemove in _bodyRemoveList)
			{
				JointEdge jointEdge = bodyRemove.JointList;
				while (jointEdge != null)
				{
					JointEdge jointEdge2 = jointEdge;
					jointEdge = jointEdge.Next;
					RemoveJoint(jointEdge2.Joint, doCheck: false);
				}
				bodyRemove.JointList = null;
				ContactEdge contactEdge = bodyRemove.ContactList;
				while (contactEdge != null)
				{
					ContactEdge contactEdge2 = contactEdge;
					contactEdge = contactEdge.Next;
					ContactManager.Destroy(contactEdge2.Contact);
				}
				bodyRemove.ContactList = null;
				for (int i = 0; i < bodyRemove.FixtureList.Count; i++)
				{
					bodyRemove.FixtureList[i].DestroyProxies(ContactManager.BroadPhase);
					bodyRemove.FixtureList[i].Destroy();
				}
				bodyRemove.FixtureList = null;
				BodyList.Remove(bodyRemove);
				if (BodyRemoved != null)
				{
					BodyRemoved(bodyRemove);
				}
			}
			_bodyRemoveList.Clear();
		}

		/// <summary>
		/// Take a time step. This performs collision detection, integration,
		/// and consraint solution.
		/// </summary>
		/// <param name="dt">The amount of time to simulate, this should not vary.</param>
		public void Step(float dt)
		{
			if (Settings.EnableDiagnostics)
			{
				_watch.Start();
			}
			ProcessChanges();
			if (Settings.EnableDiagnostics)
			{
				AddRemoveTime = _watch.ElapsedTicks;
			}
			if (dt == 0f || !Enabled)
			{
				if (Settings.EnableDiagnostics)
				{
					_watch.Stop();
					_watch.Reset();
				}
				return;
			}
			if ((Flags & WorldFlags.NewFixture) == WorldFlags.NewFixture)
			{
				ContactManager.FindNewContacts();
				Flags &= ~WorldFlags.NewFixture;
			}
			TimeStep step = default(TimeStep);
			step.inv_dt = 1f / dt;
			step.dt = dt;
			step.dtRatio = _invDt0 * dt;
			for (int i = 0; i < ControllerList.Count; i++)
			{
				ControllerList[i].Update(dt);
			}
			if (Settings.EnableDiagnostics)
			{
				ControllersUpdateTime = (float)_watch.ElapsedTicks - AddRemoveTime;
			}
			ContactManager.Collide();
			if (Settings.EnableDiagnostics)
			{
				ContactsUpdateTime = (float)_watch.ElapsedTicks - (AddRemoveTime + ControllersUpdateTime);
			}
			Solve(ref step);
			RepairNonFiniteBodies();
			if (Settings.EnableDiagnostics)
			{
				SolveUpdateTime = (float)_watch.ElapsedTicks - (AddRemoveTime + ControllersUpdateTime + ContactsUpdateTime);
			}
			if (Settings.ContinuousPhysics)
			{
				SolveTOI(ref step);
				RepairNonFiniteBodies();
			}
			if (Settings.EnableDiagnostics)
			{
				ContinuousPhysicsTime = (float)_watch.ElapsedTicks - (AddRemoveTime + ControllersUpdateTime + ContactsUpdateTime + SolveUpdateTime);
			}
			_invDt0 = step.inv_dt;
			if ((Flags & WorldFlags.ClearForces) != 0)
			{
				ClearForces();
			}
			for (int j = 0; j < BreakableBodyList.Count; j++)
			{
				BreakableBodyList[j].Update();
			}
			if (Settings.EnableDiagnostics)
			{
				_watch.Stop();
				UpdateTime = _watch.ElapsedTicks;
				_watch.Reset();
			}
		}

		/// <summary>
		/// Call this after you are done with time steps to clear the forces. You normally
		/// call this after each call to Step, unless you are performing sub-steps. By default,
		/// forces will be automatically cleared, so you don't need to call this function.
		/// </summary>
		/// <summary>Number of times a body had to be restored after the solver produced NaN.</summary>
		public int NonFiniteRepairs { get; private set; }

		/// <summary>
		/// The solver can produce NaN on degenerate contacts (e.g. a circle
		/// exactly on a polygon corner) under strict 32-bit float math. A NaN
		/// body would poison every body it touches, so restore such bodies to
		/// their pre-step position and stop them instead.
		/// </summary>
		private void RepairNonFiniteBodies()
		{
			for (int i = 0; i < BodyList.Count; i++)
			{
				Body body = BodyList[i];
				if (float.IsFinite(body.Sweep.C.X) && float.IsFinite(body.Sweep.C.Y) && float.IsFinite(body.Sweep.A)
					&& float.IsFinite(body.LinearVelocityInternal.X) && float.IsFinite(body.LinearVelocityInternal.Y) && float.IsFinite(body.AngularVelocityInternal))
				{
					continue;
				}
				NonFiniteRepairs++;
				body.Sweep.C = body.Sweep.C0;
				body.Sweep.A = body.Sweep.A0;
				if (!float.IsFinite(body.Sweep.C.X) || !float.IsFinite(body.Sweep.C.Y) || !float.IsFinite(body.Sweep.A))
				{
					body.Sweep.C = Vector2.Zero;
					body.Sweep.A = 0f;
				}
				body.LinearVelocityInternal = Vector2.Zero;
				body.AngularVelocityInternal = 0f;
				body.Force = Vector2.Zero;
				body.Torque = 0f;
				body.SynchronizeTransform();
			}
		}

		public void ClearForces()
		{
			for (int i = 0; i < BodyList.Count; i++)
			{
				Body body = BodyList[i];
				body.Force = Vector2.Zero;
				body.Torque = 0f;
			}
		}

		/// <summary>
		/// Query the world for all fixtures that potentially overlap the
		/// provided AABB.
		///
		/// Inside the callback:
		/// Return true: Continues the query
		/// Return false: Terminate the query
		/// </summary>
		/// <param name="callback">A user implemented callback class.</param>
		/// <param name="aabb">The aabb query box.</param>
		public void QueryAABB(Func<Fixture, bool> callback, ref AABB aabb)
		{
			ContactManager.BroadPhase.Query(delegate(int proxyId)
			{
				FixtureProxy proxy = ContactManager.BroadPhase.GetProxy(proxyId);
				return callback(proxy.Fixture);
			}, ref aabb);
		}

		/// <summary>
		/// Ray-cast the world for all fixtures in the path of the ray. Your callback
		/// controls whether you get the closest point, any point, or n-points.
		/// The ray-cast ignores shapes that contain the starting point.
		///
		/// Inside the callback:
		/// return -1: ignore this fixture and continue
		/// return 0: terminate the ray cast
		/// return fraction: clip the ray to this point
		/// return 1: don't clip the ray and continue
		/// </summary>
		/// <param name="callback">A user implemented callback class.</param>
		/// <param name="point1">The ray starting point.</param>
		/// <param name="point2">The ray ending point.</param>
		public void RayCast(RayCastCallback callback, Vector2 point1, Vector2 point2)
		{
			RayCastInput input = default(RayCastInput);
			input.MaxFraction = 1f;
			input.Point1 = point1;
			input.Point2 = point2;
			ContactManager.BroadPhase.RayCast(delegate(RayCastInput rayCastInput, int proxyId)
			{
				FixtureProxy proxy = ContactManager.BroadPhase.GetProxy(proxyId);
				Fixture fixture = proxy.Fixture;
				int childIndex = proxy.ChildIndex;
				if (fixture.RayCast(out var output, ref rayCastInput, childIndex))
				{
					float fraction = output.Fraction;
					Vector2 point3 = (1f - fraction) * input.Point1 + fraction * input.Point2;
					return callback(fixture, point3, output.Normal, fraction);
				}
				return rayCastInput.MaxFraction;
			}, ref input);
		}

		private void Solve(ref TimeStep step)
		{
			Island.Reset(BodyList.Count, ContactManager.ContactList.Count, JointList.Count, ContactManager);
			foreach (Body body4 in BodyList)
			{
				body4.Flags &= ~BodyFlags.Island;
			}
			for (int i = 0; i < ContactManager.ContactList.Count; i++)
			{
				Contact contact = ContactManager.ContactList[i];
				contact.Flags &= ~ContactFlags.Island;
			}
			foreach (Joint joint in JointList)
			{
				joint.IslandFlag = false;
			}
			int count = BodyList.Count;
			if (count > _stack.Length)
			{
				_stack = new Body[Math.Max(_stack.Length * 2, count)];
			}
			for (int num = BodyList.Count - 1; num >= 0; num--)
			{
				Body body = BodyList[num];
				if ((body.Flags & BodyFlags.Island) == 0 && body.Awake && body.Enabled && body.BodyType != BodyType.Static)
				{
					Island.Clear();
					int num2 = 0;
					_stack[num2++] = body;
					body.Flags |= BodyFlags.Island;
					while (num2 > 0)
					{
						Body body2 = _stack[--num2];
						Island.Add(body2);
						body2.Awake = true;
						if (body2.BodyType == BodyType.Static)
						{
							continue;
						}
						for (ContactEdge contactEdge = body2.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
						{
							Contact contact2 = contactEdge.Contact;
							if ((contact2.Flags & ContactFlags.Island) == 0 && contactEdge.Contact.Enabled && contactEdge.Contact.IsTouching())
							{
								bool isSensor = contact2.FixtureA.IsSensor;
								bool isSensor2 = contact2.FixtureB.IsSensor;
								if (!isSensor && !isSensor2)
								{
									Island.Add(contact2);
									contact2.Flags |= ContactFlags.Island;
									Body other = contactEdge.Other;
									if ((other.Flags & BodyFlags.Island) == 0)
									{
										_stack[num2++] = other;
										other.Flags |= BodyFlags.Island;
									}
								}
							}
						}
						for (JointEdge jointEdge = body2.JointList; jointEdge != null; jointEdge = jointEdge.Next)
						{
							if (!jointEdge.Joint.IslandFlag)
							{
								Body other2 = jointEdge.Other;
								if (other2 != null)
								{
									if (other2.Enabled)
									{
										Island.Add(jointEdge.Joint);
										jointEdge.Joint.IslandFlag = true;
										if ((other2.Flags & BodyFlags.Island) == 0)
										{
											_stack[num2++] = other2;
											other2.Flags |= BodyFlags.Island;
										}
									}
								}
								else
								{
									Island.Add(jointEdge.Joint);
									jointEdge.Joint.IslandFlag = true;
								}
							}
						}
					}
					Island.Solve(ref step, ref Gravity);
					for (int j = 0; j < Island.BodyCount; j++)
					{
						Body body3 = Island.Bodies[j];
						if (body3.BodyType == BodyType.Static)
						{
							body3.Flags &= ~BodyFlags.Island;
						}
					}
				}
			}
			foreach (Body body5 in BodyList)
			{
				if ((body5.Flags & BodyFlags.Island) == BodyFlags.Island && body5.BodyType != BodyType.Static)
				{
					body5.SynchronizeFixtures();
				}
			}
			ContactManager.FindNewContacts();
		}

		/// <summary>
		/// Find TOI contacts and solve them.
		/// </summary>
		/// <param name="step">The step.</param>
		private void SolveTOI(ref TimeStep step)
		{
			Island.Reset(64, 32, 0, ContactManager);
			if (_stepComplete)
			{
				for (int i = 0; i < BodyList.Count; i++)
				{
					BodyList[i].Flags &= ~BodyFlags.Island;
					BodyList[i].Sweep.Alpha0 = 0f;
				}
				for (int j = 0; j < ContactManager.ContactList.Count; j++)
				{
					Contact contact = ContactManager.ContactList[j];
					contact.Flags &= ~(ContactFlags.Island | ContactFlags.TOI);
					contact.TOICount = 0;
					contact.TOI = 1f;
				}
			}
			TimeStep subStep = default(TimeStep);
			while (true)
			{
				Contact contact2 = null;
				float num = 1f;
				for (int k = 0; k < ContactManager.ContactList.Count; k++)
				{
					Contact contact3 = ContactManager.ContactList[k];
					if (!contact3.Enabled || contact3.TOICount > 8)
					{
						continue;
					}
					float num2;
					if ((contact3.Flags & ContactFlags.TOI) == ContactFlags.TOI)
					{
						num2 = contact3.TOI;
					}
					else
					{
						Fixture fixtureA = contact3.FixtureA;
						Fixture fixtureB = contact3.FixtureB;
						if (fixtureA.IsSensor || fixtureB.IsSensor)
						{
							continue;
						}
						Body body = fixtureA.Body;
						Body body2 = fixtureB.Body;
						BodyType bodyType = body.BodyType;
						BodyType bodyType2 = body2.BodyType;
						bool flag = body.Awake && bodyType != BodyType.Static;
						bool flag2 = body2.Awake && bodyType2 != BodyType.Static;
						if (!flag && !flag2)
						{
							continue;
						}
						bool flag3 = (body.IsBullet || bodyType != BodyType.Dynamic) && !body.IgnoreCCD;
						bool flag4 = (body2.IsBullet || bodyType2 != BodyType.Dynamic) && !body2.IgnoreCCD;
						if (!flag3 && !flag4)
						{
							continue;
						}
						float alpha = body.Sweep.Alpha0;
						if (body.Sweep.Alpha0 < body2.Sweep.Alpha0)
						{
							alpha = body2.Sweep.Alpha0;
							body.Sweep.Advance(alpha);
						}
						else if (body2.Sweep.Alpha0 < body.Sweep.Alpha0)
						{
							alpha = body.Sweep.Alpha0;
							body2.Sweep.Advance(alpha);
						}
						_input.ProxyA.Set(fixtureA.Shape, contact3.ChildIndexA);
						_input.ProxyB.Set(fixtureB.Shape, contact3.ChildIndexB);
						_input.SweepA = body.Sweep;
						_input.SweepB = body2.Sweep;
						_input.TMax = 1f;
						TimeOfImpact.CalculateTimeOfImpact(out var output, _input);
						float t = output.T;
						num2 = (contact3.TOI = ((output.State != TOIOutputState.Touching) ? 1f : Math.Min(alpha + (1f - alpha) * t, 1f)));
						contact3.Flags |= ContactFlags.TOI;
					}
					if (num2 < num)
					{
						contact2 = contact3;
						num = num2;
					}
				}
				if (contact2 == null || 0.9999988f < num)
				{
					_stepComplete = true;
					return;
				}
				Fixture fixtureA2 = contact2.FixtureA;
				Fixture fixtureB2 = contact2.FixtureB;
				Body body3 = fixtureA2.Body;
				Body body4 = fixtureB2.Body;
				Sweep sweep = body3.Sweep;
				Sweep sweep2 = body4.Sweep;
				body3.Advance(num);
				body4.Advance(num);
				contact2.Update(ContactManager);
				contact2.Flags &= ~ContactFlags.TOI;
				contact2.TOICount++;
				if (!contact2.Enabled || !contact2.IsTouching())
				{
					contact2.Enabled = false;
					body3.Sweep = sweep;
					body4.Sweep = sweep2;
					body3.SynchronizeTransform();
					body4.SynchronizeTransform();
					continue;
				}
				body3.Awake = true;
				body4.Awake = true;
				Island.Clear();
				Island.Add(body3);
				Island.Add(body4);
				Island.Add(contact2);
				body3.Flags |= BodyFlags.Island;
				body4.Flags |= BodyFlags.Island;
				contact2.Flags |= ContactFlags.Island;
				Body[] array = new Body[2] { body3, body4 };
				for (int l = 0; l < 2; l++)
				{
					Body body5 = array[l];
					if (body5.BodyType != BodyType.Dynamic)
					{
						continue;
					}
					for (ContactEdge contactEdge = body5.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
					{
						Contact contact4 = contactEdge.Contact;
						if ((contact4.Flags & ContactFlags.Island) != ContactFlags.Island)
						{
							Body other = contactEdge.Other;
							if ((other.BodyType != BodyType.Dynamic || body5.IsBullet || other.IsBullet) && !contact4.FixtureA.IsSensor && !contact4.FixtureB.IsSensor)
							{
								Sweep sweep3 = other.Sweep;
								if ((other.Flags & BodyFlags.Island) == 0)
								{
									other.Advance(num);
								}
								contact4.Update(ContactManager);
								if (!contact4.Enabled)
								{
									other.Sweep = sweep3;
									other.SynchronizeTransform();
								}
								else if (!contact4.IsTouching())
								{
									other.Sweep = sweep3;
									other.SynchronizeTransform();
								}
								else
								{
									contact4.Flags |= ContactFlags.Island;
									Island.Add(contact4);
									if ((other.Flags & BodyFlags.Island) != BodyFlags.Island)
									{
										other.Flags |= BodyFlags.Island;
										if (other.BodyType != BodyType.Static)
										{
											other.Awake = true;
										}
										Island.Add(other);
									}
								}
							}
						}
					}
				}
				subStep.dt = (1f - num) * step.dt;
				subStep.inv_dt = 1f / subStep.dt;
				subStep.dtRatio = 1f;
				Island.SolveTOI(ref subStep);
				for (int m = 0; m < Island.BodyCount; m++)
				{
					Body body6 = Island.Bodies[m];
					body6.Flags &= ~BodyFlags.Island;
					if (body6.BodyType == BodyType.Dynamic)
					{
						body6.SynchronizeFixtures();
						for (ContactEdge contactEdge2 = body6.ContactList; contactEdge2 != null; contactEdge2 = contactEdge2.Next)
						{
							contactEdge2.Contact.Flags &= ~(ContactFlags.Island | ContactFlags.TOI);
						}
					}
				}
				ContactManager.FindNewContacts();
				if (EnableSubStepping)
				{
					break;
				}
			}
			_stepComplete = false;
		}

		public void AddController(Controller controller)
		{
			controller.World = this;
			ControllerList.Add(controller);
			if (ControllerAdded != null)
			{
				ControllerAdded(controller);
			}
		}

		public void RemoveController(Controller controller)
		{
			if (ControllerList.Contains(controller))
			{
				ControllerList.Remove(controller);
				if (ControllerRemoved != null)
				{
					ControllerRemoved(controller);
				}
			}
		}

		public void AddBreakableBody(BreakableBody breakableBody)
		{
			BreakableBodyList.Add(breakableBody);
		}

		public void RemoveBreakableBody(BreakableBody breakableBody)
		{
			BreakableBodyList.Remove(breakableBody);
		}

		public Fixture TestPoint(Vector2 point)
		{
			Vector2 vector = new Vector2(1.1920929E-07f, 1.1920929E-07f);
			AABB aabb = default(AABB);
			aabb.LowerBound = point - vector;
			aabb.UpperBound = point + vector;
			Fixture myFixture = null;
			QueryAABB(delegate(Fixture fixture)
			{
				if (fixture.TestPoint(ref point))
				{
					myFixture = fixture;
					return false;
				}
				return true;
			}, ref aabb);
			return myFixture;
		}

		/// <summary>
		/// Returns a list of fixtures that are at the specified point.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns></returns>
		public List<Fixture> TestPointAll(Vector2 point)
		{
			Vector2 vector = new Vector2(1.1920929E-07f, 1.1920929E-07f);
			AABB aabb = default(AABB);
			aabb.LowerBound = point - vector;
			aabb.UpperBound = point + vector;
			List<Fixture> fixtures = new List<Fixture>();
			QueryAABB(delegate(Fixture fixture)
			{
				if (fixture.TestPoint(ref point))
				{
					fixtures.Add(fixture);
				}
				return true;
			}, ref aabb);
			return fixtures;
		}

		public void Clear()
		{
			ProcessChanges();
			for (int num = BodyList.Count - 1; num >= 0; num--)
			{
				RemoveBody(BodyList[num]);
			}
			for (int num2 = ControllerList.Count - 1; num2 >= 0; num2--)
			{
				RemoveController(ControllerList[num2]);
			}
			for (int num3 = BreakableBodyList.Count - 1; num3 >= 0; num3--)
			{
				RemoveBreakableBody(BreakableBodyList[num3]);
			}
			ProcessChanges();
		}
	}
}
