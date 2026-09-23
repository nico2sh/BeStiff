using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers
{
	/// <summary>
	/// Reference implementation for forces based on AbstractForceController
	/// It supports all features provided by the base class and illustrates proper
	/// usage as an easy to understand example.
	/// As a side-effect it is a nice and easy to use wind force for your projects
	/// </summary>
	public class SimpleWindForce : AbstractForceController
	{
		/// <summary>
		/// Direction of the windforce
		/// </summary>
		public Vector2 Direction { get; set; }

		/// <summary>
		/// The amount of Direction randomization. Allowed range is 0-1.
		/// </summary>
		public float Divergence { get; set; }

		/// <summary>
		/// Ignore the position and apply the force. If off only in the "front" (relative to position and direction)
		/// will be affected
		/// </summary>
		public bool IgnorePosition { get; set; }

		public override void ApplyForce(float dt, float strength)
		{
			foreach (Body body in World.BodyList)
			{
				float decayMultiplier = GetDecayMultiplier(body);
				if (decayMultiplier == 0f)
				{
					continue;
				}
				Vector2 vector;
				if (ForceType == ForceTypes.Point)
				{
					vector = body.Position - base.Position;
				}
				else
				{
					Direction.Normalize();
					vector = Direction;
					if (vector.Length() == 0f)
					{
						vector = new Vector2(0f, 1f);
					}
				}
				if (base.Variation != 0f)
				{
					float num = (float)Randomize.NextDouble() * MathHelper.Clamp(base.Variation, 0f, 1f);
					vector.Normalize();
					body.ApplyForce(vector * strength * decayMultiplier * num);
				}
				else
				{
					vector.Normalize();
					body.ApplyForce(vector * strength * decayMultiplier);
				}
			}
		}
	}
}
