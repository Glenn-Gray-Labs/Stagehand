using System.Collections;

namespace Plugins.Stagehand {
	public static class Stagehand {
		public static void Subscribe<T>(IEnumerator job) {
			// TODO: Recursive. Bad. Traditionally a Stack... perhaps a better option exists?
			void Run(object enumerator) {
				var jobEnumerator = (IEnumerator) enumerator;
				while (jobEnumerator.MoveNext()) {
					if (jobEnumerator.Current != null) Run(jobEnumerator.Current);
				}
			}

			// New Thread
			//new Thread(Run).Start(job);

			// Main Thread
			Run(job);
		}
	}
}