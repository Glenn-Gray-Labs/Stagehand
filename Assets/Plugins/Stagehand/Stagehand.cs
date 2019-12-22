using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Plugins.Stagehand.Types.Threads;
using UnityEngine;
using Random = System.Random;

namespace Plugins.Stagehand {
	// Automatic Type Association
	public static class Stagehand<TLeft, TRight> {
		// TLeft, TRight
		static Stagehand() {
			Debug.Log($"Stagehand<{typeof(TLeft)}, {typeof(TRight)}>");

			if (Stagehand<TLeft>._root == null) Stagehand<TLeft>._root = typeof(TRight);
			if (Stagehand<TRight>._root == null) Stagehand<TRight>._root = typeof(TLeft);
		}

		public static void Stage(IEnumerator job) {
			// TODO: Fetch the Queue for TLeft and/or TRight

			Stagehand<TRight>.Stage(job);
		}
	}

	public static class Stagehand<T> {
		internal static Type _root;

		// T
		static Stagehand() {
			//
		}

		// Consume the Job
		internal static void _consumer(object _queue) {
			var queue = (Queue<IEnumerator>) _queue;
			for (;;) {
				// Waiting...
				while (queue.Count > 0) {
					_execute(queue.Dequeue());
				}
			}
		}

		internal static void _consume(Queue<IEnumerator> queue) {
			// Start the Thread!
			new Thread(_consumer) {
				Name = queue.ToString(),
			}.Start(queue);
		}

		// Stage Job
		public static void Stage(IEnumerator job = null) {
			// Add a Job to a Queue
			if (_root == null) {
				// Any Queue Will Do!
				var queue = new Random().Next(1, Stagehand.Queues.Count - 1);
				Stagehand.Queues[queue].Enqueue(job);
			} else {
				// TODO: Thread Affinity!
				Stagehand.Queues[0].Enqueue(job);
			}
		}

		private static void _execute(IEnumerator job) {
			// Execute the Job
			while (job.MoveNext()) {
				if (job.Current != null) {
					_execute((IEnumerator) job.Current);
				}
			}
		}

		public static void Execute() {
			// Consume the IThreadMain Queue
			var queue = Stagehand.Queues[0];
			while (queue.Count > 0) {
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
#if DEBUG
			// NOTICE: If this ever happens, please make a pull request to increase the maximum number of supported threads.
			if (Environment.ProcessorCount > Consumers.Count) {
				Debug.LogWarning("Stagehand does not support thread affinity to all of your logical processors.");
			}
#endif

			// Start Consumers
			Stagehand<IThread1>._consume(Queues[1]);
			Stagehand<IThread2>._consume(Queues[2]);
			Stagehand<IThread3>._consume(Queues[3]);
		}
	}
}