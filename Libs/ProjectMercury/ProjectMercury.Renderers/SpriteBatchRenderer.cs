using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury.Emitters;

namespace ProjectMercury.Renderers
{
	/// <summary>
	/// Defines a Renderer which uses the standard XNA SpriteBatch class to render Particles.
	/// </summary>
	public sealed class SpriteBatchRenderer : Renderer
	{
		private SpriteBatch Batch;

		/// <summary>
		/// A BlendState for non premultiplied additive blending.
		/// </summary>
		private BlendState NonPremultipliedAdditive { get; set; }

		/// <summary>
		/// Disposes any unmanaged resources being used by the Renderer.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && Batch != null)
			{
				Batch.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Loads any content required by the renderer.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">Thrown if the GraphicsDeviceManager has not been set.</exception>
		public override void LoadContent(ContentManager content)
		{
			if (Batch == null)
			{
				Batch = new SpriteBatch(GraphicsDeviceService.GraphicsDevice);
			}
			if (NonPremultipliedAdditive == null)
			{
				NonPremultipliedAdditive = new BlendState
				{
					AlphaBlendFunction = BlendFunction.Add,
					AlphaDestinationBlend = Blend.One,
					AlphaSourceBlend = Blend.SourceAlpha,
					ColorBlendFunction = BlendFunction.Add,
					ColorDestinationBlend = Blend.One,
					ColorSourceBlend = Blend.SourceAlpha
				};
			}
		}

		/// <summary>
		/// Renders the specified Emitter, applying the specified transformation offset.
		/// </summary>
		public override void RenderEmitter(Emitter emitter, ref Matrix transform)
		{
			if (emitter.ParticleTexture != null && emitter.ActiveParticlesCount > 0 && emitter.BlendMode != EmitterBlendMode.None)
			{
				Rectangle value = new Rectangle(0, 0, emitter.ParticleTexture.Width, emitter.ParticleTexture.Height);
				Vector2 origin = new Vector2((float)value.Width / 2f, (float)value.Height / 2f);
				BlendState blendState = GetBlendState(emitter.BlendMode);
				Batch.Begin(SpriteSortMode.Deferred, blendState, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, transform);
				for (int i = 0; i < emitter.ActiveParticlesCount; i++)
				{
					Particle particle = emitter.Particles[i];
					float scale = particle.Scale / (float)emitter.ParticleTexture.Width;
					Batch.Draw(emitter.ParticleTexture, particle.Position, value, new Color(particle.Colour), particle.Rotation, origin, scale, SpriteEffects.None, 0f);
				}
				Batch.End();
			}
		}

		/// <summary>
		/// Renders the specified ParticleEffect.
		/// </summary>
		public void RenderEffect(ParticleEffect effect, SpriteBatch spriteBatch)
		{
			for (int i = 0; i < effect.Count; i++)
			{
				RenderEmitter(effect[i], spriteBatch);
			}
		}

		public override void RenderEffect(ParticleEffect effect, ref Matrix transform)
		{
			for (int i = 0; i < effect.Count; i++)
			{
				RenderEmitter(effect[i], ref transform);
			}
		}

		/// <summary>
		/// Renders the specified Emitter.
		/// </summary>
		public unsafe void RenderEmitter(Emitter emitter, SpriteBatch spriteBatch)
		{
			if (emitter.ParticleTexture == null || emitter.ActiveParticlesCount <= 0)
			{
				return;
			}
			Rectangle value = new Rectangle(0, 0, emitter.ParticleTexture.Width, emitter.ParticleTexture.Height);
			Vector2 origin = new Vector2((float)value.Width / 2f, (float)value.Height / 2f);
			fixed (Particle* particles = emitter.Particles)
			{
				for (int i = 0; i < emitter.ActiveParticlesCount; i++)
				{
					Particle* ptr = particles + i;
					float scale = ptr->Scale / (float)emitter.ParticleTexture.Width;
					spriteBatch.Draw(emitter.ParticleTexture, ptr->Position, value, new Color(ptr->Colour), ptr->Rotation, origin, scale, SpriteEffects.None, 0f);
				}
			}
		}

		/// <summary>
		/// Gets the BlendState object corresponding to the specified EmitterBlendMode value.
		/// </summary>
		/// <param name="emitterBlendMode">The EmitterBlendMode value.</param>
		/// <returns>A BlendState object.</returns>
		private BlendState GetBlendState(EmitterBlendMode emitterBlendMode)
		{
			switch (emitterBlendMode)
			{
			case EmitterBlendMode.Alpha:
				return BlendState.NonPremultiplied;
			case EmitterBlendMode.Add:
				return NonPremultipliedAdditive;
			default:
				throw new InvalidEnumArgumentException("emitterBlendMode", (int)emitterBlendMode, typeof(EmitterBlendMode));
			}
		}
	}
}
