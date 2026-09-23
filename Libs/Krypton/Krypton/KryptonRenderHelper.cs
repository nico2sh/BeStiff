using System;
using Krypton.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton
{
	public class KryptonRenderHelper
	{
		private const int MAX_HULLS_VERTEX = 1000;

		private static VertexPositionTexture[] UnitQuad = new VertexPositionTexture[4]
		{
			new VertexPositionTexture
			{
				Position = new Vector3(-1f, 1f, 0f),
				TextureCoordinate = new Vector2(0f, 0f)
			},
			new VertexPositionTexture
			{
				Position = new Vector3(1f, 1f, 0f),
				TextureCoordinate = new Vector2(1f, 0f)
			},
			new VertexPositionTexture
			{
				Position = new Vector3(-1f, -1f, 0f),
				TextureCoordinate = new Vector2(0f, 1f)
			},
			new VertexPositionTexture
			{
				Position = new Vector3(1f, -1f, 0f),
				TextureCoordinate = new Vector2(1f, 1f)
			}
		};

		private VertexPositionColorTexture[] quad = new VertexPositionColorTexture[4];

		private VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[7];

		private int[] indicies3;

		private int[] indicies9;

		private int[] indicies15;

		private GraphicsDevice mGraphicsDevice;

		private Effect mEffect;

		private ShadowHullVertex[] mShadowHullVertices = new ShadowHullVertex[1000];

		private int mShadowHullVerticesNum;

		private int[] mShadowHullIndicies = new int[1000];

		private int mShadowHullIndiciesNum;

		public GraphicsDevice GraphicsDevice => mGraphicsDevice;

		public Effect Effect => mEffect;

		public ShadowHullVertex[] ShadowHullVertices => mShadowHullVertices;

		public int[] ShadowHullIndicies => mShadowHullIndicies;

		public KryptonRenderHelper(GraphicsDevice graphicsDevice, Effect effect)
		{
			mGraphicsDevice = graphicsDevice;
			mEffect = effect;
			for (int i = 0; i < 4; i++)
			{
				ref VertexPositionColorTexture reference = ref quad[i];
				reference = new VertexPositionColorTexture
				{
					Position = Vector3.Zero,
					Color = Color.White,
					TextureCoordinate = Vector2.Zero
				};
			}
			for (int j = 0; j < 7; j++)
			{
				ref VertexPositionColorTexture reference2 = ref vertices[j];
				reference2 = new VertexPositionColorTexture
				{
					Position = Vector3.Zero,
					Color = Color.White,
					TextureCoordinate = Vector2.Zero
				};
			}
			indicies3 = new int[3] { 0, 1, 6 };
			indicies9 = new int[9] { 0, 1, 3, 0, 3, 4, 0, 4, 6 };
			indicies15 = new int[15]
			{
				0, 1, 2, 0, 2, 3, 0, 3, 4, 0,
				4, 5, 0, 5, 6
			};
		}

		public void BufferAddShadowHull(ShadowHull hull)
		{
			Matrix matrix = Matrix.Identity;
			Matrix matrix2 = Matrix.Identity;
			float num = (float)Math.Cos(hull.Angle);
			float num2 = (float)Math.Sin(hull.Angle);
			matrix.M11 = hull.Scale.X * num;
			matrix.M12 = hull.Scale.X * num2;
			matrix.M21 = hull.Scale.Y * (0f - num2);
			matrix.M22 = hull.Scale.Y * num;
			matrix.M41 = hull.Position.X;
			matrix.M42 = hull.Position.Y;
			matrix2.M11 = 1f / hull.Scale.X * num;
			matrix2.M12 = 1f / hull.Scale.X * num2;
			matrix2.M21 = 1f / hull.Scale.Y * (0f - num2);
			matrix2.M22 = 1f / hull.Scale.Y * num;
			int num3 = mShadowHullVerticesNum;
			ShadowHullVertex shadowHullVertex = default(ShadowHullVertex);
			for (int i = 0; i < hull.NumPoints; i++)
			{
				ShadowHullPoint shadowHullPoint = hull.Points[i];
				Vector2.Transform(ref shadowHullPoint.Position, ref matrix, out shadowHullVertex.Position);
				Vector2.TransformNormal(ref shadowHullPoint.Normal, ref matrix2, out shadowHullVertex.Normal);
				shadowHullVertex.Color = Color.Black;
				mShadowHullVertices[mShadowHullVerticesNum] = shadowHullVertex;
				mShadowHullVerticesNum++;
			}
			int[] indicies = hull.Indicies;
			foreach (int num4 in indicies)
			{
				mShadowHullIndicies[mShadowHullIndiciesNum] = num3 + num4;
				mShadowHullIndiciesNum++;
			}
		}

		public void ClearHullArrays()
		{
			mShadowHullIndiciesNum = 0;
			mShadowHullVerticesNum = 0;
		}

		public void DrawSquareQuad(Vector2 position, float rotation, float size, Color color)
		{
			size /= 2f;
			size = (float)Math.Sqrt(Math.Pow(size, 2.0) + Math.Pow(size, 2.0));
			rotation += (float)Math.PI / 4f;
			float num = (float)Math.Cos(rotation) * size;
			float num2 = (float)Math.Sin(rotation) * size;
			Vector3 position2 = new Vector3(num, num2, 0f) + new Vector3(position, 0f);
			Vector3 position3 = new Vector3(0f - num2, num, 0f) + new Vector3(position, 0f);
			Vector3 position4 = new Vector3(0f - num, 0f - num2, 0f) + new Vector3(position, 0f);
			Vector3 position5 = new Vector3(num2, 0f - num, 0f) + new Vector3(position, 0f);
			ref VertexPositionColorTexture reference = ref quad[0];
			reference = new VertexPositionColorTexture
			{
				Position = position3,
				Color = color,
				TextureCoordinate = new Vector2(0f, 0f)
			};
			ref VertexPositionColorTexture reference2 = ref quad[1];
			reference2 = new VertexPositionColorTexture
			{
				Position = position2,
				Color = color,
				TextureCoordinate = new Vector2(1f, 0f)
			};
			ref VertexPositionColorTexture reference3 = ref quad[2];
			reference3 = new VertexPositionColorTexture
			{
				Position = position4,
				Color = color,
				TextureCoordinate = new Vector2(0f, 1f)
			};
			ref VertexPositionColorTexture reference4 = ref quad[3];
			reference4 = new VertexPositionColorTexture
			{
				Position = position5,
				Color = color,
				TextureCoordinate = new Vector2(1f, 1f)
			};
			mGraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, quad, 0, 2);
		}

		public void DrawClippedFov(Vector2 position, float rotation, float size, Color color, float fov)
		{
			fov = MathHelper.Clamp(fov, 0f, (float)Math.PI * 2f);
			if (fov == 0f)
			{
				return;
			}
			if (fov == (float)Math.PI * 2f)
			{
				DrawSquareQuad(position, rotation, size, color);
				return;
			}
			Vector2 value = ClampToBox(fov / 2f);
			Vector2 value2 = ClampToBox((0f - fov) / 2f);
			Vector2 textureCoordinate = new Vector2(value.X + 1f, 0f - value.Y + 1f) / 2f;
			Vector2 textureCoordinate2 = new Vector2(value2.X + 1f, 0f - value2.Y + 1f) / 2f;
			ref VertexPositionColorTexture reference = ref vertices[0];
			reference = new VertexPositionColorTexture
			{
				Position = Vector3.Zero,
				Color = color,
				TextureCoordinate = new Vector2(0.5f, 0.5f)
			};
			ref VertexPositionColorTexture reference2 = ref vertices[1];
			reference2 = new VertexPositionColorTexture
			{
				Position = new Vector3(value, 0f),
				Color = color,
				TextureCoordinate = textureCoordinate
			};
			ref VertexPositionColorTexture reference3 = ref vertices[2];
			reference3 = new VertexPositionColorTexture
			{
				Position = new Vector3(-1f, 1f, 0f),
				Color = color,
				TextureCoordinate = new Vector2(0f, 0f)
			};
			ref VertexPositionColorTexture reference4 = ref vertices[3];
			reference4 = new VertexPositionColorTexture
			{
				Position = new Vector3(1f, 1f, 0f),
				Color = color,
				TextureCoordinate = new Vector2(1f, 0f)
			};
			ref VertexPositionColorTexture reference5 = ref vertices[4];
			reference5 = new VertexPositionColorTexture
			{
				Position = new Vector3(1f, -1f, 0f),
				Color = color,
				TextureCoordinate = new Vector2(1f, 1f)
			};
			ref VertexPositionColorTexture reference6 = ref vertices[5];
			reference6 = new VertexPositionColorTexture
			{
				Position = new Vector3(-1f, -1f, 0f),
				Color = color,
				TextureCoordinate = new Vector2(0f, 1f)
			};
			ref VertexPositionColorTexture reference7 = ref vertices[6];
			reference7 = new VertexPositionColorTexture
			{
				Position = new Vector3(value2, 0f),
				Color = color,
				TextureCoordinate = textureCoordinate2
			};
			Matrix matrix = Matrix.CreateRotationZ(rotation) * Matrix.CreateScale(size / 2f) * Matrix.CreateTranslation(new Vector3(position, 0f));
			for (int i = 0; i < vertices.Length; i++)
			{
				VertexPositionColorTexture vertexPositionColorTexture = vertices[i];
				Vector3.Transform(ref vertexPositionColorTexture.Position, ref matrix, out vertexPositionColorTexture.Position);
				vertices[i] = vertexPositionColorTexture;
			}
			int[] array = ((fov <= (float)Math.PI / 2f) ? indicies3 : ((!(fov <= 4.712389f)) ? indicies15 : indicies9));
			mGraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, array, 0, array.Length / 3);
		}

		public static Vector2 ClampToBox(float angle)
		{
			double num = Math.Cos(angle);
			double num2 = Math.Sin(angle);
			double num3 = Math.Max(Math.Abs(num), Math.Abs(num2));
			return new Vector2((float)(num / num3), (float)(num2 / num3));
		}

		public void BufferDraw()
		{
			if (mShadowHullIndiciesNum >= 3)
			{
				mGraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, mShadowHullVertices, 0, mShadowHullVerticesNum, mShadowHullIndicies, 0, mShadowHullIndiciesNum / 3);
			}
		}

		public void DrawFullscreenQuad()
		{
			_ = mGraphicsDevice.RasterizerState;
			mEffect.CurrentTechnique = mEffect.Techniques["ScreenCopy"];
			mEffect.Parameters["TexelBias"].SetValue(new Vector2(0.5f / (float)mGraphicsDevice.Viewport.Width, 0.5f / (float)mGraphicsDevice.Viewport.Height));
			foreach (EffectPass pass in mEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				mGraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, UnitQuad, 0, 2);
			}
		}

		public void BlurTextureToTarget(Texture2D texture, LightMapSize mapSize, BlurTechnique blurTechnique, float bluriness)
		{
			string name = "";
			switch (blurTechnique)
			{
			case BlurTechnique.Horizontal:
				mEffect.Parameters["BlurFactorU"].SetValue(1f / (float)GraphicsDevice.PresentationParameters.BackBufferWidth);
				name = "HorizontalBlur";
				break;
			case BlurTechnique.Vertical:
				mEffect.Parameters["BlurFactorV"].SetValue(1f / (float)mGraphicsDevice.PresentationParameters.BackBufferHeight);
				name = "VerticalBlur";
				break;
			}
			float num = BiasFactorFromLightMapSize(mapSize);
			Vector2 value = new Vector2
			{
				X = num / (float)mGraphicsDevice.Viewport.Width,
				Y = num / (float)mGraphicsDevice.Viewport.Height
			};
			mEffect.Parameters["Texture0"].SetValue(texture);
			mEffect.Parameters["TexelBias"].SetValue(value);
			mEffect.Parameters["Bluriness"].SetValue(bluriness);
			mEffect.CurrentTechnique = mEffect.Techniques["Blur"];
			mEffect.CurrentTechnique.Passes[name].Apply();
			mGraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, UnitQuad, 0, 2);
		}

		public void DrawTextureToTarget(Texture2D texture, LightMapSize mapSize, BlendTechnique blend)
		{
			string name = "";
			switch (blend)
			{
			case BlendTechnique.Add:
				name = "TextureToTarget_Add";
				break;
			case BlendTechnique.Multiply:
				name = "TextureToTarget_Multiply";
				break;
			}
			float num = BiasFactorFromLightMapSize(mapSize);
			Vector2 value = new Vector2
			{
				X = num / (float)mGraphicsDevice.ScissorRectangle.Width,
				Y = num / (float)mGraphicsDevice.ScissorRectangle.Height
			};
			mEffect.Parameters["Texture0"].SetValue(texture);
			mEffect.Parameters["TexelBias"].SetValue(value);
			mEffect.CurrentTechnique = mEffect.Techniques[name];
			foreach (EffectPass pass in mEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				mGraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, UnitQuad, 0, 2);
			}
		}

		private static float BiasFactorFromLightMapSize(LightMapSize mapSize)
		{
			switch (mapSize)
			{
			case LightMapSize.Full:
				return 0.5f;
			case LightMapSize.Fourth:
				return 0.6f;
			case LightMapSize.Eighth:
				return 0.7f;
			default:
				return 0f;
			}
		}

		public void BufferAddBoundOutline(BoundingRect boundingRect)
		{
			int num = mShadowHullVerticesNum;
			ref ShadowHullVertex reference = ref mShadowHullVertices[mShadowHullVerticesNum];
			reference = new ShadowHullVertex
			{
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Left, boundingRect.Top)
			};
			mShadowHullVerticesNum++;
			ref ShadowHullVertex reference2 = ref mShadowHullVertices[mShadowHullVerticesNum];
			reference2 = new ShadowHullVertex
			{
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Right, boundingRect.Top)
			};
			mShadowHullVerticesNum++;
			ref ShadowHullVertex reference3 = ref mShadowHullVertices[mShadowHullVerticesNum];
			reference3 = new ShadowHullVertex
			{
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Right, boundingRect.Bottom)
			};
			mShadowHullVerticesNum++;
			ref ShadowHullVertex reference4 = ref mShadowHullVertices[mShadowHullVerticesNum];
			reference4 = new ShadowHullVertex
			{
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Left, boundingRect.Bottom)
			};
			mShadowHullVerticesNum++;
			mShadowHullIndicies[mShadowHullIndiciesNum] = num;
			mShadowHullIndiciesNum++;
			mShadowHullIndicies[mShadowHullIndiciesNum] = num + 1;
			mShadowHullIndiciesNum++;
			mShadowHullIndicies[mShadowHullIndiciesNum] = num + 2;
			mShadowHullIndiciesNum++;
			mShadowHullIndicies[mShadowHullIndiciesNum] = num;
			mShadowHullIndiciesNum++;
			mShadowHullIndicies[mShadowHullIndiciesNum] = num + 2;
			mShadowHullIndiciesNum++;
			mShadowHullIndicies[mShadowHullIndiciesNum] = num + 3;
			mShadowHullIndiciesNum++;
		}
	}
}
