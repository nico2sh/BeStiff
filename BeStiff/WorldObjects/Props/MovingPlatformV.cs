using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.WorldObjects.Props
{
	/// <summary>Vertical moving platform (a moving wall the hero can slide on).</summary>
	internal class MovingPlatformV : MovingPlatform
	{
		protected override string ObjectName => "MovingPlatformV";

		protected override string SpriteSuffix => "V";

		protected override SpriteEffects FarBorderFlip => SpriteEffects.FlipVertically;

		private float Height => size.Y;

		protected override Vector2 MeasureSize(WorldScaledOgmoObject obj)
		{
			return new Vector2(FloorThick, obj.Height);
		}

		protected override Vector2[] HullPoints()
		{
			return new Vector2[4]
			{
				ConvertUnits.ToDisplayUnits(FloorThick, 0f),
				ConvertUnits.ToDisplayUnits(FloorThick, Height),
				ConvertUnits.ToDisplayUnits(0f, Height),
				ConvertUnits.ToDisplayUnits(0f, 0f)
			};
		}

		protected override void DecorOffsets(out Vector2 pivot1, out Vector2 pivot2, out Vector2 border1, out Vector2 border2)
		{
			pivot1 = new Vector2(0.25f, 0.25f);
			pivot2 = new Vector2(0.25f, Height - 0.25f);
			border1 = new Vector2(0.25f, 0f);
			border2 = new Vector2(0.25f, Height);
		}

		public override bool CanWallSlide(Side side, ref float posX, ref Fixture wsFixture)
		{
			posX = side == Side.Left ? mainBody.Position.X + FloorThick : mainBody.Position.X;
			wsFixture = fixture;
			return true;
		}
	}
}
