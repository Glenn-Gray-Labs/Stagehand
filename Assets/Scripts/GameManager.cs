using System.Collections.Generic;
using Plugins.Stagehand;
using Plugins.Stagehand.Core;
using Plugins.Stagehand.Jobs;
using UnityEngine;

public class GameManager : MonoBehaviour {
	private struct Config {
		public string Name;
	}

	static GameManager() {
		Stagehand.Do(new Job<Config>(new Log("Job")));

		Stagehand.Do(_log("IEnumerator<Job>", new Log("Job")));

		Stagehand.Do(
			new Sleep(1f).SetNext(new Log("1.0")),
			new Sleep(2f).SetNext(new Log("2.0")),
			new Sleep(0.5f).SetNext(new Log("0.5"))
		);
	}

	private static IEnumerator<Job> _log(string message, Job job = null) {
		Debug.Log(message);
		yield return job;
	}
}