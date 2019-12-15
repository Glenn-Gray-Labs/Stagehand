using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class Jobs : Job {
		private readonly Queue<IEnumerator> _works;
		private int _counter;

		public Jobs(params Job[] works) {
			_works = new Queue<IEnumerator>(works);
			_counter = _works.Count;
		}

		public override bool MoveNext() {
			if (_counter == 0) return false;

			do {
				var work = _works.Dequeue();
				if (work.MoveNext()) _works.Enqueue(work);
			} while (--_counter > 0);

			_counter = _works.Count;
			if (_counter > 0 || then == null) return true;

			_works.Enqueue(then);
			_counter = 1;
			return true;
		}

		public override void Reset() {
			_counter = 0;
			_works.Clear();
		}
	}
}