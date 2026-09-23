using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ProjectMercury.Controllers;
using ProjectMercury.Emitters;

namespace ProjectMercury
{
	/// <summary>
	/// Defines the root of a particle effect hierarchy.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.ParticleEffectTypeConverter, Projectmercury.Design")]
	public class ParticleEffect : EmitterCollection
	{
		private string _name;

		/// <summary>
		/// Gets or sets the author of the ParticleEffect.
		/// </summary>
		public string Author;

		/// <summary>
		/// Gets or sets the description of the ParticleEffect.
		/// </summary>
		public string Description;

		/// <summary>
		/// Gets or sets the name of the ParticleEffect.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (Name != value)
				{
					_name = value;
					OnNameChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Gets or sets the controller which is assigned to the ParticleEffect.
		/// </summary>
		[ContentSerializerIgnore]
		public ControllerCollection Controllers { get; set; }

		/// <summary>
		/// Gets the total number of active Particles in the ParticleEffect.
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
		/// Occurs when name of the ParticleEffect has been changed.
		/// </summary>
		public event EventHandler NameChanged;

		/// <summary>
		/// Raises the <see cref="E:NameChanged" /> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
		protected virtual void OnNameChanged(EventArgs e)
		{
			if (this.NameChanged != null)
			{
				this.NameChanged(this, e);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ProjectMercury.ParticleEffect" /> class.
		/// </summary>
		public ParticleEffect()
		{
			Name = "Particle Effect";
			Controllers = new ControllerCollection
			{
				Owner = this
			};
		}

		/// <summary>
		/// Returns a deep copy of the ParticleEffect.
		/// </summary>
		public virtual ParticleEffect DeepCopy()
		{
			ParticleEffect particleEffect = new ParticleEffect();
			particleEffect.Author = Author;
			particleEffect.Description = Description;
			particleEffect.Name = Name;
			ParticleEffect particleEffect2 = particleEffect;
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Emitter current = enumerator.Current;
					particleEffect2.Add(current.DeepCopy());
				}
				return particleEffect2;
			}
		}

		/// <summary>
		/// Triggers the ParticleEffect at the specified position.
		/// </summary>
		public virtual void Trigger(Vector2 position)
		{
			if (Controllers.Count > 0)
			{
				for (int i = 0; i < Controllers.Count; i++)
				{
					Controllers[i].Trigger(ref position);
				}
			}
			else
			{
				for (int j = 0; j < base.Count; j++)
				{
					base[j].Trigger(ref position);
				}
			}
		}

		/// <summary>
		/// Triggers the ParticleEffect at the specified position.
		/// </summary>
		public virtual void Trigger(ref Vector2 position)
		{
			if (Controllers.Count > 0)
			{
				for (int i = 0; i < Controllers.Count; i++)
				{
					Controllers[i].Trigger(ref position);
				}
			}
			else
			{
				for (int j = 0; j < base.Count; j++)
				{
					base[j].Trigger(ref position);
				}
			}
		}

		/// <summary>
		/// Initialises all Emitters within the ParticleEffect.
		/// </summary>
		public virtual void Initialise()
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].Initialise();
			}
		}

		/// <summary>
		/// Terminates all Emitters within the ParticleEffect with immediate effect.
		/// </summary>
		public virtual void Terminate()
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].Terminate();
			}
		}

		/// <summary>
		/// Loads content required by Emitters within the ParticleEffect.
		/// </summary>
		public virtual void LoadContent(ContentManager content)
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].LoadContent(content);
			}
		}

		/// <summary>
		/// Updates all Emitters within the ParticleEffect.
		/// </summary>
		/// <param name="deltaSeconds">Elapsed frame time in whole and fractional seconds.</param>
		public virtual void Update(float deltaSeconds)
		{
			if (Controllers.Count > 0)
			{
				for (int i = 0; i < Controllers.Count; i++)
				{
					Controllers[i].Update(deltaSeconds);
				}
			}
			else
			{
				for (int j = 0; j < base.Count; j++)
				{
					base[j].Update(deltaSeconds);
				}
			}
		}

		/// <summary>
		/// Updates all Emitters within the ParticleEffect.
		/// </summary>
		/// <param name="totalSeconds">Total game time in whole and fractional seconds.</param>
		/// <param name="deltaSeconds">Elapsed frame time in whole and fractional seconds.</param>
		[Obsolete("Use Update(deltaSeconds) instead.", false)]
		public virtual void Update(float totalSeconds, float deltaSeconds)
		{
			Update(deltaSeconds);
		}
	}
}
