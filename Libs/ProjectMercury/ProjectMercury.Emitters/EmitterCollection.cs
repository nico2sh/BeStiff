using System.Collections.Generic;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Defines a collection of Emitter objects.
	/// </summary>
	public class EmitterCollection : List<Emitter>
	{
		/// <summary>
		/// Gets the element with the specified name.
		/// </summary>
		/// <param name="name">The name of the Emitter to fetch.</param>
		/// <returns>The first Emitter whose name matches the specified name.</returns>
		public Emitter this[string name]
		{
			get
			{
				for (int i = 0; i < base.Count; i++)
				{
					if (base[i].Name.Equals(name))
					{
						return base[i];
					}
				}
				throw new KeyNotFoundException();
			}
		}
	}
}
