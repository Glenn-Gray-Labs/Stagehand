using Plugins.Stagehand.Core;

namespace Plugins.Stagehand {
	public static class Stagehand {
		public static void Do(Job job) {
			void Run() {
				while (job.MoveNext()) {
					//
				}

				/*var then = job.GetThen();
				if (then != null) job = then;*/
			}

			// Main Thread
			Run();

			// New Thread
			//new Thread(Run).Start();
		}

		public static void Do(params Job[] jobs) {
			Do(new Jobs(jobs));
		}
	}
}