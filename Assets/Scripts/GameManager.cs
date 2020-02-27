using System.Collections;
using System.Collections.Generic;
using Plugins.Stagehand;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public struct Config {
		public string ConfigName;
	}

	private struct Main {
		public string MainName;
	}

	/*private class FileWatcher : IEnumerator {
		public interface IFile {
			//
		}

		static FileWatcher() {
			Stagehand<FileWatcher>.Attach(new FileWatcher());
		}

		private FileWatcher() {
			//
		}
	}*/

	public class ConfigFile : IEnumerator<Config> {
		private string _filename;

		public ConfigFile(string filename) {
			Debug.Log($"ConfigFile({_filename})");
			_filename = filename;
		}

		public bool MoveNext() {
			Debug.Log($"ConfigFile({_filename}).MoveNext()");
			return false;
		}

		public void Reset() {
			throw new System.NotImplementedException();
		}

		public Config Current { get; }

		object IEnumerator.Current => Current;

		public void Dispose() {
			throw new System.NotImplementedException();
		}
	}

	static GameManager() {
		IEnumerator _watchFile(string filename) {
			Debug.Log($"_watchFile({filename})");
			yield return new WaitForSeconds(0.1f);
		}

		IEnumerator _downloadUri(string uri) {
			Debug.Log($"_downloadUri({uri})");
			yield return new WaitForSeconds(0.2f);
		}

		IEnumerator _parseConfig(Config config) {
			Debug.Log($"_parseConfig({config.ConfigName})");
			config.ConfigName = "Config Read";
			yield break;
		}

		Hand<Config>.To(new ConfigFile("stagehand.json"));
		//Hand<ConfigFile>.To(_watchFile);
		Hand<Config>.To(_downloadUri("http://www.google.com"));

		Hand<MonoBehaviour>.To(_downloadUri("http://www.google.com"));
		
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