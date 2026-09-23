using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.Graphics
{
	public class Camera2D
	{
		private const float _minZoom = 0.02f;

		private const float _maxZoom = 20f;

		private static GraphicsDevice _graphics;

		private Matrix _batchView;

		private Vector2 _currentPosition;

		private float _currentRotation;

		private float _currentZoom;

		private Vector2 _maxPosition;

		private float _maxRotation;

		private Vector2 _minPosition;

		private float _minRotation;

		private bool _positionTracking;

		private Matrix _projection;

		private bool _rotationTracking;

		private Vector2 _targetPosition;

		private float _targetRotation;

		private Body _trackingBody;

		private Vector2 _translateCenter;

		private Matrix _view;

		private Vector2 halfSize;

		public Matrix View => _batchView;

		public Matrix SimView => _view;

		public Matrix SimProjection => _projection;

		public Vector2 HalfSize => halfSize;

		public Vector2 CurrentSize => Vector2.Multiply(HalfSize * 2f, 1f / _currentZoom);

		public Vector2 Position
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(_currentPosition);
			}
			set
			{
				_targetPosition = ConvertUnits.ToSimUnits(value);
				if (_minPosition != _maxPosition)
				{
					Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out _targetPosition);
				}
			}
		}

		public Vector2 MinPosition
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(_minPosition);
			}
			set
			{
				_minPosition = ConvertUnits.ToSimUnits(value);
			}
		}

		public Vector2 MaxPosition
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(_maxPosition);
			}
			set
			{
				_maxPosition = ConvertUnits.ToSimUnits(value);
			}
		}

		public float Rotation
		{
			get
			{
				return _currentRotation;
			}
			set
			{
				_targetRotation = value % ((float)Math.PI * 2f);
				if (_minRotation != _maxRotation)
				{
					_targetRotation = MathHelper.Clamp(_targetRotation, _minRotation, _maxRotation);
				}
			}
		}

		public float MinRotation
		{
			get
			{
				return _minRotation;
			}
			set
			{
				_minRotation = MathHelper.Clamp(value, -(float)Math.PI, 0f);
			}
		}

		public float MaxRotation
		{
			get
			{
				return _maxRotation;
			}
			set
			{
				_maxRotation = MathHelper.Clamp(value, 0f, (float)Math.PI);
			}
		}

		public float Zoom
		{
			get
			{
				return _currentZoom;
			}
			set
			{
				_currentZoom = value;
				_currentZoom = MathHelper.Clamp(_currentZoom, 0.02f, 20f);
			}
		}

		public Body TrackingBody
		{
			get
			{
				return _trackingBody;
			}
			set
			{
				_trackingBody = value;
				if (_trackingBody != null)
				{
					_positionTracking = true;
				}
			}
		}

		public bool EnablePositionTracking
		{
			get
			{
				return _positionTracking;
			}
			set
			{
				if (value && _trackingBody != null)
				{
					_positionTracking = true;
				}
				else
				{
					_positionTracking = false;
				}
			}
		}

		public bool EnableRotationTracking
		{
			get
			{
				return _rotationTracking;
			}
			set
			{
				if (value && _trackingBody != null)
				{
					_rotationTracking = true;
				}
				else
				{
					_rotationTracking = false;
				}
			}
		}

		public bool EnableTracking
		{
			set
			{
				EnablePositionTracking = value;
				EnableRotationTracking = value;
			}
		}

		public Camera2D(GraphicsDevice graphics)
		{
			_graphics = graphics;
			_view = Matrix.Identity;
			_batchView = Matrix.Identity;
			RefreshUnits();
			halfSize = new Vector2(_graphics.Viewport.Width, _graphics.Viewport.Height) / 2f;
			ResetCamera();
		}

		/// <summary>
		/// Recomputes the parts that depend on the display-to-sim ratio. Must be
		/// called after ConvertUnits.SetDisplayUnitToSimUnitRatio changes.
		/// </summary>
		public void RefreshUnits()
		{
			_projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(_graphics.Viewport.Width), ConvertUnits.ToSimUnits(_graphics.Viewport.Height), 0f, 0f, 1f);
			_translateCenter = new Vector2(ConvertUnits.ToSimUnits((float)_graphics.Viewport.Width / 2f), ConvertUnits.ToSimUnits((float)_graphics.Viewport.Height / 2f));
		}

		public void MoveCamera(Vector2 amount)
		{
			_currentPosition += amount;
			if (_minPosition != _maxPosition)
			{
				Vector2.Clamp(ref _currentPosition, ref _minPosition, ref _maxPosition, out _currentPosition);
			}
			_targetPosition = _currentPosition;
			_positionTracking = false;
			_rotationTracking = false;
		}

		public void RotateCamera(float amount)
		{
			_currentRotation += amount;
			if (_minRotation != _maxRotation)
			{
				_currentRotation = MathHelper.Clamp(_currentRotation, _minRotation, _maxRotation);
			}
			_targetRotation = _currentRotation;
			_positionTracking = false;
			_rotationTracking = false;
		}

		public void ResetCamera()
		{
			_currentPosition = Vector2.Zero;
			_targetPosition = Vector2.Zero;
			_minPosition = Vector2.Zero;
			_maxPosition = Vector2.Zero;
			_currentRotation = 0f;
			_targetRotation = 0f;
			_minRotation = -(float)Math.PI;
			_maxRotation = (float)Math.PI;
			_positionTracking = false;
			_rotationTracking = false;
			_currentZoom = 1f;
			SetView();
		}

		public void Jump2Target()
		{
			_currentPosition = _targetPosition;
			_currentRotation = _targetRotation;
			SetView();
		}

		private void SetView()
		{
			Matrix matrix = Matrix.CreateRotationZ(_currentRotation);
			Matrix matrix2 = Matrix.CreateScale(_currentZoom);
			Vector3 vector = new Vector3(_translateCenter, 0f);
			Vector3 vector2 = new Vector3(-_currentPosition, 0f);
			_view = Matrix.CreateTranslation(vector2) * matrix * matrix2 * Matrix.CreateTranslation(vector);
			vector = ConvertUnits.ToDisplayUnits(vector);
			vector2 = ConvertUnits.ToDisplayUnits(vector2);
			vector = new Vector3((int)vector.X, (int)vector.Y, (int)vector.Z);
			vector2 = new Vector3((int)vector2.X, (int)vector2.Y, (int)vector2.Z);
			_batchView = Matrix.CreateTranslation(vector2) * matrix * matrix2 * Matrix.CreateTranslation(vector);
		}

		public void Update(GameTime gameTime)
		{
			if (_trackingBody != null)
			{
				if (_positionTracking)
				{
					_targetPosition = _trackingBody.Position;
					if (_minPosition != _maxPosition)
					{
						Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out _targetPosition);
					}
				}
				if (_rotationTracking)
				{
					_targetRotation = (0f - _trackingBody.Rotation) % ((float)Math.PI * 2f);
					if (_minRotation != _maxRotation)
					{
						_targetRotation = MathHelper.Clamp(_targetRotation, _minRotation, _maxRotation);
					}
				}
			}
			Vector2 vector = _targetPosition - _currentPosition;
			float num = vector.Length();
			if (num > 0f)
			{
				vector /= num;
			}
			float num2 = ((!(num < 10f)) ? 1f : ((float)Math.Pow((double)num / 10.0, 2.0)));
			float num3 = _targetRotation - _currentRotation;
			float num4 = ((!(Math.Abs(num3) < 5f)) ? 1f : ((float)Math.Pow((double)num3 / 5.0, 2.0)));
			if (Math.Abs(num3) > 0f)
			{
				num3 /= Math.Abs(num3);
			}
			if (num > 0.2f)
			{
				_currentPosition += 100f * vector * num2 * (float)gameTime.ElapsedGameTime.TotalSeconds;
			}
			else
			{
				_currentPosition = _targetPosition;
			}
			_currentRotation += 80f * num3 * num4 * (float)gameTime.ElapsedGameTime.TotalSeconds;
			SetView();
		}

		public Vector2 ConvertScreenToWorld(Vector2 location)
		{
			Vector3 source = new Vector3(location, 0f);
			source = _graphics.Viewport.Unproject(source, _projection, _view, Matrix.Identity);
			return new Vector2(source.X, source.Y);
		}

		public Vector2 ConvertWorldToScreen(Vector2 location)
		{
			Vector3 source = new Vector3(location, 0f);
			source = _graphics.Viewport.Project(source, _projection, _view, Matrix.Identity);
			return new Vector2(source.X, source.Y);
		}
	}
}
