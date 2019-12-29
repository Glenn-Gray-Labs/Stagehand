using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Plugins.Stagehand.Types.Threads;
using Debug = UnityEngine.Debug;

namespace Plugins.Stagehand {
	public static class Stagehand<T> {
		private static T _value;
		private static readonly HashSet<Type> _parents = new HashSet<Type>();
		private static readonly Type _mainThreadType = typeof(IThreadMain);

		public static void Stage(IEnumerator job) {
			if (_parents.Contains(_mainThreadType) || typeof(T) == _mainThreadType) {
				Stagehand._mainThreadExecutor.AddLast(job);
			} else {
				Stagehand.Executors[1].AddLast(job);
			}
		}

		public static void Stage(T value, IEnumerator job) {
			_value = value;
			Stage(job);
		}

		public static void Stage<TChild>(IEnumerator job) {
			// Store Reverse Relationship
			Stagehand<TChild>._parents.Add(typeof(T));
			Stagehand<TChild>.Stage(job);
		}

		public static IEnumerator Inject(Func<T, IEnumerator> job) {
			return job(_value);
		}
	}

	public static class Stagehand {
		/******************************************************************************************************************/
		// Pool de Threads
		/******************************************************************************************************************/
		internal class Executor : LinkedList<IEnumerator> {
			public void Execute() {
				var next = First;
				while (next != null) {
					if (!next.Value.MoveNext()) {
						Remove(next);
					}

					if (next.Value.Current != null) {
						var enumerator = (IEnumerator) next.Value.Current;
						while (enumerator.MoveNext()) {
							//
						}
					}

					next = next.Next;
				}
			}
		}

		internal static readonly Executor[] Executors = {
			new Executor(), // IThreadMain
			new Executor(), // IThread1
			new Executor(), // IThread2
			new Executor(), // IThread3
		};

		public const int ExecutorCount = 3; // Be sure this matches the number of Executors, minus one.

		internal static readonly Executor _mainThreadExecutor = Executors[0];

		public static void ExecuteThreadMain() {
			_mainThreadExecutor.Execute();
		}

		private static void _consume(int executor) {
			// Start the Thread!
			void _consumer() {
				var _executor = Executors[executor];
				for (;;) {
					// IThread1..n
					_executor.Execute();
				}
			}

			new Thread(_consumer) {
				Name = $"IThread{executor}",
			}.Start();
		}

		static Stagehand() {
#if DEBUG
			// NOTICE: If this ever happens, please make a pull request to increase the maximum number of supported threads.
			if (Environment.ProcessorCount > ExecutorCount) {
				Debug.LogWarning("Stagehand does not support thread affinity to all of your logical processors.");
			}
#endif

			// Start Consumers
			_consume(1);
			_consume(2);
			_consume(3);
		}
		/******************************************************************************************************************/

		/******************************************************************************************************************/
		// Job Runners
		/******************************************************************************************************************/
		public static IEnumerator ConsecutiveParallelJobs(params IEnumerator[] jobs) {
			var jobsLength = jobs.Length;
			for (;;) {
				for (var index = 0; index < jobsLength; ++index) {
					if (!jobs[index].MoveNext()) {
						// Job Finished!
						jobs[index] = jobs[--jobsLength];
						if (jobsLength == 0) yield break;
					} else if (jobs[index].Current != null) {
						// Nesting...
						yield return jobs[index].Current;
					}
				}

				// Yield...
				yield return null;
			}
		}

		public static IEnumerator ConsecutiveJobs(params IEnumerator[] jobs) {
			return jobs.GetEnumerator();
		}
		/******************************************************************************************************************/

		/******************************************************************************************************************/
		// Job Helpers
		/******************************************************************************************************************/
		public static IEnumerator Sleep(float durationInSeconds) {
			var endTime = Stopwatch.GetTimestamp() + (long) (10000000L * durationInSeconds);
			while (Stopwatch.GetTimestamp() < endTime) yield return null;
		}

		/******************************************************************************************************************/
	}
}