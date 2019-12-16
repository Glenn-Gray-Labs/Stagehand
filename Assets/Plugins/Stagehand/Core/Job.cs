using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class Job : IEnumerator<Job> {
		//protected IEnumerator<Job> next;

		public Job Current { get; protected set; }

		object IEnumerator.Current => null;

		protected Job() {
			//
		}

		public virtual bool MoveNext() {
			/*if (next == null) return false;
			return next.MoveNext();*/
			return false;
		}

		public virtual void Reset() {
			//
		}

		public virtual Job SetNext(IEnumerator<Job> work) {
			//next = work;
			return this;
		}

		public virtual IEnumerator<Job> GetNext() {
			return null;
			//return next;
		}

		public virtual void Dispose() {
			//
		}
	}

	public class Job<T> : Job {
		public static T value;

		public Job(Job job) {
			Current = job;
		}
	}
}