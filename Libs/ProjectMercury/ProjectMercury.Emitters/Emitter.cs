using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury.Modifiers;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Defines the base class for a Particle Emitter. The basic implementation releases Particles from a single point.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Emitters.EmitterTypeConverter, ProjectMercury.Design")]
	public class Emitter
	{
		private static int CreationIndex;

		private string _name;

		private float TotalSeconds;

		/// <summary>
		/// Gets or sets a value indicating wether or not the Emitter is enabled (can be triggered).
		/// </summary>
		public bool Enabled;

		private int _budget;

		private float _term;

		/// <summary>
		/// Gets or sets the array of particles managed by the emitter.
		/// </summary>
		[ContentSerializerIgnore]
		public Particle[] Particles;

		private int Idle;

		private int _releaseQuantity;

		/// <summary>
		/// Gets or sets the speed at which Particles travel when they are released.
		/// </summary>
		public VariableFloat ReleaseSpeed;

		/// <summary>
		/// Gets or sets the colour of released Particles.
		/// </summary>
		public VariableFloat3 ReleaseColour;

		/// <summary>
		/// Gets or sets the opacity of released Particles.
		/// </summary>
		public VariableFloat ReleaseOpacity;

		/// <summary>
		/// Gets or sets the scale of released particles.
		/// </summary>
		public VariableFloat ReleaseScale;

		/// <summary>
		/// Gets or sets the rotation of released Particles.
		/// </summary>
		public VariableFloat ReleaseRotation;

		/// <summary>
		/// Gets or sets the initial impulse applied to Particles as they are relased.
		/// </summary>
		[ContentSerializer(Optional = true)]
		public Vector2 ReleaseImpulse;

		/// <summary>
		/// Gets the asset name of a texture to load in the LoadContent method.
		/// </summary>
		[ContentSerializer(Optional = true)]
		public string ParticleTextureAssetName;

		/// <summary>
		/// Gets or sets the Texture2D used to display the Particles.
		/// </summary>
		[ContentSerializerIgnore]
		public Texture2D ParticleTexture;

		/// <summary>
		/// Gets the collection of Modifiers which are acting upon the Emitter.
		/// </summary>
		public ModifierCollection Modifiers;

		/// <summary>
		/// The blending mode to be used by Renderers when rendering this Emitter.
		/// </summary>
		public EmitterBlendMode BlendMode;

		/// <summary>
		/// The Emitters trigger offset in relation to the ParticleEffect.
		/// </summary>
		[ContentSerializer(Optional = true)]
		public Vector2 TriggerOffset;

		/// <summary>
		/// Defines the minimum amount of time between triggers for the Emitter, expressed in
		/// whole and fractional seconds. Triggers which occur during this period will be ignored.
		/// </summary>
		[ContentSerializer(Optional = true)]
		public float MinimumTriggerPeriod;

		/// <summary>
		/// Stores the time at which the Emitter was most recently triggered.
		/// </summary>
		private float MostRecentTrigger;

		/// <summary>
		/// Gets or sets the name of the Emitter.
		/// </summary>
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
		/// True if the Emitter object has been initialised, else false.
		/// </summary>
		[ContentSerializerIgnore]
		public bool Initialised { get; private set; }

		/// <summary>
		/// Gets or sets the number of Particles which are available to the Emitter.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">Thrown if trying to set this property after the Emitter has been initialised.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the specified value is less than 1.</exception>
		public int Budget
		{
			get
			{
				return _budget;
			}
			set
			{
				_budget = value;
			}
		}

		/// <summary>
		/// Gets or sets the length of time that released Particles will remain active, in whole and fractional seconds.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">Thrown if trying to set this property after the Emitter has been initialised.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the supplied value is less than or equal to 0.</exception>
		public float Term
		{
			get
			{
				return _term;
			}
			set
			{
				_term = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of Particles which will be released on each trigger.
		/// </summary>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the specified value is less than 1.</exception>
		public int ReleaseQuantity
		{
			get
			{
				return _releaseQuantity;
			}
			set
			{
				_releaseQuantity = value;
			}
		}

		/// <summary>
		/// Gets the number of Particles which are currently active.
		/// </summary>
		public int ActiveParticlesCount => Idle;

		/// <summary>
		/// Raised when the name of the Emitter has been changed.
		/// </summary>
		public event EventHandler NameChanged;

		/// <summary>
		/// Gets a default name for the next Emitter.
		/// </summary>
		private static string NextEmitterName()
		{
			return $"Emitter{CreationIndex++:00}";
		}

		/// <summary>
		/// Raises the NameChanged event.
		/// </summary>
		protected virtual void OnNameChanged(EventArgs e)
		{
			if (this.NameChanged != null)
			{
				this.NameChanged(this, e);
			}
		}

		/// <summary>
		/// Instantiates a new instance of the Emitter class.
		/// </summary>
		public Emitter()
		{
			Name = NextEmitterName();
			Enabled = true;
			Modifiers = new ModifierCollection();
		}

		/// <summary>
		/// Returns an unitialised deep copy of the Emitter.
		/// </summary>
		/// <returns>A deep copy of the Emitter.</returns>
		public virtual Emitter DeepCopy()
		{
			Emitter emitter = new Emitter();
			CopyBaseFields(emitter);
			return emitter;
		}

		/// <summary>
		/// Copies the fields of the Emitter base class into the specified Emitter.
		/// </summary>
		/// <param name="emitter">The Emitter which will be copied into.</param>
		protected void CopyBaseFields(Emitter emitter)
		{
			emitter.BlendMode = BlendMode;
			emitter.Budget = Budget;
			emitter.Enabled = Enabled;
			emitter.MinimumTriggerPeriod = MinimumTriggerPeriod;
			emitter.Modifiers = Modifiers.DeepCopy();
			emitter.Name = $"Copy of {Name}";
			emitter.ParticleTexture = ParticleTexture;
			emitter.ParticleTextureAssetName = string.Copy(ParticleTextureAssetName ?? string.Empty);
			emitter.ReleaseColour = ReleaseColour;
			emitter.ReleaseOpacity = ReleaseOpacity;
			emitter.ReleaseQuantity = ReleaseQuantity;
			emitter.ReleaseRotation = ReleaseRotation;
			emitter.ReleaseScale = ReleaseScale;
			emitter.ReleaseSpeed = ReleaseSpeed;
			emitter.ReleaseImpulse = ReleaseImpulse;
			emitter.Term = Term;
			emitter.TriggerOffset = TriggerOffset;
		}

		/// <summary>
		/// Initialises the Emitter.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">Thrown if the Term and/or Budget properties have not been set.</exception>
		public virtual void Initialise()
		{
			Particles = new Particle[Budget];
			Idle = 0;
			TotalSeconds = 0f;
			MostRecentTrigger = 0f;
			Initialised = true;
		}

		/// <summary>
		/// Initialises the Emitter.
		/// </summary>
		/// <param name="budget">The number of Particles which are available to the Emitter.</param>
		/// <param name="term">The length of time that released Particles will remain active, in whole and fractional seconds.</param>
		/// <exception cref="T:System.ArgumentException">Thrown if the budget parameter is less than one, or if the term paramter
		/// is not a positive value.</exception>
		public void Initialise(int budget, float term)
		{
			Initialised = false;
			Budget = budget;
			Term = term;
			Initialise();
		}

		/// <summary>
		/// Terminates the emitter immediately.
		/// </summary>
		public void Terminate()
		{
			Idle = 0;
		}

		/// <summary>
		/// Forces the Emitter to execute its next trigger, even if it has a minimum trigger period and is
		/// currently 'cooling down'.
		/// </summary>
		public void ForceNextTrigger()
		{
			MostRecentTrigger = 0f;
		}

		/// <summary>
		/// Loads resources required by the Emitter via a ContentManager.
		/// </summary>
		/// <param name="content">The ContentManager used to load resources.</param>
		/// <exception cref="T:Microsoft.Xna.Framework.Content.ContentLoadException">Thrown if the asset defined
		/// in the ParticleTextureAssetName property could not be loaded.</exception>
		public virtual void LoadContent(ContentManager content)
		{
			if (string.IsNullOrEmpty(ParticleTextureAssetName))
			{
				return;
			}
			try
			{
				if (ParticleTexture == null)
				{
					ParticleTexture = content.Load<Texture2D>(ParticleTextureAssetName);
				}
			}
			catch (ContentLoadException innerException)
			{
				string message = $"Unable to load the specified content item '{ParticleTextureAssetName}'\r\n                                                    Please check the 'ParticleTextureAssetName' property!";
				throw new ContentLoadException(message, innerException);
			}
		}

		/// <summary>
		/// Retires the specified number of Particles.
		/// </summary>
		[Obsolete("Old implementation, may still be faster in some scenarios.")]
		private void RetireParticles(int count)
		{
			Array.Copy(Particles, count, Particles, 0, Idle - count);
			Idle -= count;
		}

		/// <summary>
		/// Retires the specified number of particles from the particle array.
		/// </summary>
		/// <param name="particleArray">A pointer to the first element in an array of particles.</param>
		/// <param name="count">The number of particles to retire.</param>
		private unsafe void RetireParticles(Particle* particleArray, int count)
		{
			Particle* ptr = particleArray + count;
			Particle* ptr2 = particleArray;
			int num = Idle - count;
			for (int i = 0; i < num; i++)
			{
				*ptr2 = *ptr;
				ptr++;
				ptr2++;
			}
			Idle -= count;
		}

		/// <summary>
		/// Updates the Emitter and all Particles within.
		/// </summary>
		/// <param name="deltaSeconds">Elapsed frame time in whole and fractional seconds.</param>
		public unsafe void Update(float deltaSeconds)
		{
			TotalSeconds += deltaSeconds;
			fixed (Particle* particles = Particles)
			{
				int num = Idle;
				Particle* ptr = particles + (num - 1);
				while (--num >= 0)
				{
					float num2 = TotalSeconds - ptr->Inception;
					if (num2 > Term)
					{
						break;
					}
					ptr->Age = num2 / Term;
					ptr->Momentum.X += ptr->Velocity.X;
					ptr->Momentum.Y += ptr->Velocity.Y;
					ptr->Velocity.X = (ptr->Velocity.Y = 0f);
					ptr->Position.X += ptr->Momentum.X * deltaSeconds;
					ptr->Position.Y += ptr->Momentum.Y * deltaSeconds;
					ptr--;
				}
				if (num >= 0)
				{
					RetireParticles(particles, num + 1);
				}
				Modifiers.RunProcessors(deltaSeconds, particles, ActiveParticlesCount);
			}
		}

		/// <summary>
		///  Triggers the Emitter at the specified position...
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">Thrown if the Emitter has not been initialised.</exception>
		public unsafe void Trigger(ref Vector2 triggerPosition)
		{
			if (!Enabled || TotalSeconds - MostRecentTrigger < MinimumTriggerPeriod)
			{
				return;
			}
			Vector2 vector = new Vector2
			{
				X = triggerPosition.X + TriggerOffset.X,
				Y = triggerPosition.Y + TriggerOffset.Y
			};
			int idle = Idle;
			for (int i = idle; i < idle + ReleaseQuantity && i < Budget; i++)
			{
				fixed (Particle* ptr = &Particles[i])
				{
					GenerateOffsetAndForce(out var offset, out var force);
					float num = ReleaseSpeed.Sample();
					ptr->Inception = TotalSeconds;
					ptr->Position.X = vector.X + offset.X;
					ptr->Position.Y = vector.Y + offset.Y;
					ptr->Velocity.X = force.X * num;
					ptr->Velocity.Y = force.Y * num;
					ptr->Momentum = ReleaseImpulse;
					ptr->Age = 0f;
					ptr->Colour = new Vector4(ReleaseColour.Sample(), ReleaseOpacity.Sample());
					ptr->Scale = ReleaseScale.Sample();
					ptr->Rotation = 0f;
					ptr->Rotate(ReleaseRotation.Sample());
				}
				Idle++;
			}
			MostRecentTrigger = TotalSeconds;
		}

		/// <summary>
		///  Triggers the Emitter at the specified position...
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">Thrown if the Emitter has not been initialised.</exception>
		public void Trigger(Vector2 position)
		{
			Trigger(ref position);
		}

		/// <summary>
		/// Generates an offset vector and force vector for a Particle when it is released.
		/// </summary>
		/// <param name="offset">The offset of the Particle from the trigger location.</param>
		/// <param name="force">A unit vector defining the initial force of the Particle.</param>
		protected virtual void GenerateOffsetAndForce(out Vector2 offset, out Vector2 force)
		{
			offset = Vector2.Zero;
			force = RandomHelper.NextUnitVector();
		}
	}
}
