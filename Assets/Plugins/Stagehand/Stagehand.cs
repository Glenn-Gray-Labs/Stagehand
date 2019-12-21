using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Plugins.Stagehand.Types.Threads;
using UnityEngine;

namespace Plugins.Stagehand {
	// Automatic Type Association
	public static class Stagehand<TLeft, TRight> {
		static Stagehand() {
			Debug.Log($"Stagehand<{typeof(TLeft)}, {typeof(TRight)}>");
		}

		public static void Stage(IEnumerator job = null) {
			// TODO
		}
	}

	public static class Stagehand<T> {
		// Work Queue
		private static readonly Queue<IEnumerator> _queue = new Queue<IEnumerator>();

		// Links T and TRight
		private static class Link<TRight> {
			static Link() {
				Debug.Log($"Link: {typeof(T)}, {typeof(TRight)}");
			}

			public static void Stage() {
				Debug.Log($"Link.Stage: {typeof(T)}, {typeof(TRight)}");
			}
		}

		// T
		static Stagehand() {
			Debug.Log($"Stagehand<{typeof(T)}>");
		}

		// Stage Work
		public static void Stage(IEnumerator job = null) {
			// Link the Types
			//Link<T>.Stage();

			// Consume the Work
			void Consumer() {
				for (;;) {
					// Waiting...
					while (_queue.Count > 0) {
						Debug.Log($"Thread<{typeof(T)}>: Consuming...");

						void RunJob(IEnumerator _job) {
							while (_job.MoveNext()) {
								Debug.Log($"Thread<{typeof(T)}>: Running...");
								if (_job.Current != null) {
									Debug.Log($"Thread<{typeof(T)}>: Inception...");
									RunJob((IEnumerator) _job.Current);
								}
							}
						}

						RunJob(_queue.Dequeue());
					}
				}
			}

			// Start the Thread?
			_queue.Enqueue(job);
			new Thread(Consumer).Start();
		}

		public static void Execute() {
			// TODO
		}
	}

	internal static class Stagehand {
		static Stagehand() {
#if DEBUG
			// NOTICE: If this ever happens, please make a pull request to increase the maximum number of supported threads.
			if (Environment.ProcessorCount > 4) {
				Debug.LogWarning("Stagehand does not support thread affinity to all of your logical processors.");
			}
#endif

			// Start Consumers
			Stagehand<IThreadMain>.Stage();
			Stagehand<IThread1>.Stage();
			Stagehand<IThread2>.Stage();
			Stagehand<IThread3>.Stage();
		}

		public static void Stage<T>() {
			// TODO: Check the Type Links

			Stagehand<T>.Stage();
		}

		public static void Stage<TLeft, TRight>() {
			// TODO: Check the Type Links

			// Doubly Link Types
			Stagehand<TRight>.Stage();
			Stagehand<TLeft>.Stage();
		}

		public static void Stage<T>(IEnumerator job) {
			// TODO: Check the Type Links

			Stagehand<T>.Stage();
		}

		public static void Stage<TLeft, TRight>(IEnumerator job) {
			// TODO: Check the Type Links

			// Doubly Link Types
			Stagehand<TRight>.Stage();
			Stagehand<TLeft>.Stage();
		}
	}
}