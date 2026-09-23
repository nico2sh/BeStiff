using System;
using System.Collections.Generic;
using Krypton.Common;
using Krypton.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton
{
	public class KryptonEngine : DrawableGameComponent
	{
		private string mEffectAssetName;

		private Effect mEffect;

		private CullMode mCullMode = CullMode.CullCounterClockwiseFace;

		private List<ShadowHull> mHulls = new List<ShadowHull>();

		private List<ILight2D> mLights = new List<ILight2D>();

		private Matrix mWVP = Matrix.Identity;

		private bool mSpriteBatchCompatabilityEnabled;

		private BoundingRect mBounds = BoundingRect.MinMax;

		private float mBluriness = 0.25f;

		private RenderTarget2D mMapBlur;

		public RenderTarget2D mMap;
		private int debugFrames;

		private Color mAmbientColor = new Color(35, 35, 35);

		private LightMapSize mLightMapSize = LightMapSize.Full;

		public KryptonRenderHelper RenderHelper { get; private set; }

		public CullMode CullMode
		{
			get
			{
				return mCullMode;
			}
			set
			{
				mCullMode = value;
			}
		}

		public List<ILight2D> Lights => mLights;

		public List<ShadowHull> Hulls => mHulls;

		public Matrix Matrix
		{
			get
			{
				return mWVP;
			}
			set
			{
				if (mWVP != value)
				{
					mWVP = value;
					Matrix matrix = Matrix.Invert(value);
					Vector2 vector = Vector2.Transform(new Vector2(1f, 1f), matrix);
					Vector2 value2 = Vector2.Transform(new Vector2(1f, -1f), matrix);
					Vector2 value3 = Vector2.Transform(new Vector2(-1f, -1f), matrix);
					Vector2 value4 = Vector2.Transform(new Vector2(-1f, 1f), matrix);
					mBounds.Min = vector;
					mBounds.Min = Vector2.Min(mBounds.Min, value2);
					mBounds.Min = Vector2.Min(mBounds.Min, value3);
					mBounds.Min = Vector2.Min(mBounds.Min, value4);
					mBounds.Max = vector;
					mBounds.Max = Vector2.Max(mBounds.Max, value2);
					mBounds.Max = Vector2.Max(mBounds.Max, value3);
					mBounds.Max = Vector2.Max(mBounds.Max, value4);
					mBounds = BoundingRect.MinMax;
				}
			}
		}

		public bool SpriteBatchCompatablityEnabled
		{
			get
			{
				return mSpriteBatchCompatabilityEnabled;
			}
			set
			{
				mSpriteBatchCompatabilityEnabled = value;
			}
		}

		public Color AmbientColor
		{
			get
			{
				return mAmbientColor;
			}
			set
			{
				mAmbientColor = value;
			}
		}

		public LightMapSize LightMapSize
		{
			get
			{
				return mLightMapSize;
			}
			set
			{
				if (mLightMapSize != value)
				{
					mLightMapSize = value;
					DisposeRenderTargets();
					CreateRenderTargets();
				}
			}
		}

		public float Bluriness
		{
			get
			{
				return mBluriness;
			}
			set
			{
				mBluriness = Math.Max(0f, value);
			}
		}

		public KryptonEngine(Game game, string effectAssetName)
			: base(game)
		{
			mEffectAssetName = effectAssetName;
		}

		public override void Initialize()
		{
			base.Initialize();
			base.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
		}

		private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
		{
			DisposeRenderTargets();
			CreateRenderTargets();
		}

		protected override void LoadContent()
		{
			mEffect = base.Game.Content.Load<Effect>(mEffectAssetName);
			RenderHelper = new KryptonRenderHelper(base.GraphicsDevice, mEffect);
			CreateRenderTargets();
		}

		protected override void UnloadContent()
		{
			DisposeRenderTargets();
		}

		private void CreateRenderTargets()
		{
			int width = base.GraphicsDevice.Viewport.Width / (int)mLightMapSize;
			int height = base.GraphicsDevice.Viewport.Height / (int)mLightMapSize;
			mMap = new RenderTarget2D(base.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
			mMapBlur = new RenderTarget2D(base.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
		}

		private void DisposeRenderTargets()
		{
			TryDispose(mMap);
			TryDispose(mMapBlur);
		}

		private static void TryDispose(IDisposable obj)
		{
			if (obj != null)
			{
				obj.Dispose();
				obj = null;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			LightMapPresent();
		}

		public void LightMapPrepare()
		{
			_ = base.GraphicsDevice.ScissorRectangle.Width;
			_ = base.GraphicsDevice.ScissorRectangle.Height;
			Matrix matrix = LightmapMatrixGet();
			mEffect.Parameters["Matrix"].SetValue(matrix);
			// MonoGame's effect compiler does not keep the HLSL default values;
			// without the stretch the shadow hulls cast no projected shadow.
			mEffect.Parameters["ShadowStrech"].SetValue(1000000f);
			RenderTargetBinding[] renderTargets = base.GraphicsDevice.GetRenderTargets();
			base.GraphicsDevice.SetRenderTarget(mMap);
			// Alpha 0 everywhere; the shadow pass raises it to 1 behind hulls so the
			// game's line-of-sight overlay knows what the hero cannot see.
			base.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.Stencil, Color.Transparent, 0f, 1);
			base.GraphicsDevice.RasterizerState = RasterizerStateGetFromCullMode(mCullMode);
			Vector2 targetSize = new Vector2(mMap.Width, mMap.Height);
			debugFrames++;
			bool dbg = System.Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null && debugFrames <= 3;
			if (dbg) System.Console.Error.WriteLine($"Krypton: lights={mLights.Count} hulls={mHulls.Count} bounds={mBounds} map={mMap.Width}x{mMap.Height} ambient={AmbientColor} blur={mBluriness} technique={mEffect.Techniques["PointLight_Shadow_Fast"] != null}");
			foreach (ILight2D mLight in mLights)
			{
				if (mLight.Bounds.Intersects(mBounds))
				{
					base.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0f, 1);
					Rectangle scissor = ScissorRectCreateForLight(mLight, matrix, targetSize);
					if (System.Environment.GetEnvironmentVariable("BESTIFF_KR_FLIPSCISSOR") != null)
					{
						scissor.Y = mMap.Height - scissor.Y - scissor.Height;
					}
					base.GraphicsDevice.ScissorRectangle = scissor;
					if (dbg) System.Console.Error.WriteLine($"  light bounds={mLight.Bounds} scissor={base.GraphicsDevice.ScissorRectangle} scissorEnabled={base.GraphicsDevice.RasterizerState.ScissorTestEnable}");
					mLight.Draw(RenderHelper, mHulls);
				}
				else if (dbg) System.Console.Error.WriteLine($"  light SKIPPED bounds={mLight.Bounds}");
			}
			string dumpPrefix = System.Environment.GetEnvironmentVariable("BESTIFF_KRYPTON_DUMP");
			if (dumpPrefix != null && debugFrames == 60)
			{
				using (var fs = System.IO.File.Create(dumpPrefix + "_postlight.png")) mMap.SaveAsPng(fs, mMap.Width, mMap.Height);
			}
			if (mBluriness > 0f)
			{
				base.GraphicsDevice.SetRenderTarget(mMapBlur);
				RenderHelper.BlurTextureToTarget(mMap, LightMapSize.Full, BlurTechnique.Horizontal, mBluriness);
				base.GraphicsDevice.SetRenderTarget(mMap);
				RenderHelper.BlurTextureToTarget(mMapBlur, LightMapSize.Full, BlurTechnique.Vertical, mBluriness);
				if (dumpPrefix != null && debugFrames == 60)
				{
					using (var fs = System.IO.File.Create(dumpPrefix + "_blurH.png")) mMapBlur.SaveAsPng(fs, mMapBlur.Width, mMapBlur.Height);
					using (var fs = System.IO.File.Create(dumpPrefix + "_postblur.png")) mMap.SaveAsPng(fs, mMap.Width, mMap.Height);
				}
			}
			base.GraphicsDevice.SetRenderTargets(renderTargets);
		}

		private Matrix LightmapMatrixGet()
		{
			if (mSpriteBatchCompatabilityEnabled)
			{
				float num = ((base.GraphicsDevice.Viewport.Width > 0) ? (1f / (float)base.GraphicsDevice.Viewport.Width) : 0f);
				float num2 = ((base.GraphicsDevice.Viewport.Height > 0) ? (-1f / (float)base.GraphicsDevice.Viewport.Height) : 0f);
				Matrix matrix = new Matrix
				{
					M11 = num * 2f,
					M22 = num2 * 2f,
					M33 = 1f,
					M44 = 1f,
					M41 = -1f - num,
					M42 = 1f - num2
				};
				return mWVP * matrix;
			}
			return mWVP;
		}

		private static Rectangle ScissorRectCreateForLight(ILight2D light, Matrix matrix, Vector2 targetSize)
		{
			BoundingRect bounds = light.Bounds;
			Vector2 value = VectorToPixel(bounds.Min, matrix, targetSize);
			Vector2 value2 = VectorToPixel(bounds.Max, matrix, targetSize);
			Vector2 value3 = Vector2.Min(value, value2);
			Vector2 value4 = Vector2.Max(value, value2);
			value = Vector2.Clamp(value3, Vector2.Zero, targetSize);
			value2 = Vector2.Clamp(value4, Vector2.Zero, targetSize);
			return new Rectangle((int)value.X, (int)value.Y, (int)(value2.X - value.X), (int)(value2.Y - value.Y));
		}

		private static Vector2 VectorToPixel(Vector2 v, Matrix matrix, Vector2 targetSize)
		{
			Vector2.Transform(ref v, ref matrix, out v);
			v.X = (1f + v.X) * (targetSize.X / 2f);
			v.Y = (1f - v.Y) * (targetSize.Y / 2f);
			return v;
		}

		private static Vector2 ScaleToPixel(Vector2 v, Matrix matrix, Vector2 targetSize)
		{
			v.X *= matrix.M11 * (targetSize.X / 2f);
			v.Y *= matrix.M22 * (targetSize.Y / 2f);
			return v;
		}

		private static RasterizerState RasterizerStateGetFromCullMode(CullMode cullMode)
		{
			switch (cullMode)
			{
			case CullMode.CullCounterClockwiseFace:
				return RasterizerState.CullCounterClockwise;
			case CullMode.CullClockwiseFace:
				return RasterizerState.CullClockwise;
			default:
				return RasterizerState.CullNone;
			}
		}

		private void LightMapPresent()
		{
			RenderHelper.DrawTextureToTarget(mMap, mLightMapSize, BlendTechnique.Multiply);
		}
	}
}
