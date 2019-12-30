using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Plugins.Stagehand.Types.Threads;
using Debug = UnityEngine.Debug;

namespace Plugins.Stagehand {
	public static class Stagehand<T> {
		private static T _value;
		private static readonly Type _mainThreadType = typeof(IThreadMain);

		/******************************************************************************************************************/
		// Modified From: https://github.com/thomas-villagers/avltree.cs/blob/master/src/avltree.cs
		/******************************************************************************************************************/
		// TODO: This implementation should be rewritten, or at least fortified with some statistical analysis,
		//       serialization, and caches.
		/******************************************************************************************************************/
		private static readonly Node root = new Node(typeof(T));

		public static void Stage(IEnumerator job) {
			if (root.Find(_mainThreadType) != null || typeof(T) == _mainThreadType) {
				Stagehand._mainThreadExecutor.AddLast(job);
			} else {
				Stagehand.Executors[1].AddLast(job);
			}
		}

		public static void Stage(T value, IEnumerator job) {
			_value = value;
			Stage(job);
		}

		public static void Stage<TChild>(IEnumerator job) {
			// Add Relationship to Tree
			root.Insert(typeof(T));

			// Stage the Work in the Child
			Stagehand<TChild>.Stage(job);
		}

		public static IEnumerator Inject(Func<T, IEnumerator> job) {
			return job(_value);
		}

		internal class Node {
			public readonly Type value;
			public int height;
			public Node left;
			Node parent;
			public Node right;

			internal Node(Type value) {
				this.value = value;
				height = 1;
			}

			private Node(Type value, Node parent) {
				this.value = value;
				this.parent = parent;
				height = 1;
			}

			private int compare(Type left, Type right) {
				var leftValue = left == null ? 0 : left.GetHashCode();
				var rightValue = right == null ? 0 : right.GetHashCode();
				return leftValue - rightValue;
			}

			internal Node Insert(Type value) {
				Node Insert(ref Node node) {
					if (node == null) {
						node = new Node(value, this);

						// Re-Balance Tree:
						Node v = node;
						Node newRoot = node;
						bool restructured = false;
						while (v != null) {
							if (!restructured && Math.Abs(ChildHeight(v.left) - ChildHeight(v.right)) > 1) {
								var y = v.ChildWithMaxHeight();
								var x = y.ChildWithMaxHeight();
								Node a, b, c;
								Node T1, T2;
								if (x == y.left && y == v.left) {
									a = x;
									b = y;
									c = v;
									T1 = a.right;
									T2 = b.right;
								} else if (x == y.right && y == v.right) {
									a = v;
									b = y;
									c = x;
									T1 = b.left;
									T2 = c.left;
								} else if (x == y.left && y == v.right) {
									a = v;
									b = x;
									c = y;
									T1 = b.left;
									T2 = b.right;
								} else {
									a = y;
									b = x;
									c = v;
									T1 = b.left;
									T2 = b.right;
								}

								if (v.parent != null) {
									if (v == v.parent.left)
										v.parent.left = b;
									else v.parent.right = b;
								}

								b.parent = v.parent;

								b.left = a;
								a.parent = b;
								b.right = c;
								c.parent = b;

								a.right = T1;
								if (T1 != null) T1.parent = a;
								c.left = T2;
								if (T2 != null) T2.parent = c;
								a.height = 1 + a.MaxChildHeight();
								b.height = 1 + b.MaxChildHeight();
								c.height = 1 + c.MaxChildHeight();

								v = b;

								restructured = true;
							}

							v.height = 1 + v.MaxChildHeight();
							newRoot = v;
							v = v.parent;
						}

						return newRoot;
					} else
						return node.Insert(value);
				}

				if (compare(value, this.value) < 0) {
					return Insert(ref left);
				} else {
					return Insert(ref right);
				}
			}

			internal Node Find(Type value) {
				int cmp = compare(this.value, value);
				if (cmp == 0) return this;
				if (cmp > 0) return left.Find(value);
				return right.Find(value);
			}

			private static int ChildHeight(Node child) {
				return (child == null) ? 0 : child.height;
			}

			private int MaxChildHeight() {
				return Math.Max(ChildHeight(left), ChildHeight(right));
			}

			private Node ChildWithMaxHeight() {
				return (ChildHeight(left) > ChildHeight(right)) ? left : right;
			}
		}

		/******************************************************************************************************************/
	}

	public static class Stagehand {
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
	}
}