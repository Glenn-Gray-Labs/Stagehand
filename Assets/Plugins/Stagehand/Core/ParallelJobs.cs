using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public class ParallelJobs<T> : Job<T> {
		private readonly Queue<IEnumerator> _jobs;
		private int _counter;

		public ParallelJobs(params IEnumerator<Job<T>>[] jobs) {
			_jobs = new Queue<IEnumerator>(jobs);
			_counter = _jobs.Count;
		}

		public override bool MoveNext() {
			if (_counter == 0) return false;

			do {
				// Execute One Part of the Larger jobs
				var job = _jobs.Dequeue();
				if (!job.MoveNext()) continue;

				// Add to the jobs (TODO: This doesn't job because it doesn't nest Current below its parent.)
				if (job.Current != null) _jobs.Enqueue((IEnumerator<Job<T>>) job.Current);
				_jobs.Enqueue(job);
			} while (--_counter > 0);

			_counter = _jobs.Count;
			return _counter > 0;
			/*if (_counter > 0 || next == null) return true;

			_jobs.Enqueue(next);
			_counter = 1;
			return true;*/
		}

		public override void Reset() {
			_counter = 0;
			_jobs.Clear();
		}
	}
}