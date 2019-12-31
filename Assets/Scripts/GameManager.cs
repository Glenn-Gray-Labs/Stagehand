using System.Collections;
using System.Diagnostics;
using System.Threading;
using Plugins.Stagehand;
using Plugins.Stagehand.Types.Threads;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {
	private class Config {
		public string ConfigName;
	}

	private struct Main {
		public string MainName;
	}

	static GameManager() {
		// 1. Load Config
		var _config = new Config();
		IEnumerator _readConfig(Config config) {
			config.ConfigName = "Config Read";
			yield break;
		}
		Stagehand<Config>.Stage(Stagehand.ConsecutiveParallelJobs(
			_log<Config>($"1"),
			_nestedLog<Main>($"2", $"3"),
			Stagehand.ConsecutiveJobs(
				Stagehand.Sleep(0.2f),
				_log<Config>($"7"),
				_nestedLog<Main>($"8", $"9")
			),
			Stagehand.ConsecutiveJobs(
				Stagehand.Sleep(0.1f),
				_log<Config>($"4"),
				_nestedLog<Main>($"5", $"6")
			),
			Stagehand.ConsecutiveJobs(Stagehand.Sleep(0.3f), _readConfig(_config))
		));

		// 2. Thread Affinity
		Stagehand<IThreadMain>.Stage<MonoBehaviour>(_log<MonoBehaviour>("Main Thread #1"));
		Stagehand<MonoBehaviour>.Stage(_nestedLog<MonoBehaviour>("Main Thread #2", "Main Thread Nested!"));

		// 3. Load Main
		IEnumerator _processConfig(Config config) {
			Log($"_processConfig Started: {config.ConfigName}");
			while (config.ConfigName == null) {
				yield return null;
			}
			Log($"_processConfig Finished: {config.ConfigName}");
		}
		Stagehand<Config>.Stage<Main>(_processConfig(_config));
	}

	private static readonly long _firstTime = Stopwatch.GetTimestamp();
	private static long _lastTime = _firstTime;

	public static void Log(string message) {
		var thisTime = Stopwatch.GetTimestamp();
		Debug.Log($"{Thread.CurrentThread.Name} // {thisTime - _firstTime} ({thisTime - _lastTime}):\t{message}");
		_lastTime = thisTime;
	}

	private static IEnumerator _log<T>(string message) {
		Log($"_log: {typeof(T)}: {message}");
		yield break;
	}

	private static IEnumerator _nestedLog<T>(string message, string nestedMessage) {
		Log($"_nestedLog: {typeof(T)}: {message}");

		IEnumerator InnerLog() {
			Log($"_nestedLog: {typeof(T)}: {message}: {nestedMessage}");
			yield break;
		}

		yield return InnerLog();
	}
}