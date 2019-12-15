using System.Threading;
using Plugins.Stagehand.Core;

namespace Plugins.Stagehand {
	public static class Stagehand {
		public static void Do(params IWork[] works) {
			var work = (works.Length == 1) ? works[0] : new Jobs(works);
			void Run() {
				while (work.MoveNext()) {
					//
				}

				var then = work.GetThen();
				if (then != null) work = then;
			}

			// Main Thread
			Run();

			// New Thread
			new Thread(Run).Start();
		}
	}
}