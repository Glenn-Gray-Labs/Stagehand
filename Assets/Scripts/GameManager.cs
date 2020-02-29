using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Plugins.Backstage;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {
	public class Config {
		public readonly Queue<long> times = new Queue<long>();

		public void Time() {
			times.Enqueue(Stopwatch.GetTimestamp());
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

		IEnumerator<string> _readFile(string filename) {
			using (var sr = new StreamReader(filename)) {
				while (!sr.EndOfStream) {
					yield return sr.ReadLine();
				}
			}
		}

		IEnumerator<string> _parseJsonObject(IEnumerator<string> reader, int characterIndex) {
			while (reader.MoveNext()) {
				for (; characterIndex < reader.Current.Length; ++characterIndex) {
					switch (reader.Current[characterIndex]) {
					case '}':
						yield break;
					}
				}
			}
		}

		IEnumerator<string> _parseJson(IEnumerator<string> reader) {
			while (reader.MoveNext()) {
				foreach (var characterIndex in reader.Current) {
					switch (reader.Current[characterIndex]) {
					case '{':
						var parseObject = _parseJsonObject(reader, characterIndex);
						while (parseObject.MoveNext()) {
							Debug.Log(parseObject.Current);
						}
						break;
					}
				}
				yield return reader.Current;
			}
		}

		IEnumerator<Config> _deserializeInto(Config config, IEnumerator<string> parser) {
			while (parser.MoveNext()) {
				Debug.Log(parser.Current);
				yield return null;
			}
			yield return config;
		}

		// MonoBehaviour
		Stage<MonoBehaviour>.Hand((ref MonoBehaviour monoBehaviour) => null);

		// Config
		var localConfig = new Config();
		Stage<Config>.Hand(ref localConfig);
		Stage<Config>.Hand((ref Config config) => _deserializeInto(config, _parseJson(_readFile("Assets/Tests/backstage.json"))));

		// Config
		/*IEnumerator _parseConfig(Config cfg) {
			cfg.Time(Stopwatch.GetTimestamp());
			yield return _log($"_parseConfig({cfg.Times()})");
		}

		var localConfig = new Config();
		Stage<Config>.Hand(ref localConfig);
		Stage<Config>.Hand(_parseConfig(localConfig));
		Stage<Config>.Hand((ref Config config) => _parseConfig(config));*/

		// Main
		/*IEnumerator _runMain(Main main, Config config) {
			Debug.Log($"main:{config.Times()}");
			yield return Sleep(0.1f);
			Debug.Log(main.MainName = "MAIN");
		}

		var localMain = new Main();
		Stage<Main>.Hand(ref localMain);
		Stage<Main>.Hand((ref Main main, ref Config config) => _runMain(main, config));*/

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