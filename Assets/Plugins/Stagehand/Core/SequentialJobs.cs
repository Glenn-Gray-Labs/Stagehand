using System.Collections;

namespace Plugins.Stagehand.Core {
	public class SequentialJobs<T> : Job<T> {
		private int _current = 0;
		private readonly IEnumerator[] _jobs;
		private IEnumerator _currentJob;

		public SequentialJobs(params IEnumerator[] jobs) {
			_jobs = jobs;
			_currentJob = _jobs[0];
		}

		public override bool MoveNext() {
			if (_current == _jobs.Length) return false;
			if (_currentJob.MoveNext()) return true;
			++_current;
			if (_current == _jobs.Length) return false;
			_currentJob = _jobs[_current];
			return true;
		}

		public override void Reset() {
			//
		}
	}
}