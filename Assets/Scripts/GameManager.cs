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

		IEnumerator _skipJsonWhitespace(IEnumerator<char> reader) {
			do {
				switch (reader.Current) {
					case ' ':
					case '\t':
					case '\n':
					case '\r':
#if STAGEHAND_VERY_VERBOSE
						Debug.Log("WHITESPACE");
#endif
						break;
					default:
						yield break;
				}
			} while (reader.MoveNext());
		}

		string _parseJsonString(IEnumerator<char> reader) {
			var stringBuilder = new StringBuilder();
			reader.MoveNext();
			do {
				switch (reader.Current) {
					case '"':
#if STAGEHAND_VERY_VERBOSE
						Debug.Log("STRING_END: \"");
#endif
						reader.MoveNext();
						return stringBuilder.ToString();
					case '\\':
						reader.MoveNext();
#if STAGEHAND_VERY_VERBOSE
						Debug.Log($"STRING_LITERAL: \\{reader.Current}");
#endif
						stringBuilder.Append(reader.Current);
						break;
					default:
#if STAGEHAND_VERY_VERBOSE
						Debug.Log($"STRING: {reader.Current}");
#endif
						stringBuilder.Append(reader.Current);
						break;
				}
			} while (reader.MoveNext());
			return null;
		}

		IEnumerator<object> _parseJsonObject(IEnumerator<char> reader) {
			do {
				yield return _skipJsonWhitespace(reader);

				switch (reader.Current) {
					case '"':
#if STAGEHAND_VERY_VERBOSE
						Debug.Log("STRING_START: \"");
#endif
						yield return _parseJsonString(reader);
						yield return _skipJsonWhitespace(reader);
						if (reader.Current != ':') throw new SyntaxErrorException($"Suspicious character encountered while looking for your colon: {reader.Current}");
#if STAGEHAND_VERY_VERBOSE
						Debug.Log("OBJECT_PAIR: :");
#endif
						reader.MoveNext();
						yield return _parseJsonValue(reader);
						break;
					case ',':
#if STAGEHAND_VERY_VERBOSE
						Debug.Log("OBJECT_ELEMENT");
#endif
						break;
					case '}':
#if STAGEHAND_VERY_VERBOSE
						Debug.Log("OBJECT_END: }");
#endif
						reader.MoveNext();
						yield break;
				}
			} while (reader.MoveNext());
		}

		IEnumerator<object> _parseJsonValue(IEnumerator<char> reader) {
			yield return _skipJsonWhitespace(reader);

			switch (reader.Current) {
				case '"':
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("STRING_START: \"");
#endif
					yield return _parseJsonString(reader);
					yield break;
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
#if STAGEHAND_VERY_VERBOSE
					Debug.Log($"NUMBER_START: {reader.Current}");
#endif
					yield return _parseJsonNumber(reader);
					yield break;
				case '{':
					reader.MoveNext();
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("OBJECT_START: {");
#endif
					yield return _parseJsonObject(reader);
					yield break;
				case '[':
					reader.MoveNext();
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("ARRAY_START: [");
#endif
					yield return _parseJsonArray(reader);
					yield break;
				case 't':
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("TRUE_START");
#endif
					reader.MoveNext();
					if (reader.Current == 'r') reader.MoveNext(); else throw new SyntaxErrorException($"Expected the 'r' in 'true' but found the following instead: {reader.Current}");
					if (reader.Current == 'u') reader.MoveNext(); else throw new SyntaxErrorException($"Expected the 'u' in 'true' but found the following instead: {reader.Current}");
					if (reader.Current != 'e') throw new SyntaxErrorException($"Expected the 'e' in 'true' but found the following instead: {reader.Current}");
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("TRUE_END");
#endif
					yield break;
				case 'f':
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("FALSE_START");
#endif
					reader.MoveNext();
					if (reader.Current == 'a') reader.MoveNext(); else throw new SyntaxErrorException($"Expected the 'a' in 'false' but found the following instead: {reader.Current}");
					if (reader.Current == 'l') reader.MoveNext(); else throw new SyntaxErrorException($"Expected the 'l' in 'false' but found the following instead: {reader.Current}");
					if (reader.Current == 's') reader.MoveNext(); else throw new SyntaxErrorException($"Expected the 's' in 'false' but found the following instead: {reader.Current}");
					if (reader.Current != 'e') throw new SyntaxErrorException($"Expected the 'e' in 'false' but found the following instead: {reader.Current}");
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("FALSE_END");
#endif
					yield break;
				case 'n':
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("NULL_START");
#endif
					reader.MoveNext();
					if (reader.Current == 'u') reader.MoveNext(); else throw new SyntaxErrorException($"Expected the 'u' in 'null' but found the following instead: {reader.Current}");
					if (reader.Current == 'l') reader.MoveNext(); else throw new SyntaxErrorException($"Expected the 'l' in 'null' but found the following instead: {reader.Current}");
					if (reader.Current != 'l') throw new SyntaxErrorException($"Expected the 'l' in 'null' but found the following instead: {reader.Current}");
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("NULL_END");
#endif
					yield break;
				default:
					throw new SyntaxErrorException($"Bad character found in your stream: {reader.Current}");
			}
		}

		IEnumerator<object> _parseJsonArray(IEnumerator<char> reader) {
			do {
				yield return _skipJsonWhitespace(reader);

				if (reader.Current == ']') {
					reader.MoveNext();
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("ARRAY_END: ]");
#endif
					yield break;
				}

				yield return _parseJsonValue(reader);

#if STAGEHAND_VERY_VERBOSE
				if (reader.Current == ',') Debug.Log("ARRAY_ELEMENT");
#endif
			} while (reader.MoveNext());
		}

		object _parseJsonNumber(IEnumerator<char> reader) {
			// TODO: Switch to stdlib implementation for this.

			// Negative?
			var negative = reader.Current == '-' ? -1 : 1;
			if (negative == -1) reader.MoveNext();

			// Integer Portion
			long value = 0, multiplier = 1;
			do {
				switch (reader.Current) {
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
						reader.MoveNext();

						// Double!
						double dblValue = negative * value, dblDivider = 10.0;
						do {
							switch (reader.Current) {
								case '0':
									break;
								case '1':
									dblValue += 1.0 / dblDivider;
									break;
								case '2':
									dblValue += 2.0 / dblDivider;
									break;
								case '3':
									dblValue += 3.0 / dblDivider;
									break;
								case '4':
									dblValue += 4.0 / dblDivider;
									break;
								case '5':
									dblValue += 5.0 / dblDivider;
									break;
								case '6':
									dblValue += 6.0 / dblDivider;
									break;
								case '7':
									dblValue += 7.0 / dblDivider;
									break;
								case '8':
									dblValue += 8.0 / dblDivider;
									break;
								case '9':
									dblValue += 9.0 / dblDivider;
									break;
								case 'e':
								case 'E':
									goto PARSE_EXPONENT;
								default:
#if STAGEHAND_VERY_VERBOSE
									Debug.Log($"NUMBER_END: {dblValue}");
#endif
									return dblValue;
							}
							dblDivider *= 10;
						} while (reader.MoveNext());
						return dblValue;
					case 'e':
					case 'E':
PARSE_EXPONENT:
#if STAGEHAND_VERY_VERBOSE
						Debug.Log("NUMBER: ^");
#endif
						// TODO: Exponent!
						break;
					default:
#if STAGEHAND_VERY_VERBOSE
						Debug.Log($"NUMBER_END: {negative * value}");
#endif
						return negative * value;
				}
				multiplier *= 10;
			} while (reader.MoveNext());
			return negative * value;
		}

		IEnumerator<object> _parseJson(IEnumerator<char> reader) {
			while (reader.MoveNext()) {
				yield return _skipJsonWhitespace(reader);

				// If you find yourself,
				// Reading this code,
				// It's probably because,
				// You've not linted your JSON.
				switch (reader.Current) {
				case '{':
					reader.MoveNext();
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("OBJECT_START: {");
#endif
					yield return _parseJsonObject(reader);
					break;
				// TODO: Technically this isn't legal, but I refuse to enforce arbitrary/meaningless standards.
				case '[':
					reader.MoveNext();
#if STAGEHAND_VERY_VERBOSE
					Debug.Log("ARRAY_START: [");
#endif
					yield return _parseJsonArray(reader);
					break;
				default:
					throw new SyntaxErrorException($"Crazy character spotted in your JSON: {reader.Current}");
				}
			}
		}

		IEnumerator _deserializeInto<T>(T target, IEnumerator parser) {
			yield return parser;
		}

		IEnumerator<char> _readFile(string filename) {
			using (var sr = new StreamReader(filename)) {
				while (!sr.EndOfStream) {
					// Strategy #1: One Character at a Time
					/*foreach (var character in sr.ReadLine()) {
						yield return character;
					}*/

					// Strategy #2: One Line at a Time
					//yield return new StageReader(sr.ReadLine());

					// TODO: Strategy #3: Block Based Reads

					// Strategy #4: The Entire Stream
					foreach (var character in sr.ReadToEnd()) {
						yield return character;
					}
					yield break;
				}
			}
		}

		// MonoBehaviour
		Stage<MonoBehaviour>.Hand((ref MonoBehaviour monoBehaviour) => null);

		// Config
		var localConfig = new Config();
		Stage<Config>.Hand(ref localConfig);
		//Stage<Config>.Hand((ref Config config) => _deserializeInto(config, _parseJson(_readFile("Assets/Tests/backstage.json"))));
		Stage<Main>.Hand((ref Main main) => _deserializeInto(main, _parseJson(_readFile("Assets/Tests/backstage.json"))));

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