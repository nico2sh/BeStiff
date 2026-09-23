using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Defines an Emitter which releases Particles based on a mask array, typically from an image.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Emitters.MaskEmitterTypeConverter, ProjectMercury.Design")]
	public sealed class MaskEmitter : Emitter
	{
		private byte[][] _mask;

		private float _threshold;

		/// <summary>
		/// Gets or sets the mask array.
		/// </summary>
		/// <value>The mask array.</value>
		public byte[][] Mask
		{
			get
			{
				return _mask;
			}
			set
			{
				_mask = value;
				RecalculateMaskHits();
				Width = Mask.Length;
				Height = Mask[0].Length;
			}
		}

		/// <summary>
		/// Gets or sets the threshold value above which samples in the mask will be used as release points.
		/// </summary>
		/// <value>The threshold value.</value>
		public float Threshold
		{
			get
			{
				return _threshold;
			}
			set
			{
				_threshold = value;
				if (Mask != null)
				{
					RecalculateMaskHits();
				}
			}
		}

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>The width.</value>
		public float Width { get; set; }

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public float Height { get; set; }

		/// <summary>
		/// Gets or sets the content path to the mask texture.
		/// </summary>
		/// <value>The mask texture content path.</value>
		public string MaskTextureContentPath { get; set; }

		private Vector2[] MaskHits { get; set; }

		/// <summary>
		/// Recalculates the points on the mask array which will be used as release points.
		/// </summary>
		private void RecalculateMaskHits()
		{
			int num = Mask.Length;
			int num2 = Mask[0].Length;
			List<Vector2> list = new List<Vector2>();
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					byte b = Mask[i][j];
					if ((float)(int)b / 255f >= Threshold)
					{
						list.Add(new Vector2
						{
							X = (float)i / (float)num - 0.5f,
							Y = (float)j / (float)num2 - 0.5f
						});
					}
				}
			}
			MaskHits = list.ToArray();
		}

		/// <summary>
		/// Applies a mask texture to the MaskEmitter.
		/// </summary>
		/// <param name="maskTexture">A texture reference representing the mask.</param>
		/// <remarks>This method will also change the Width and Height properties to match the dimensions
		/// of the mask texture.</remarks>
		public void ApplyMaskTexture(Texture2D maskTexture)
		{
			byte[][] array = new byte[maskTexture.Width][];
			for (int i = 0; i < maskTexture.Height; i++)
			{
				array[i] = new byte[maskTexture.Height];
			}
			for (int j = 0; j < maskTexture.Width; j++)
			{
				for (int k = 0; k < maskTexture.Height; k++)
				{
					Rectangle value = new Rectangle(j, k, 1, 1);
					Color[] array2 = new Color[1];
					maskTexture.GetData(0, value, array2, 0, 1);
					Color color = array2[0];
					int num = color.R + color.G + color.B;
					array[j][k] = (byte)(num / 3);
				}
			}
			Mask = array;
		}

		/// <summary>
		/// Loads resources required by the Emitter via a ContentManager.
		/// </summary>
		/// <param name="content">The ContentManager used to load resources.</param>
		/// <exception cref="T:Microsoft.Xna.Framework.Content.ContentLoadException">Thrown if the asset defined
		/// in the ParticleTextureAssetName property could not be loaded.</exception>
		public override void LoadContent(ContentManager content)
		{
			base.LoadContent(content);
			if (MaskTextureContentPath != null)
			{
				Texture2D maskTexture = content.Load<Texture2D>(MaskTextureContentPath);
				ApplyMaskTexture(maskTexture);
			}
		}

		/// <summary>
		/// Returns an unitialised deep copy of the Emitter.
		/// </summary>
		/// <returns>A deep copy of the Emitter.</returns>
		public override Emitter DeepCopy()
		{
			MaskEmitter maskEmitter = new MaskEmitter();
			maskEmitter.Mask = (byte[][])Mask.Clone();
			maskEmitter.Threshold = Threshold;
			maskEmitter.Width = Width;
			maskEmitter.Height = Height;
			MaskEmitter maskEmitter2 = maskEmitter;
			CopyBaseFields(maskEmitter2);
			return maskEmitter2;
		}

		/// <summary>
		/// Generates an offset vector and force vector for a Particle when it is released.
		/// </summary>
		/// <param name="offset">The offset of the Particle from the trigger location.</param>
		/// <param name="force">A unit vector defining the initial force of the Particle.</param>
		protected override void GenerateOffsetAndForce(out Vector2 offset, out Vector2 force)
		{
			force = RandomHelper.NextUnitVector();
			offset = RandomHelper.ChooseOne(MaskHits);
			offset.X *= Width;
			offset.Y *= Height;
		}
	}
}
