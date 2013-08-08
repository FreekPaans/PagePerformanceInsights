using PagePerformanceInsights.Handler.PerformanceData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

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
		
		private static void StartReader() {
			while(true) {
				var queueSize = _requestsQueue.Count;
				var res = new HttpRequestData[queueSize];
				for(var i=0;i<queueSize;i++) {
					_requestsQueue.TryDequeue(out res[i]);
				}

 				_store.Store(res);

				Thread.Sleep(WriteInterval);
			}
			
		}	
	}
}