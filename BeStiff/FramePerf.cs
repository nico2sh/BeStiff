using System;
using System.Diagnostics;

namespace Be_Stiff
{
	/// <summary>
	/// Debug frame profiler (BESTIFF_PERF): logs slow frames, garbage
	/// collections and allocation rates to stderr.
	/// </summary>
	internal static class FramePerf
	{
		private const double SlowFrameMs = 25.0;

		private const int SummaryFrames = 300;

		private static readonly Stopwatch clock = Stopwatch.StartNew();

		private static double lastFrameMs;

		private static long lastAllocated = GC.GetAllocatedBytesForCurrentThread();

		private static readonly int[] lastCollections = new int[3];

		private static int frames;

		private static long summaryAllocated;

		private static double summaryMaxMs;

		private static double updateMs;

		private static int updates;

		private static double drawMs;

		public static double Now => clock.Elapsed.TotalMilliseconds;

		/// <summary>Number of frames drawn so far.</summary>
		public static int Frame => frames;

		private static readonly System.Text.StringBuilder marks = new System.Text.StringBuilder();

		private static double lastMark;

		/// <summary>Records the time since the previous mark under a label (null starts a sequence).</summary>
		public static void Mark(string label)
		{
			if (!DebugFlags.Perf)
			{
				return;
			}
			double now = Now;
			if (label != null)
			{
				marks.Append(' ').Append(label).Append('=').Append((now - lastMark).ToString("F1"));
			}
			lastMark = now;
		}

		/// <summary>Adds one Update call's time (several can run per drawn frame).</summary>
		public static void AddUpdate(double ms)
		{
			updateMs += ms;
			updates++;
		}

		public static void AddDraw(double ms)
		{
			drawMs += ms;
		}

		public static void EndFrame()
		{
			double now = clock.Elapsed.TotalMilliseconds;
			double frameMs = now - lastFrameMs;
			lastFrameMs = now;
			long allocated = GC.GetAllocatedBytesForCurrentThread();
			long frameAlloc = allocated - lastAllocated;
			lastAllocated = allocated;
			string gcs = "";
			for (int gen = 0; gen < 3; gen++)
			{
				int count = GC.CollectionCount(gen);
				if (count != lastCollections[gen])
				{
					gcs += $" gen{gen}x{count - lastCollections[gen]}";
					lastCollections[gen] = count;
				}
			}
			frames++;
			summaryAllocated += frameAlloc;
			summaryMaxMs = Math.Max(summaryMaxMs, frameMs);
			if (frames > 1 && (frameMs > SlowFrameMs || gcs.Length > 0))
			{
				Console.Error.WriteLine($"perf frame {frames}: {frameMs:F1} ms (update {updateMs:F1} ms x{updates}, draw {drawMs:F1} ms, other {frameMs - updateMs - drawMs:F1} ms), alloc {frameAlloc / 1024} KB{(gcs.Length > 0 ? ", GC" + gcs : "")} |{marks}");
			}
			marks.Clear();
			updateMs = 0.0;
			updates = 0;
			drawMs = 0.0;
			if (frames % SummaryFrames == 0)
			{
				Console.Error.WriteLine($"perf summary frames {frames - SummaryFrames + 1}-{frames}: avg alloc {summaryAllocated / SummaryFrames / 1024.0:F1} KB/frame, max frame {summaryMaxMs:F1} ms, heap {GC.GetTotalMemory(false) / (1024 * 1024)} MB");
				summaryAllocated = 0;
				summaryMaxMs = 0.0;
			}
		}
	}
}
