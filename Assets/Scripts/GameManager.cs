using System.Collections.Generic;
using Plugins.Stagehand;
using Plugins.Stagehand.Core;
using Plugins.Stagehand.Work;
using UnityEngine;

public class GameManager : MonoBehaviour {
	static GameManager() {
		var job = _timerFinished("0.5");

		Stagehand.Do(new Sleep(0.5f).Then(job));

		Stagehand.Do(
			new Sleep(1f).Then(_timerFinished("1.0")),
			new Sleep(2f).Then(_timerFinished("2.0")),
			new Sleep(0.5f).Then(_timerFinished("0.5"))
		);
	}

	private static IEnumerator<Job> _timerFinished(string duration) {
		Debug.Log($"Timer Finished: {duration}s");
		yield break;
	}
}