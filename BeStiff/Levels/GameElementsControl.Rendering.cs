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
		private static void DrawMaskedObjects()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			alphaMaskEffect.Parameters["AlphaMap"].SetValue(krypton.mMap);
			alphaMaskEffect.CurrentTechnique = alphaMaskEffect.Techniques["AlphaMapShader"];
			alphaMaskEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetObjects, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
		}

		private static void DrawHero()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			heroDrawEffect.Parameters["AlphaMap"].SetValue(renderTargetObjects);
			heroDrawEffect.CurrentTechnique = heroDrawEffect.Techniques["BlackShadowMapShader"];
			heroDrawEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetHero, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
		}

		private static void DrawBackground()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			grayShadowEffect.Parameters["AlphaMap"].SetValue(krypton.mMap);
			grayShadowEffect.CurrentTechnique = grayShadowEffect.Techniques["GrayShadowMapShader"];
			grayShadowEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetBackground, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
		}

		private static void DrawLineOfSightShadows()
		{
			ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)null, Camera.View);
			alphaShadowEffect.Parameters["ShadowColor"].SetValue(hero.ShadowColor.ToVector4());
			alphaShadowEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(krypton.mMap, Camera.Position, null, Color.White, 0f, camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
			ScreenManager.SpriteBatch.End();
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
			ScreenManager.GraphicsDevice.SetRenderTarget(null);
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
			ScreenManager.GraphicsDevice.Clear(Color.Transparent);
			SetRenderTargets();
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
			alphaNoisesEffect.Parameters["AlphaMap"].SetValue(krypton.mMap);
			alphaNoisesEffect.Parameters["ObjectsMap"].SetValue(renderTargetObjects);
			alphaNoisesEffect.Parameters["HeroMap"].SetValue(renderTargetHero);
			alphaNoisesEffect.CurrentTechnique = alphaNoisesEffect.Techniques["AlphaNoisesShader"];
			alphaNoisesEffect.CurrentTechnique.Passes[0].Apply();
			ScreenManager.SpriteBatch.Draw(renderTargetNoises, Camera.Position, null, Color.White, 0f, Camera.HalfSize, 1f / Camera.Zoom, SpriteEffects.None, 0f);
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
