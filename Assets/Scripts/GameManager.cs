using Plugins.Stagehand;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {
	static GameManager() {
		Stagehand.Do(new Stagehand.SimultaneousWork(
			new Stagehand.WorkTimeout(1f, () => {
				Debug.Log("Timer Finished: 1.0s");
			}),
			new Stagehand.WorkTimeout(2f, () => {
				Debug.Log("Timer Finished: 2.0s");
			}),
			new Stagehand.WorkTimeout(0.5f, () => {
				Debug.Log("Timer Finished: 0.5s");
			})
		));
	}
}