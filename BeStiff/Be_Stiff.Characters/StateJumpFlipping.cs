using System;
using Microsoft.Xna.Framework;

namespace Be_Stiff.Characters
{
	internal class StateJumpFlipping : HeroState
	{
		private Side wallJumpSide;

		public StateJumpFlipping(StateMachine sm, Hero h)
			: base(sm, h)
		{
			base.StateID = 8;
		}

		public void SetData(Side side)
		{
			wallJumpSide = side;
		}

		internal override void OnEnter()
		{
			hero.SetFixedSide(wallJumpSide);
			hero.Skeleton.SetAnimation("JUMPCONTINUE");
			hero.BaseAngle = (float)(0 - wallJumpSide) * (float)Math.PI / 2f;
		}

		protected override void During()
		{
			float num = MathHelper.WrapAngle(hero.MainBody.Rotation);
			float num2 = (float)Math.Round(num - MathHelper.WrapAngle(hero.BaseAngle), 1);
			if (num2 == 0f)
			{
				int num3 = Math.Sign(hero.MainBody.LinearVelocity.X);
				hero.BaseAngle += (float)num3 * (float)Math.PI / 2f;
			}
		}

		protected override void CheckRules()
		{
			if (Math.Round(MathHelper.WrapAngle(hero.BaseAngle), 1) == 0.0)
			{
				hero.BaseAngle = 0f;
				SwitchState(5, useExitTransition: true);
			}
		}

		internal override void OnExit()
		{
			hero.UnsetFixedSide();
		}
	}
}
