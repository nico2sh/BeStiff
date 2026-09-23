using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class HookLink
	{
		private GameSprite linkSprite;

		private Body body;

		private Fixture fixture;

		private Hook mainHook;

		private bool enabled;

		public float Mass => body.Mass;

		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (!value)
				{
					body.Enabled = false;
				}
				else
				{
					body.Enabled = true;
				}
				enabled = value;
			}
		}

		public Body Body
		{
			get
			{
				return body;
			}
			set
			{
				body = value;
			}
		}

		public Fixture Fixture
		{
			get
			{
				return fixture;
			}
			set
			{
				fixture = value;
			}
		}

		public HookLink()
		{
			enabled = false;
		}

		public void Load(Hook hook)
		{
			mainHook = hook;
			body = BodyFactory.CreateBody(GameElementsControl.World);
			body.BodyType = BodyType.Dynamic;
			PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(Hook.MaxSpaceBetweenLinks / 2f, Hook.LinkWidth), 20f);
			fixture = body.CreateFixture(shape);
			fixture.CollisionCategories = Category.Cat3;
			fixture.CollisionCategories |= Category.Cat7;
			fixture.CollidesWith &= ~Category.Cat3;
			fixture.CollidesWith &= ~Category.Cat8;
			fixture.CollisionGroup = -1;
			body.Enabled = false;
			linkSprite = GameElementsControl.GetSprite("hookLink");
		}

		public void Attach()
		{
			body.LinearVelocity = Vector2.Zero;
		}

		public void Detach()
		{
		}

		public void Deactivate()
		{
			body.Enabled = false;
		}

		public void Dissapear()
		{
		}

		public void AddToChain(Vector2 worldPosition, float angle)
		{
			Enabled = true;
			body.LinearVelocity = Vector2.Zero;
			body.Rotation = angle;
			body.Position = worldPosition;
		}

		private void ChainBroke(object sender, EventArgs e)
		{
			mainHook.Break();
		}

		public void Draw()
		{
			linkSprite.Draw(GameElementsControl.ConvertWorldToScreen(Body.Position));
		}
	}
}
