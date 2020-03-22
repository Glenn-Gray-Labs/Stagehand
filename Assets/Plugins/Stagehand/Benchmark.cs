using System;
using System.Diagnostics;

namespace Stagehand {
	public static class Benchmark {
#if DEBUG
		public class BenchmarkConfig {
			public int Iterations = 1000000;
		}
		private static readonly BenchmarkConfig _defaultBenchmarkConfig = new BenchmarkConfig();

		public static void Run(Action action, BenchmarkConfig benchmarkConfig = null) {
			if (benchmarkConfig == null) benchmarkConfig = _defaultBenchmarkConfig;
			var stopwatch = Stopwatch.StartNew();
			for (var i = 0; i < benchmarkConfig.Iterations; ++i) {
				action();
			}
			stopwatch.Stop();
			UnityEngine.Debug.Log($"{benchmarkConfig.Iterations}x in {stopwatch.ElapsedMilliseconds / 1000f}s");
		}
#endif
	}
}