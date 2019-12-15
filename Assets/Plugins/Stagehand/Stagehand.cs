using System.Collections.Generic;
using System.Threading;
using Plugins.Stagehand.Core;

namespace Plugins.Stagehand {
	public static class Stagehand {
		public static void Do(IEnumerator<Job> job) {
			void Run() {
				while (job.MoveNext()) {
					//
				}
			}

			// Main Thread
			Run();

			// New Thread
			new Thread(Run).Start();
		}

		public static void Do(params IEnumerator<Job>[] jobs) {
			Do(new Core.Jobs(jobs));
		}
	}
}