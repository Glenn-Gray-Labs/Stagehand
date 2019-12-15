using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class Job : IWork {
		protected IWork _then;

		public IWork Current { get; set; }
		object IEnumerator.Current => null;

		public Job() {
			Current = this;
		}

		public Job(IWork work) {
			Current = work;
		}

		public Job(IEnumerator<IWork> work) {
			Current = work.Current;
		}

		public virtual bool MoveNext() {
			// TODO: This is awful. Can't we use a delegate (without indirection) instead of this concrete implementation?
			if (Current == null) {
				if (_then == null) return false;
				return _then.MoveNext();
				;
			}

			if (Current.MoveNext()) return true;
			Current = null;
			return _then != null;
		}

		public virtual void Reset() {
			Current.Reset();
		}

		public virtual IWork Then(IWork then) {
			_then = then;
			return this;
		}

		public virtual IWork Then(IEnumerator<IWork> then) {
			return Then(then.Current);
		}

		public virtual IWork GetThen() {
			return _then;
		}

		public virtual void Dispose() {
			//
		}
	}
}