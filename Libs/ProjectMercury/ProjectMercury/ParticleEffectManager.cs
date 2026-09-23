using System.Collections.Generic;
using ProjectMercury.Renderers;
using ProjectMercury.Threading;

namespace ProjectMercury
{
	public class ParticleEffectManager : List<ParticleEffect>
	{
		/// <summary>
		/// Gets or sets the renderer which is used to render the particle effects.
		/// </summary>
		public Renderer Renderer { get; set; }

		/// <summary>
		/// Gets the number of active particles in each particle effect.
		/// </summary>
		public int ActiveParticlesCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < base.Count; i++)
				{
					num += base[i].ActiveParticlesCount;
				}
				return num;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ProjectMercury.ParticleEffectManager" /> class.
		/// </summary>
		/// <param name="renderer">The renderer which will be used to render particles.</param>
		public ParticleEffectManager(Renderer renderer)
			: base(20)
		{
			Renderer = renderer;
		}

		/// <summary>
		/// Updates all the particle effects which are managed by the manager.
		/// </summary>
		/// <param name="deltaSeconds">The elapsed time in whole and fractional seconds.</param>
		/// <param name="multithreaded">True to update particle effects asynchronously.</param>
		public void Update(float deltaSeconds, bool multithreaded)
		{
			if (!multithreaded)
			{
				for (int i = 0; i < base.Count; i++)
				{
					base[i].Update(deltaSeconds);
				}
			}
			else if (base.Count > 0)
			{
				Parallel.For(0, base.Count, delegate(int index)
				{
					base[index].Update(deltaSeconds);
				});
			}
		}

		/// <summary>
		/// Draws each particle effect.
		/// </summary>
		public void Draw()
		{
			for (int i = 0; i < base.Count; i++)
			{
				Renderer.RenderEffect(base[i]);
			}
		}
	}
}
