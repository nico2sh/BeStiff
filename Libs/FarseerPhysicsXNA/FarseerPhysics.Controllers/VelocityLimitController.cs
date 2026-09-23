using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Controllers
{
	/// <summary>
	/// Put a limit on the linear (translation - the movespeed) and angular (rotation) velocity
	/// of bodies added to this controller.
	/// </summary>
	public class VelocityLimitController : Controller
	{
		public bool LimitAngularVelocity = true;

		public bool LimitLinearVelocity = true;

		private List<Body> _bodies = new List<Body>();

		private float _maxAngularSqared;

		private float _maxAngularVelocity;

		private float _maxLinearSqared;

		private float _maxLinearVelocity;

		/// <summary>
		/// Gets or sets the max angular velocity.
		/// </summary>
		/// <value>The max angular velocity.</value>
		public float MaxAngularVelocity
		{
			get
			{
				return _maxAngularVelocity;
			}
			set
			{
				_maxAngularVelocity = value;
				_maxAngularSqared = _maxAngularVelocity * _maxAngularVelocity;
			}
		}

		/// <summary>
		/// Gets or sets the max linear velocity.
		/// </summary>
		/// <value>The max linear velocity.</value>
		public float MaxLinearVelocity
		{
			get
			{
				return _maxLinearVelocity;
			}
			set
			{
				_maxLinearVelocity = value;
				_maxLinearSqared = _maxLinearVelocity * _maxLinearVelocity;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FarseerPhysics.Controllers.VelocityLimitController" /> class.
		/// Sets the max linear velocity to Settings.MaxTranslation
		/// Sets the max angular velocity to Settings.MaxRotation
		/// </summary>
		public VelocityLimitController()
			: base(ControllerType.VelocityLimitController)
		{
			MaxLinearVelocity = 2f;
			MaxAngularVelocity = (float)Math.PI / 2f;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FarseerPhysics.Controllers.VelocityLimitController" /> class.
		/// Pass in 0 or float.MaxValue to disable the limit.
		/// maxAngularVelocity = 0 will disable the angular velocity limit.
		/// </summary>
		/// <param name="maxLinearVelocity">The max linear velocity.</param>
		/// <param name="maxAngularVelocity">The max angular velocity.</param>
		public VelocityLimitController(float maxLinearVelocity, float maxAngularVelocity)
			: base(ControllerType.VelocityLimitController)
		{
			if (maxLinearVelocity == 0f || maxLinearVelocity == float.MaxValue)
			{
				LimitLinearVelocity = false;
			}
			if (maxAngularVelocity == 0f || maxAngularVelocity == float.MaxValue)
			{
				LimitAngularVelocity = false;
			}
			MaxLinearVelocity = maxLinearVelocity;
			MaxAngularVelocity = maxAngularVelocity;
		}

		public override void Update(float dt)
		{
			foreach (Body body in _bodies)
			{
				if (!IsActiveOn(body))
				{
					continue;
				}
				if (LimitLinearVelocity)
				{
					float num = dt * body.LinearVelocityInternal.X;
					float num2 = dt * body.LinearVelocityInternal.Y;
					float num3 = num * num + num2 * num2;
					if (num3 > dt * _maxLinearSqared)
					{
						float num4 = (float)Math.Sqrt(num3);
						float num5 = _maxLinearVelocity / num4;
						body.LinearVelocityInternal.X *= num5;
						body.LinearVelocityInternal.Y *= num5;
					}
				}
				if (LimitAngularVelocity)
				{
					float num6 = dt * body.AngularVelocityInternal;
					if (num6 * num6 > _maxAngularSqared)
					{
						float num7 = _maxAngularVelocity / Math.Abs(num6);
						body.AngularVelocityInternal *= num7;
					}
				}
			}
		}

		public void AddBody(Body body)
		{
			_bodies.Add(body);
		}

		public void RemoveBody(Body body)
		{
			_bodies.Remove(body);
		}
	}
}
