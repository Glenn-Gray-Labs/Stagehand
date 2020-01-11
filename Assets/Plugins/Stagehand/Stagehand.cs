using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Plugins.Stagehand.Types.Threads;
using Debug = UnityEngine.Debug;

namespace Plugins.Stagehand {
	public static class Stagehand<T> {
		// TODO: Implement per-type queues: search for nearest unresolved dependency in the type chain then add work to result.
		private static T _value;

		public static void Stage(IEnumerator job) {
			if (root.Find(Stagehand._mainThreadNode)._value.Type == typeof(IThreadMain) || typeof(T) == typeof(IThreadMain)) {
				Stagehand._mainThreadExecutor.AddLast(job);
			} else {
				Stagehand.Executors[Stagehand._nextExecutor = Stagehand._nextExecutor % Stagehand.ExecutorCount + 1].AddLast(job);
			}
		}

		public static void Stage(T value, IEnumerator job) {
			_value = value;
			Stage(job);
		}

		public static void Stage<TChild>(IEnumerator job) {
			// Add Relationship to Tree
			if (typeof(T) == typeof(IThreadMain)) {
				Stagehand<TChild>.root.Insert(Stagehand._mainThreadNode);
			} else {
				Stagehand<TChild>.root.Insert(new Stagehand.NodeValue {
					Type = typeof(T),
				});
			}

			// Stage the Work in the Child
			Stagehand<TChild>.Stage(job);
		}

		public static IEnumerator Inject(Func<T, IEnumerator> job) {
			return job(_value);
		}

		/******************************************************************************************************************/
		// Modified From: https://github.com/thomas-villagers/avltree.cs/blob/master/src/avltree.cs
		/******************************************************************************************************************/
		// TODO: This implementation should be rewritten, or at least fortified with some statistical analysis,
		//       serialization, and caches.
		/******************************************************************************************************************/
		private static readonly Node root = new Node(new Stagehand.NodeValue {
			Type = typeof(T),
		});

		internal class Node {
			internal readonly Stagehand.NodeValue _value;
			int _height;
			Node _left;
			Node _parent;
			Node _right;

			internal Node(Stagehand.NodeValue value) {
				_value = value;
				_height = 1;
			}

			private Node(Stagehand.NodeValue value, Node parent) {
				_value = value;
				_parent = parent;
				_height = 1;
			}

			private int compare(Stagehand.NodeValue left, Stagehand.NodeValue right) {
				var leftValue = left.Type == null ? 0 : left.Type.GetHashCode();
				var rightValue = right.Type == null ? 0 : right.Type.GetHashCode();
				return leftValue - rightValue;
			}

			internal Node Insert(Stagehand.NodeValue value) {
				Node Insert(ref Node node) {
					if (node == null) {
						node = new Node(value, this);

						// Re-Balance Tree:
						Node v = node;
						Node newRoot = node;
						bool restructured = false;
						while (v != null) {
							if (!restructured && Math.Abs(ChildHeight(v._left) - ChildHeight(v._right)) > 1) {
								var y = v.ChildWithMaxHeight();
								var x = y.ChildWithMaxHeight();
								Node left, center, right;
								Node T1, T2;
								if (x == y._left && y == v._left) {
									left = x;
									center = y;
									right = v;
									T1 = left._right;
									T2 = center._right;
								} else if (x == y._right && y == v._right) {
									left = v;
									center = y;
									right = x;
									T1 = center._left;
									T2 = right._left;
								} else if (x == y._left && y == v._right) {
									left = v;
									center = x;
									right = y;
									T1 = center._left;
									T2 = center._right;
								} else {
									left = y;
									center = x;
									right = v;
									T1 = center._left;
									T2 = center._right;
								}

								if (v._parent != null) {
									if (v == v._parent._left)
										v._parent._left = center;
									else v._parent._right = center;
								}

								center._parent = v._parent;

								center._left = left;
								left._parent = center;
								center._right = right;
								right._parent = center;

								left._right = T1;
								if (T1 != null) T1._parent = left;
								right._left = T2;
								if (T2 != null) T2._parent = right;
								left._height = 1 + left.MaxChildHeight();
								center._height = 1 + center.MaxChildHeight();
								right._height = 1 + right.MaxChildHeight();

								v = center;

								restructured = true;
							}

							v._height = 1 + v.MaxChildHeight();
							newRoot = v;
							v = v._parent;
						}

						return newRoot;
					}
					
					return node.Insert(value);
				}

				return compare(value, _value) < 0 ? Insert(ref _left) : Insert(ref _right);
			}

			internal Node Find(Stagehand.NodeValue value) {
				var cmp = compare(_value, value);
				if (cmp == 0) return this;
				if (_left != null && cmp > 0) return _left.Find(value);
				return _right == null ? this : _right.Find(value);
			}

			private static int ChildHeight(Node child) {
				return child?._height ?? 0;
			}

			private int MaxChildHeight() {
				return Math.Max(ChildHeight(_left), ChildHeight(_right));
			}

			private Node ChildWithMaxHeight() {
				return (ChildHeight(_left) > ChildHeight(_right)) ? _left : _right;
			}
		}
		/******************************************************************************************************************/
	}

	public static class Stagehand {
		internal static int _nextExecutor;

		public const int ExecutorCount = 3; // Be sure this matches the number of Executors, minus one.

		internal static readonly Executor[] Executors = {
			new Executor(), // IThreadMain
			new Executor(), // IThread1
			new Executor(), // IThread2
			new Executor(), // IThread3
		};

		internal static readonly Executor _mainThreadExecutor = Executors[0];

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
		/******************************************************************************************************************/
		
		/******************************************************************************************************************/
		// Tree
		/******************************************************************************************************************/
		internal struct NodeValue {
			public Type Type;
		}
		internal static NodeValue _mainThreadNode = new NodeValue {
			Type = typeof(IThreadMain),
		}; 
		/******************************************************************************************************************/
	}
}