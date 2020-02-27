using System.Collections;
using System.Collections.Generic;

namespace Plugins.Stagehand {
	public static class Hand {
		// TODO: Start Threads Here
	}

	public static class Hand<T> {
		public static T _data { get; set; }

		private static List<IEnumerator> _actions = new List<IEnumerator>();

		static Hand() {
			//
		}

		public static IEnumerator To() {
			foreach (var action in _actions) {
				yield return action;
			}
		}

		public static void To(IEnumerator action) {
			_actions.Add(action);
		}

		public static void To(IEnumerator<T> action) {
			_actions.Add(action);
		}
	}
}