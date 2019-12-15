using System.Collections;

namespace Plugins.Stagehand.Core {
	public class Work : IWork {
		private IEnumerator _work;
		private IWork _then;

		object IEnumerator.Current => null;

		public Work(IEnumerator work) {
			_work = work;
		}

		public bool MoveNext() {
			if (_work.MoveNext()) return true;
			if (_then == null) return false;
			_work = _then;
			return true;
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