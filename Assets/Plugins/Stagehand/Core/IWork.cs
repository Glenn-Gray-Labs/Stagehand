using System.Collections;

namespace Plugins.Stagehand.Core {
		public interface IWork : IEnumerator {
			IWork Then(IWork then);
			IWork GetThen();
		}
}