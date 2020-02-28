using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plugins.Backstage;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {
	public class Config {
		public readonly Queue<long> times = new Queue<long>();

		public void Time(long time) {
			times.Enqueue(time);
		}

		public string Times() {
			return string.Join(", ", times.Select(x => x.ToString()).ToArray());
		}
	}

	public static IEnumerator Sleep(float durationInSeconds) {
		var endTime = Stopwatch.GetTimestamp() + (long) (10000000L * durationInSeconds);
		while (Stopwatch.GetTimestamp() < endTime) yield return null;
	}

	private class Main {
		public string MainName;
	}

	static GameManager() {
		IEnumerator _log(string message) {
			Debug.Log(message);
			yield break;
		}
		
		IEnumerator _watchFile(string filename) {
			yield return _log($"beg_watchFile({filename})");
			yield return Sleep(0.1f);
			yield return _log($"end_watchFile({filename})");
		}

		IEnumerator _downloadUri(string uri) {
			yield return _log($"beg_downloadUri({uri})");
			yield return _watchFile(uri);
			yield return _log($"end_downloadUri({uri})");
		}

		IEnumerator _parseConfig(Config cfg) {
			cfg.Time(Stopwatch.GetTimestamp());
			yield return _log($"_parseConfig({cfg.Times()})");
		}

		// MonoBehaviour
		Stage<MonoBehaviour>.Hand((ref MonoBehaviour monoBehaviour) => _downloadUri("Stage<MonoBehaviour>.Hand(_downloadUri)"));

		// Config
		var localConfig = new Config();
		Stage<Config>.Hand(ref localConfig);
		Stage<Config>.Hand(_parseConfig(localConfig));
		Stage<Config>.Hand((ref Config config) => _parseConfig(config));
		Stage<Config>.Hand((ref Config config) => _parseConfig(config));
		Stage<Config>.Hand((ref Config config) => _parseConfig(config));

		// Main
		Stage<Main>.Hand((ref Main main) => _watchFile("Stage<Main>.Hand(_watchFile)"));
		Stage<Main>.Hand((ref Main main) => _downloadUri("Stage<Main>.Hand(_downloadUri)"));

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
}