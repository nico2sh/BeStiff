using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Obsolete.
	/// </summary>
	[Obsolete("Replaced by MaskEmitter")]
	[TypeConverter("ProjectMercury.Design.Emitters.TextureEmitterTypeConverter, ProjectMercury.Design")]
	public class TextureEmitter : Emitter
	{
		private Matrix ScaleMatrix;

		private Vector2 TextureOrigin;

		private Vector2[] PixelOffsets;

		private Vector3[] PixelColours;

		private Texture2D _texture;

		/// <summary>
		/// Gets or sets the threshold over which pixels will trigger the release of particles.
		/// </summary>
		public float Threshold;

		/// <summary>
		/// Gets or sets the scale factor of the texture (in screen space).
		/// </summary>
		public float Scale
		{
			get
			{
				return ScaleMatrix.M11;
			}
			set
			{
				ScaleMatrix = Matrix.CreateScale(value);
			}
		}

		/// <summary>
		/// Gets or sets the texture used to lookup particle release offsets.
		/// </summary>
		/// <value>The texture.</value>
		public Texture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				if (Texture != value)
				{
					_texture = value;
					CalculateEmissionPoints();
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether particles should assume the colour of the underlying
		/// pixel in the texture
		/// </summary>
		public bool ApplyPixelColours { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ProjectMercury.Emitters.TextureEmitter" /> class.
		/// </summary>
		public TextureEmitter()
		{
			Scale = 1f;
			PixelOffsets = new Vector2[0];
			Threshold = 0.5f;
		}

		/// <summary>
		/// Calculates the emission points.
		/// </summary>
		private void CalculateEmissionPoints()
		{
			if (Texture == null)
			{
				TextureOrigin = Vector2.Zero;
				Array.Resize(ref PixelOffsets, 0);
				Array.Resize(ref PixelColours, 0);
				return;
			}
			TextureOrigin = new Vector2(Texture.Width / 2, Texture.Height / 2);
			List<Vector2> list = new List<Vector2>();
			List<Vector3> list2 = new List<Vector3>();
			Color[] array = new Color[Texture.Width * Texture.Height];
			Texture.GetData(array);
			int num = 0;
			byte b = Convert.ToByte(Threshold * 255f);
			for (int i = 0; i < Texture.Width; i++)
			{
				for (int j = 0; j < Texture.Height; j++)
				{
					int num2 = Texture.Width * j + i;
					if (array[num2].A >= b)
					{
						list.Add(new Vector2
						{
							X = (float)i - TextureOrigin.X,
							Y = (float)j - TextureOrigin.Y
						});
						list2.Add(array[num2].ToVector3());
						num++;
					}
				}
			}
			PixelOffsets = list.ToArray();
			PixelColours = list2.ToArray();
		}

		/// <summary>
		/// Returns an uninitialised deep copy of the Emitter.
		/// </summary>
		/// <returns>A deep copy of the Emitter.</returns>
		public override Emitter DeepCopy()
		{
			TextureEmitter textureEmitter = new TextureEmitter();
			textureEmitter.ApplyPixelColours = ApplyPixelColours;
			textureEmitter.Scale = Scale;
			textureEmitter.Texture = Texture;
			textureEmitter.Threshold = Threshold;
			Emitter emitter = textureEmitter;
			CopyBaseFields(emitter);
			return emitter;
		}

		/// <summary>
		/// Generates an offset vector and force vector for a Particle when it is released.
		/// </summary>
		/// <param name="offset">The offset of the Particle from the trigger location.</param>
		/// <param name="force">A unit vector defining the initial force of the Particle.</param>
		protected override void GenerateOffsetAndForce(out Vector2 offset, out Vector2 force)
		{
			int num = RandomHelper.NextInt(PixelOffsets.Length);
			offset = PixelOffsets[num];
			offset.X *= Scale;
			offset.Y *= Scale;
			if (ApplyPixelColours)
			{
				ReleaseColour = PixelColours[num];
			}
			force = RandomHelper.NextUnitVector();
		}
	}
}
