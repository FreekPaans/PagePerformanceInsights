﻿using PagePerformanceInsights.Handler.PerformanceData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Diagnostics;
using PagePerformanceInsights.Events;
using PagePerformanceInsights.Configuration;
using System.Configuration;

namespace PagePerformanceInsights.CommBus {
	class Buffer {
		readonly static ConcurrentQueue<HttpRequestData> _requestsQueue = new ConcurrentQueue<HttpRequestData>();
		//todo config
		readonly static TimeSpan _writeInterval;
		readonly static int? _maxQueueSize;

		readonly static EventLogHelper _logger = new EventLogHelper(typeof(Buffer));
		
		static bool _seenMaxSize=  false;

		public static void EnqueueRequest(HttpRequestData data) {
			if(_maxQueueSize!=null &&_requestsQueue.Count >= _maxQueueSize.Value) {
				if(!_seenMaxSize) {
					_logger.Warn(() => string.Format("Queue size reached max size ({0}), ignoring",_maxQueueSize.Value));
				}
				_seenMaxSize = true;
				return;
			}
			_requestsQueue.Enqueue(data);
		}

		readonly static IStorePerformanceData _store = null;

		static Buffer() {
			var config = BufferSection.Get();

			_writeInterval = config.WriteInterval;
			if(config.MaxBufferSize>=0) {
				_maxQueueSize= config.MaxBufferSize;
			}
			
			_store = SettingsStoreFactory.GetDataStorer();
			new Thread(() => {
				try {
					StartReader();
				}
				catch(Exception e) {
					_logger.LogException(string.Format("Error in queue runner thread, stopping execution"), e);
				}
			}).Start();
		}
		
				
		static double _analyzedRequestsFrequency;
		readonly static object _flushFrequencyLockerObject = new object();

		private static void StartReader() {
			_logger.Info(()=>"starting the reader");
			while(true) {
				
				var sw = new Stopwatch();
				sw.Start();
				var items = RunQueue();
				sw.Stop();

				if(items!=0 && sw.ElapsedMilliseconds!=0) {
					lock(_flushFrequencyLockerObject) {
						_analyzedRequestsFrequency =  (0.2*_analyzedRequestsFrequency) + 0.8 * (double)items/sw.Elapsed.TotalSeconds;
					}
				}
								
				Thread.Sleep(_writeInterval);
				UpdateBufferFlushFrequency();
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
				}
			}

			_lastRun = DateTime.Now;
		}

		[DebuggerStepThrough]
		private static int RunQueue() {
			

			try {
				return ForwardToStore();
			}
			catch(Exception e) {
				_logger.LogException("Exception storing data", e);
				return 0;
			}

			
		}

		private static int ForwardToStore() {
			_seenMaxSize = false;
			var queueSize = _requestsQueue.Count;

			var res = new HttpRequestData[queueSize];

			for(var i=0;i<queueSize;i++) {
				_requestsQueue.TryDequeue(out res[i]);
			}


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