using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class Job : IEnumerator<Job> {
		private IEnumerator<Job> _current;
		protected IEnumerator<Job> then;

		public Job Current => null;
		object IEnumerator.Current => null;

		public Job() {
			_current = this;
		}

		public Job(IEnumerator<Job> work) {
			_current = work;
		}

		public virtual bool MoveNext() {
			// TODO: This is awful. Can't we use a delegate (without indirection) instead of this concrete implementation?
			if (_current == null) {
				if (then == null) return false;
				return then.MoveNext();
			}

			if (_current.MoveNext()) return true;
			_current = null;
			return then != null;
		}

		public virtual void Reset() {
			_current.Reset();
		}

		public virtual Job Then(IEnumerator<Job> then) {
			this.then = then;
			return this;
		}

		public virtual IEnumerator<Job> GetThen() {
			return then;
		}

		public virtual void Dispose() {
			//
		}
	}
}