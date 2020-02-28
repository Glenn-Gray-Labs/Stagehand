using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Plugins.Backstage;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {
	public class Config {
		public string ConfigName;
	}

	public static IEnumerator Sleep(float durationInSeconds) {
		var endTime = Stopwatch.GetTimestamp() + (long) (10000000L * durationInSeconds);
		while (Stopwatch.GetTimestamp() < endTime) yield return null;
	}

	private class Main {
		public string MainName;
	}

	GameManager() {
		IEnumerator _watchFile(string filename) {
			Debug.Log($"beg_watchFile({filename})");
			yield break;
		}

		IEnumerator _downloadUri(string uri) {
			Debug.Log($"beg_downloadUri({uri})");
			yield return _watchFile(uri);
			Debug.Log($"end_downloadUri({uri})");
		}

		IEnumerator _parseConfig(Config cfg) {
			/*if (cfg.ConfigName == null) {
				cfg.ConfigName = Stopwatch.GetTimestamp().ToString();
			} else {
				cfg.ConfigName += $"\n{Stopwatch.GetTimestamp()}";
			}*/
			//Debug.Log($"_parseConfig({cfg.ConfigName})");
			yield break;
		}

		// MonoBehaviour
		//Stage<MonoBehaviour>.Hand(_downloadUri("Stage<MonoBehaviour>.Hand(_downloadUri)"));

		int inCounter = 0;
		int refCounter = 0;
		int plainCounter = 0;

		float inTimer = 0f;
		float refTimer = 0f;
		float plainTimer = 0f;

		// Config
		var end = Stopwatch.GetTimestamp() + 10000000L;
		while (Stopwatch.GetTimestamp() < end) {
			++inCounter;
			Stage<Config>.Hand((in Config cfg) => _parseConfig(cfg));
		}
		var realEnd = Stopwatch.GetTimestamp();
		inTimer += (realEnd - (end - 10000000f)) / 10000000f;

		// power nap
		Thread.Sleep(10);

		// ref
		end = Stopwatch.GetTimestamp() + 10000000L;
		while (Stopwatch.GetTimestamp() < end) {
			++refCounter;
			Stage<Config>.Hand((ref Config cfg) => _parseConfig(cfg));
		}
		realEnd = Stopwatch.GetTimestamp();
		refTimer += (realEnd - (end - 10000000f)) / 10000000f;

		// power nap
		Thread.Sleep(10);

		// plain
		end = Stopwatch.GetTimestamp() + 10000000L;
		while (Stopwatch.GetTimestamp() < end) {
			++plainCounter;
			Stage<Config>.Hand(_parseConfig);
		}
		realEnd = Stopwatch.GetTimestamp();
		plainTimer += (realEnd - (end - 10000000f)) / 10000000f;

		// power nap
		Thread.Sleep(10);

		// Stats
		Debug.LogWarning($"in | {inCounter}x | {inTimer}s");
		Debug.LogWarning($"ref | {refCounter}x | {refTimer}s");
		Debug.LogWarning($"plain | {plainCounter}x | {plainTimer}s");
		Debug.LogWarning($"in-ref:{inCounter - refCounter} in-pl:{inCounter - plainCounter} ref-pl:{refCounter - plainCounter}");

		// Main
		/*Stage<Main>.Hand(_watchFile("Stage<Main>.Hand(_watchFile)"));
		Stage<Main>.Hand(_downloadUri("Stage<Main>.Hand(_downloadUri)"));*/

		// 1. Load Config
		/*var _config = new Config();
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
		Stagehand<MonoBehaviour>.Stage(_nestedLog<MonoBehaviour>("Main Thread #2", "Main Thread #3: Nested!"));

		// 3. Load Main
		IEnumerator _processConfig(Config config) {
			_log($"_processConfig Started: {config.ConfigName}");
			while (config.ConfigName == null) {
				yield return null;
			}
			_log($"_processConfig Finished: {config.ConfigName}");
		}
		Stagehand<Config>.Stage<Main>(_processConfig(_config));*/
	}

	/*private static readonly long _firstTime = Stopwatch.GetTimestamp();
	private static long _lastTime = _firstTime;

	private static void _log(string message) {
		var thisTime = Stopwatch.GetTimestamp();
		Debug.Log($"{Thread.CurrentThread.Name} // {thisTime - _firstTime} ({thisTime - _lastTime}):\t{message}");
		_lastTime = thisTime;
	}

	private static IEnumerator _log<T>(string message) {
		_log($"_log: {typeof(T)}: {message}");
		yield break;
	}

	private static IEnumerator _nestedLog<T>(string message, string nestedMessage) {
		_log($"_nestedLog: {typeof(T)}: {message}");

		IEnumerator InnerLog() {
			_log($"_nestedLog: {typeof(T)}: {message}: {nestedMessage}");
			yield break;
		}

		yield return InnerLog();
	}*/
}