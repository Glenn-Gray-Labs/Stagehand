using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Stagehand {
	public static class Stage {
#if DEBUG
		public static HashSet<Type> Children { get; } = new HashSet<Type>();
		public static Dictionary<Type, HashSet<Type>> Relationships { get; } = new Dictionary<Type, HashSet<Type>>();

		// Debugging Enhancements (Only In-Editor)
		internal class EnumeratorQueue : Queue<IEnumerator> {
			private readonly string _name;

			public EnumeratorQueue(string name) {
				_name = name;
			}

			public override string ToString() {
				return _name;
			}
		}

		public static Dictionary<Type, List<IEnumerator>> _GetQueue(Type type) {
			var dictionary = new Dictionary<Type, List<IEnumerator>>();
			foreach (var queue in _queues) {
				foreach (var action in queue) {
					if (!dictionary.TryGetValue(type, out var actions)) {
						actions = new List<IEnumerator>();
						dictionary.Add(type, actions);						
					}
					actions.Add(action);
				}
			}
			return dictionary;
		}
#else
		// Thin Wrapper for Convenience and Code Clarity
		internal class EnumeratorQueue : Queue<IEnumerator> {
			public EnumeratorQueue(string name) {
				//
			}
		}
#endif

		// Kill Switch
		public static bool Running { get; } = true;

		// Per-Thread Queues
		internal static EnumeratorQueue[] _queues = new EnumeratorQueue[Environment.ProcessorCount];

		// TODO: How should routing work?
		internal static int _enumeratorIndex;

		static Stage() {
			// Main Thread
			_queues[0] = new EnumeratorQueue("ThreadMain");

			// The Rest...
			for (var processorIndex = 1; processorIndex < Environment.ProcessorCount; ++processorIndex) {
				new Thread(_consumer).Start(_queues[processorIndex] = new EnumeratorQueue($"Thread{processorIndex}"));
			}
		}

		internal static void _consume(EnumeratorQueue actions) {
			while (actions.Count > 0) {
				void _recurse(IEnumerator action) {
					try {
						while (action.MoveNext()) {
							try {
								_recurse((IEnumerator) action.Current);
							} catch (InvalidCastException e) {
#if STAGEHAND_VERY_VERBOSE
								UnityEngine.Debug.LogException(e);
#endif
							} catch (Exception e) {
#if STAGEHAND_VERBOSE
								UnityEngine.Debug.LogException(e);
#endif
							}
						}
					} catch (Exception e) {
#if STAGEHAND_VERBOSE
						UnityEngine.Debug.LogException(e);
#endif
					}
				}
				_recurse(actions.Dequeue());
			}
		}

		private static void _consumer(object enumerators) {
			var actions = (EnumeratorQueue) enumerators;
			Thread.CurrentThread.Name = actions.ToString();
			while (Running) {
				_consume(actions);

				// TODO: Delete this when the router is complete.
				Thread.Sleep(2);
			}
		}
	}

	public static class Stage<T> where T : class {
#if DEBUG
		public static HashSet<Type> Parents { get; } = new HashSet<Type>();
		public static HashSet<Type> Children { get; } = new HashSet<Type>();
		public static bool IsDisconnected => Parents.Count == 0;
		public static bool IsLeaf => Children.Count == 0;
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

		public delegate IEnumerator ActionWrapper(ref T value);
		public static void Hand(ActionWrapper action) {
#if DEBUG
			// Track Connections
			Stage.Children.Add(typeof(T));
#endif

			Stage._queues[_enumeratorIndex].Enqueue(action(ref _value));
		}

		public delegate IEnumerator ActionWrapper<T2>(ref T value1, ref T2 value2);
		public static void Hand<T2>(ActionWrapper<T2> action) where T2 : class {
#if DEBUG
			// Track Connections
			Stage.Children.Add(typeof(T));
			if (!Stage.Relationships.TryGetValue(typeof(T), out var children)) {
				Stage.Relationships.Add(typeof(T), children = new HashSet<Type>());
				children.Add(typeof(T2));
				Stage<T2>.Parents.Add(typeof(T));
				Children.Add(typeof(T2));
			}
#endif

			// TODO: Explore global actions which execute in-between through a mask of flags.

			// TODO: Explore the many data distribution options.
			Stage._queues[_enumeratorIndex].Enqueue(action(ref _value, ref Stage<T2>._value));
		}

		// Hand a work queue back to the application. Used to execute code on the main thread.
		public static void Hand() {
			Stage._consume(Stage._queues[_enumeratorIndex]);
		}
	}
}