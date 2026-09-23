using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public abstract class Joint
	{
		/// <summary>
		/// The Breakpoint simply indicates the maximum Value the JointError can be before it breaks.
		/// The default value is float.MaxValue
		/// </summary>
		public float Breakpoint = float.MaxValue;

		internal JointEdge EdgeA = new JointEdge();

		internal JointEdge EdgeB = new JointEdge();

		public bool Enabled = true;

		protected float InvIA;

		protected float InvIB;

		protected float InvMassA;

		protected float InvMassB;

		internal bool IslandFlag;

		protected Vector2 LocalCenterA;

		protected Vector2 LocalCenterB;

		/// <summary>
		/// Gets or sets the type of the joint.
		/// </summary>
		/// <value>The type of the joint.</value>
		public JointType JointType { get; protected set; }

		/// <summary>
		/// Get the first body attached to this joint.
		/// </summary>
		/// <value></value>
		public Body BodyA { get; set; }

		/// <summary>
		/// Get the second body attached to this joint.
		/// </summary>
		/// <value></value>
		public Body BodyB { get; set; }

		/// <summary>
		/// Get the anchor point on body1 in world coordinates.
		/// </summary>
		/// <value></value>
		public abstract Vector2 WorldAnchorA { get; }

		/// <summary>
		/// Get the anchor point on body2 in world coordinates.
		/// </summary>
		/// <value></value>
		public abstract Vector2 WorldAnchorB { get; set; }

		/// <summary>
		/// Set the user data pointer.
		/// </summary>
		/// <value>The data.</value>
		public object UserData { get; set; }

		/// <summary>
		/// Short-cut function to determine if either body is inactive.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active
		{
			get
			{
				if (BodyA.Enabled)
				{
					return BodyB.Enabled;
				}
				return false;
			}
		}

		/// <summary>
		/// Set this flag to true if the attached bodies should collide.
		/// </summary>
		public bool CollideConnected { get; set; }

		/// <summary>
		/// Fires when the joint is broken.
		/// </summary>
		public event Action<Joint, float> Broke;

		protected Joint()
		{
		}

		protected Joint(Body body, Body bodyB)
		{
			BodyA = body;
			BodyB = bodyB;
			CollideConnected = false;
		}

		/// <summary>
		/// Constructor for fixed joint
		/// </summary>
		protected Joint(Body body)
		{
			BodyA = body;
			CollideConnected = false;
		}

		/// <summary>
		/// Get the reaction force on body2 at the joint anchor in Newtons.
		/// </summary>
		/// <param name="inv_dt">The inv_dt.</param>
		/// <returns></returns>
		public abstract Vector2 GetReactionForce(float inv_dt);

		/// <summary>
		/// Get the reaction torque on body2 in N*m.
		/// </summary>
		/// <param name="inv_dt">The inv_dt.</param>
		/// <returns></returns>
		public abstract float GetReactionTorque(float inv_dt);

		protected void WakeBodies()
		{
			BodyA.Awake = true;
			if (BodyB != null)
			{
				BodyB.Awake = true;
			}
		}

		/// <summary>
		/// Return true if the joint is a fixed type.
		/// </summary>
		public bool IsFixedType()
		{
			if (JointType != JointType.FixedRevolute && JointType != JointType.FixedDistance && JointType != JointType.FixedPrismatic && JointType != JointType.FixedLine && JointType != JointType.FixedMouse && JointType != JointType.FixedAngle)
			{
				return JointType == JointType.FixedFriction;
			}
			return true;
		}

		internal abstract void InitVelocityConstraints(ref TimeStep step);

		internal void Validate(float invDT)
		{
			if (!Enabled)
			{
				return;
			}
			float num = GetReactionForce(invDT).Length();
			if (!(Math.Abs(num) <= Breakpoint))
			{
				Enabled = false;
				if (this.Broke != null)
				{
					this.Broke(this, num);
				}
			}
		}

		internal abstract void SolveVelocityConstraints(ref TimeStep step);

		/// <summary>
		/// Solves the position constraints.
		/// </summary>
		/// <returns>returns true if the position errors are within tolerance.</returns>
		internal abstract bool SolvePositionConstraints();
	}
}
