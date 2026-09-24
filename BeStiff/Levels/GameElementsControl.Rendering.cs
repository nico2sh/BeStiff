using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using Krypton;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OgmoXNA;

namespace Be_Stiff.Levels
{
	internal static partial class GameElementsControl
	{
		// Effect parameters looked up once at load instead of by name per frame.
		private static EffectParameter alphaMaskMap;

		private static EffectParameter heroDrawMap;

		private static EffectParameter grayShadowMap;

		private static EffectParameter alphaShadowColor;

		private static EffectParameter noisesAlphaMap;

		private static EffectParameter noisesObjectsMap;

		private static EffectParameter noisesHeroMap;

		private static void CacheEffectParameters()
		{
			alphaMaskEffect.CurrentTechnique = alphaMaskEffect.Techniques["AlphaMapShader"];
			alphaMaskMap = alphaMaskEffect.Parameters["AlphaMap"];
			heroDrawEffect.CurrentTechnique = heroDrawEffect.Techniques["BlackShadowMapShader"];
			heroDrawMap = heroDrawEffect.Parameters["AlphaMap"];
			grayShadowEffect.CurrentTechnique = grayShadowEffect.Techniques["GrayShadowMapShader"];
			grayShadowMap = grayShadowEffect.Parameters["AlphaMap"];
			alphaShadowColor = alphaShadowEffect.Parameters["ShadowColor"];
			alphaNoisesEffect.CurrentTechnique = alphaNoisesEffect.Techniques["AlphaNoisesShader"];
			noisesAlphaMap = alphaNoisesEffect.Parameters["AlphaMap"];
			noisesObjectsMap = alphaNoisesEffect.Parameters["ObjectsMap"];
			noisesHeroMap = alphaNoisesEffect.Parameters["HeroMap"];
		}

		/// <summary>Draws a screen-sized texture over the camera view.</summary>
		private static void DrawCameraQuad(Texture2D texture)
		{
			ScreenManager.SpriteBatch.Draw(texture, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
		}

		/// <summary>
		/// Composites a screen-sized render target through a pixel shader whose
		/// parameters the caller has already set.
		/// </summary>
		private static void DrawCameraPass(Texture2D texture, Effect effect, SamplerState sampler)
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			effect.CurrentTechnique.Passes[0].Apply();
			DrawCameraQuad(texture);
			ScreenManager.SpriteBatch.End();
		}

		private static void DrawMaskedObjects()
		{
			alphaMaskMap.SetValue(krypton.mMap);
			DrawCameraPass(renderTargetObjects, alphaMaskEffect, null);
		}

		private static void DrawHero()
		{
			heroDrawMap.SetValue(renderTargetObjects);
			DrawCameraPass(renderTargetHero, heroDrawEffect, SamplerState.LinearWrap);
		}

		private static void DrawBackground()
		{
			grayShadowMap.SetValue(krypton.mMap);
			DrawCameraPass(renderTargetBackground, grayShadowEffect, null);
		}

		private static void DrawLineOfSightShadows()
		{
			alphaShadowColor.SetValue(hero.ShadowColor.ToVector4());
			DrawCameraPass(krypton.mMap, alphaShadowEffect, null);
		}

		private static void SetRenderTargets()
		{
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetObjects);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			foreach (WorldObject value in worldObjects.Values)
			{
				value.Draw();
			}
			enemiesControl.Draw();
			ScreenManager.SpriteBatch.End();
			particlesmanager.Draw(Camera.View);
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetNoises);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			if (!hero.IsDead())
			{
				noiseManager.Draw();
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetHero);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			if (!hero.Disposed)
			{
				hero.Draw();
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetBackground);
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			level.DrawBackgroundLayers();
			level.DrawBackground();
			foreach (WorldObject value2 in worldShadowObjects.Values)
			{
				value2.Draw();
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.GraphicsDevice.SetRenderTarget(null);
		}

		public static void Draw()
		{
			krypton.Matrix = Camera.View;
			krypton.LightMapPrepare();
			SetRenderTargets();
			// Switching render targets doesn't preserve the back buffer, so clear it here.
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			DrawBackground();
			DrawLineOfSightShadows();
			DrawMaskedObjects();
			DrawHero();
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			foreach (WorldObject value in worldShadowObjects.Values)
			{
				IShadowCaster shadowCaster = (IShadowCaster)value;
				shadowCaster.DrawHull();
			}
			level.DrawFloor();
			level.DrawGoals();
			hero.DrawCrossHair();
			level.DrawFrames();
			noisesAlphaMap.SetValue(krypton.mMap);
			noisesObjectsMap.SetValue(renderTargetObjects);
			noisesHeroMap.SetValue(renderTargetHero);
			alphaNoisesEffect.CurrentTechnique.Passes[0].Apply();
			DrawCameraQuad(renderTargetNoises);
			ScreenManager.SpriteBatch.End();
			ScreenManager.SpriteBatch.Begin();
			userInterface.Draw();
			ScreenManager.SpriteBatch.End();
			if (debugViewEnabled)
			{
				Matrix projection = Camera.SimProjection;
				Matrix view = Camera.SimView;
				debugView.RenderDebugData(ref projection, ref view);
			}
		}
	}
}
