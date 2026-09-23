using System;
using System.Collections.Generic;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;

namespace Be_Stiff
{
	public class Explosive
	{
		private Explosion explosion;

		private CircleEmitter emitterExplosion;

		private Vector2 explodedPosition;

		private Dictionary<Fixture, List<Vector2>> explodedFixtures;

		public Explosive()
		{
			explosion = new Explosion(GameElementsControl.World);
			explodedFixtures = new Dictionary<Fixture, List<Vector2>>();
			emitterExplosion = (CircleEmitter)GameElementsControl.Particlesmanager.getGrenadeEmitter();
			GameElementsControl.NoiseManager.LoadNoise("noiseExplosion", "boom", "explosion", 500.0, 25f);
		}

		public void Explode(Vector2 position, float radius, float maxForce)
		{
			emitterExplosion.Radius = ConvertUnits.ToDisplayUnits(radius / 4f);
			float num = ConvertUnits.ToDisplayUnits(radius) / emitterExplosion.Term;
			emitterExplosion.ReleaseSpeed = new VariableFloat
			{
				Value = num,
				Variation = num
			};
			emitterExplosion.Trigger(GameElementsControl.ConvertWorldToScreen(position));
			explodedPosition = position;
			GameElementsControl.NoiseManager.AddVisualNoise("noiseExplosion", position);
			explodedFixtures = explosion.Activate(position, radius, maxForce);
			foreach (Fixture key in explodedFixtures.Keys)
			{
				int num2 = Math.Sign(explodedPosition.X - key.Body.Position.X);
				int num3 = Math.Sign(explodedPosition.Y - key.Body.Position.Y);
				if (!(key.UserData is WorldObjectData worldObjectData))
				{
					continue;
				}
				Vector2 zero = Vector2.Zero;
				if (explodedFixtures.TryGetValue(key, out var value))
				{
					foreach (Vector2 item in value)
					{
						zero.X += (float)num2 * Math.Abs(item.X);
						zero.Y += (float)num3 * Math.Abs(item.Y);
					}
				}
				worldObjectData.Object.Hit(zero, Vector2.Zero, HitType.Explosion);
			}
		}

		private void Exploded(Fixture fixture, List<Vector2> impulses)
		{
			int num = Math.Sign(explodedPosition.X - fixture.Body.Position.X);
			int num2 = Math.Sign(explodedPosition.Y - fixture.Body.Position.Y);
			if (!(fixture.UserData is WorldObjectData worldObjectData))
			{
				return;
			}
			Vector2 zero = Vector2.Zero;
			foreach (Vector2 impulse in impulses)
			{
				zero.X += (float)num * Math.Abs(impulse.X);
				zero.Y += (float)num2 * Math.Abs(impulse.Y);
			}
			worldObjectData.Object.Hit(zero, Vector2.Zero, HitType.Explosion);
		}
	}
}
