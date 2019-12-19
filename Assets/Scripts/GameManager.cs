using System.Collections.Generic;
using Plugins.Stagehand;
using Plugins.Stagehand.Core;
using Plugins.Stagehand.Jobs;
using UnityEngine;

public class GameManager : MonoBehaviour {
	private struct Config {
		public string ConfigName;
	}

	private struct Main {
		public string MainName;
	}

	private class ReadFromCache<T> : Job<T> {
		//
	}

	private class RestartJob<T> : Job<T> {
		//
	}

	private class Download<T> : Job<T> {
		//
	}

	private class WriteToCache<T> : Job<T> {
		//
	}

	static GameManager() {
		Stagehand.Subscribe<Config>(new ParallelJobs<Config>(
			_log("log"), // test logging
			new ReadFromCache<Config>(), // if SomeUsefulType has already been cached, immediately resolve the type so that listeners can start processing until the download completes.
			new SequentialJobs<Config>(new Sleep<Config>(10f), new RestartJob<Config>()), // if 10 seconds pass, restart parent job.
			new SequentialJobs<Config>(new Download<Config>(), new WriteToCache<Config>()) // this could be extended to: download headers first, check cache to see if the date/time/size/etc have changed, if so, continue to download the body, and then finally write new results to cache as well as (re-)triggering any listeners of SomeUsefulType.
		));
	}

	private static IEnumerator<Job<Config>> _log(string message) {
		Debug.Log(message);
		yield break;
	}
}