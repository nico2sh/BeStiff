using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Krypton;
using Krypton.Lights;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Lamp : WorldObject, IShadowCaster
	{
		private Light2D light;

		private GameSprite sprite;

		private GameSpriteVariables spriteVariables;

		private Body attachedBody;

		private WeldJoint joint;

		public override Body MainBody => mainBody;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Lamp"))
			{
				return false;
			}
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			base.Name = "Lamp-" + GameElementsControl.Counter;
			string value = obj.GetValue<OgmoStringValue>("Object").Value;
			if (value != "")
			{
				WorldObject worldObjectByName = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName == null)
				{
					return false;
				}
			}
			float num = 0f - MathHelper.ToRadians(obj.Rotation);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.Position = worldScaledOgmoObject.Position;
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Rotation = num;
			Fixture fixture = FixtureFactory.AttachRectangle(0.1f, 0.1f, 1f, Vector2.Zero, mainBody);
			fixture.CollidesWith = Category.None;
			fixture.CollisionCategories = Category.None;
			if (value == "")
			{
				attachedBody = BodyFactory.CreateBody(GameElementsControl.World);
				attachedBody.Position = mainBody.Position;
				attachedBody.BodyType = BodyType.Static;
				joint = new WeldJoint(mainBody, attachedBody, Vector2.Zero, Vector2.Zero);
				GameElementsControl.World.AddJoint(joint);
			}
			else
			{
				WorldObject worldObjectByName2 = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName2 == null)
				{
					return false;
				}
				attachedBody = worldObjectByName2.MainBody;
				joint = new WeldJoint(mainBody, attachedBody, Vector2.Zero, attachedBody.GetLocalPoint(mainBody.GetWorldPoint(Vector2.Zero)));
				GameElementsControl.World.AddJoint(joint);
			}
			GameElementsControl.LoadSprite("lamp", "sprites\\objects\\lamp");
			sprite = GameElementsControl.GetSprite("lamp");
			spriteVariables = GameSprite.GetDefaultVariables();
			spriteVariables.rotation = num;
			int value2 = obj.GetValue<OgmoIntegerValue>("R").Value;
			int value3 = obj.GetValue<OgmoIntegerValue>("G").Value;
			int value4 = obj.GetValue<OgmoIntegerValue>("B").Value;
			light = new Light2D();
			light.Texture = LightTextureBuilder.CreatePointLight(GameElementsControl.ScreenManager.GraphicsDevice, 640);
			light.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			light.Color = new Color(value2, value3, value4);
			light.Angle = num + (float)Math.PI / 2f;
			light.Fov = (float)Math.PI * 3f / 8f;
			light.Range = 640f;
			GameElementsControl.Krypton.Lights.Add(light);
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override void Update()
		{
			spriteVariables.rotation = 0f - mainBody.Rotation;
			light.Angle = 0f - mainBody.Rotation + (float)Math.PI / 2f;
			light.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
		}

		public override void Draw()
		{
		}

		public void DrawHull()
		{
			sprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), spriteVariables);
		}
	}
}
