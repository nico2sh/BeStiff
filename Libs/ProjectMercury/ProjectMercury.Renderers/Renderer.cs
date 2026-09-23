using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury.Emitters;

namespace ProjectMercury.Renderers
{
	/// <summary>
	/// Defines the abstract base class for a Renderer.
	/// </summary>
	public abstract class Renderer : IDisposable
	{
		/// <summary>
		/// Hold a reference to the games GraphicsDeviceService.
		/// </summary>
		public IGraphicsDeviceService GraphicsDeviceService;

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Disposes any unmanaged resources being used by this instance.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="T:ProjectMercury.Renderers.Renderer" /> is reclaimed by garbage collection.
		/// </summary>
		~Renderer()
		{
			Dispose(disposing: false);
		}

		/// <summary>
		/// Loads any content needed by the Renderer.
		/// </summary>
		public virtual void LoadContent(ContentManager content)
		{
		}

		/// <summary>
		/// Renders the specified Emitter.
		/// </summary>
		public virtual void RenderEmitter(Emitter emitter)
		{
			Matrix transform = Matrix.Identity;
			RenderEmitter(emitter, ref transform);
		}

		/// <summary>
		/// Renders the specified Emitter, applying the specified transformation offset.
		/// </summary>
		public abstract void RenderEmitter(Emitter emitter, ref Matrix transform);

		/// <summary>
		/// Renders the specified ParticleEffect.
		/// </summary>
		public virtual void RenderEffect(ParticleEffect effect)
		{
			Matrix transform = Matrix.Identity;
			RenderEffect(effect, ref transform);
		}

		/// <summary>
		/// Renders the specified ParticleEffect, applying the specified transformation offset.
		/// </summary>
		public virtual void RenderEffect(ParticleEffect effect, ref Matrix transform)
		{
			for (int i = 0; i < effect.Count; i++)
			{
				RenderEmitter(effect[i], ref transform);
			}
		}
	}
}
