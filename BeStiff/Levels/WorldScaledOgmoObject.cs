using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OgmoXNA;

namespace Be_Stiff.Levels
{
	public class WorldScaledOgmoObject
	{
		private List<OgmoNode> nodes = new List<OgmoNode>();

		public float Height { get; private set; }

		public OgmoNode[] Nodes => nodes.ToArray();

		public Vector2 Origin { get; private set; }

		public Vector2 Position { get; private set; }

		public float Rotation { get; private set; }

		public float Width { get; private set; }

		public WorldScaledOgmoObject(OgmoObject obj)
		{
			Height = ConvertUnits.ToSimUnits(obj.Height);
			Width = ConvertUnits.ToSimUnits(obj.Width);
			Position = ConvertUnits.ToSimUnits(obj.Position);
			nodes = new List<OgmoNode>();
			OgmoNode[] array = obj.Nodes;
			foreach (OgmoNode ogmoNode in array)
			{
				OgmoNode item = new OgmoNode(ConvertUnits.ToSimUnits(ogmoNode.Position));
				nodes.Add(item);
			}
			Rotation = 0f - obj.Rotation;
		}
	}
}
