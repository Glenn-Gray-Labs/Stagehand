using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Plugins.Stagehand {
	public static class Stagehand {
		public interface IWork : IEnumerator {
			//
		}

		public class SimultaneousWork : IWork {
			private readonly Queue<IWork> _works;
			private int _counter;

			object IEnumerator.Current => null;

			public SimultaneousWork(params IWork[] works) {
				_works = new Queue<IWork>(works);
				_counter = _works.Count;
			}

			public bool MoveNext() {
				if (_counter == 0) return false;

				do {
					var work = _works.Dequeue();
					if (work.MoveNext()) _works.Enqueue(work);
				} while (--_counter > 0);

				_counter = _works.Count;
				return _counter > 0;
			}

			public void Reset() {
				_counter = 0;
				_works.Clear();
			}
		}

		public class WorkTimeout : IWork {
			private long _duration;
			private long _endTime;
			private Action _onTimeout;

			object IEnumerator.Current => null;

			public WorkTimeout(float durationInSeconds, Action onTimeout) {
				_duration = (long) (10000000L * durationInSeconds);
				Reset();
				_onTimeout = onTimeout;
			}

			public bool MoveNext() {
				var running = Stopwatch.GetTimestamp() < _endTime;
				if (running) return true;

				_onTimeout();
				return false;
			}

			public void Reset() {
				_endTime = Stopwatch.GetTimestamp() + _duration;
			}

			public void Dispose() {
				//
			}
		}

		public static void Do(IWork work) {
			void Run() {
				while (work.MoveNext()) {
					//
				}
			}

			// Main Thread
			//Run();

			// New Thread
			new Thread(Run).Start();
		}
	}
}