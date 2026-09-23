using System.Collections.Generic;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a collection of Modifiers.
	/// </summary>
	public class ModifierCollection : List<Modifier>
	{
		/// <summary>
		/// Returns a deep copy of the ModifierCollection.
		/// </summary>
		/// <returns>A deep copy of the ModifierCollection.</returns>
		public ModifierCollection DeepCopy()
		{
			ModifierCollection modifierCollection = new ModifierCollection();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Modifier current = enumerator.Current;
					modifierCollection.Add(current.DeepCopy());
				}
				return modifierCollection;
			}
		}

		/// <summary>
		/// Causes all Modifiers in the collection to process the particles.
		/// </summary>
		internal unsafe void RunProcessors(float dt, Particle* particleArray, int count)
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].Process(dt, particleArray, count);
			}
		}
	}
}
