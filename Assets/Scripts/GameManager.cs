using System.Collections.Generic;
using Plugins.Stagehand;
using Plugins.Stagehand.Core;
using Plugins.Stagehand.Work;
using UnityEngine;

public class GameManager : MonoBehaviour {
	static GameManager() {
		Stagehand.Do(
			new Sleep(1f).Then(new Job(_timerFinished("1.0"))),
			new Sleep(2f).Then(new Job(_timerFinished("2.0"))),
			new Sleep(0.5f).Then(_timerFinished("0.5"))
		);
	}

	private static IEnumerator<IWork> _timerFinished(string duration) {
		Debug.Log($"Timer Finished: {duration}s");
		yield break;
	}
}