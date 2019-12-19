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

	GameManager() {
		Stagehand.Subscribe<Config>(new ParallelJobs<Config>(
			_log<Config>("Config"), // test logging
			new ReadFromCache<Config>(), // if SomeUsefulType has already been cached, immediately resolve the type so that listeners can start processing until the download completes.
			new SequentialJobs<Config>(new Sleep<Config>(2f), new RestartJob<Config>()), // if 10 seconds pass, restart parent job.
			new SequentialJobs<Config>(new Download<Config>(), new WriteToCache<Config>()) // this could be extended to: download headers first, check cache to see if the date/time/size/etc have changed, if so, continue to download the body, and then finally write new results to cache as well as (re-)triggering any listeners of SomeUsefulType.
		));

		Stagehand.Subscribe<Main>(new ParallelJobs<Main>(
			_log<Main>("Main"), // test logging
			new ReadFromCache<Main>(), // if SomeUsefulType has already been cached, immediately resolve the type so that listeners can start processing until the download completes.
			new SequentialJobs<Main>(new Sleep<Main>(1f), new RestartJob<Main>()), // if 10 seconds pass, restart parent job.
			new SequentialJobs<Main>(new Download<Main>(), new WriteToCache<Main>()) // this could be extended to: download headers first, check cache to see if the date/time/size/etc have changed, if so, continue to download the body, and then finally write new results to cache as well as (re-)triggering any listeners of SomeUsefulType.
		));

		Stagehand.Setup<MonoBehaviour>(0);
		Stagehand.Subscribe<MonoBehaviour>(new ParallelJobs<MonoBehaviour>(
			_log<MonoBehaviour>("MonoBehaviour") // test thread affinity
		));
	}

	private static IEnumerator<Job<T>> _log<T>(string message) {
		Debug.Log($"{typeof(T)}: {message}");
		yield break;
	}
}