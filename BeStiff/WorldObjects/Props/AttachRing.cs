using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Be_Stiff.WorldObjects.Props
{
	public class AttachRing : WorldObject, IShadowCaster
	{
		private GameSprite sprite;

		private Body attachedBody;

		private WeldJoint joint;

		public override Body MainBody => mainBody;

		public Vector2 Position
		{
			get
			{
				return mainBody.Position;
			}
			set
			{
				mainBody.Position = value;
			}
		}

		public override void Load()
		{
			base.Name = "AttachRing-" + GameElementsControl.Counter;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Static;
			CircleShape shape = new CircleShape(0.25f, 1f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			GameElementsControl.LoadSprite("attachRing", "sprites\\objects\\attachring");
			sprite = GameElementsControl.GetSprite("attachRing");
			GameElementsControl.AddShadowCasterWorldObject(this);
		}

		public void AttachToWorld()
		{
			attachedBody = BodyFactory.CreateBody(GameElementsControl.World);
			attachedBody.Position = mainBody.Position;
			attachedBody.BodyType = BodyType.Static;
			joint = new WeldJoint(mainBody, attachedBody, Vector2.Zero, Vector2.Zero);
			GameElementsControl.World.AddJoint(joint);
		}

		public void AttachToObject(WorldObject wo)
		{
			mainBody.BodyType = BodyType.Dynamic;
			attachedBody = wo.MainBody;
			joint = new WeldJoint(mainBody, attachedBody, Vector2.Zero, attachedBody.GetLocalPoint(mainBody.Position));
			GameElementsControl.World.AddJoint(joint);
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			switch (hitType)
			{
			case HitType.Pistol:
				Destroy();
				break;
			case HitType.Kick:
				Destroy();
				break;
			}
		}

		private void Destroy()
		{
			mainBody.Enabled = false;
			mainBody.Dispose();
			base.Enabled = false;
		}

		public override void Draw()
		{
			sprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
		}

		public void DrawHull()
		{
		}
	}
}
