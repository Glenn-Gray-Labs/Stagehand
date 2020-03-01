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

	public class StageReader {
		public string Chunk;
		public int Index;
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

		IEnumerator _skipJsonWhitespace(IEnumerator<StageReader> reader) {
			do {
				var stageReader = reader.Current;
				for (; stageReader.Index < stageReader.Chunk.Length; ++stageReader.Index) {
					switch (stageReader.Chunk[stageReader.Index]) {
						case ' ':
						case '\t':
						case '\n':
						case '\r':
							Debug.Log($"Whitespace: {stageReader.Chunk[stageReader.Index]}");
							break;
						default:
							Debug.Log($"Non-Whitespace: {stageReader.Chunk[stageReader.Index]}");
							yield break;
					}
				}
			} while (reader.MoveNext());
		}

		string _parseJsonString(IEnumerator<StageReader> reader) {
			var stringBuilder = new StringBuilder();
			do {
				var stageReader = reader.Current;
				++stageReader.Index;
				for (; stageReader.Index < stageReader.Chunk.Length; ++stageReader.Index) {
					switch (stageReader.Chunk[stageReader.Index]) {
						case '"':
							++stageReader.Index;
							return stringBuilder.ToString();
						case '\\':
							stringBuilder.Append(stageReader.Chunk[++stageReader.Index]);
							break;
						default:
							stringBuilder.Append(stageReader.Chunk[stageReader.Index]);
							break;
					}
				}
			} while (reader.MoveNext());
			return null;
		}

		IEnumerator<object> _parseJsonObject(IEnumerator<StageReader> reader) {
			do {
				var stageReader = reader.Current;
				for (; stageReader.Index < stageReader.Chunk.Length; ++stageReader.Index) {
					yield return _skipJsonWhitespace(reader);
					switch (stageReader.Chunk[stageReader.Index]) {
						case '"':
							yield return _parseJsonString(reader);
							yield return _skipJsonWhitespace(reader);
							if (stageReader.Chunk[stageReader.Index] != ':') throw new SyntaxErrorException($"Suspicious character encountered while looking for your colon: {stageReader.Chunk[stageReader.Index]}");
							++stageReader.Index;
							yield return _parseJsonValue(reader);
							break;
						case '}':
							yield break;
					}
				}
			} while (reader.MoveNext());
		}

		IEnumerator<object> _parseJsonValue(IEnumerator<StageReader> reader) {
			do {
				var stageReader = reader.Current;
				for (; stageReader.Index < stageReader.Chunk.Length; ++stageReader.Index) {
					yield return _skipJsonWhitespace(reader);

					switch (stageReader.Chunk[stageReader.Index]) {
						case '"':
							yield return _parseJsonString(reader);
							break;
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
							yield return _parseJsonNumber(reader);
							yield break;
						case '{':
							++stageReader.Index;
							yield return _parseJsonObject(reader);
							yield break;
						case '[':
							++stageReader.Index;
							yield return _parseJsonArray(reader);
							yield break;
						case 't': // TODO: true
							break;
						case 'f': // TODO: false
							break;
						case 'n': // TODO: null
							break;
						default:
							throw new SyntaxErrorException($"Bad character found in your stream: {stageReader.Chunk[stageReader.Index]}");
					}
				}
			} while (reader.MoveNext());
		}

		IEnumerator<object> _parseJsonArray(IEnumerator<StageReader> reader) {
			do {
				var stageReader = reader.Current;
				for (; stageReader.Index < stageReader.Chunk.Length; ++stageReader.Index) {
					yield return _skipJsonWhitespace(reader);
					yield return _parseJsonValue(reader);
					
					switch (stageReader.Chunk[stageReader.Index]) {
						case ',':
							break;
						case ']':
							++stageReader.Index;
							yield break;
					}
				}
			} while (reader.MoveNext());
		}

		object _parseJsonNumber(IEnumerator<StageReader> reader) {
			var stageReader = reader.Current;
			var negative = stageReader.Chunk[stageReader.Index] == '-' ? -1 : 1;
			if (negative == -1) ++stageReader.Index;
			(long, long, double) _internalParseJsonNumber(ref int charIdx) {
				long value = 0;
				long multiplier = 1;
				for (; charIdx < stageReader.Chunk.Length; ++charIdx) {
					switch (stageReader.Chunk[charIdx]) {
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
							++charIdx;
							(var m2, var v2, double d2) = _internalParseJsonNumber(ref charIdx);
							double dValue = value;
							dValue += v2 / (double) m2;
							return (0, 0, dValue);
						case 'e':
						case 'E':
							// TODO: Exponent!
							break;
						default:
							return (multiplier, negative * value, 0.0);
					}
					multiplier *= 10;
				}
				return (multiplier, negative * value, 0.0);
			}
			(var m, var v, double d) = _internalParseJsonNumber(ref stageReader.Index);
			return m == 0 ? d : v;
		}

		IEnumerator<object> _parseJson(IEnumerator<StageReader> reader) {
			while (reader.MoveNext()) {
				var stageReader = reader.Current;
				for (; stageReader.Index < stageReader.Chunk.Length; ++stageReader.Index) {
					yield return _skipJsonWhitespace(reader);

					// If you find yourself,
					// Reading this code,
					// It's probably because,
					// You've not linted your JSON.
					switch (stageReader.Chunk[stageReader.Index]) {
					case '{':
						++stageReader.Index;
						yield return _parseJsonObject(reader);
						break;
					case '[':
						++stageReader.Index;
						yield return _parseJsonArray(reader);
						break;
					default:
						throw new SyntaxErrorException($"Crazy character spotted in your JSON: {stageReader.Chunk[stageReader.Index]}");
					}
				}
			};
		}

		IEnumerator _deserializeInto<T>(T target, IEnumerator parser) {
			do {
				yield return parser;
			} while (parser.Current != null);
		}

		IEnumerator<StageReader> _readFile(string filename) {
			using (var sr = new StreamReader(filename)) {
				while (!sr.EndOfStream) {
					// TODO: Also implement: one character at a time, block based reads, and reading the entire stream into memory at once.
					foreach (var character in sr.ReadLine()) {
						yield return new StageReader {
							Chunk = character.ToString(),
							Index = 0,
						};
					}
					
					/*yield return new StageReader {
						Chunk = sr.ReadLine(),
						Index = 0,
					};*/
				}
			}
		}

		// MonoBehaviour
		Stage<MonoBehaviour>.Hand((ref MonoBehaviour monoBehaviour) => null);

		// Config
		var localConfig = new Config();
		Stage<Config>.Hand(ref localConfig);
		//Stage<Config>.Hand((ref Config config) => _deserializeInto(config, _parseJson(_readFile("Assets/Tests/backstage.json"))));
		Stage<Main>.Hand((ref Main main) => _parseJson(_readFile("Assets/Tests/backstage.json")));

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