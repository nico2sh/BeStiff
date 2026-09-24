using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Weapons
{
	public class Hook : Weapon, IWeaponHero
	{
		private const double maxAttachTime = 5000.0;

		private const double coolDownDetachTime = 1000.0;

		private const float cannonLenght = 33f;

		private const int maxLinks = 15;

		private GameSprite hookSprite;

		private GameSpriteVariables hookSpriteVariables;

		private GameSprite targetSprite;

		private GameSprite noTargetSprite;

		private float targetSpriteRotation;

		private GameSprite spriteRope;

		private Grapple grapple;

		private HookLink[] linkPool;

		private RevoluteJoint[] jointPool;

		private RevoluteJoint grappleLinkJoint;

		private DistanceJoint heroChainJoint;

		private Vector2 anchor;

		private int activeLinks;

		protected GameSprite uiSprite;

		private RevoluteJoint grappleJoint;

		private RopeJoint connectRope;

		private bool active;

		private bool broken;

		private double breakTime;

		private float lastLinkSpace;

		private bool stiffed;

		private double attachTime;

		private double detachTime;

		private static float maxSpaceBetweenLinks = 0.4f;

		private static float linkWidth = 0.05f;

		private Hero owner;

		private bool attached;

		private bool retracted;

		private bool hasTarget;

		private bool hadTarget;

		private Vector2 targetPosition;

		private Vector2 modifiedTargetPosition;

		private float gunRange;

		private List<Fixture> candidateFixtures;

		private WorldObject targetObject;

		public bool Attached => grapple.Hooked;

		public bool Retracted => retracted;

		public static float MaxSpaceBetweenLinks => maxSpaceBetweenLinks;

		public static float LinkWidth => linkWidth;

		public GameSprite UISprite => uiSprite;

		public int InfoNumber => (int)MathHelper.Clamp((float)((GameElementsControl.CurrentTimeInMS - detachTime) / 1000.0) * 100f, 0f, 100f);

		public int SecondaryInfoNumber
		{
			get
			{
				if (attached)
				{
					return 5 - (int)((GameElementsControl.CurrentTimeInMS - attachTime) / 1000.0);
				}
				return 0;
			}
		}

		public Hook(Arm arm, Hero hero)
			: base(arm)
		{
			collectHookableFixture = CollectHookableFixture;
			base.WeaponType = WeaponType.Hook;
			owner = hero;
		}

		public void Init()
		{
			active = false;
			broken = false;
			retracted = false;
			activeLinks = 0;
			attachTime = 0.0;
			detachTime = 0.0;
		}

		public override void Load()
		{
			GameElementsControl.LoadSprite("hook", "sprites\\guns\\hook\\hook");
			GameElementsControl.LoadSprite("rope", "sprites\\objects\\rope");
			GameElementsControl.LoadSprite("hookUI", "sprites\\ui\\hookui");
			GameElementsControl.LoadSprite("target", "sprites\\guns\\hook\\target");
			GameElementsControl.LoadSprite("noTarget", "sprites\\guns\\hook\\notarget");
			GameElementsControl.LoadSprite("hookLink", "sprites\\guns\\hook\\hooklink");
			hookSprite = GameElementsControl.GetSprite("hook");
			spriteRope = GameElementsControl.GetSprite("rope");
			uiSprite = GameElementsControl.GetSprite("hookUI");
			targetSprite = GameElementsControl.GetSprite("target");
			noTargetSprite = GameElementsControl.GetSprite("noTarget");
			targetSpriteRotation = 0f;
			hookSpriteVariables = GameSprite.GetDefaultVariables();
			GameElementsControl.ScreenManager.AudioManager.LoadSound("launchHook", "audio\\noises\\launchHook");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("hookAttach", "audio\\noises\\hookAttach");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("hookRetract", "audio\\noises\\hookRetract");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("hookSteam", "audio\\noises\\hooksteam");
			linkPool = new HookLink[15];
			jointPool = new RevoluteJoint[14];
			grapple = new Grapple();
			grapple.Load(this);
			HookLink hookLink = null;
			anchor = new Vector2(MaxSpaceBetweenLinks / 2f, 0f);
			for (int i = 0; i < 15; i++)
			{
				HookLink hookLink2 = new HookLink();
				hookLink2.Load(this);
				linkPool[i] = hookLink2;
				if (hookLink != null)
				{
					RevoluteJoint revoluteJoint = new RevoluteJoint(hookLink.Body, hookLink2.Body, -anchor, anchor);
					jointPool[i - 1] = revoluteJoint;
				}
				else
				{
					grappleLinkJoint = new RevoluteJoint(hookLink2.Body, grapple.MainBody, anchor, Vector2.Zero);
					grappleLinkJoint.CollideConnected = false;
				}
				hookLink = hookLink2;
			}
			heroChainJoint = new DistanceJoint(hookLink.Body, owner.MainBody, Vector2.Zero, owner.MainBody.GetLocalPoint(ownerArm.Position));
			grappleJoint = new RevoluteJoint(linkPool.First().Body, linkPool.Last().Body, Vector2.Zero, Vector2.Zero);
			grappleJoint.CollideConnected = false;
			connectRope = new RopeJoint(grapple.MainBody, owner.MainBody, Vector2.Zero, Vector2.Zero);
			connectRope.CollideConnected = false;
			stiffed = false;
			gunRange = ConvertUnits.ToSimUnits(33f) + maxSpaceBetweenLinks * 16f;
			targetPosition = Vector2.Zero;
			hasTarget = false;
			hadTarget = false;
			candidateFixtures = new List<Fixture>();
		}

		// Cached so the per-frame aim query doesn't allocate a new delegate.
		private readonly Func<Fixture, bool> collectHookableFixture;

		private bool CollectHookableFixture(Fixture fixture)
		{
			if (fixture.UserData is WorldObjectData worldObjectData && worldObjectData.Object.Name != "Hero" && worldObjectData.Object.CanBeHooked() && !fixture.IsSensor)
			{
				candidateFixtures.Add(fixture);
			}
			return true;
		}

		public void HandleInput(InputHelper input, PlayerIndex? playerIndex)
		{
			if (input.ControlSecondaryShoot(playerIndex))
			{
				SecondaryShoot(activate: true);
			}
			else
			{
				SecondaryShoot(activate: false);
			}
			if (input.ControlShoot(playerIndex))
			{
				Shoot();
			}
			if (Attached && input.ControlJump(playerIndex))
			{
				Shoot();
			}
		}

		public override bool ManualAim(out float angle)
		{
			angle = 0f;
			if (hasTarget || active)
			{
				if (active)
				{
					if (!attached)
					{
						Vector2 v = grapple.MainBody.Position - ownerArm.Position;
						angle = MathHelper.WrapAngle(v.GetAngle());
					}
					else if ((ownerArm.Position - grapple.MainBody.Position).Length() > 1f)
					{
						if (activeLinks > 0)
						{
							angle = (linkPool[activeLinks - 1].Body.Position - ownerArm.Position).GetAngle();
						}
						else
						{
							angle = (grapple.MainBody.Position - ownerArm.Position).GetAngle();
						}
					}
				}
				else
				{
					Vector2 v2 = modifiedTargetPosition - ownerArm.Position;
					if (!(v2.Length() > 1.5f))
					{
						return true;
					}
					angle = MathHelper.WrapAngle(v2.GetAngle());
				}
			}
			if (!hasTarget)
			{
				return !active;
			}
			return false;
		}

		public void Break()
		{
			if (!broken)
			{
				broken = true;
				breakTime = GameElementsControl.CurrentTimeInMS;
				for (int i = 0; i < activeLinks; i++)
				{
					linkPool[i].Deactivate();
				}
				grapple.Detach();
			}
		}

		public void Deactivate()
		{
			active = false;
			broken = false;
			retracted = false;
			LooseRope();
			for (int i = 0; i < activeLinks; i++)
			{
				linkPool[i].Dissapear();
				linkPool[i].Enabled = false;
			}
			for (int j = 0; j < activeLinks - 1; j++)
			{
				GameElementsControl.World.RemoveJoint(jointPool[j]);
			}
			activeLinks = 0;
			GameElementsControl.World.RemoveJoint(grappleJoint);
			GameElementsControl.World.RemoveJoint(heroChainJoint);
			GameElementsControl.World.RemoveJoint(grappleLinkJoint);
			grapple.Dissapear();
			grapple.IsEnabled = false;
			if (attached)
			{
				detachTime = GameElementsControl.CurrentTimeInMS;
				grapple.Detach();
				GameElementsControl.NoiseManager.AddNoise("hookSteam", ownerArm.Position);
				attached = false;
			}
			owner.HookAttached = false;
		}

		public override void UpdateDrawSide(Side side)
		{
			if (side == Side.Left)
			{
				hookSpriteVariables.spriteEffects = SpriteEffects.FlipVertically;
			}
			else
			{
				hookSpriteVariables.spriteEffects = SpriteEffects.None;
			}
			hookSpriteVariables.rotation = ownerArm.GetRotation();
		}

		public override void Holster()
		{
			if (active)
			{
				Deactivate();
			}
		}

		public override void SetArmAnimation()
		{
			ownerArm.IgnoreAnimation(value: true);
		}

		public override void Shoot()
		{
			if (active)
			{
				Deactivate();
			}
			else if (detachTime + 1000.0 < GameElementsControl.CurrentTimeInMS)
			{
				float rotation = ((!hasTarget) ? ownerArm.GetRotation() : (modifiedTargetPosition - ownerArm.Position).GetAngle());
				if (CheckCleanShot())
				{
					grapple.Shoot(ownerArm.CannonPosition(33f), rotation, owner.MainBody.LinearVelocity);
				}
				else
				{
					grapple.Shoot(ownerArm.CannonPosition(1f), rotation, owner.MainBody.LinearVelocity);
				}
				active = true;
				GameElementsControl.NoiseManager.AddNoise("launchHook", ownerArm.Position);
			}
		}

		private bool CheckCleanShot()
		{
			Vector2 po;
			return !RayCastCallBacks.RayCastPistolCrossHair(ownerArm.Position, ownerArm.CannonPosition(33f), owner.Name, out po);
		}

		public override StateEnum GetNewState(StateEnum curState)
		{
			if (attached)
			{
				if (broken)
				{
					return StateEnum.Jumping;
				}
				return StateEnum.Swinging;
			}
			return StateEnum.Jumping;
		}

		public override void SecondaryShoot(bool activate)
		{
			if (activate && !retracted && attached)
			{
				for (int i = 0; i < activeLinks; i++)
				{
					linkPool[i].Dissapear();
					linkPool[i].Enabled = false;
				}
				for (int j = 0; j < activeLinks - 1; j++)
				{
					GameElementsControl.World.RemoveJoint(jointPool[j]);
				}
				GameElementsControl.World.RemoveJoint(heroChainJoint);
				connectRope.MaxLength = ConvertUnits.ToSimUnits(33f);
				activeLinks = 0;
				GameElementsControl.NoiseManager.AddNoise("hookRetract", ownerArm.Position);
				retracted = true;
			}
		}

		public override void Update()
		{
			if (active)
			{
				grapple.Update();
				if (!attached)
				{
					if (grapple.Hooked)
					{
						attached = true;
						Attach();
						for (int i = 0; i < activeLinks; i++)
						{
							linkPool[i].Attach();
						}
						return;
					}
					Vector2 zero = Vector2.Zero;
					if (activeLinks > 0)
					{
						HookLink hookLink = linkPool[activeLinks - 1];
						zero = hookLink.Body.Position;
					}
					else
					{
						zero = grapple.GetWorldAnchorPoint();
					}
					Vector2 v = zero - ownerArm.CannonPosition(33f);
					float num = v.Length();
					if (num > maxSpaceBetweenLinks && !grapple.Hooked)
					{
						float angle = v.GetAngle();
						int num2 = (int)(num / maxSpaceBetweenLinks) - 1;
						int num3 = 0;
						Vector2 newLastLinkEdge;
						while (AddLink(angle, zero, out newLastLinkEdge) && num3 < num2)
						{
							zero = newLastLinkEdge;
							num3++;
						}
					}
				}
				else if (attachTime + 5000.0 < GameElementsControl.CurrentTimeInMS)
				{
					Deactivate();
				}
				else if (broken && GameElementsControl.CurrentTimeInMS - breakTime > 1000.0)
				{
					Deactivate();
				}
			}
			else
			{
				SetCrossHairPosition();
				targetSpriteRotation += (float)GameElementsControl.LastFrameTimeInMS * (float)Math.PI / 500f;
			}
		}

		private void SetCrossHairPosition()
		{
			Vector2 value = ((!(GameElementsControl.Input.CursorPosition == Vector2.Zero)) ? GameElementsControl.Input.CursorPosition : new Vector2((float)Math.Cos(ownerArm.GetRotation()), (float)Math.Sin(ownerArm.GetRotation())));
			Vector2 vector = Vector2.Normalize(value) * gunRange;
			Vector2 point = ownerArm.Position + vector;
			hasTarget = false;
			if (RayCastCallBacks.RayCastHookCrossHair(ownerArm.Position, point, owner.Name, out targetObject, out targetPosition))
			{
				if (targetObject.CanBeHooked())
				{
					hasTarget = true;
				}
			}
			else
			{
				targetPosition = point;
			}
			if (hasTarget)
			{
				AABB aabb = new AABB(targetPosition, 0.5f, 0.5f);
				candidateFixtures.Clear();
				GameElementsControl.World.QueryAABB(collectHookableFixture, ref aabb);
				int num = Math.Sign(targetPosition.X - ownerArm.Position.X);
				Vector2 vector2 = new Vector2(0f, float.MaxValue);
				bool flag = false;
				foreach (Fixture candidateFixture in candidateFixtures)
				{
					if (!(candidateFixture.Shape is PolygonShape polygonShape))
					{
						continue;
					}
					foreach (Vector2 vertex in polygonShape.Vertices)
					{
						Vector2 worldPoint = candidateFixture.Body.GetWorldPoint(vertex);
						if ((worldPoint - targetPosition).Length() < 1f && worldPoint.Y < vector2.Y && Math.Sign(worldPoint.X - ownerArm.Position.X) == num)
						{
							vector2 = worldPoint;
							flag = true;
						}
					}
				}
				if (flag)
				{
					modifiedTargetPosition = vector2;
					hadTarget = true;
				}
				else
				{
					modifiedTargetPosition = targetPosition;
				}
			}
			else if (!CheckModifiedAgainstTarget())
			{
				modifiedTargetPosition = targetPosition;
				hadTarget = false;
			}
			else if (hadTarget)
			{
				hasTarget = true;
			}
		}

		private bool CheckModifiedAgainstTarget()
		{
			Vector2 vector = modifiedTargetPosition - ownerArm.Position;
			Vector2 vector2 = Vector2.Normalize(targetPosition - ownerArm.Position) * vector.Length();
			if ((vector - vector2).Length() < 1f)
			{
				return true;
			}
			return false;
		}

		private void AttachLink()
		{
			RevoluteJoint joint = jointPool[activeLinks - 1];
			GameElementsControl.World.AddJoint(joint);
		}

		private void AttachFirstLink()
		{
			GameElementsControl.World.AddJoint(grappleLinkJoint);
		}

		private void AttachLastLink()
		{
			Body body = ((activeLinks <= 0) ? grapple.MainBody : linkPool[activeLinks - 1].Body);
			heroChainJoint = new DistanceJoint(body, owner.MainBody, -anchor, owner.MainBody.GetLocalPoint(ownerArm.Position));
			lastLinkSpace = MathHelper.Clamp((body.GetWorldPoint(-anchor) - ownerArm.Position).Length(), 0f, maxSpaceBetweenLinks + ownerArm.GetLength());
			heroChainJoint.Length = lastLinkSpace;
			GameElementsControl.World.AddJoint(heroChainJoint);
		}

		private void Attach()
		{
			grappleJoint.BodyA = grapple.MainBody;
			grappleJoint.BodyB = grapple.AttachBody;
			Vector2 position = grapple.MainBody.Position;
			grappleJoint.LocalAnchorA = grapple.MainBody.GetLocalPoint(position);
			grappleJoint.LocalAnchorB = grapple.AttachBody.GetLocalPoint(position);
			GameElementsControl.World.AddJoint(grappleJoint);
			AttachLastLink();
			attachTime = GameElementsControl.CurrentTimeInMS;
			attached = true;
			grapple.Attach();
			StiffRope();
			owner.HookAttached = true;
		}

		public void StiffRope()
		{
			if (!stiffed && attached)
			{
				float maxLength = (float)(activeLinks + 1) * MaxSpaceBetweenLinks + lastLinkSpace;
				connectRope = new RopeJoint(grapple.MainBody, owner.MainBody, Vector2.Zero, owner.MainBody.GetLocalPoint(ownerArm.Position));
				connectRope.MaxLength = maxLength;
				GameElementsControl.World.AddJoint(connectRope);
				stiffed = true;
			}
		}

		public void LooseRope()
		{
			if (stiffed)
			{
				GameElementsControl.World.RemoveJoint(connectRope);
				stiffed = false;
			}
		}

		private bool AddLink(float angle, Vector2 lastLinkEdge, out Vector2 newLastLinkEdge)
		{
			if (activeLinks == 15)
			{
				Deactivate();
				newLastLinkEdge = Vector2.Zero;
				return false;
			}
			HookLink hookLink = linkPool[activeLinks];
			if (activeLinks == 0)
			{
				float x = maxSpaceBetweenLinks * (float)Math.Cos(angle);
				float y = maxSpaceBetweenLinks * (float)Math.Sin(angle);
				newLastLinkEdge = grapple.MainBody.Position - new Vector2(x, y);
				hookLink.AddToChain(newLastLinkEdge, angle);
				AttachFirstLink();
			}
			else
			{
				_ = linkPool[activeLinks - 1];
				float x2 = maxSpaceBetweenLinks * (float)Math.Cos(angle);
				float y2 = maxSpaceBetweenLinks * (float)Math.Sin(angle);
				newLastLinkEdge = lastLinkEdge - new Vector2(x2, y2);
				hookLink.AddToChain(newLastLinkEdge, angle);
				AttachLink();
			}
			activeLinks++;
			return true;
		}

		public override void Draw()
		{
			if (active)
			{
				for (int i = 0; i < activeLinks; i++)
				{
					if (i > 0)
					{
						DrawRope(linkPool[i].Body.Position, linkPool[i - 1].Body.Position);
					}
					linkPool[i].Draw();
				}
				if (activeLinks > 0)
				{
					DrawRope(grapple.MainBody.Position, linkPool[0].Body.Position);
					DrawRope(linkPool[activeLinks - 1].Body.Position, ownerArm.CannonPosition(33f));
				}
				else
				{
					DrawRope(grapple.MainBody.Position, ownerArm.CannonPosition(33f));
				}
				grapple.Draw();
			}
			else if (hasTarget)
			{
				targetSprite.Draw(GameElementsControl.ConvertWorldToScreen(modifiedTargetPosition), targetSpriteRotation);
			}
			else
			{
				noTargetSprite.Draw(GameElementsControl.ConvertWorldToScreen(targetPosition));
			}
			hookSprite.Draw(ownerArm.HandPosition(), hookSpriteVariables);
		}

		private void DrawRope(Vector2 pos1, Vector2 pos2)
		{
			Vector2 v = GameElementsControl.ConvertWorldToScreen(pos2 - pos1);
			Vector2 scale = new Vector2(v.Length(), 1f);
			float rotation = v.GetAngle();
			spriteRope.Draw(GameElementsControl.ConvertWorldToScreen(pos1), rotation, scale);
		}
	}
}
