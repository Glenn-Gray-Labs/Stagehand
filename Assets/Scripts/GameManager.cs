using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Plugins.Stagehand;
using Plugins.Stagehand.Core;
using Plugins.Stagehand.Jobs;
using Plugins.Stagehand.Types.Threads;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
			_log<Config>($"Config({Thread.CurrentThread.Name})"), // test logging
			new ReadFromCache<Config>(), // if SomeUsefulType has already been cached, immediately resolve the type so that listeners can start processing until the download completes.
			new SequentialJobs<Config>(new Sleep<Config>(0.2f), new RestartJob<Config>()), // if 10 seconds pass, restart parent job.
			new SequentialJobs<Config>(new Download<Config>(), new WriteToCache<Config>()) // this could be extended to: download headers first, check cache to see if the date/time/size/etc have changed, if so, continue to download the body, and then finally write new results to cache as well as (re-)triggering any listeners of SomeUsefulType.
		));

		// 2. Main Depends on Config; Execute After Config Job Finishes
		Stagehand<Config, Main>.Stage(_nestedLog<Main>($"Main({Thread.CurrentThread.Name})", $"InnerMain({Thread.CurrentThread.Name})"));

		// 3. Main Has Work Which Depends on Config; Execute After Config Job Finishes
		Stagehand<Main>.Stage(new ParallelJobs<Main>(
			_log<Main>($"Main({Thread.CurrentThread.Name})"), // test logging
			new ReadFromCache<Main>(), // if SomeUsefulType has already been cached, immediately resolve the type so that listeners can start processing until the download completes.
			new SequentialJobs<Main>(new Sleep<Main>(0.1f), new RestartJob<Main>()), // if 10 seconds pass, restart parent job.
			new SequentialJobs<Main>(new Download<Main>(), new WriteToCache<Main>()) // this could be extended to: download headers first, check cache to see if the date/time/size/etc have changed, if so, continue to download the body, and then finally write new results to cache as well as (re-)triggering any listeners of SomeUsefulType.
		));

		// 4. MonoBehaviour Depends on IThreadMain; Executed by a GameObject in User's App
		Stagehand<IThreadMain, MonoBehaviour>.Stage(new ParallelJobs<MonoBehaviour>(
			_log<MonoBehaviour>($"MonoBehaviour({Thread.CurrentThread.Name})") // test thread affinity
		));
	}

	private static long _firstTime = Stopwatch.GetTimestamp();

	public static void Log(string message) {
		var thisTime = Stopwatch.GetTimestamp();
		Debug.Log($"{thisTime - _firstTime}:\t{message}");
	}

	private static IEnumerator<Job<T>> _log<T>(string message) {
		Log($"_log({Thread.CurrentThread.Name}): {typeof(T)}: {message}");
		yield break;
	}

	private static IEnumerator _nestedLog<T>(string message, string nestedMessage) {
		Log($"_nestedLog({Thread.CurrentThread.Name}): {typeof(T)}: {message}");

		IEnumerator InnerLog() {
			Log($"_nestedLog({Thread.CurrentThread.Name}): {typeof(T)}: {nestedMessage}");
			yield break;
		}

		yield return InnerLog();
	}
}