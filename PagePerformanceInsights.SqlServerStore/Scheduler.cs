using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace PagePerformanceInsights.SqlServerStore {
	class Scheduler : IDisposable {
		readonly Timer _timer;
		volatile bool isRunning = false;
		//private INeedToBeWokenUp[] _toCall;
		
		public Scheduler(params INeedToBeWokenUp[] toCall) {
			_timer = new Timer((c) =>{
				if(isRunning) {
					return;
				}
				isRunning= true;
				IterateThroughWakeups(toCall);
				isRunning= false;
			});

			_timer.Change(TimeSpan.Zero,TimeSpan.FromMinutes(1));
		}

		[DebuggerStepThrough]
		private static void IterateThroughWakeups(INeedToBeWokenUp[] toCall) {
			foreach(var call in toCall) {
				try {
					call.Wakeup();
				}
				catch {
					//TODO: log?
				}
			}
		}

		public void Dispose() {
			_timer.Dispose();
		}
	}
}
