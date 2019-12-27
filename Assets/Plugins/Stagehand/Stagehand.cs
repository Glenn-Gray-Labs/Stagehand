using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Plugins.Stagehand.Types.Threads;
using UnityEngine;

namespace Plugins.Stagehand {
	// Automatic Type Association
	public static class Stagehand<TLeft, TRight> {
		static Stagehand() {
			/****************************************************************************************************************/
			// HACK: REVERSE LOOK-UP TABLE (child, parent)
			/****************************************************************************************************************/
			// TODO: Investigate hybrid/preprocessed approaches to the hierarchy problem.
			// TODO: Look for a way to build a tree structure out of the generic template types.
			Stagehand._rlut.Add(typeof(TRight), typeof(TLeft));
			/****************************************************************************************************************/
		}

		// TODO: Convenience Method; Determine Performance Penalty
		public static void Stage(IEnumerator job) {
			Stagehand<TLeft>.Stage(job);
		}
	}

	public static class Stagehand<T> {
		// Stage Job
		public static void Stage(IEnumerator job = null) {
			/****************************************************************************************************************/
			// TODO: This is awful and MUST be rewritten.
			/****************************************************************************************************************/
			// Find the Root Type
			var root = typeof(T);
			while (Stagehand._rlut.TryGetValue(root, out var next)) {
				root = next;
			}

			// TODO: Investigate best multithreaded stores that allow inserting work anywhere in the queue.

			// Enqueue the Job
			if (root == typeof(IThreadMain)) {
				// Main Thread!
				Stagehand.Queues[0].Enqueue(job);
			} else {
				// TODO: Any Thread Will NOT Do! This could/should point to the queue with the least amount of work to do.
				Stagehand._nextQueue = ++Stagehand._nextQueue % Stagehand.QueueCount + 1;
				Stagehand.Queues[Stagehand._nextQueue].Enqueue(job);
			}

			/****************************************************************************************************************/
		}
	}

	internal static class Stagehand {
		/****************************************************************************************************************/
		// HACKS
		/****************************************************************************************************************/
		// TODO: This is a hacked solution to the much larger problem of preprocessing the tree of dependencies.
		internal static readonly Dictionary<Type, Type> _rlut = new Dictionary<Type, Type>();

		// TODO: This should be replaced by a more intelligent system.
		public static int _nextQueue;
		/****************************************************************************************************************/

		// Be sure QueueCount matches the number of Queues:
		public const int QueueCount = 3;

		internal static readonly ReadOnlyCollection<Queue<IEnumerator>> Queues = new ReadOnlyCollection<Queue<IEnumerator>>(new[] {
			new Queue<IEnumerator>(), // IThreadMain
			new Queue<IEnumerator>(), // IThread1
			new Queue<IEnumerator>(), // IThread2
			new Queue<IEnumerator>(), // IThread3
		});

		private static void _execute(IEnumerator job) {
			// Execute the Job
			while (job.MoveNext()) {
				if (job.Current != null) {
					_execute((IEnumerator) job.Current);
				}
			}
		}

		public static void ExecuteThreadMain() {
			// Consume the IThreadMain Queue
			var queue = Queues[0];
			while (queue.Count > 0) {
				_execute(queue.Dequeue());
			}
		}

		// Consume the Job
		private static void _consumer(object queue) {
			var _queue = Queues[(int) queue];
			for (;;) {
				// Waiting...
				while (_queue.Count > 0) {
					// Executing...
					_execute(_queue.Dequeue());
				}
			}
		}

		private static void _consume(int queue) {
			// Start the Thread!
			new Thread(_consumer) {
				Name = $"IThread{queue}",
			}.Start(queue);
		}

		static Stagehand() {
#if DEBUG
			// NOTICE: If this ever happens, please make a pull request to increase the maximum number of supported threads.
			if (Environment.ProcessorCount > QueueCount) {
				Debug.LogWarning("Stagehand does not support thread affinity to all of your logical processors.");
			}
#endif

			// Start Consumers
			_consume(1);
			_consume(2);
			_consume(3);
		}
	}
}