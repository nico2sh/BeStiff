using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff
{
	public class GirderSmall : WorldObject, IShadowCaster
	{
		private const float width = 6f;

		private const float height = 1f;

		private GameSprite sprite;

		private GameSprite spriteRope;

		private GameSpriteVariables spriteVariables;

		private GameSpriteVariables spriteRopeVariables1;

		private GameSpriteVariables spriteRopeVariables2;

		private RopeJoint joint1;

		private RopeJoint joint2;

		private bool joint1Enabled;

		private bool joint2Enabled;

		private AttachRing[] attachRings;

		private int numberOfRings;

		public override Body MainBody => mainBody;

		public ShadowHull[] shadowHull { get; set; }

		public override float Mass => mainBody.Mass;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("GirderSmall"))
			{
				return false;
			}
			base.Name = obj.GetValue<OgmoStringValue>("ID").Value;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			// The body follows the art: the shipped girder sprite is 144x24, the
			// original code hard-coded a 6x1 sim-unit body (288x48 px).
			GameElementsControl.LoadSprite("girderSmall", "sprites\\objects\\girdersmall");
			sprite = GameElementsControl.GetSprite("girderSmall");
			float hw = ConvertUnits.ToSimUnits(sprite.Width / 2f);
			float hh = ConvertUnits.ToSimUnits(sprite.Height / 2f);
			string value = obj.GetValue<OgmoStringValue>("Object").Value;
			if (value != "")
			{
				WorldObject worldObjectByName = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName == null)
				{
					return false;
				}
			}
			shadowHull = new ShadowHull[1];
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(-hw, -hh));
			vertices.Add(new Vector2(hw, -hh));
			vertices.Add(new Vector2(hw, hh));
			vertices.Add(new Vector2(-hw, hh));
			PolygonShape shape = new PolygonShape(vertices, 200f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.Restitution = 0.1f;
			fixture.Friction = 1f;
			fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			spriteVariables = GameSprite.GetDefaultVariables();
			GameElementsControl.LoadSprite("rope", "sprites\\objects\\rope");
			spriteRope = GameElementsControl.GetSprite("rope");
			spriteRopeVariables1 = GameSprite.GetDefaultVariables();
			spriteRopeVariables2 = GameSprite.GetDefaultVariables();
			Vector2[] points = new Vector2[4]
			{
				ConvertUnits.ToDisplayUnits(-hw, -hh),
				ConvertUnits.ToDisplayUnits(hw, -hh),
				ConvertUnits.ToDisplayUnits(hw, hh),
				ConvertUnits.ToDisplayUnits(-hw, hh)
			};
			shadowHull[0] = ShadowHull.CreateConvex(ref points);
			shadowHull[0].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			if (value == "")
			{
				bool value2 = obj.GetValue<OgmoBooleanValue>("DoubleRope").Value;
				float value3 = obj.GetValue<OgmoNumberValue>("PositionY").Value;
				if (value2)
				{
					attachRings = new AttachRing[2];
					numberOfRings = 2;
					attachRings[0] = new AttachRing();
					attachRings[0].Load();
					attachRings[0].Position = new Vector2(mainBody.Position.X - hw, ConvertUnits.ToSimUnits(value3));
					attachRings[1] = new AttachRing();
					attachRings[1].Load();
					attachRings[1].Position = new Vector2(mainBody.Position.X + hw, ConvertUnits.ToSimUnits(value3));
					joint1 = new RopeJoint(mainBody, attachRings[0].MainBody, new Vector2(-hw, -hh), Vector2.Zero);
					GameElementsControl.World.AddJoint(joint1);
					joint2 = new RopeJoint(mainBody, attachRings[1].MainBody, new Vector2(hw, -hh), Vector2.Zero);
					GameElementsControl.World.AddJoint(joint2);
				}
				else
				{
					attachRings = new AttachRing[1];
					numberOfRings = 1;
					attachRings[0] = new AttachRing();
					attachRings[0].Load();
					attachRings[0].Position = new Vector2(mainBody.Position.X, ConvertUnits.ToSimUnits(value3));
					joint1 = new RopeJoint(mainBody, attachRings[0].MainBody, new Vector2(-hw, -hh), Vector2.Zero);
					GameElementsControl.World.AddJoint(joint1);
					joint2 = new RopeJoint(mainBody, attachRings[0].MainBody, new Vector2(hw, -hh), Vector2.Zero);
					GameElementsControl.World.AddJoint(joint2);
				}
			}
			else
			{
				WorldObject worldObjectByName2 = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName2 == null)
				{
					return false;
				}
				bool value4 = obj.GetValue<OgmoBooleanValue>("DoubleRope").Value;
				float value5 = obj.GetValue<OgmoNumberValue>("PositionY").Value;
				if (value4)
				{
					attachRings = new AttachRing[2];
					numberOfRings = 2;
					attachRings[0] = new AttachRing();
					attachRings[0].Load();
					attachRings[0].Position = new Vector2(mainBody.Position.X - hw, ConvertUnits.ToSimUnits(value5));
					attachRings[0].AttachToObject(worldObjectByName2);
					attachRings[1] = new AttachRing();
					attachRings[1].Load();
					attachRings[1].Position = new Vector2(mainBody.Position.X + hw, ConvertUnits.ToSimUnits(value5));
					attachRings[1].AttachToObject(worldObjectByName2);
					joint1 = new RopeJoint(mainBody, worldObjectByName2.MainBody, new Vector2(-hw, -hh), worldObjectByName2.MainBody.GetLocalPoint(attachRings[0].Position));
					GameElementsControl.World.AddJoint(joint1);
					joint2 = new RopeJoint(mainBody, worldObjectByName2.MainBody, new Vector2(hw, -hh), worldObjectByName2.MainBody.GetLocalPoint(attachRings[1].Position));
					GameElementsControl.World.AddJoint(joint2);
				}
				else
				{
					attachRings = new AttachRing[1];
					numberOfRings = 1;
					attachRings[0] = new AttachRing();
					attachRings[0].Load();
					attachRings[0].Position = new Vector2(mainBody.Position.X, ConvertUnits.ToSimUnits(value5));
					attachRings[0].AttachToObject(worldObjectByName2);
					joint1 = new RopeJoint(mainBody, worldObjectByName2.MainBody, new Vector2(-hw, -hh), worldObjectByName2.MainBody.GetLocalPoint(attachRings[0].Position));
					GameElementsControl.World.AddJoint(joint1);
					joint2 = new RopeJoint(mainBody, worldObjectByName2.MainBody, new Vector2(hw, -hh), worldObjectByName2.MainBody.GetLocalPoint(attachRings[0].Position));
					GameElementsControl.World.AddJoint(joint2);
				}
			}
			joint1Enabled = true;
			joint2Enabled = true;
			GameElementsControl.NoiseManager.LoadNoise("noiseGirder", "clonk", "girder", 300.0, 10f);
			GameElementsControl.Krypton.Hulls.Add(shadowHull[0]);
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			if (wod.Type == WorldObjectType.CollisionWorldObject && CalculateRelativeSpeedFromContact(ref contact, ref oldManifold) > 1f)
			{
				GameElementsControl.NoiseManager.AddVisualNoise("noiseGirder", mainBody.Position);
			}
		}

		public override bool CanHanged()
		{
			return true;
		}

		/// <summary>Debug: rope joint state.</summary>
		public string DebugRopes()
		{
			string R(FarseerPhysics.Dynamics.Joints.RopeJoint j) => j == null ? "null" : $"max={j.MaxLength:F2} len={(j.WorldAnchorB - j.WorldAnchorA).Length():F2} enabled={j.Enabled} inWorld={GameElementsControl.World.JointList.Contains(j)} ringType={j.BodyB.BodyType} ringEnabled={j.BodyB.Enabled} girderType={j.BodyA.BodyType} girderMass={j.BodyA.Mass:F0} inertia={j.BodyA.Inertia:F0} awakeA={j.BodyA.Awake} sleepAllowed={j.BodyA.SleepingAllowed} ignoreG={j.BodyA.IgnoreGravity}";
			int edges = 0;
			for (var e = mainBody.JointList; e != null; e = e.Next) edges++;
			return $"edges={edges} j1[" + R(joint1) + "] j2[" + R(joint2) + "]";
		}

		public override void Update()
		{
			spriteVariables.rotation = 0f - mainBody.Rotation;
			shadowHull[0].Angle = 0f - mainBody.Rotation;
			shadowHull[0].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			Vector2 v = GameElementsControl.ConvertWorldToScreen(joint1.WorldAnchorB - joint1.WorldAnchorA);
			Vector2 v2 = GameElementsControl.ConvertWorldToScreen(joint2.WorldAnchorB - joint2.WorldAnchorA);
			// Ropes run from the girder corners to their anchor rings; the
			// screen-space direction maps directly onto SpriteBatch rotation.
			spriteRopeVariables1.scale = new Vector2(v.Length(), 1f);
			spriteRopeVariables1.rotation = v.GetAngle();
			spriteRopeVariables2.scale = new Vector2(v2.Length(), 1f);
			spriteRopeVariables2.rotation = v2.GetAngle();
			if (!attachRings[0].MainBody.Enabled)
			{
				System.Console.Error.WriteLine("GIRDER: ring 0 disabled, removing rope joint(s)");
				joint1Enabled = false;
				GameElementsControl.World.RemoveJoint(joint1);
				if (numberOfRings == 1)
				{
					joint2Enabled = false;
					GameElementsControl.World.RemoveJoint(joint2);
				}
			}
			if (numberOfRings == 2 && !attachRings[1].MainBody.Enabled)
			{
				joint2Enabled = false;
				GameElementsControl.World.RemoveJoint(joint2);
			}
		}

		public override void Draw()
		{
		}

		public void DrawHull()
		{
			if (joint1Enabled)
			{
				spriteRope.Draw(GameElementsControl.ConvertWorldToScreen(joint1.WorldAnchorA), spriteRopeVariables1);
			}
			if (joint2Enabled)
			{
				spriteRope.Draw(GameElementsControl.ConvertWorldToScreen(joint2.WorldAnchorA), spriteRopeVariables2);
			}
			sprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), spriteVariables);
		}
	}
}
