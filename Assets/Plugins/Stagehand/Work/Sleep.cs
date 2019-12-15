using System.Diagnostics;
using Plugins.Stagehand.Core;

namespace Plugins.Stagehand.Work {
	public class Sleep : Job {
		private long _duration;
		private long _endTime;

		public Sleep(float durationInSeconds) {
			_duration = (long) (10000000L * durationInSeconds);
			_endTime = Stopwatch.GetTimestamp() + _duration;
		}

		public override bool MoveNext() {
			if (Stopwatch.GetTimestamp() < _endTime) return true;
			if (then == null) return false;
			return then.MoveNext();
		}
	}
}