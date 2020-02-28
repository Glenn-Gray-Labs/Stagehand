using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
				new Thread(enumerators => {
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
						Thread.Sleep(16);
					}
				}).Start(_enumerators[processorIndex] = new Queue<IEnumerator>());
			}
		}
	}

	public static class Stage<T> where T : class {
		private static int _enumeratorIndex;
		private static T _value;

		static Stage() {
			// TODO: Find a better way to link types to threads.
			_enumeratorIndex = Stage._enumeratorIndex++ % Environment.ProcessorCount;
		}

		public static IEnumerator Hand() {
			foreach (var action in Stage._enumerators[_enumeratorIndex]) {
				yield return action;
			}
		}

		public static void Hand(in T value) {
			_value = value;
		}

		public static void Hand(IEnumerator action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action);
		}

		public static void Hand(IEnumerator<T> action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action);
		}
	
		public delegate IEnumerator ActionWrapper(T data);
		public static void Hand(ActionWrapper action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action(_value));
		}

		public delegate IEnumerator ActionWrapperIn(in T data);
		public static void Hand(ActionWrapperIn action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action(in _value));
		}

		public delegate IEnumerator ActionWrapperRef(ref T data);
		public static void Hand(ActionWrapperRef action) {
			Stage._enumerators[_enumeratorIndex].Enqueue(action(ref _value));
		}
	}
}