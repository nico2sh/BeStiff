using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Controllers
{
	/// <summary>
	/// Defines a simple controller which triggers emitters based on a timeline.
	/// </summary>
	public sealed class TimelineController : Controller
	{
		/// <summary>
		/// Counts the total seconds.
		/// </summary>
		public float TotalSeconds { get; private set; }

		/// <summary>
		/// Gets the timeline for a single trigger.
		/// </summary>
		public List<TimelineEvent> Timeline { get; private set; }

		/// <summary>
		/// Gets or sets the queue of timeline events which are due to be processed.
		/// </summary>
		private List<TimelineEvent> EventQueue { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ProjectMercury.Controllers.TimelineController" /> class.
		/// </summary>
		public TimelineController()
		{
			Timeline = new List<TimelineEvent>();
			EventQueue = new List<TimelineEvent>();
		}

		/// <summary>
		/// Called by the particle effect when it is triggered.
		/// </summary>
		/// <param name="position">The desired position of the trigger.</param>
		/// <remarks>This method should not be called directly, the ParticleEffect class
		/// will defer control to this method when the controller is assigned.</remarks>
		protected internal override void Trigger(ref Vector2 position)
		{
			foreach (TimelineEvent item in Timeline)
			{
				EventQueue.Add(new TimelineEvent
				{
					EmitterName = item.EmitterName,
					TimeOffset = item.TimeOffset + TotalSeconds,
					TriggerPosition = position
				});
			}
		}

		/// <summary>
		/// Called by the particle effect when it is updated.
		/// </summary>
		/// <param name="deltaSeconds">Elapsed time in whole and fractional seconds.</param>
		/// <remarks>This method should not be called directly, the ParticleEffect class
		/// will defer control to this method when the controller is assigned.</remarks>
		protected internal override void Update(float deltaSeconds)
		{
			TotalSeconds += deltaSeconds;
			if (EventQueue.Count > 0)
			{
				for (int num = EventQueue.Count - 1; num >= 0; num--)
				{
					TimelineEvent timelineEvent = EventQueue[num];
					if (timelineEvent.TimeOffset <= TotalSeconds)
					{
						base.ParticleEffect[timelineEvent.EmitterName].Trigger(ref timelineEvent.TriggerPosition);
						EventQueue.RemoveAt(num);
					}
				}
			}
			base.Update(deltaSeconds);
		}
	}
}
