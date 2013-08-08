using PagePerformanceInsights.CommBus;
using PagePerformanceInsights.Handler.Algorithms.Median;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	class PageStatistics:INeedNewRequestData {
		readonly List<HttpRequestData> _notProcessedPackets;
		//readonly object locker = new object();
		readonly Dictionary<DateTime,List<int>> _perDateDuration;
		readonly Dictionary<DateTime,Dictionary<string,List<int>>> _perDateAndPageDuration;
		readonly object _readerLock = new object();
		
		public PageStatistics() {
			_notProcessedPackets = new List<HttpRequestData>();
			_perDateAndPageDuration = new Dictionary<DateTime,Dictionary<string,List<int>>>();
			_perDateDuration = new Dictionary<DateTime,List<int>>();
		}

		internal Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			HttpRequestData[] toProcessPackets;

			lock(_notProcessedPackets) {
				toProcessPackets= _notProcessedPackets.ToArray();
				_notProcessedPackets.Clear();
			}

			lock(_readerLock) {
				foreach(var perDate in toProcessPackets.GroupBy(p=>p.Timestamp.Date)) {
					var durationsPerDate = _perDateDuration.GetOrAdd(perDate.Key,new List<int>());
					durationsPerDate.AddRange(perDate.Select(t => t.Duration));

					var pagesPerDate = _perDateAndPageDuration.GetOrAdd(perDate.Key,new Dictionary<string,List<int>>());

					foreach(var perPage in perDate.GroupBy(p=>p.Page)) {
						var durationsPerPageAndDate = pagesPerDate.GetOrAdd(perPage.Key,new List<int>());
						durationsPerPageAndDate.AddRange(perPage.Select(t=>t.Duration));
					}

				}

				var pages = _perDateAndPageDuration.GetOrAdd(forDate,new Dictionary<string,List<int>>()).Select(p=>PerformanceStatisticsForPage.Calculate(p.Value.ToArray(), p.Key)).ToArray();
					
				var allPages = PerformanceStatisticsForPage.Calculate(_perDateDuration.GetOrAdd(forDate,new List<int>()).ToArray(), "All Pages");

				return new PerformanceStatisticsForPageCollection(pages,allPages);
			}
				//lock(_requestDataPackets)  {
					
				//	_mergedDataPackets.AddRange(_requestDataPackets.SelectMany(r => r));
				//	_requestDataPackets.Clear();
				//}
				

		}

		


		public void NewRequestDataArrived(CommBus.HttpRequestData[] res) {
			lock(_notProcessedPackets) {
				_notProcessedPackets.AddRange(res);
			}
		}
	}

	static class DictionaryExt {
		public static T2 GetOrAdd<T1,T2>(this Dictionary<T1,T2> input, T1 key, T2 add) {
			if(!input.ContainsKey(key)) {
				input[key] = add;
			}
			return input[key];
		}
	}
}
