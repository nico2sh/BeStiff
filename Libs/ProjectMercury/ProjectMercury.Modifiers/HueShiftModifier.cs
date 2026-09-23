using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which adjusts the hue of a Particles colour over time.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.HueShiftModifierTypeConverter, ProjectMercury.Design")]
	public sealed class HueShiftModifier : Modifier
	{
		/// <summary>
		/// The transformation matrices which convert from RGB to YIQ and back.
		/// </summary>
		private static Matrix YIQTransformMatrix;

		/// <summary>
		/// The transformation matrices which convert from RGB to YIQ and back.
		/// </summary>
		private static Matrix RGBTransformMatrix;

		/// <summary>
		/// The amount to adjust the hue in degrees per second.
		/// </summary>
		public float HueShift;

		/// <summary>
		/// Initializes the <see cref="T:ProjectMercury.Modifiers.HueShiftModifier" /> class.
		/// </summary>
		static HueShiftModifier()
		{
			YIQTransformMatrix = new Matrix(0.299f, 0.587f, 0.114f, 0f, 0.596f, -0.274f, -0.321f, 0f, 0.211f, -0.523f, 0.311f, 0f, 0f, 0f, 0f, 1f);
			Matrix.Invert(ref YIQTransformMatrix, out RGBTransformMatrix);
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			HueShiftModifier hueShiftModifier = new HueShiftModifier();
			hueShiftModifier.HueShift = HueShift;
			return hueShiftModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			float value = HueShift * dt * 3.141593f / 180f;
			float num = Calculator.Cos(value);
			float num2 = Calculator.Sin(value);
			Matrix matrix = new Matrix(1f, 0f, 0f, 0f, 0f, num, 0f - num2, 0f, 0f, num2, num, 0f, 0f, 0f, 0f, 1f);
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				Vector4.Transform(ref ptr->Colour, ref YIQTransformMatrix, out var result);
				Vector4.Transform(ref result, ref matrix, out result);
				Vector4.Transform(ref result, ref RGBTransformMatrix, out result);
				ptr->Colour.X = result.X;
				ptr->Colour.Y = result.Y;
				ptr->Colour.Z = result.Z;
			}
		}
	}
}
