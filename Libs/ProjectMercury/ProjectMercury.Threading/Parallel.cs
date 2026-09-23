using System;
using System.Threading;

namespace ProjectMercury.Threading
{
	/// <summary>
	/// The class provides support for parallel computations, paralleling loop's iterations.
	/// </summary>
	///
	/// <remarks><para>The class allows to parallel loop's iteration computing them in separate threads,
	/// what allows their simultaneous execution on multiple CPUs/cores.
	/// </para></remarks>
	public sealed class Parallel
	{
		/// <summary>
		/// Delegate defining for-loop's body.
		/// </summary>
		///
		/// <param name="index">Loop's index.</param>
		public delegate void ForLoopBody(int index);

		private static int threadsCount = Environment.ProcessorCount;

		private static object sync = new object();

		private static volatile Parallel instance = null;

		private Thread[] threads;

		private AutoResetEvent[] jobAvailable;

		private ManualResetEvent[] threadIdle;

		private int currentIndex;

		private int stopIndex;

		private ForLoopBody loopBody;

		/// <summary>
		/// Number of threads used for parallel computations.
		/// </summary>
		///
		/// <remarks><para>The property sets how many worker threads are created for paralleling
		/// loops' computations.</para>
		///
		/// <para>By default the property is set to number of CPU's in the system
		/// (see <see cref="P:System.Environment.ProcessorCount" />).</para>
		/// </remarks>
		public static int ThreadsCount
		{
			get
			{
				return threadsCount;
			}
			set
			{
				lock (sync)
				{
					threadsCount = Math.Max(1, value);
				}
			}
		}

		private static Parallel Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Parallel();
					instance.Initialize();
				}
				else if (instance.threads.Length != threadsCount)
				{
					instance.Terminate();
					instance.Initialize();
				}
				return instance;
			}
		}

		/// <summary>
		/// Executes a for-loop in which iterations may run in parallel.
		/// </summary>
		///
		/// <param name="start">Loop's start index.</param>
		/// <param name="stop">Loop's stop index.</param>
		/// <param name="loopBody">Loop's body.</param>
		///
		/// <remarks><para>The method is used to parallel for-loop running its iterations in
		/// different threads. The <b>start</b> and <b>stop</b> parameters define loop's
		/// starting and ending loop's indexes. The number of iterations is equal to <b>stop - start</b>.
		/// </para>
		///
		/// <para>Sample usage:</para>
		/// <code>
		/// Parallel.For( 0, 20, delegate( int i )
		/// // which is equivalent to
		/// // for ( int i = 0; i &lt; 20; i++ )
		/// {
		///     System.Diagnostics.Debug.WriteLine( "Iteration: " + i );
		///     // ...
		/// } );
		/// </code>
		/// </remarks>
		public static void For(int start, int stop, ForLoopBody loopBody)
		{
			lock (sync)
			{
				Parallel parallel = Instance;
				parallel.currentIndex = start - 1;
				parallel.stopIndex = stop;
				parallel.loopBody = loopBody;
				for (int i = 0; i < threadsCount; i++)
				{
					parallel.threadIdle[i].Reset();
					parallel.jobAvailable[i].Set();
				}
				for (int j = 0; j < threadsCount; j++)
				{
					parallel.threadIdle[j].WaitOne();
				}
			}
		}

		private Parallel()
		{
		}

		private void Initialize()
		{
			jobAvailable = new AutoResetEvent[threadsCount];
			threadIdle = new ManualResetEvent[threadsCount];
			threads = new Thread[threadsCount];
			for (int i = 0; i < threadsCount; i++)
			{
				jobAvailable[i] = new AutoResetEvent(initialState: false);
				threadIdle[i] = new ManualResetEvent(initialState: true);
				threads[i] = new Thread(WorkerThread);
				threads[i].Name = "ProjectMercury.Parallel";
				threads[i].IsBackground = true;
				threads[i].Start(i);
			}
		}

		private void Terminate()
		{
			loopBody = null;
			int i = 0;
			for (int num = threads.Length; i < num; i++)
			{
				jobAvailable[i].Set();
				threads[i].Join();
				jobAvailable[i].Close();
				threadIdle[i].Close();
			}
			jobAvailable = null;
			threadIdle = null;
			threads = null;
		}

		private void WorkerThread(object index)
		{
			int num = (int)index;
			int num2 = 0;
			while (true)
			{
				jobAvailable[num].WaitOne();
				if (loopBody == null)
				{
					break;
				}
				while (true)
				{
					num2 = Interlocked.Increment(ref currentIndex);
					if (num2 >= stopIndex)
					{
						break;
					}
					loopBody(num2);
				}
				threadIdle[num].Set();
			}
		}
	}
}
