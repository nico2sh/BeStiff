using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.old
{
	public class Camera2D
	{
		private const float SmoothingSpeed = 0.15f;

		public static Matrix View;

		public static Matrix Projection;

		private static GraphicsDevice _graphics;

		public Action ProjectionUpdated;

		public Action ViewUpdated;

		private Vector2 _position;

		private float _rotation;

		private Vector2 _targetPosition;

		private float _targetRotation;

		private bool _targetRotationReached = true;

		private bool _targetXPositionReached = true;

		private bool _targetYPositionReached = true;

		private float _targetZoom;

		private bool _targetZoomReached = true;

		private float _zoom;

		public Vector2 Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = Vector2.Clamp(value, MinPosition * Zoom, MaxPosition * Zoom);
				Resize();
			}
		}

		public float Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = MathHelper.Clamp(value, MinRotation, MaxRotation);
				Resize();
			}
		}

		public float Zoom
		{
			get
			{
				return _zoom;
			}
			set
			{
				_zoom = MathHelper.Clamp(value, MinZoom, MaxZoom);
				Resize();
			}
		}

		public Vector2 CenterPosition
		{
			get
			{
				return _position + CurrentSize / 2f;
			}
			set
			{
				_position = Vector2.Clamp(value - CurrentSize / 2f, MinPosition * Zoom, MaxPosition * Zoom);
				Resize();
			}
		}

		public float MaxZoom { get; set; }

		public float MinZoom { get; set; }

		public float RotationRate { get; set; }

		public float ZoomRate { get; set; }

		public Vector2 ScreenCenter => new Vector2((float)_graphics.Viewport.Width / 2f, (float)_graphics.Viewport.Height / 2f);

		public int ScreenWidth => _graphics.Viewport.Width;

		public int ScreenHeight => _graphics.Viewport.Height;

		public Vector2 CurrentSize => Vector2.Multiply(new Vector2(_graphics.Viewport.Width, _graphics.Viewport.Height), 1f / _zoom);

		public Vector2 MinPosition { get; set; }

		public Vector2 MaxPosition { get; set; }

		public float TargetRotation
		{
			get
			{
				return _targetRotation;
			}
			set
			{
				if (_targetRotation == value)
				{
					_targetRotationReached = true;
					return;
				}
				_targetRotation = value;
				_targetRotationReached = false;
			}
		}

		public float TargetZoom
		{
			get
			{
				return _targetZoom;
			}
			set
			{
				if (_targetZoom == value)
				{
					_targetZoomReached = true;
					return;
				}
				_targetZoom = value;
				_targetZoomReached = false;
			}
		}

		public Vector2 TargetPosition
		{
			get
			{
				return _targetPosition;
			}
			set
			{
				if (_targetPosition == value)
				{
					_targetXPositionReached = true;
					_targetYPositionReached = true;
				}
				else
				{
					_targetPosition = value;
					_targetXPositionReached = false;
					_targetYPositionReached = false;
				}
			}
		}

		public float MaxRotation { get; set; }

		public float MinRotation { get; set; }

		public Vector2 MoveRate { get; set; }

		public Camera2D(GraphicsDevice graphics)
		{
			_graphics = graphics;
			Projection = Matrix.Identity;
			View = Matrix.Identity;
			CreateProjection();
			ResetCamera();
		}

		public void ZoomIn(float amount)
		{
			Zoom += amount;
		}

		public void ZoomOut(float amount)
		{
			Zoom -= amount;
		}

		public void MoveCamera(Vector2 amount)
		{
			Position += amount;
		}

		public void CreateProjection()
		{
			Projection = Matrix.CreateOrthographicOffCenter(-25f * _graphics.Viewport.AspectRatio, 25f * _graphics.Viewport.AspectRatio, -25f, 25f, -1f, 1f);
			if (ProjectionUpdated != null)
			{
				ProjectionUpdated();
			}
		}

		public void ResetCamera()
		{
			ZoomRate = 0.1f;
			MoveRate = new Vector2(1f, 1f);
			RotationRate = 0.1f;
			MinZoom = 0.5f;
			MaxZoom = 2f;
			MinRotation = -(float)Math.PI / 2f;
			MaxRotation = (float)Math.PI / 2f;
			MaxPosition = new Vector2(25f, 25f);
			MinPosition = new Vector2(-25f, 0f);
			_targetPosition = Vector2.Zero;
			_targetRotation = 0f;
			_targetZoom = 1f;
			_zoom = 1f;
			_position = Vector2.Zero;
			_rotation = 0f;
			Resize();
		}

		public void SmoothResetCamera()
		{
			ZoomRate = 0.1f;
			MoveRate = new Vector2(1f, 1f);
			RotationRate = 0.1f;
			MinZoom = 0.5f;
			MaxZoom = 2f;
			MinRotation = -(float)Math.PI / 2f;
			MaxRotation = (float)Math.PI / 2f;
			MaxPosition = new Vector2(25f, 25f);
			MinPosition = new Vector2(-25f, 0f);
			TargetPosition = Vector2.Zero;
			TargetRotation = 0f;
			TargetZoom = 1f;
		}

		private void Resize()
		{
			View = Matrix.CreateRotationZ(_rotation) * Matrix.CreateTranslation(-(int)_position.X, -(int)_position.Y, 0f) * Matrix.CreateScale(_zoom);
			if (ViewUpdated != null)
			{
				ViewUpdated();
			}
		}

		public void Update()
		{
			if (!_targetYPositionReached)
			{
				if (TargetPosition.X > Position.X)
				{
					float value = Math.Min(MaxPosition.X * Zoom, _position.X + MoveRate.X);
					Position = new Vector2(MathHelper.SmoothStep(_position.X, value, 0.15f), Position.Y);
					if (Position.X >= TargetPosition.X)
					{
						_targetYPositionReached = true;
					}
				}
				else if (TargetPosition.X < Position.X)
				{
					float value = Math.Max(MinPosition.X * Zoom, _position.X - MoveRate.X);
					Position = new Vector2(MathHelper.SmoothStep(_position.X, value, 0.15f), Position.Y);
					if (Position.X <= TargetPosition.X)
					{
						_targetYPositionReached = true;
					}
				}
			}
			if (!_targetXPositionReached)
			{
				if (TargetPosition.Y > Position.Y)
				{
					float value2 = Math.Min(MaxPosition.Y * Zoom, _position.Y + MoveRate.Y);
					Position = new Vector2(Position.X, MathHelper.SmoothStep(_position.Y, value2, 0.15f));
					if (Position.Y >= TargetPosition.Y)
					{
						_targetXPositionReached = true;
					}
				}
				else if (TargetPosition.Y < Position.Y)
				{
					float value2 = Math.Max(MinPosition.Y * Zoom, _position.Y - MoveRate.Y);
					Position = new Vector2(Position.X, MathHelper.SmoothStep(_position.Y, value2, 0.15f));
					if (Position.Y <= TargetPosition.Y)
					{
						_targetXPositionReached = true;
					}
				}
			}
			if (!_targetRotationReached)
			{
				if (TargetRotation > Rotation)
				{
					float value3 = Math.Min(MaxRotation, _rotation + RotationRate);
					Rotation = MathHelper.SmoothStep(_rotation, value3, 0.15f);
					if (Rotation >= TargetRotation)
					{
						_targetRotationReached = true;
					}
				}
				else if (TargetRotation < Rotation)
				{
					float value3 = Math.Max(MinRotation, _rotation - RotationRate);
					Rotation = MathHelper.SmoothStep(_rotation, value3, 0.15f);
					if (Rotation <= TargetRotation)
					{
						_targetRotationReached = true;
					}
				}
			}
			if (_targetZoomReached)
			{
				return;
			}
			if (TargetZoom > Zoom)
			{
				float value4 = Math.Min(MaxZoom, _zoom + ZoomRate);
				Zoom = MathHelper.SmoothStep(_zoom, value4, 0.15f);
				if (Zoom >= TargetZoom)
				{
					_targetZoomReached = true;
				}
			}
			else if (TargetZoom < Zoom)
			{
				float value4 = Math.Max(MinZoom, _zoom - ZoomRate);
				Zoom = MathHelper.SmoothStep(_zoom, value4, 0.15f);
				if (Zoom <= TargetZoom)
				{
					_targetZoomReached = true;
				}
			}
		}

		public static Vector2 ConvertScreenToWorld(Vector2 location)
		{
			Vector3 source = new Vector3(location, 0f);
			source = _graphics.Viewport.Unproject(source, Projection, View, Matrix.Identity);
			return new Vector2(source.X, source.Y);
		}

		public static Vector2 ConvertWorldToScreen(Vector2 location)
		{
			Vector3 source = new Vector3(location, 0f);
			source = _graphics.Viewport.Project(source, Projection, View, Matrix.Identity);
			return new Vector2(source.X, source.Y);
		}
	}
}
