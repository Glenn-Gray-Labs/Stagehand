using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Plugins.Backstage {
	public static class Stage {
		private static bool _running = true;
		public static Queue<IEnumerator>[] _enumerators = new Queue<IEnumerator>[Environment.ProcessorCount];
		public static int _enumeratorIndex;

		static Stage() {
			// Main Thread
			_enumerators[0] = new Queue<IEnumerator>();

			// The Rest...
			for (var processorIndex = 1; processorIndex < Environment.ProcessorCount; ++processorIndex) {
				new Thread(_consumer).Start(_enumerators[processorIndex] = new Queue<IEnumerator>());
			}
		}

		private static void _consumer(object enumerators) {
			var actions = (Queue<IEnumerator>) enumerators;
			Thread.CurrentThread.Name = actions.ToString();
			while (_running) {
				while (actions != null && actions.Count > 0) {
					void _recurse(IEnumerator enumerator) {
						while (enumerator.MoveNext()) {
							if (enumerator.Current != null) {
								_recurse((IEnumerator) enumerator.Current);
							}
						}
					}
					_recurse(actions.Dequeue());
				}
				// TODO: DELETE ME
				Thread.Sleep(1);
			}
		}
	}

	public static class Stage<T> where T : class {
		private static readonly int _enumeratorIndex;

		private static T _value;

		static Stage() {
			// TODO: Find a better way to link types to threads.
			_enumeratorIndex = Stage._enumeratorIndex++ % Environment.ProcessorCount;
		}

		public static void Hand(ref T value) {
			_value = value;
		}

		// TODO: Investigate methods for benchmarking ref, in, and standard copy/move semantics in runtime.
		public delegate IEnumerator ActionWrapper(ref T data);
		public static void Hand(ActionWrapper action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action(ref _value));
		}

		public delegate IEnumerator CoroutineWrapper(T data);
		public static void Hand(IEnumerator action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action);
		}

		// Hand a work queue back to the application. Used to execute code on the main thread.
		public static IEnumerator Hand() {
			var queue = Stage._enumerators[_enumeratorIndex];
			while (queue.Count > 0) {
				yield return queue.Dequeue();
			}
		}
	}
}