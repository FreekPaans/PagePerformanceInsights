using PagePerformanceInsights.Handler.PerformanceData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Diagnostics;

namespace PagePerformanceInsights.CommBus {
	class Buffer {
		readonly static ConcurrentQueue<HttpRequestData> _requestsQueue = new ConcurrentQueue<HttpRequestData>();
		//todo config
		readonly static TimeSpan WriteInterval = TimeSpan.FromSeconds(1);

		public static void EnqueueRequest(HttpRequestData data) {
			_requestsQueue.Enqueue(data);
		}

		readonly static IStorePerformanceData _store = null;

		static Buffer() {
			_store = SettingsStoreFactory.GetDataStorer();
			new Thread(StartReader).Start();
		}
		
		static Dictionary<DateTime,int> counts = new Dictionary<DateTime,int>();

		
		static double _analyzedRequestsFrequency;
		readonly static object _flushFrequencyLockerObject = new object();

		private static void StartReader() {
			while(true) {
				UpdateBufferFlushFrequency();
				var sw = new Stopwatch();
				sw.Start();
				var items = RunQueue();
				sw.Stop();

				if(items!=0) {
					lock(_flushFrequencyLockerObject) {
						_analyzedRequestsFrequency =  (0.2*_analyzedRequestsFrequency) + 0.8 * (double)items/sw.ElapsedMilliseconds;
						Trace.TraceInformation(string.Format("Items: {0}, Elapsed: {1}, Freq: {2}", items,sw.ElapsedMilliseconds,_analyzedRequestsFrequency));
					}
				}
								
				Thread.Sleep(WriteInterval);
			}
		}

		static DateTime _lastRun = DateTime.Now;

		readonly static object _bufferFlushFrequencyLocker = new object();
		static double _bufferFlushFrequency;

		private static void UpdateBufferFlushFrequency() {
			var now = DateTime.Now;
				
			var sinceLast = (now - _lastRun);
			if(sinceLast.TotalSeconds>0) {
				var frequency = 1 / sinceLast.TotalSeconds;
				lock(_bufferFlushFrequencyLocker) {
					_bufferFlushFrequency = (0.2 * _bufferFlushFrequency) + 0.8 * frequency;
					Trace.TraceInformation("sinceLast: {0}", sinceLast.TotalMilliseconds);
				}
			}

			_lastRun = DateTime.Now;
		}

		private static int RunQueue() {
			var queueSize = _requestsQueue.Count;

			var res = new HttpRequestData[queueSize];

			for(var i=0;i<queueSize;i++) {
				_requestsQueue.TryDequeue(out res[i]);
				if(!counts.ContainsKey(res[i].Timestamp.Date)) {
					counts[res[i].Timestamp.Date] = 0;
				}
				counts[res[i].Timestamp.Date]++;
			}
			//}	 

			_store.Store(res);

			return queueSize;
		}



		public static int GetBacklogSize() {
			return _requestsQueue.Count;
		}

		public static double GetAnalyzedRequestsPerSecond() {
			lock(_flushFrequencyLockerObject) {
				return _analyzedRequestsFrequency;
			}
		}

		public static double GetBufferFlushFrequency() {
			lock(_bufferFlushFrequencyLocker) {
				return _bufferFlushFrequency;
			}
		}
	}
}