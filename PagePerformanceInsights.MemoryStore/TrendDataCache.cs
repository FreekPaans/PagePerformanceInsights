using PagePerformanceInsights.Handler.Algorithms.Median;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	class TrendDataCache {
		readonly Dictionary<string,TrendDataCache> _durationPerPage;
		readonly List<int> _durationTotal;
		bool _cacheValid = false;

		
		int __90pct;
		int _count;
		int _median;
		int _mean;
		

		TrendDataCache() {
			_durationPerPage = new Dictionary<string,TrendDataCache>();
			_durationTotal = new List<int>();
		}

		public static TrendDataCache Empty {
			get {
				return new TrendDataCache();
			}
		}

		public void Update(IEnumerable<CommBus.HttpRequestData> perHour) {
			Update(perHour.Select(p=>p.Duration));
			//foreach(var record in perHour) {
			//	_durationTotal.Add(record.Duration);
				
			//}

			lock(_durationPerPage) {
				foreach(var perPage in perHour.GroupBy(p=>p.Page)) {
					var cache = _durationPerPage.GetOrAdd(perPage.Key,TrendDataCache.Empty);
					cache.Update(perPage.Select(p=>p.Duration));
				}
			}
		}

		void Update(IEnumerable<int> durations) {
			if(!durations.Any()) {
				return;
			}
			_cacheValid =false;

			lock(_durationTotal) {
				_durationTotal.AddRange(durations);
			}
		}

		void AssertCacheValid() {
			if(_cacheValid ) {
				return;
			}

			int[] durations;

			lock(_durationTotal) {
				durations = _durationTotal.ToArray();
			}
			if(!durations.Any()) {
				return;
			}

			__90pct = new QuickSelect().Select(durations,0.9);
			_median = new QuickSelect().Select(durations,0.5);
			_count = durations.Length;
			_mean = (int)durations.Average();
			_cacheValid = true;
		}

		public int _90PCT {
			get {
				AssertCacheValid();
				return __90pct;
			}
		}

		public int Count { 
			get {
				AssertCacheValid();
				return _count;
			}
		}
		public int Median {
			get {
				AssertCacheValid();
				return _median;
			}
		}
		public int Mean {
			get {
				AssertCacheValid();
				return _mean;
			}
		}

		internal TrendDataCache ForPage(string forPage) {
			if(forPage==null) {
				return this;
			}
			return _durationPerPage.GetOrAdd(forPage,TrendDataCache.Empty);
		}
	}
}
