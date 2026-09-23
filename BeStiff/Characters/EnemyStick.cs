using System;
using FarseerPhysics.Dynamics;
using SKAnimation;

namespace Be_Stiff.Characters
{
	public class EnemyStick : Enemy
	{
		protected override void LoadEnemyType()
		{
			baseEnergy = 150f;
			currentEnergy = baseEnergy;
			BoneReader boneReader = new BoneReader();
			skeleton = boneReader.CreateFromFile(GameElementsControl.ScreenManager.Content, "skeletons\\enemy\\regularguy\\regularguyskeleton", "skeletons\\enemy\\regularguy\\regularguyanimations", "sprites\\enemy\\stickarmed\\", GameElementsControl.World, ConvertUnits.DisplayToSimUnitsRatio, bodyRect, GameElementsControl.IsLegacyLevel ? 1f : 2f, true, GameElementsControl.IsLegacyLevel ? 1f : 2f);
			skeleton.Angle = (float)Math.PI;
			skeleton.SwitchSide((int)sideLooking);
			arm = new Arm(skeleton.GetBoneByName("rightUpArm"), skeleton.GetBoneByName("rightLoArm"));
			headBone = skeleton.GetBoneByName("head");
			headBone.IgnoreAnimation = IgnoreAnimationType.Angle;
			currentAnimation = "STAND";
			Fixture fixture = skeleton.GetBoneByName("torso").BoneBody.FixtureList[0];
			fixture.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture.AfterCollision, new AfterCollisionEventHandler(base.AfterHumanCollision));
			skeleton.SetCollisionCategory(Category.Cat3);
			skeleton.SetCollidesWith(Category.Cat1 | Category.Cat2 | Category.Cat4 | Category.Cat5 | Category.Cat6 | Category.Cat7 | Category.Cat8 | Category.Cat9 | Category.Cat10 | Category.Cat11 | Category.Cat12 | Category.Cat13 | Category.Cat14 | Category.Cat15 | Category.Cat16 | Category.Cat17 | Category.Cat18 | Category.Cat19 | Category.Cat20 | Category.Cat21 | Category.Cat22 | Category.Cat23 | Category.Cat24 | Category.Cat25 | Category.Cat26 | Category.Cat27 | Category.Cat28 | Category.Cat29 | Category.Cat30 | Category.Cat31);
			skeleton.SetUserData(new WorldObjectData(WorldObjectType.Human, this));
			weapon = new Weapon[1];
			numberOfWeapons = 1;
			Stick stick = new Stick(arm);
			stick.Init(base.Name);
			stick.Load();
			weapon[0] = stick;
			weapon[activeWeapon].SetArmAnimation();
			mainCollisionCategory = Category.Cat2;
			SetCollisionCategories();
		}
	}
}
