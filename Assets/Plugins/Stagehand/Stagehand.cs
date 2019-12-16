using System.Collections.Generic;
using Plugins.Stagehand.Core;

namespace Plugins.Stagehand {
	public static class Stagehand {
		public static void Assign<T>(params IEnumerator<Job<T>>[] jobs) {
			// Pass the Value to the Jobs
			var works = new Queue<IEnumerator<Job<T>>>(jobs);
			var counter = works.Count;

			do {
				// Execute One Part of the Larger Works
				var work = works.Dequeue();
				if (!work.MoveNext()) continue;

				// Add to the Works (TODO: This doesn't work because it doesn't nest Current below its parent.)
				//if (work.Current != null) works.Enqueue((IEnumerator<Job>) work.Current);
				works.Enqueue(work);
			} while (--counter > 0);
		}

		public static void Do(IEnumerator<Job> job) {
			// TODO: Recursive. Bad. Traditionally a Stack... perhaps a better option exists?
			void Run(IEnumerator<Job> jobEnumerator) {
				while (jobEnumerator.MoveNext()) {
					if (jobEnumerator.Current != null) Run(jobEnumerator.Current);
				}
			}

			// Main Thread
			Run(job);

			// New Thread
			//new Thread(Run).Start();
		}

		public static void Do(params IEnumerator<Job>[] jobs) {
			Do(new Core.Jobs(jobs));
		}
	}
}