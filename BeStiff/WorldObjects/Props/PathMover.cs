using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Be_Stiff.WorldObjects.Props
{
	/// <summary>
	/// Moves a kinematic body back and forth along a path of points, easing in
	/// and out of each stop and waiting there, with a looping sound while it
	/// moves. Shared by the moving platforms and the elevator.
	/// </summary>
	internal sealed class PathMover
	{
		private readonly Body body;

		private readonly Vector2[] path;

		private readonly float speed;

		private readonly float decelerateDistance;

		private readonly float deceleration;

		private readonly double timeInStop;

		private readonly string loopSound;

		private int currentPathPosition;

		private int nextPathPosition = 1;

		private int pathForward = 1;

		private double stopTime;

		private double estimatedArriveTime;

		private bool stopped;

		private int soundIndex = -1;

		/// <summary>Raised when the body reaches a path point.</summary>
		public event Action Arrived;

		/// <summary>Speed set this update (0 while stopped or waiting).</summary>
		public float CurrentSpeed { get; private set; }

		/// <summary>Direction of travel this update.</summary>
		public Vector2 Direction { get; private set; }

		/// <summary>Path point the body is stopped at, or -1 while moving.</summary>
		public int CurrentFloor => stopped ? currentPathPosition : -1;

		/// <summary>The object's position followed by its Ogmo nodes.</summary>
		public static Vector2[] BuildPath(WorldScaledOgmoObject obj)
		{
			OgmoXNA.OgmoNode[] nodes = obj.Nodes;
			Vector2[] path = new Vector2[nodes.Length + 1];
			path[0] = obj.Position;
			for (int i = 0; i < nodes.Length; i++)
			{
				path[i + 1] = nodes[i].Position;
			}
			return path;
		}

		public PathMover(Body body, Vector2[] path, float speed, float decelerateDistance, double timeInStop, string loopSound, bool startStopped)
		{
			stopped = startStopped;
			this.body = body;
			this.path = path;
			this.speed = speed;
			this.decelerateDistance = decelerateDistance;
			this.timeInStop = timeInStop;
			this.loopSound = loopSound;
			deceleration = speed * speed / (2f * decelerateDistance);
			estimatedArriveTime = GameElementsControl.CurrentTimeInMS + LegDurationMs();
		}

		public Vector2 PathPoint(int index)
		{
			return path[index];
		}

		/// <summary>
		/// Time to travel the current leg (accelerate, cruise, decelerate) plus
		/// the wait at its end.
		/// </summary>
		private double LegDurationMs()
		{
			double seconds = ((path[currentPathPosition] - path[nextPathPosition]).Length() - decelerateDistance * 2f) / speed;
			seconds += 2.0 * Math.Sqrt(2f * decelerateDistance / deceleration);
			return seconds * 1000.0 + timeInStop;
		}

		public void Update()
		{
			CurrentSpeed = 0f;
			if (stopTime + timeInStop <= GameElementsControl.CurrentTimeInMS)
			{
				if (MoveToPoint(path[nextPathPosition], path[currentPathPosition]))
				{
					int last = path.Length - 1;
					int next = currentPathPosition + pathForward;
					if (next < 0 || next > last)
					{
						pathForward = -pathForward;
					}
					nextPathPosition = (int)MathHelper.Clamp(currentPathPosition + pathForward, 0f, last);
				}
				else
				{
					stopped = false;
					UpdateLoopSound();
				}
			}
			else
			{
				stopped = true;
			}
		}

		/// <summary>
		/// Keeps the loop sound playing at a distance-based volume while moving,
		/// holding a sound slot only while the hero can hear it.
		/// </summary>
		private void UpdateLoopSound()
		{
			AudioManager audio = GameElementsControl.ScreenManager.AudioManager;
			float hearing = MathHelper.Clamp(1f - (GameElementsControl.Hero.Position - body.Position).Length() / NoiseManager.MaxDistanceToHear, 0f, 1f);
			float volume = hearing * hearing;
			if (soundIndex == -1)
			{
				if (volume > 0f && CurrentSpeed > 0f)
				{
					soundIndex = audio.PlaySoundLoop(loopSound, volume);
				}
			}
			else if (volume > 0f)
			{
				audio.SoundLoopVolume(soundIndex, volume);
			}
			else
			{
				audio.StopSoundLoop(soundIndex);
				soundIndex = -1;
			}
		}

		private bool MoveToPoint(Vector2 destPoint, Vector2 originPoint)
		{
			float frameMs = (float)GameElementsControl.LastFrameTimeInMS;
			if (frameMs <= 0f)
			{
				// Time is frozen (e.g. the "get ready" box); the speed maths below
				// divides by the frame time and would produce NaN velocities.
				return false;
			}
			float toDest = (body.Position - destPoint).Length();
			float fromOrigin = (body.Position - originPoint).Length();
			Vector2 direction = VectorUtil.SafeNormalize(destPoint - body.Position);
			if (toDest > 0f)
			{
				float currentSpeed = body.LinearVelocity.Length();
				float speedToArrive = toDest * 1000f / frameMs;
				float speedChange = deceleration * (frameMs / 1000f);
				float newSpeed;
				if (toDest < decelerateDistance)
				{
					newSpeed = currentSpeed - speedChange;
					if (newSpeed < 0f)
					{
						newSpeed = speedToArrive;
					}
				}
				else if (fromOrigin < decelerateDistance)
				{
					newSpeed = fromOrigin != 0f ? currentSpeed + speedChange : speedChange;
				}
				else
				{
					newSpeed = currentSpeed > speedToArrive ? speedToArrive : speed;
				}
				if (DebugFlags.Dump && (float.IsNaN(newSpeed) || float.IsInfinity(newSpeed) || float.IsNaN(direction.X)))
				{
					Console.Error.WriteLine($"PathMover bad velocity: speed={newSpeed} dir={direction} dest={destPoint} origin={originPoint} pos={body.Position} frameMs={frameMs}");
				}
				CurrentSpeed = newSpeed;
				Direction = direction;
				body.LinearVelocity = direction * newSpeed;
				return false;
			}
			if (currentPathPosition != nextPathPosition)
			{
				stopped = true;
				stopTime = estimatedArriveTime;
				// Timed from the leg just completed, as the original did.
				estimatedArriveTime = stopTime + LegDurationMs();
				currentPathPosition = nextPathPosition;
				body.LinearVelocity = Vector2.Zero;
				if (soundIndex != -1)
				{
					GameElementsControl.ScreenManager.AudioManager.StopSoundLoop(soundIndex);
					soundIndex = -1;
				}
				Arrived?.Invoke();
			}
			return true;
		}
	}
}
