using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	// TODO: Debugging information (like StackTrace) is lost inside a Job. Research ways to collect and display complete debugging information in DEBUG builds.
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