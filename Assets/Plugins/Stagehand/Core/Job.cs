using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	// TODO: Debugging information (like StackTrace) is lost inside a Job. Research ways to collect and display complete debugging information in DEBUG builds.
	public class Job<T> : IEnumerator<Job<T>> {
		private static T _value;

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

	public class Job<T1, T2> : IEnumerator<Job<T1, T2>> {
		protected static Job<T1> job1;
		protected static Job<T2> job2;

		public Job<T1, T2> Current => null;
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