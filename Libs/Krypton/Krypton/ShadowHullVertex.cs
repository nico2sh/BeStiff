using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton
{
	public struct ShadowHullVertex : IVertexType
	{
		public Vector2 Position;

		public Vector2 Normal;

		public Color Color;

		private static readonly VertexDeclaration mVertexDeclaration;

		public VertexDeclaration VertexDeclaration => mVertexDeclaration;

		static ShadowHullVertex()
		{
			VertexElement[] elements = new VertexElement[3]
			{
				new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
				new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.Normal, 0),
				new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
			};
			mVertexDeclaration = new VertexDeclaration(elements);
		}

		public ShadowHullVertex(Vector2 position, Vector2 normal, Color color)
		{
			Position = position;
			Normal = normal;
			Color = color;
		}
	}
}
