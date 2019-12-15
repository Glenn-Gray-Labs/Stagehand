using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class Job : IEnumerator<Job> {
		private readonly IEnumerator<Job> _current;
		protected IEnumerator<Job> next;

		public Job Current => null;
		object IEnumerator.Current => null;

		protected Job() {
			_current = this;
		}

		public virtual bool MoveNext() {
			if (next == null) return false;
			return next.MoveNext();
		}

		public virtual void Reset() {
			_current.Reset();
		}

		public virtual Job SetNext(IEnumerator<Job> work) {
			next = work;
			return this;
		}

		public virtual IEnumerator<Job> GetNext() {
			return next;
		}

		public virtual void Dispose() {
			//
		}
	}
}