using UnityEngine;

namespace Plugins.Backstage.Vendors.Unity {
	public class StagehandMonoBehaviour : MonoBehaviour {
		// Living and Breathing Code
		//
		// If you build it with no uncovered edge cases,
		// it will do exactly what you programmed it to do...
		// every time.
		//
		// Therefore, we always execute the code as its written
		// in order to immediately expose edge cases, as they
		// come up, and they cannot be ignored.
#if UNITY_EDITOR
		private void OnValidate() {
			StartCoroutine(Stage<MonoBehaviour>.Hand());
		}
#else
		private void OnEnable() {
			StartCoroutine(Hand<MonoBehaviour>.To());
		}
#endif
	}
}