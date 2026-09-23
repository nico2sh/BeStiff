using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers
{
	public sealed class BuoyancyController : Controller
	{
		/// <summary>
		/// Controls the rotational drag that the fluid exerts on the bodies within it. Use higher values will simulate thick fluid, like honey, lower values to
		/// simulate water-like fluids. 
		/// </summary>
		public float AngularDragCoefficient;

		/// <summary>
		/// Density of the fluid. Higher values will make things more buoyant, lower values will cause things to sink.
		/// </summary>
		public float Density;

		/// <summary>
		/// Controls the linear drag that the fluid exerts on the bodies within it.  Use higher values will simulate thick fluid, like honey, lower values to
		/// simulate water-like fluids.
		/// </summary>
		public float LinearDragCoefficient;

		/// <summary>
		/// Acts like waterflow. Defaults to 0,0.
		/// </summary>
		public Vector2 Velocity;

		private AABB _container;

		private Vector2 _gravity;

		private Vector2 _normal;

		private float _offset;

		private Dictionary<int, Body> _uniqueBodies = new Dictionary<int, Body>();

		public AABB Container
		{
			get
			{
				return _container;
			}
			set
			{
				_container = value;
				_offset = _container.UpperBound.Y;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FarseerPhysics.Controllers.BuoyancyController" /> class.
		/// </summary>
		/// <param name="container">Only bodies inside this AABB will be influenced by the controller</param>
		/// <param name="density">Density of the fluid</param>
		/// <param name="linearDragCoefficient">Linear drag coefficient of the fluid</param>
		/// <param name="rotationalDragCoefficient">Rotational drag coefficient of the fluid</param>
		/// <param name="gravity">The direction gravity acts. Buoyancy force will act in opposite direction of gravity.</param>
		public BuoyancyController(AABB container, float density, float linearDragCoefficient, float rotationalDragCoefficient, Vector2 gravity)
			: base(ControllerType.BuoyancyController)
		{
			Container = container;
			_normal = new Vector2(0f, 1f);
			Density = density;
			LinearDragCoefficient = linearDragCoefficient;
			AngularDragCoefficient = rotationalDragCoefficient;
			_gravity = gravity;
		}

		public override void Update(float dt)
		{
			_uniqueBodies.Clear();
			World.QueryAABB(delegate(Fixture fixture2)
			{
				if (fixture2.Body.IsStatic || !fixture2.Body.Awake)
				{
					return true;
				}
				if (!_uniqueBodies.ContainsKey(fixture2.Body.BodyId))
				{
					_uniqueBodies.Add(fixture2.Body.BodyId, fixture2.Body);
				}
				return true;
			}, ref _container);
			foreach (KeyValuePair<int, Body> uniqueBody in _uniqueBodies)
			{
				Body value = uniqueBody.Value;
				Vector2 zero = Vector2.Zero;
				Vector2 zero2 = Vector2.Zero;
				float num = 0f;
				float num2 = 0f;
				for (int num3 = 0; num3 < value.FixtureList.Count; num3++)
				{
					Fixture fixture = value.FixtureList[num3];
					if (fixture.Shape.ShapeType == ShapeType.Polygon || fixture.Shape.ShapeType == ShapeType.Circle)
					{
						Shape shape = fixture.Shape;
						Vector2 sc;
						float num4 = shape.ComputeSubmergedArea(_normal, _offset, value.Xf, out sc);
						num += num4;
						zero.X += num4 * sc.X;
						zero.Y += num4 * sc.Y;
						num2 += num4 * shape.Density;
						zero2.X += num4 * sc.X * shape.Density;
						zero2.Y += num4 * sc.Y * shape.Density;
					}
				}
				zero.X /= num;
				zero.Y /= num;
				zero2.X /= num2;
				zero2.Y /= num2;
				if (!(num < 1.1920929E-07f))
				{
					Vector2 force = (0f - Density) * num * _gravity;
					value.ApplyForce(force, zero2);
					Vector2 force2 = value.GetLinearVelocityFromWorldPoint(zero) - Velocity;
					force2 *= (0f - LinearDragCoefficient) * num;
					value.ApplyForce(force2, zero);
					value.ApplyTorque((0f - value.Inertia) / value.Mass * num * value.AngularVelocity * AngularDragCoefficient);
				}
			}
		}
	}
}
