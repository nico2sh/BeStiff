using System;
using System.Collections.Generic;
using System.Threading;
using ProjectMercury.Emitters;

namespace ProjectMercury
{
	/// <summary>
	/// Defines a class which handles updating of particle effects asynchronously.
	/// </summary>
	public class AsyncUpdateManager : IDisposable
	{
		/// <summary>
		/// Gets or sets the elapsed time in whole and fractional seconds.
		/// </summary>
		private volatile float DeltaSeconds;

		/// <summary>
		/// Notifies the worker thread when to stop running.
		/// </summary>
		private volatile bool RunWorkerThread;

		/// <summary>
		/// Gets or sets the worker thread.
		/// </summary>
		private Thread WorkerThread { get; set; }

		/// <summary>
		/// Thread event, raised when there is work available for the worker thread.
		/// </summary>
		private ManualResetEventSlim WorkAvailable { get; set; }

		/// <summary>
		/// Thread event, raised when the worker thread has finished its current work.
		/// </summary>
		private ManualResetEventSlim WorkDone { get; set; }

		/// <summary>
		/// Gets or sets the queue of particle effects which need to be updated by the worker thread.
		/// </summary>
		private Queue<ParticleEffect> WorkQueue { get; set; }

		/// <summary>
		/// Initialises a new instance of the AsyncUpdateManager class.
		/// </summary>
		public AsyncUpdateManager()
		{
			WorkerThread = new Thread(WorkerThread_Body)
			{
				Name = "AsyncUpdateManager",
				IsBackground = true
			};
			WorkAvailable = new ManualResetEventSlim(initialState: false);
			WorkDone = new ManualResetEventSlim(initialState: true);
			WorkQueue = new Queue<ParticleEffect>();
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (WorkAvailable != null)
				{
					WorkAvailable.Dispose();
				}
				if (WorkDone != null)
				{
					WorkDone.Dispose();
				}
			}
		}

		/// <summary>
		/// Dispose any unmanaged resources being used by this instance.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the <see cref="T:ProjectMercury.AsyncUpdateManager" />
		/// is reclaimed by garbage collection.
		/// </summary>
		~AsyncUpdateManager()
		{
			Dispose(disposing: false);
		}

		/// <summary>
		/// Starts the worker thread running in the background.
		/// </summary>
		public void Start()
		{
			RunWorkerThread = true;
			WorkerThread.Start();
		}

		/// <summary>
		/// Stops the worker thread.
		/// </summary>
		public void Stop()
		{
			RunWorkerThread = false;
			WorkAvailable.Set();
			WorkerThread.Join(-1);
			WorkAvailable.WaitHandle.Close();
			WorkDone.WaitHandle.Close();
		}

		/// <summary>
		/// Passes the specified particle effects to the worker thread for updating.
		/// </summary>
		/// <param name="deltaSeconds">Elapsed time in whole and fractional seconds.</param>
		/// <param name="effects">The particle effects that should be updated.</param>
		public void BeginUpdate(float deltaSeconds, params ParticleEffect[] effects)
		{
			DeltaSeconds = deltaSeconds;
			foreach (ParticleEffect item in effects)
			{
				WorkQueue.Enqueue(item);
			}
			WorkDone.Reset();
			WorkAvailable.Set();
		}

		/// <summary>
		/// Blocks the calling thread until the worker thread has finished updating outstanding particle effects.
		/// </summary>
		public void EndUpdate()
		{
			WorkDone.Wait(-1, CancellationToken.None);
			WorkAvailable.Reset();
		}

		/// <summary>
		/// Worker thread body.
		/// </summary>
		private void WorkerThread_Body()
		{
			while (RunWorkerThread)
			{
				WorkAvailable.Wait(-1, CancellationToken.None);
				lock (WorkQueue)
				{
					while (WorkQueue.Count > 0)
					{
						ParticleEffect particleEffect = WorkQueue.Dequeue();
						lock (particleEffect)
						{
							foreach (Emitter item in particleEffect)
							{
								item.Update(DeltaSeconds);
							}
						}
					}
					WorkDone.Set();
				}
			}
		}
	}
}
