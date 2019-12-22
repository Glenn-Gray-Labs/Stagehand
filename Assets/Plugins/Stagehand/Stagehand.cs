using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Plugins.Stagehand.Types;
using Plugins.Stagehand.Types.Threads;
using UnityEngine;

namespace Plugins.Stagehand {
	// Automatic Type Association
	public static class Stagehand<TLeft, TRight> {
		public static bool Linked;

		// TLeft, TRight
		static Stagehand() {
			Debug.Log($"Stagehand<{typeof(TLeft)}, {typeof(TRight)}>");
		}

		public static void Stage(IEnumerator job) {
			// TODO: Fetch the Queue for TLeft and/or TRight

			Stagehand<TRight>.Stage(job);
		}
	}

	public static class Stagehand<T> {
		// T
		static Stagehand() {
			Debug.Log($"Stagehand<{typeof(T)}>");
		}

		// Consume the Job
		internal static void _consumer(object _queue) {
			var queue = (Queue<IEnumerator>) _queue;
			for (;;) {
				// Waiting...
				while (queue.Count > 0) {
					Debug.Log($"Thread<{typeof(T)}>: Consuming...");
					_execute(queue.Dequeue());
				}
			}
		}

		internal static void _consume(Queue<IEnumerator> queue) {
			// Start the Thread!
			new Thread(_consumer).Start(queue);
		}

		// Stage Job
		public static void Stage(IEnumerator job = null) {
			// 
			if (!Stagehand<IRootNode, T>.Linked) {
			}

			// TODO: Get a Queue to Push To!
			Stagehand.Queues[0].Enqueue(job);

			// Link the Types

			//Link<T>.Stage();

			// Add a Job to the Queue
			//queue.Enqueue(job);
		}

		private static void _execute(IEnumerator job) {
			while (job.MoveNext()) {
				Debug.Log($"Thread<{typeof(T)}>: Running...");
				if (job.Current != null) {
					Debug.Log($"Thread<{typeof(T)}>: Inception...");
					_execute((IEnumerator) job.Current);
				}
			}
		}

		public static void Execute() {
			var queue = Stagehand.Queues[0];
			while (queue.Count > 0) {
				Debug.Log("Executing...");
				_execute(queue.Dequeue());
			}
		}
	}

	internal static class Stagehand {
		internal static readonly ReadOnlyCollection<Type> Consumers = new ReadOnlyCollection<Type>(new[] {
			typeof(IThreadMain),
			typeof(IThread1),
			typeof(IThread2),
			typeof(IThread3),
		});

		internal static readonly ReadOnlyCollection<Queue<IEnumerator>> Queues = new ReadOnlyCollection<Queue<IEnumerator>>(new[] {
			new Queue<IEnumerator>(),
			new Queue<IEnumerator>(),
			new Queue<IEnumerator>(),
			new Queue<IEnumerator>(),
		});

		static Stagehand() {
			Debug.Log("Stagehand!");

#if DEBUG
			// NOTICE: If this ever happens, please make a pull request to increase the maximum number of supported threads.
			if (Environment.ProcessorCount > Consumers.Count) {
				Debug.LogWarning("Stagehand does not support thread affinity to all of your logical processors.");
			}
#endif

			// Start Consumers
			//Stagehand<IThreadMain>._consume(Queues[0]);
			Stagehand<IThread1>._consume(Queues[1]);
			Stagehand<IThread2>._consume(Queues[2]);
			Stagehand<IThread3>._consume(Queues[3]);
		}
	}
}