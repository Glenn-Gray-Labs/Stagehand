using System.Diagnostics;
using Plugins.Stagehand.Core;

namespace Plugins.Stagehand.Jobs {
	public class Sleep : Job {
		private readonly long _endTime;

		public Sleep(float durationInSeconds) {
			_endTime = Stopwatch.GetTimestamp() + (long) (10000000L * durationInSeconds);
		}

		public override bool MoveNext() {
			return Stopwatch.GetTimestamp() < _endTime;
		}
	}
}