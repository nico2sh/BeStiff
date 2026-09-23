using System;

namespace FarseerPhysics
{
	[Flags]
	public enum DebugViewFlags
	{
		/// <summary>
		/// Draw shapes.
		/// </summary>
		Shape = 1,
		/// <summary>
		/// Draw joint connections.
		/// </summary>
		Joint = 2,
		/// <summary>
		/// Draw axis aligned bounding boxes.
		/// </summary>
		AABB = 4,
		/// <summary>
		/// Draw broad-phase pairs.
		/// </summary>
		Pair = 8,
		/// <summary>
		/// Draw center of mass frame.
		/// </summary>
		CenterOfMass = 0x10,
		/// <summary>
		/// Draw useful debug data such as timings and number of bodies, joints, contacts and more.
		/// </summary>
		DebugPanel = 0x20,
		/// <summary>
		/// Draw contact points between colliding bodies.
		/// </summary>
		ContactPoints = 0x40,
		/// <summary>
		/// Draw contact normals. Need ContactPoints to be enabled first.
		/// </summary>
		ContactNormals = 0x80,
		/// <summary>
		/// Draws the vertices of polygons.
		/// </summary>
		PolygonPoints = 0x100,
		/// <summary>
		/// Draws the performance graph.
		/// </summary>
		PerformanceGraph = 0x200,
		/// <summary>
		/// Draws controllers.
		/// </summary>
		Controllers = 0x400
	}
}
