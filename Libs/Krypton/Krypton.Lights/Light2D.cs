using System;
using System.Collections.Generic;
using Krypton.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton.Lights
{
	public class Light2D : ILight2D
	{
		private bool mIsOn = true;

		private Vector2 mPosition = Vector2.Zero;

		private float mAngle;

		private Texture2D mTexture;

		private Color mColor = Color.White;

		private float mRange = 1f;

		private float mFov = (float)Math.PI * 2f;

		private float mIntensity = 1f;

		public Vector2 Position
		{
			get
			{
				return mPosition;
			}
			set
			{
				mPosition = value;
			}
		}

		public float X
		{
			get
			{
				return mPosition.X;
			}
			set
			{
				mPosition.X = value;
			}
		}

		public float Y
		{
			get
			{
				return mPosition.Y;
			}
			set
			{
				mPosition.Y = value;
			}
		}

		public float Angle
		{
			get
			{
				return mAngle;
			}
			set
			{
				mAngle = value;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return mTexture;
			}
			set
			{
				mTexture = value;
			}
		}

		public Color Color
		{
			get
			{
				return mColor;
			}
			set
			{
				mColor = value;
			}
		}

		public float Range
		{
			get
			{
				return mRange;
			}
			set
			{
				mRange = value;
			}
		}

		public float Fov
		{
			get
			{
				return mFov;
			}
			set
			{
				mFov = MathHelper.Clamp(value, 0f, (float)Math.PI * 2f);
			}
		}

		public float Intensity
		{
			get
			{
				return mIntensity;
			}
			set
			{
				mIntensity = MathHelper.Clamp(value, 0.01f, 3f);
			}
		}

		public bool IsOn
		{
			get
			{
				return mIsOn;
			}
			set
			{
				mIsOn = value;
			}
		}

		public BoundingRect Bounds
		{
			get
			{
				BoundingRect mEmpty = BoundingRect.mEmpty;
				mEmpty.Min.X = mPosition.X - mRange;
				mEmpty.Min.Y = mPosition.Y - mRange;
				mEmpty.Max.X = mPosition.X + mRange;
				mEmpty.Max.Y = mPosition.Y + mRange;
				return mEmpty;
			}
		}

		private static int logCount;

		public void Draw(KryptonRenderHelper helper, List<ShadowHull> hulls)
		{
			if (!mIsOn)
			{
				return;
			}
			helper.ClearHullArrays();
			foreach (ShadowHull hull in hulls)
			{
				if (hull.Visible && IsInRange(hull.Position - Position, hull.MaxRadius * Math.Max(hull.Scale.X, hull.Scale.Y) + Range))
				{
					helper.BufferAddShadowHull(hull);
				}
			}
			EffectTechnique effectTechnique = helper.Effect.Techniques["PointLight_Shadow_Fast"];
			helper.Effect.CurrentTechnique = effectTechnique;
			helper.Effect.Parameters["LightPosition"].SetValue(mPosition);
			helper.Effect.Parameters["Texture0"].SetValue(mTexture);
			helper.Effect.Parameters["LightIntensityFactor"].SetValue(1f / (mIntensity * mIntensity));
			if (DebugLog && logCount < 2)
				System.Console.Error.WriteLine($"ShadowStrech param = {helper.Effect.Parameters["ShadowStrech"].GetValueSingle()}");
			effectTechnique.Passes["ShadowStencil"].Apply();
			helper.BufferDraw();
			effectTechnique.Passes["Light"].Apply();
			var gd = helper.GraphicsDevice;
			if (DebugNoStencil)
			{
				gd.DepthStencilState = DepthStencilState.None;
			}
			if (DebugNoScissor)
			{
				gd.RasterizerState = new RasterizerState { CullMode = gd.RasterizerState.CullMode, ScissorTestEnable = false };
			}
			if (DebugNoCull)
			{
				gd.RasterizerState = new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = gd.RasterizerState.ScissorTestEnable };
			}
			if (DebugLog && logCount++ < 2)
			{
				var ds = gd.DepthStencilState; var rs = gd.RasterizerState; var bs = gd.BlendState;
				System.Console.Error.WriteLine($"Light pass: stencilEnable={ds.StencilEnable} func={ds.StencilFunction} ref={ds.ReferenceStencil} pass={ds.StencilPass} depth={ds.DepthBufferEnable} scissor={rs.ScissorTestEnable} cull={rs.CullMode} blend={bs.ColorSourceBlend}/{bs.ColorDestinationBlend} write={bs.ColorWriteChannels} tex={(mTexture == null ? "null" : mTexture.Width + "x" + mTexture.Height)} pos={mPosition} range={mRange} color={mColor} intensity={mIntensity} fov={mFov} viewport={gd.Viewport.Width}x{gd.Viewport.Height}");
			}
			helper.DrawClippedFov(mPosition, mAngle, mRange * 2f, mColor, mFov);
		}

		// Debug switches, read once instead of per light per frame.
		private static readonly bool DebugLog = System.Environment.GetEnvironmentVariable("BESTIFF_KR_LOG") != null;

		private static readonly bool DebugNoStencil = System.Environment.GetEnvironmentVariable("BESTIFF_KR_NOSTENCIL") != null;

		private static readonly bool DebugNoScissor = System.Environment.GetEnvironmentVariable("BESTIFF_KR_NOSCISSOR") != null;

		private static readonly bool DebugNoCull = System.Environment.GetEnvironmentVariable("BESTIFF_KR_NOCULL") != null;

		private static bool IsInRange(Vector2 offset, float dist)
		{
			if (offset.X * offset.X + offset.Y * offset.Y < dist * dist)
			{
				return true;
			}
			return false;
		}
	}
}
