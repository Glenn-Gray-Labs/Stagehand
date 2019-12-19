using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Plugins.Stagehand {
	// Associate Types with Threads
	internal static class Stagehand<T> {
		public static int Thread = (Stagehand.NextThread++ % Environment.ProcessorCount) + 1;

		static Stagehand() {
			Debug.Log($"{typeof(T)}: {Thread}");
		}
	}

	public interface IThreadMain {
	}

	public interface IThread0 {
	}

	public interface IThread1 {
	}

	public interface IThread2 {
	}

	public interface IThread3 {
	}

	public static class Stagehand {
		// One Queue Per (Logical) Processor
		private static readonly List<Queue<IEnumerator>> _queues;

		// Used
		public static int NextThread = 0;

		static Stagehand() {
			// Initialize the Consumer Queues
			_queues = new List<Queue<IEnumerator>>(Environment.ProcessorCount);

			// Start Queue Consumers
			for (var processor = 0; processor < Environment.ProcessorCount; ++processor) {
				new Thread(thread => {
					//Stagehand<IThreadMain>.Thread++;

					var queue = _queues[(int) thread];
					for (;;) {
						if (queue.Count == 0) {
							Thread.Sleep(8);
							continue;
						}

						try {
							var current = queue.Dequeue();
							Debug.Log($"Thread Consuming: {thread}");
							while (current.MoveNext()) {
								// Execute the Job...
							}
						} catch (Exception e) {
							Debug.LogException(e);
						}
					}
				}).Start(processor);
			}
		}

		public static void Subscribe<T>(IEnumerator job) {
			// TODO: Add thread safety.
			Debug.Log($"{typeof(T)}: {Stagehand<T>.Thread}");
			_queues[Stagehand<T>.Thread].Enqueue(job);
		}

		public static void Setup<T>(int thread) {
			// Setup the Type
			Debug.Log($"{typeof(T)}: {Stagehand<T>.Thread}: {thread}");
			Stagehand<T>.Thread = thread;
		}
	}
}