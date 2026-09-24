using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;
using ProjectMercury;
using ProjectMercury.Emitters;

namespace Be_Stiff.WorldObjects.Goals
{
	public class Vault : Goal, IOneSidedWorldObject, IShadowCaster
	{
		private GameSprite closedVault;

		private GameSprite openVault;

		private GameSprite openVaultFull;

		private bool projectile;

		private bool setUpAsProjectile;

		private List<WorldObject> safeWorldObjects;

		private bool open;

		private float strength;

		private CircleEmitter emitterVaultOpen;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => !projectile;

		public bool IsProjectile => projectile;

		public override float Mass => mainBody.Mass;

		public override void Update()
		{
			if (setUpAsProjectile)
			{
				setUpAsProjectile = false;
				projectile = true;
			}
			if (!projectile && mainBody.LinearVelocity.Length() > 5f)
			{
				ContactEdge contactEdge = mainBody.ContactList;
				if (contactEdge != null)
				{
					do
					{
						foreach (Fixture fixture in contactEdge.Other.FixtureList)
						{
							if (fixture.UserData is WorldObjectData worldObjectData && worldObjectData.Type != WorldObjectType.CollisionWorldObject && !safeWorldObjects.Contains(worldObjectData.Object))
							{
								safeWorldObjects.Add(worldObjectData.Object);
							}
						}
						contactEdge = contactEdge.Next;
					}
					while (contactEdge != null);
				}
				setUpAsProjectile = true;
			}
			if (projectile && mainBody.LinearVelocity.Length() < 5f)
			{
				projectile = false;
				safeWorldObjects.Clear();
			}
			if (strength <= 0f && !open)
			{
				Open();
				GameElementsControl.Score.AddPoints(Globals.ScoreAchieveVault);
			}
			base.Update();
		}

		protected override void Achieve()
		{
			if (!open)
			{
				Open();
				interactionTime = 0.0;
			}
			else
			{
				GameElementsControl.Score.AddPoints(Globals.ScoreOpenVaultExplosive);
				base.Achieve();
			}
		}

		private void Open()
		{
			if (!open)
			{
				emitterVaultOpen.Radius = ConvertUnits.ToDisplayUnits(1f);
				float num = ConvertUnits.ToDisplayUnits(1f) / emitterVaultOpen.Term;
				emitterVaultOpen.ReleaseSpeed = new VariableFloat
				{
					Value = num,
					Variation = num
				};
				emitterVaultOpen.Trigger(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
				timeForAchieving = 1000.0;
				open = true;
			}
		}

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Vault"))
			{
				return false;
			}
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			base.Name = "Vault-" + GameElementsControl.Counter;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			TheVertices = new Vertices();
			TheVertices.Add(new Vector2(-0.75f, 1f));
			TheVertices.Add(new Vector2(-0.75f, -1f));
			TheVertices.Add(new Vector2(0.75f, -1f));
			TheVertices.Add(new Vector2(0.75f, 1f));
			PolygonShape shape = new PolygonShape(TheVertices, 300f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.CollisionCategories = Category.Cat8;
			fixture.CollidesWith &= ~Category.Cat7;
			fixture.UserData = new WorldObjectData(WorldObjectType.Goal, this);
			sensorFixture = FixtureFactory.AttachRectangle(1f, 1f, 1f, Vector2.Zero, mainBody, new WorldObjectData(WorldObjectType.InteractiveSensor, this));
			sensorFixture.IsSensor = true;
			sensorFixture.CollisionCategories = Category.Cat8;
			sensorFixture.CollidesWith &= ~Category.Cat7;
			GameElementsControl.LoadSprite("vaultClosed", "sprites\\goals\\vaultclosed");
			GameElementsControl.LoadSprite("vaultOpen", "sprites\\goals\\vaultopen");
			GameElementsControl.LoadSprite("vaultOpenFull", "sprites\\goals\\vaultopenfull");
			closedVault = GameElementsControl.GetSprite("vaultClosed");
			openVault = GameElementsControl.GetSprite("vaultOpen");
			openVaultFull = GameElementsControl.GetSprite("vaultOpenFull");
			safeWorldObjects = new List<WorldObject>(8);
			timeForAchieving = 5000.0;
			open = false;
			strength = 400f;
			emitterVaultOpen = (CircleEmitter)GameElementsControl.Particlesmanager.getGrenadeEmitter();
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public override bool Interacts(WorldObjectType type)
		{
			return type == WorldObjectType.Hero;
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			if (hitType == HitType.Explosion)
			{
				strength -= hitDirection.Length() * 2f;
			}
		}

		public bool ConditionalContacts(WorldObject wo)
		{
			if (projectile)
			{
				if (safeWorldObjects.Contains(wo))
				{
					return false;
				}
				return true;
			}
			return true;
		}

		public override void Draw()
		{
			if (achieved)
			{
				openVault.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation);
			}
			else if (open)
			{
				openVaultFull.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation);
			}
			else
			{
				closedVault.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation);
			}
		}

		public void DrawHull()
		{
		}

		public override bool CanBeHooked()
		{
			return false;
		}

		public override bool CanSeeThrough(bool alerted)
		{
			return true;
		}
	}
}
