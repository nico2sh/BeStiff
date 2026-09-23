using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
	public static class LinkFactory
	{
		/// <summary>
		/// Creates a chain.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <param name="linkWidth">The width.</param>
		/// <param name="linkHeight">The height.</param>
		/// <param name="fixStart">if set to <c>true</c> [fix start].</param>
		/// <param name="fixEnd">if set to <c>true</c> [fix end].</param>
		/// <param name="numberOfLinks">The number of links.</param>
		/// <param name="linkDensity">The link density.</param>
		/// <returns></returns>
		public static Path CreateChain(World world, Vector2 start, Vector2 end, float linkWidth, float linkHeight, bool fixStart, bool fixEnd, int numberOfLinks, float linkDensity)
		{
			Path path = new Path();
			path.Add(start);
			path.Add(end);
			PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(linkWidth, linkHeight), linkDensity);
			List<Body> list = PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, BodyType.Dynamic, numberOfLinks);
			if (fixStart)
			{
				JointFactory.CreateFixedRevoluteJoint(world, list[0], new Vector2(0f, 0f - linkHeight / 2f), list[0].Position);
			}
			if (fixEnd)
			{
				JointFactory.CreateFixedRevoluteJoint(world, list[list.Count - 1], new Vector2(0f, linkHeight / 2f), list[list.Count - 1].Position);
			}
			PathManager.AttachBodiesWithRevoluteJoint(world, list, new Vector2(0f, 0f - linkHeight), new Vector2(0f, linkHeight), connectFirstAndLast: false, collideConnected: false);
			return path;
		}
	}
}
