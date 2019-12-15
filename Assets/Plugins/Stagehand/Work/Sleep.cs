using System.Collections;
using System.Diagnostics;
using Plugins.Stagehand.Core;

namespace Plugins.Stagehand.Work {
	public class Sleep : IWork {
		private long _duration;
		private long _endTime;

		private IWork _then;

		object IEnumerator.Current => null;

		public Sleep(float durationInSeconds) {
			_duration = (long) (10000000L * durationInSeconds);
			Reset();
		}

		public bool MoveNext() {
			if (Stopwatch.GetTimestamp() < _endTime) return true;
			if (_then == null) return false;
			return _then.MoveNext();
		}

		public void Reset() {
			_endTime = Stopwatch.GetTimestamp() + _duration;
		}

		public IWork Then(IWork then) {
			_then = then;
			return this;
		}

		public IWork GetThen() {
			return _then;
		}
	}
}