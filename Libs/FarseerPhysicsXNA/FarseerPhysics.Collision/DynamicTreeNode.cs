namespace FarseerPhysics.Collision
{
	/// <summary>
	/// A node in the dynamic tree. The client does not interact with this directly.
	/// </summary>
	internal struct DynamicTreeNode<T>
	{
		/// <summary>
		/// This is the fattened AABB.
		/// </summary>
		internal AABB AABB;

		internal int Child1;

		internal int Child2;

		internal int LeafCount;

		internal int ParentOrNext;

		internal T UserData;

		internal bool IsLeaf()
		{
			return Child1 == -1;
		}
	}
}
