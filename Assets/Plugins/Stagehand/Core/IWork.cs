using System.Collections.Generic;

namespace Plugins.Stagehand.Core {
	public interface IWork : IEnumerator<IWork> {
		IWork Then(IWork then);
		IWork Then(IEnumerator<IWork> then);
		IWork GetThen();
	}
}