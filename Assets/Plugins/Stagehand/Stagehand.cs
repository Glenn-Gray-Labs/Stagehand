using System.Threading;
using Plugins.Stagehand.Core;

namespace Plugins.Stagehand {
	public static class Stagehand {
		public static void Do(IWork work) {
			void Run() {
				while (work.MoveNext()) {
					//
				}

				var then = work.GetThen();
				if (then != null) work = then;
			}

			// Main Thread
			//Run();

			// New Thread
			new Thread(Run).Start();
		}

		public static void Do(params IWork[] works) {
			Do(new Jobs(works));
		}
	}
}