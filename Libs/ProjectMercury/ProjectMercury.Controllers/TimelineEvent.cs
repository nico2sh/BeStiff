using Microsoft.Xna.Framework;

namespace ProjectMercury.Controllers
{
	/// <summary>
	/// A single event on the timeline.
	/// </summary>
	public struct TimelineEvent
	{
		/// <summary>
		/// The time offset of the event in whole and fractional seconds.
		/// </summary>
		public float TimeOffset;

		/// <summary>
		/// The name of the emitter to trigger at the specified time offset.
		/// </summary>
		public string EmitterName;

		/// <summary>
		/// The position of the trigger.
		/// </summary>
		public Vector2 TriggerPosition;
	}
}
