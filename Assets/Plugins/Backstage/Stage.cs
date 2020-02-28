using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Plugins.Backstage {
	public static class Stage {
#if UNITY_EDITOR
		
#endif

		// Kill Switch
		public static bool Running { get; } = true;

		// Per-Thread Queues
		public static Queue<IEnumerator>[] _enumerators = new Queue<IEnumerator>[Environment.ProcessorCount];

		// TODO: How should routing work?
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
			while (Running) {
				while (actions.Count > 0) {
					void _recurse(IEnumerator action) {
						while (action.MoveNext()) {
							if (action.Current != null && action.Current.GetType() == typeof(IEnumerator)) {
								_recurse((IEnumerator) action.Current);
							}
						}
					}
					_recurse(actions.Dequeue());
				}

				// TODO: Delete this when the router is complete.
				Thread.Sleep(2);
			}
		}
	}

	public static class Stage<T> where T : class {
#if UNITY_EDITOR
		public static HashSet<Type> Parents { get; } = new HashSet<Type>();
		public static HashSet<Type> Children { get; } = new HashSet<Type>();
		public static bool Disconnected => Parents.Count == 0 && Children.Count == 0;
#endif

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
		public delegate IEnumerator ActionWrapper(ref T value);
		public static void Hand(ActionWrapper action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action(ref _value));
		}

		public delegate IEnumerator ActionWrapper<T2>(ref T value1, ref T2 value2);
		public static void Hand<T2>(ActionWrapper<T2> action) where T2 : class {
#if UNITY_EDITOR
			// Track Connections
			Stage<T2>.Parents.Add(typeof(T));
			Children.Add(typeof(T2));
#endif

			// TODO: Explore global actions which execute in-between a mask of types.

			// TODO: Explore the many data distribution options.
			Stage._enumerators[_enumeratorIndex].Enqueue(action(ref _value, ref Stage<T2>._value));
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
		
#if UNITY_EDITOR
		
#endif
	}
}