using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

		int _skipJsonWhitespace(IEnumerator<string> reader, int characterIndex) {
			do {
				for (; characterIndex < reader.Current.Length; ++characterIndex) {
					switch (reader.Current[characterIndex]) {
						case ' ':
						case '\t':
						case '\n':
						case '\r':
							break;
						default:
							return characterIndex;
					}
				}
				characterIndex = 0;
			} while (reader.MoveNext());
			return -1;
		}

		IEnumerator<object> _parseJsonArray(IEnumerator<string> reader, int characterIndex) {
			do {
				for (; characterIndex < reader.Current.Length; ++characterIndex) {
					characterIndex = _skipJsonWhitespace(reader, characterIndex);
					yield return _parseJsonValue(reader, ref characterIndex);
					
					switch (reader.Current[characterIndex]) {
						case ',':
							break;
						case ']':
							yield break;
					}
				}
				characterIndex = 0;
			} while (reader.MoveNext());
		}

		object _parseJsonNumber(IEnumerator<string> reader, ref int characterIndex) {
			var negative = reader.Current[characterIndex] == '-' ? -1 : 1;
			if (negative == -1) ++characterIndex;

			(long, long) _internalParseJsonNumber(ref int charIdx) {
				long value = 0;
				long multiplier = 1;
				for (; charIdx < reader.Current.Length; ++charIdx) {
					switch (reader.Current[charIdx]) {
						case '0':
							break;
						case '1':
							value += multiplier;
							break;
						case '2':
							value += 2 * multiplier;
							break;
						case '3':
							value += 3 * multiplier;
							break;
						case '4':
							value += 4 * multiplier;
							break;
						case '5':
							value += 5 * multiplier;
							break;
						case '6':
							value += 6 * multiplier;
							break;
						case '7':
							value += 7 * multiplier;
							break;
						case '8':
							value += 8 * multiplier;
							break;
						case '9':
							value += 9 * multiplier;
							break;
						case '.':
							// TODO: Parse float.
							// _internalParseJsonNumber();
							
							break;
						case 'e':
						case 'E':
							// TODO: Exponent!
							break;
						default:
							return (multiplier, negative * value);
					}
					multiplier *= 10;
				}
				return (multiplier, negative * value);
			}
			(var m, var v) = _internalParseJsonNumber(ref characterIndex);
			return v;
		}

		string _parseJsonString(IEnumerator<string> reader, ref int characterIndex) {
			var stringBuilder = new StringBuilder();
			do {
				++characterIndex;
				for (; characterIndex < reader.Current.Length; ++characterIndex) {
					switch (reader.Current[characterIndex]) {
						case '"':
							++characterIndex;
							return stringBuilder.ToString();
						case '\\':
							stringBuilder.Append(reader.Current[++characterIndex]);
							break;
						default:
							stringBuilder.Append(reader.Current[characterIndex]);
							break;
					}
				}
				characterIndex = 0;
			} while (reader.MoveNext());
			return null;
		}

		object _parseJsonValue(IEnumerator<string> reader, ref int characterIndex) {
			do {
				for (; characterIndex < reader.Current.Length; ++characterIndex) {
					characterIndex = _skipJsonWhitespace(reader, characterIndex);

					switch (reader.Current[characterIndex]) {
					case '"':
						return _parseJsonString(reader, ref characterIndex);
					case '-':
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9': // TODO: I really dislike the redundancy of double checking, here and inside _parseJsonNumber.
						return _parseJsonNumber(reader, ref characterIndex);
					case '{':
						return _parseJsonObject(reader, characterIndex);
					case '[':
						return _parseJsonArray(reader, characterIndex);
					case 't': // TODO: true
						break;
					case 'f': // TODO: false
						break;
					case 'n': // TODO: null
						break;
					}
				}
				characterIndex = 0;
			} while (reader.MoveNext());
			return null;
		}

		IEnumerator<object> _parseJsonObject(IEnumerator<string> reader, int characterIndex) {
			do {
				for (; characterIndex < reader.Current.Length; ++characterIndex) {
					characterIndex = _skipJsonWhitespace(reader, characterIndex);

					switch (reader.Current[characterIndex]) {
					case '"':
						yield return _parseJsonString(reader, ref characterIndex);
						characterIndex = _skipJsonWhitespace(reader, characterIndex);
						if (reader.Current[characterIndex] != ':') throw new SyntaxErrorException("Invalid character encountered while looking for a colon.");
						++characterIndex;
						yield return _parseJsonValue(reader, ref characterIndex);
						break;
					case '}':
						yield break;
					}
				}
				characterIndex = 0;
			} while (reader.MoveNext());
		}

		IEnumerator<object> _parseJson(IEnumerator<string> reader) {
			while (reader.MoveNext()) {
				for (var characterIndex = 0; characterIndex < reader.Current.Length; ++characterIndex) {
					characterIndex = _skipJsonWhitespace(reader, characterIndex);

					// If you find yourself,
					// Reading this code,
					// It's probably because,
					// You've not linted your JSON.
					switch (reader.Current[characterIndex]) {
					case '{':
						var parseObject = _parseJsonObject(reader, characterIndex);
						while (parseObject.MoveNext()) {
							var key = (string) parseObject.Current;
							Debug.Log(key);
							parseObject.MoveNext();
							var value = parseObject.Current;
							Debug.Log(value);
						}
						break;
					case '[':
						var parseArray = _parseJsonArray(reader, characterIndex);
						while (parseArray.MoveNext()) {
							Debug.Log(parseArray.Current);
						}
						break;
					default:
						throw new SyntaxErrorException($"Invalid opening character encountered in JSON stream: {reader.Current[characterIndex]}");
					}
				}
				yield return reader.Current;
			}
		}

		IEnumerator<Config> _deserializeInto(Config config, IEnumerator<object> parser) {
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