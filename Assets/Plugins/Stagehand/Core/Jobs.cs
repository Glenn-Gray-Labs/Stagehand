using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class Jobs : IWork {
		private readonly Queue<IWork> _works;
		private int _counter;

		private IWork _then;

		object IEnumerator.Current => null;

		public Jobs(params IWork[] works) {
			_works = new Queue<IWork>(works);
			_counter = _works.Count;
		}

		public bool MoveNext() {
			if (_counter == 0) return false;

			do {
				var work = _works.Dequeue();
				if (work.MoveNext()) _works.Enqueue(work);
			} while (--_counter > 0);

			_counter = _works.Count;
			if (_counter > 0 || _then == null) return true;

			_works.Enqueue(_then);
			_counter = 1;
			return true;
		}

		public void Reset() {
			_counter = 0;
			_works.Clear();
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