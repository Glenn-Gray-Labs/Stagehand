using System.Collections;

namespace Plugins.Stagehand.Core {
	public class Job : IWork {
		private IWork _work;
		private IWork _then;

		object IEnumerator.Current => null;

		public Job(IWork work) {
			_work = work;
		}

		public bool MoveNext() {
			if (_work.MoveNext()) return true;
			if (_then == null) return true;
			_work = _then;
			_then = null;
			return _work.MoveNext();
		}

		public void Reset() {
			_work.Reset();
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