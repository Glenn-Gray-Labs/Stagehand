using System.Collections;
using System.Collections.Generic;
using Plugins.Stagehand;
using Plugins.Stagehand.Core;
using Plugins.Stagehand.Jobs;
using Plugins.Stagehand.Types.Threads;
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
		// 1. Load the Config
		Stagehand<Config>.Stage(new ParallelJobs<Config>(
			_log<Config>("Config"), // test logging
			new ReadFromCache<Config>(), // if SomeUsefulType has already been cached, immediately resolve the type so that listeners can start processing until the download completes.
			new SequentialJobs<Config>(new Sleep<Config>(2f), new RestartJob<Config>()), // if 10 seconds pass, restart parent job.
			new SequentialJobs<Config>(new Download<Config>(), new WriteToCache<Config>()) // this could be extended to: download headers first, check cache to see if the date/time/size/etc have changed, if so, continue to download the body, and then finally write new results to cache as well as (re-)triggering any listeners of SomeUsefulType.
		));

		// 2. Main Depends on Config; Execute After Config Job Finishes
		Stagehand<Config, Main>.Stage(_nestedLog<Main>("Main", "InnerMain"));

		// 3. Main Has Work Which Depends on Config; Execute After Config Job Finishes
		Stagehand<Main>.Stage(new ParallelJobs<Main>(
			_log<Main>("Main"), // test logging
			new ReadFromCache<Main>(), // if SomeUsefulType has already been cached, immediately resolve the type so that listeners can start processing until the download completes.
			new SequentialJobs<Main>(new Sleep<Main>(1f), new RestartJob<Main>()), // if 10 seconds pass, restart parent job.
			new SequentialJobs<Main>(new Download<Main>(), new WriteToCache<Main>()) // this could be extended to: download headers first, check cache to see if the date/time/size/etc have changed, if so, continue to download the body, and then finally write new results to cache as well as (re-)triggering any listeners of SomeUsefulType.
		));

		// 4. MonoBehaviour Depends on IThreadMain; Executed by a MonoBehaviour in User's App
		//Stagehand<IThreadMain, MonoBehaviour>.Stage();
		Stagehand<MonoBehaviour>.Stage(new ParallelJobs<MonoBehaviour>(
			_log<MonoBehaviour>("MonoBehaviour") // test thread affinity
		));
	}

	private void Update() {
		Stagehand<IThreadMain>.Execute();
	}

	private static IEnumerator<Job<T>> _log<T>(string message) {
		Debug.Log($"_log: {typeof(T)}: {message}");
		yield break;
	}

	private static IEnumerator _nestedLog<T>(string message, string nestedMessage) {
		Debug.Log($"_nestedLog: {typeof(T)}: {message}");

		IEnumerator InnerLog() {
			Debug.Log($"_nestedLog: {typeof(T)}: {nestedMessage}");
			yield break;
		}

		yield return InnerLog();
	}
}