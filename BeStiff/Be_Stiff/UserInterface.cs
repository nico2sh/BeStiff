using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff
{
	public class UserInterface
	{
		private Vector2 basePosition;

		private GameSprite uiBar;

		private Vector2 uiBarPosition;

		private GameSprite clockBar;

		private Vector2 clockBarPosition;

		private GameSprite backgroundBar;

		private Vector2 healthBarPosition;

		private Vector2 bulletTimeBarPosition;

		private Vector2 staminaChargeBarPosition;

		private GameSprite energyBar;

		private Vector2 energyBarPosition;

		private Vector2 energyBarSize;

		private GameSprite bulletTimeGaugeBar;

		private Vector2 bulletTimeGaugeBarPosition;

		private Vector2 bulletTimeGaugeBarSize;

		private GameSprite staminaBar;

		private GameSprite staminaChargeGaugeBar;

		private Vector2 staminaChargeGaugeBarPosition;

		private Vector2 staminaChargeGaugeBarSize;

		private GameSprite faceHero;

		private Vector2 faceHeroPosition;

		private GameSprite sandClock;

		private Vector2 sandClockPosition;

		private Vector2 WeaponPosition;

		private Vector2 WeaponInfoPosition;

		private Vector2 WeaponSecondaryInfoPosition;

		private SpriteFont weaponInfoFont;

		private StringBuilder weaponInfo;

		private StringBuilder weaponSecondaryInfo;

		private StopWatch stopWatch;

		private ScoreManager score;

		private Vector2 scorePosition;

		private GameSprite scoreBar;

		private Vector2 scoreBarPosition;

		public StopWatch StopWatch => stopWatch;

		public ScoreManager Score => score;

		public UserInterface()
		{
			basePosition = new Vector2(70f, 40f);
			energyBarSize = new Vector2(1f, 1f);
			bulletTimeGaugeBarSize = new Vector2(1f, 1f);
			staminaChargeGaugeBarSize = new Vector2(1f, 1f);
			weaponInfo = new StringBuilder();
			weaponSecondaryInfo = new StringBuilder();
			stopWatch = new StopWatch();
			score = new ScoreManager();
		}

		public void Load()
		{
			GameElementsControl.LoadSprite("uiBar", "sprites\\ui\\uibar");
			GameElementsControl.LoadSprite("clockBar", "sprites\\ui\\clockbar");
			GameElementsControl.LoadSprite("scoreBar", "sprites\\ui\\scorebar");
			GameElementsControl.LoadSprite("healthBar", "sprites\\ui\\healthbar");
			GameElementsControl.LoadSprite("energy", "sprites\\ui\\energy");
			GameElementsControl.LoadSprite("stamina", "sprites\\ui\\stamina");
			GameElementsControl.LoadSprite("bulletTimeGauge", "sprites\\ui\\bullettimegauge");
			GameElementsControl.LoadSprite("face", "sprites\\ui\\face");
			GameElementsControl.LoadSprite("sandClock", "sprites\\ui\\sandclock");
			GameElementsControl.LoadSprite("staminaBar", "sprites\\ui\\staminabar");
			GameElementsControl.LoadFont("stopWatchSecondsFont", "fonts\\stopwatchsecondsfont");
			uiBar = GameElementsControl.GetSprite("uiBar");
			uiBarPosition = new Vector2(uiBar.Width / 2, uiBar.Height / 2);
			clockBar = GameElementsControl.GetSprite("clockBar");
			clockBarPosition = new Vector2(GameElementsControl.ScreenManager.GraphicsDevice.Viewport.Width - clockBar.Width / 2, clockBar.Height / 2);
			scoreBar = GameElementsControl.GetSprite("scoreBar");
			scoreBarPosition = new Vector2(GameElementsControl.ScreenManager.GraphicsDevice.Viewport.Width - scoreBar.Width / 2, GameElementsControl.ScreenManager.GraphicsDevice.Viewport.Height - scoreBar.Height / 2);
			backgroundBar = GameElementsControl.GetSprite("healthBar");
			staminaBar = GameElementsControl.GetSprite("staminaBar");
			healthBarPosition = basePosition + new Vector2(backgroundBar.Width / 2, 0f);
			bulletTimeBarPosition = basePosition + new Vector2(220f, 0f) + new Vector2(backgroundBar.Width / 2, 0f);
			staminaChargeBarPosition = basePosition + new Vector2(staminaBar.Width / 2, backgroundBar.Height / 2 + staminaBar.Height / 2);
			energyBarPosition = healthBarPosition - new Vector2(backgroundBar.Width / 2 - 2, 0f);
			energyBar = GameElementsControl.GetSprite("energy");
			staminaChargeGaugeBarPosition = staminaChargeBarPosition - new Vector2(staminaBar.Width / 2 - 2, 0f);
			staminaChargeGaugeBar = GameElementsControl.GetSprite("stamina");
			bulletTimeGaugeBarPosition = bulletTimeBarPosition - new Vector2(backgroundBar.Width / 2 - 2, 0f);
			bulletTimeGaugeBar = GameElementsControl.GetSprite("bulletTimeGauge");
			faceHero = GameElementsControl.GetSprite("face");
			faceHeroPosition = basePosition - new Vector2(20f, 0f);
			sandClock = GameElementsControl.GetSprite("sandClock");
			sandClockPosition = basePosition + new Vector2(200f, 0f);
			WeaponPosition = new Vector2(560f, 40f);
			WeaponInfoPosition = WeaponPosition + new Vector2(30f, 20f);
			WeaponSecondaryInfoPosition = WeaponPosition - new Vector2(52f, -3f);
			weaponInfoFont = GameElementsControl.GetFont("stopWatchSecondsFont");
			scorePosition = scoreBarPosition - new Vector2(30f, 13f);
			stopWatch.Load();
			score.Load();
		}

		public void Update()
		{
			energyBarSize.X = GameElementsControl.Hero.Energy * 1.6f;
			bulletTimeGaugeBarSize.X = GameElementsControl.Hero.BulletTimeGauge * 1.6f;
			staminaChargeGaugeBarSize.X = GameElementsControl.Hero.StaminaChargeGauge * 1f;
			weaponInfo.Length = 0;
			int weaponInfoNumber = GameElementsControl.Hero.WeaponInfoNumber;
			if (weaponInfoNumber != -1)
			{
				weaponInfo.AppendNumber(weaponInfoNumber, 0);
			}
			weaponSecondaryInfo.Length = 0;
			int weaponSecondaryInfoNumber = GameElementsControl.Hero.WeaponSecondaryInfoNumber;
			if (weaponSecondaryInfoNumber != -1)
			{
				weaponSecondaryInfo.AppendNumber(weaponSecondaryInfoNumber, 0);
			}
			stopWatch.Update();
			score.Update();
		}

		public void Draw()
		{
			uiBar.Draw(uiBarPosition);
			clockBar.Draw(clockBarPosition);
			scoreBar.Draw(scoreBarPosition);
			faceHero.Draw(faceHeroPosition);
			sandClock.Draw(sandClockPosition);
			backgroundBar.Draw(healthBarPosition);
			backgroundBar.Draw(bulletTimeBarPosition);
			staminaBar.Draw(staminaChargeBarPosition);
			energyBar.Draw(energyBarPosition, energyBarSize);
			bulletTimeGaugeBar.Draw(bulletTimeGaugeBarPosition, bulletTimeGaugeBarSize);
			staminaChargeGaugeBar.Draw(staminaChargeGaugeBarPosition, staminaChargeGaugeBarSize);
			GameElementsControl.Hero.WeaponSprite.Draw(WeaponPosition);
			GameElementsControl.ScreenManager.SpriteBatch.DrawString(weaponInfoFont, weaponInfo, WeaponInfoPosition, Color.DarkRed);
			GameElementsControl.ScreenManager.SpriteBatch.DrawString(weaponInfoFont, weaponSecondaryInfo, WeaponSecondaryInfoPosition, Color.DarkRed);
			stopWatch.Draw(clockBarPosition - new Vector2(-20f, 18f));
			score.Draw(scorePosition);
		}
	}
}
