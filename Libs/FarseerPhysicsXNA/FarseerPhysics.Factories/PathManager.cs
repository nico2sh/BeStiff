using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
	/// <summary>
	/// An easy to use manager for creating paths.
	/// </summary>
	public static class PathManager
	{
		public enum LinkType
		{
			Revolute,
			Slider
		}

		/// <summary>
		/// Convert a path into a set of edges and attaches them to the specified body.
		/// Note: use only for static edges.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="body">The body.</param>
		/// <param name="subdivisions">The subdivisions.</param>
		public static void ConvertPathToEdges(Path path, Body body, int subdivisions)
		{
			Vertices vertices = path.GetVertices(subdivisions);
			if (path.Closed)
			{
				LoopShape shape = new LoopShape(vertices);
				body.CreateFixture(shape);
				return;
			}
			for (int i = 1; i < vertices.Count; i++)
			{
				body.CreateFixture(new EdgeShape(vertices[i], vertices[i - 1]));
			}
		}

		/// <summary>
		/// Convert a closed path into a polygon.
		/// Convex decomposition is automatically performed.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="body">The body.</param>
		/// <param name="density">The density.</param>
		/// <param name="subdivisions">The subdivisions.</param>
		public static void ConvertPathToPolygon(Path path, Body body, float density, int subdivisions)
		{
			if (!path.Closed)
			{
				throw new Exception("The path must be closed to convert to a polygon.");
			}
			List<Vector2> vertices = path.GetVertices(subdivisions);
			List<Vertices> list = EarclipDecomposer.ConvexPartition(new Vertices(vertices));
			foreach (Vertices item in list)
			{
				body.CreateFixture(new PolygonShape(item, density));
			}
		}

		/// <summary>
		/// Duplicates the given Body along the given path for approximatly the given copies.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="path">The path.</param>
		/// <param name="shapes">The shapes.</param>
		/// <param name="type">The type.</param>
		/// <param name="copies">The copies.</param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, IEnumerable<Shape> shapes, BodyType type, int copies, object userData)
		{
			List<Vector3> list = path.SubdivideEvenly(copies);
			List<Body> list2 = new List<Body>();
			for (int i = 0; i < list.Count; i++)
			{
				Body body = new Body(world);
				body.BodyType = type;
				body.Position = new Vector2(list[i].X, list[i].Y);
				body.Rotation = list[i].Z;
				foreach (Shape shape in shapes)
				{
					body.CreateFixture(shape, userData);
				}
				list2.Add(body);
			}
			return list2;
		}

		public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, IEnumerable<Shape> shapes, BodyType type, int copies)
		{
			return EvenlyDistributeShapesAlongPath(world, path, shapes, type, copies, null);
		}

		/// <summary>
		/// Duplicates the given Body along the given path for approximatly the given copies.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="path">The path.</param>
		/// <param name="shape">The shape.</param>
		/// <param name="type">The type.</param>
		/// <param name="copies">The copies.</param>
		/// <param name="userData">The user data.</param>
		/// <returns></returns>
		public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, Shape shape, BodyType type, int copies, object userData)
		{
			List<Shape> list = new List<Shape>(1);
			list.Add(shape);
			return EvenlyDistributeShapesAlongPath(world, path, list, type, copies, userData);
		}

		public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, Shape shape, BodyType type, int copies)
		{
			return EvenlyDistributeShapesAlongPath(world, path, shape, type, copies, null);
		}

		/// <summary>
		/// Moves the body on the path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="body">The body.</param>
		/// <param name="time">The time.</param>
		/// <param name="strength">The strength.</param>
		/// <param name="timeStep">The time step.</param>
		public static void MoveBodyOnPath(Path path, Body body, float time, float strength, float timeStep)
		{
			Vector2 position = path.GetPosition(time);
			Vector2 vector = body.Position - position;
			Vector2 vector2 = vector / timeStep * strength;
			body.LinearVelocity = -vector2;
		}

		/// <summary>
		/// Attaches the bodies with revolute joints.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="bodies">The bodies.</param>
		/// <param name="localAnchorA">The local anchor A.</param>
		/// <param name="localAnchorB">The local anchor B.</param>
		/// <param name="connectFirstAndLast">if set to <c>true</c> [connect first and last].</param>
		/// <param name="collideConnected">if set to <c>true</c> [collide connected].</param>
		public static List<RevoluteJoint> AttachBodiesWithRevoluteJoint(World world, List<Body> bodies, Vector2 localAnchorA, Vector2 localAnchorB, bool connectFirstAndLast, bool collideConnected)
		{
			List<RevoluteJoint> list = new List<RevoluteJoint>(bodies.Count + 1);
			for (int i = 1; i < bodies.Count; i++)
			{
				RevoluteJoint revoluteJoint = new RevoluteJoint(bodies[i], bodies[i - 1], localAnchorA, localAnchorB);
				revoluteJoint.CollideConnected = collideConnected;
				world.AddJoint(revoluteJoint);
				list.Add(revoluteJoint);
			}
			if (connectFirstAndLast)
			{
				RevoluteJoint revoluteJoint2 = new RevoluteJoint(bodies[0], bodies[bodies.Count - 1], localAnchorA, localAnchorB);
				revoluteJoint2.CollideConnected = collideConnected;
				world.AddJoint(revoluteJoint2);
				list.Add(revoluteJoint2);
			}
			return list;
		}

		/// <summary>
		/// Attaches the bodies with revolute joints.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="bodies">The bodies.</param>
		/// <param name="localAnchorA">The local anchor A.</param>
		/// <param name="localAnchorB">The local anchor B.</param>
		/// <param name="connectFirstAndLast">if set to <c>true</c> [connect first and last].</param>
		/// <param name="collideConnected">if set to <c>true</c> [collide connected].</param>
		/// <param name="minLength">Minimum length of the slider joint.</param>
		/// <param name="maxLength">Maximum length of the slider joint.</param>
		/// <returns></returns>
		public static List<SliderJoint> AttachBodiesWithSliderJoint(World world, List<Body> bodies, Vector2 localAnchorA, Vector2 localAnchorB, bool connectFirstAndLast, bool collideConnected, float minLength, float maxLength)
		{
			List<SliderJoint> list = new List<SliderJoint>(bodies.Count + 1);
			for (int i = 1; i < bodies.Count; i++)
			{
				SliderJoint sliderJoint = new SliderJoint(bodies[i], bodies[i - 1], localAnchorA, localAnchorB, minLength, maxLength);
				sliderJoint.CollideConnected = collideConnected;
				world.AddJoint(sliderJoint);
				list.Add(sliderJoint);
			}
			if (connectFirstAndLast)
			{
				SliderJoint sliderJoint2 = new SliderJoint(bodies[0], bodies[bodies.Count - 1], localAnchorA, localAnchorB, minLength, maxLength);
				sliderJoint2.CollideConnected = collideConnected;
				world.AddJoint(sliderJoint2);
				list.Add(sliderJoint2);
			}
			return list;
		}
	}
}
