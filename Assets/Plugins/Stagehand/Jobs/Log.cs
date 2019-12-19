using Plugins.Stagehand.Core;

namespace Plugins.Stagehand.Jobs {
	public class Log<T> : Job<T> {
		private readonly string _message;

		public Log(string message) {
			_message = message;
		}

		public override bool MoveNext() {
			UnityEngine.Debug.Log(_message);
			return false;
		}
	}
}