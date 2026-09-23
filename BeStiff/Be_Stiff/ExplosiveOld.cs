using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury;
using ProjectMercury.Emitters;

namespace Be_Stiff
{
	public class ExplosiveOld
	{
		private const int MAX_SHAPES = 100;

		private CircleEmitter emitterExplosion;

		private Dictionary<WorldObject, Vector2> exploded;

		private rayDataComparer rdc;

		private List<shapeData> data = new List<shapeData>();

		public ExplosiveOld()
		{
			exploded = new Dictionary<WorldObject, Vector2>();
			rdc = new rayDataComparer();
			data = new List<shapeData>();
			emitterExplosion = (CircleEmitter)GameElementsControl.Particlesmanager.getGrenadeEmitter();
		}

		public void Explode(Vector2 pos, float radius, float maxForce)
		{
			emitterExplosion.Radius = ConvertUnits.ToDisplayUnits(radius / 4f);
			float num = ConvertUnits.ToDisplayUnits(radius) / emitterExplosion.Term;
			emitterExplosion.ReleaseSpeed = new VariableFloat
			{
				Value = num,
				Variation = num
			};
			emitterExplosion.Trigger(GameElementsControl.ConvertWorldToScreen(pos));
			exploded.Clear();
			AABB aabb = default(AABB);
			aabb.LowerBound = pos + new Vector2(0f - radius, 0f - radius);
			aabb.UpperBound = pos + new Vector2(radius, radius);
			Fixture[] shapes = new Fixture[100];
			int shapeCount = 0;
			GameElementsControl.World.QueryAABB(delegate(Fixture fixture2)
			{
				if (fixture2.UserData is WorldObjectData)
				{
					shapes[shapeCount++] = fixture2;
				}
				return true;
			}, ref aabb);
			if (false)
			{
				return;
			}
			rayData[] array = new rayData[shapeCount * 2];
			int num2 = 0;
			for (int num3 = 0; num3 < shapeCount; num3++)
			{
				PolygonShape polygonShape;
				if (shapes[num3].Shape is CircleShape circleShape)
				{
					Vertices vertices = new Vertices();
					Vector2 item = Vector2.Zero + new Vector2(circleShape.Radius, 0f);
					vertices.Add(item);
					item = Vector2.Zero + new Vector2(0f, circleShape.Radius);
					vertices.Add(item);
					item = Vector2.Zero + new Vector2(0f - circleShape.Radius, circleShape.Radius);
					vertices.Add(item);
					item = Vector2.Zero + new Vector2(0f, 0f - circleShape.Radius);
					vertices.Add(item);
					polygonShape = new PolygonShape(vertices, 1f);
				}
				else
				{
					polygonShape = shapes[num3].Shape as PolygonShape;
				}
				if (shapes[num3].Body.BodyType != BodyType.Dynamic || polygonShape == null)
				{
					continue;
				}
				Vector2 vector = shapes[num3].Body.GetWorldPoint(polygonShape.MassData.Centroid) - pos;
				float num4 = (float)Math.Atan2(vector.Y, vector.X);
				float num5 = float.MaxValue;
				float num6 = float.MinValue;
				float angle = 0f;
				float angle2 = 0f;
				Vector2 pos2 = new Vector2(-1f, -1f);
				Vector2 pos3 = new Vector2(1f, 1f);
				for (int num7 = 0; num7 < polygonShape.Vertices.Count(); num7++)
				{
					Vector2 vector2 = shapes[num3].Body.GetWorldPoint(polygonShape.Vertices[num7]) - pos;
					float num8 = (float)Math.Atan2(vector2.Y, vector2.X);
					float num9 = num8 - num4;
					num9 = (num9 - (float)Math.PI) % ((float)Math.PI * 2f);
					if (num9 < 0f)
					{
						num9 += (float)Math.PI * 2f;
					}
					num9 -= (float)Math.PI;
					if (Math.Abs(num9) > (float)Math.PI)
					{
						throw new ArgumentException("OMG!");
					}
					if (num9 > num6)
					{
						num6 = num9;
						angle2 = num8;
						pos3 = shapes[num3].Body.GetWorldPoint(polygonShape.Vertices[num7]);
					}
					if (num9 < num5)
					{
						num5 = num9;
						angle = num8;
						pos2 = shapes[num3].Body.GetWorldPoint(polygonShape.Vertices[num7]);
					}
				}
				array[num2].angle = angle;
				array[num2].pos = pos2;
				num2++;
				array[num2].angle = angle2;
				array[num2].pos = pos3;
				num2++;
			}
			Array.Sort(array, 0, num2, rdc);
			data.Clear();
			bool flag = true;
			shapeData item2 = default(shapeData);
			shapeData item3 = default(shapeData);
			for (int num10 = 0; num10 < num2; num10++)
			{
				Fixture shape = null;
				int num11 = ((num10 != num2 - 1) ? (num10 + 1) : 0);
				if (array[num10].angle == array[num11].angle)
				{
					continue;
				}
				float num12 = ((num10 != num2 - 1) ? (array[num10 + 1].angle + array[num10].angle) : (array[0].angle + (float)Math.PI * 2f + array[num10].angle));
				num12 /= 2f;
				Vector2 point = pos;
				Vector2 point2 = radius * new Vector2((float)Math.Cos(num12), (float)Math.Sin(num12)) + pos;
				float fraction = 0f;
				bool hitClosest = false;
				GameElementsControl.World.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
				{
					if (Globals.Collides(f))
					{
						hitClosest = true;
						shape = f;
						fraction = fr;
						return fr;
					}
					return -1f;
				}, point, point2);
				if (hitClosest && shape.Body.BodyType == BodyType.Dynamic)
				{
					if (data.Count() > 0 && data.Last().body == shape.Body && !flag)
					{
						int index = data.Count - 1;
						shapeData value = data[index];
						value.max = array[num11].angle;
						data[index] = value;
					}
					else
					{
						item2.body = shape.Body;
						item2.min = array[num10].angle;
						item2.max = array[num11].angle;
						data.Add(item2);
					}
					if (data.Count() > 1 && num10 == num2 - 1 && data.Last().body == data.First().body && data.Last().max == data.First().min)
					{
						shapeData value2 = data[0];
						value2.min = data.Last().min;
						data.RemoveAt(data.Count() - 1);
						data[0] = value2;
						while (data.First().min >= data.First().max)
						{
							value2.min -= (float)Math.PI * 2f;
							data[0] = value2;
						}
					}
					int index2 = data.Count - 1;
					shapeData value3 = data[index2];
					while (data.Count() > 0 && data.Last().min >= data.Last().max)
					{
						value3.min = data.Last().min - (float)Math.PI * 2f;
						data[index2] = value3;
					}
					flag = false;
					continue;
				}
				if (data.Count() > 0 && flag && data.Last().body == null)
				{
					int index3 = data.Count - 1;
					shapeData value4 = data[index3];
					value4.max = array[num11].angle;
					data[index3] = value4;
				}
				else
				{
					item3.body = null;
					item3.min = array[num10].angle;
					item3.max = array[num11].angle;
					data.Add(item3);
				}
				if (data.Count() > 1 && num10 == num2 - 1 && data.First().body == null && data.Last().max == data.First().min)
				{
					shapeData value5 = data[0];
					value5.min = data.Last().min;
					data.RemoveAt(data.Count() - 1);
					while (data.First().min >= data.First().max)
					{
						value5.min -= (float)Math.PI * 2f;
						data[0] = value5;
					}
				}
				int index4 = data.Count - 1;
				shapeData value6 = data[index4];
				while (data.Count() > 0 && data.Last().min >= data.Last().max)
				{
					value6.min = data.Last().min - (float)Math.PI * 2f;
					data[index4] = value6;
				}
				flag = true;
			}
			RayCastInput input = default(RayCastInput);
			for (int num13 = 0; num13 < data.Count(); num13++)
			{
				float num14 = data[num13].max - data[num13].min;
				if (data[num13].body == null)
				{
					for (float num15 = data[num13].min; num15 <= data[num13].max; num15 += (float)Math.PI / 15f)
					{
					}
					continue;
				}
				float num16 = MathHelper.Min((float)Math.PI / 90f, 0.025f * num14);
				int num17 = (int)Math.Ceiling((num14 - 2f * num16 - (float)Math.PI * 4f / 15f) / ((float)Math.PI / 15f));
				if (num17 < 0)
				{
					num17 = 0;
				}
				float num18 = (num14 - num16 * 2f) / (5f + (float)num17 - 1f);
				int num19 = 0;
				for (float num20 = data[num13].min + num16; num20 <= data[num13].max; num20 += num18)
				{
					Vector2 vector3 = pos;
					Vector2 vector4 = pos + radius * new Vector2((float)Math.Cos(num20), (float)Math.Sin(num20));
					Vector2 point3 = Vector2.Zero;
					float num21 = float.MaxValue;
					List<Fixture> fixtureList = data[num13].body.FixtureList;
					for (int num22 = 0; num22 < fixtureList.Count; num22++)
					{
						Fixture fixture = fixtureList[num22];
						input.Point1 = vector3;
						input.Point2 = vector4;
						input.MaxFraction = 50f;
						if (fixture.RayCast(out var output, ref input, 0) && num21 > output.Fraction)
						{
							num21 = output.Fraction;
							point3 = output.Fraction * vector4 + (1f - output.Fraction) * vector3;
						}
						float num23 = num14 / (float)(5 + num17) * maxForce * 180f / (float)Math.PI * (1f - Math.Min(1f, num21));
						Vector2 impulse = Vector2.Dot(num23 * new Vector2((float)Math.Cos(num20), (float)Math.Sin(num20)), -output.Normal) * new Vector2((float)Math.Cos(num20), (float)Math.Sin(num20));
						data[num13].body.ApplyLinearImpulse(ref impulse, ref point3);
						int num24 = Math.Sign(pos.X - data[num13].body.Position.X);
						int num25 = Math.Sign(pos.Y - data[num13].body.Position.Y);
						if (fixture.UserData is WorldObjectData worldObjectData)
						{
							Vector2 value7 = Vector2.Zero;
							if (exploded.TryGetValue(worldObjectData.Object, out value7))
							{
								value7.X += (float)num24 * Math.Abs(impulse.X);
								value7.Y += (float)num25 * Math.Abs(impulse.Y);
								exploded[worldObjectData.Object] = value7;
							}
							else
							{
								value7.X = (float)num24 * Math.Abs(impulse.X);
								value7.Y = (float)num25 * Math.Abs(impulse.Y);
								exploded.Add(worldObjectData.Object, value7);
							}
						}
						if (num21 > 1f)
						{
							point3 = vector4;
						}
						num19++;
					}
				}
			}
			Vector2 zero = Vector2.Zero;
			foreach (WorldObject key in exploded.Keys)
			{
				zero = exploded[key];
				key.Hit(zero, Vector2.Zero, HitType.Explosion);
			}
		}
	}
}
