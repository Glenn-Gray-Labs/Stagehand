using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class Job<T> : IEnumerator<Job<T>> {
		protected static T value;

		public Job<T> Current => null;
		object IEnumerator.Current => null;

		protected Job() {
			//
		}

		public virtual bool MoveNext() {
			return false;
		}

		public virtual void Reset() {
			//
		}

		public virtual void Dispose() {
			//
		}
	}
}