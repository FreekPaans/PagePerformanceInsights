using PagePerformanceInsights.CommBus;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	class TrendStatistics:INeedNewRequestData {
		public ConcurrentDictionary<DateTime, TrendDataCache> _trendCache = new ConcurrentDictionary<DateTime,TrendDataCache>();
		readonly List<HttpRequestData> _notProcessed;
		readonly object _readerLock = new object();

		public TrendStatistics() {
			_notProcessed = new List<HttpRequestData>();
		}

		public PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			FlushRequests();


			var hours = Enumerable.Range(0,24).Select(i=>forDate.Date.AddHours(i)).ToArray();

			return new PageStatisticsTrend(hours.Select(h=>new { h, t = _trendCache.ContainsKey(h)?_trendCache[h].ForPage(forPage):TrendDataCache.Empty}).Select(_=>new PageStatisticsTrend._TrendData {
				TimeStamp = _.h,
				_90PCT = _.t._90PCT,
				Count = _.t.Count,
				Mean = _.t.Mean,
				Median = _.t.Median,
			}).ToArray());

			//return new PageStatisticsTrend(_trendCache.Keys.Where(k=>k>=forDate&&k<forDate.AddDays(1)).OrderBy(k=>k).Select(k=>_trendCache[k]

			//foreach(var keys in ) {
				
			//}
			//throw new NotImplementedException();
		}

		private void FlushRequests() {
			HttpRequestData[] _toProcesses;
			lock(_notProcessed) {
				_toProcesses = _notProcessed.ToArray();
				_notProcessed.Clear();
			}
			lock(_readerLock) {
				foreach(var perHour in _toProcesses.GroupBy(tp=>SnapHour(tp.Timestamp))) {
					var cache = _trendCache.GetOrAdd(perHour.Key,TrendDataCache.Empty);
					cache.Update(perHour);
				}
			}
		}
		

		readonly static DateTime Ref = new DateTime(2012,1,1);

		public static DateTime SnapHour(DateTime @in) {
			return Ref.AddHours((long)(@in-Ref).TotalHours);
		}

		public void NewRequestDataArrived(CommBus.HttpRequestData[] res) {
			lock(_notProcessed) {
				_notProcessed.AddRange(res);
			}
		}
	}
}
