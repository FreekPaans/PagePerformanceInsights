using PagePerformanceInsights.CommBus;
using PagePerformanceInsights.Handler.Algorithms.Median;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	class DurationStatistics:INeedNewRequestData {
		readonly List<HttpRequestData> _notProcessedPackets;
		//readonly object locker = new object();
		readonly Dictionary<DateTime,List<int>> _perDateDuration;
		readonly Dictionary<DateTime,Dictionary<string,List<int>>> _perDateAndPageDuration;
		readonly object _readerLock = new object();
		
		public DurationStatistics() {
			_notProcessedPackets = new List<HttpRequestData>();
			_perDateAndPageDuration = new Dictionary<DateTime,Dictionary<string,List<int>>>();
			_perDateDuration = new Dictionary<DateTime,List<int>>();
		}

		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			FlushNotProcessedPackets();
			lock(_readerLock) {
				var pages = GetPerPageDistributionDictionary(forDate).Select(p => PerformanceStatisticsForPage.Calculate(p.Value.ToArray(),p.Key)).ToArray();

				var allPages = PerformanceStatisticsForPage.Calculate(GetAllPagesDistribution(forDate).ToArray(),"All Pages");

				return new PerformanceStatisticsForPageCollection(pages,allPages);
			}
		}

		private List<int> GetAllPagesDistribution(DateTime forDate) {
			return _perDateDuration.GetOrAdd(forDate,new List<int>());
		}

		private Dictionary<string,List<int>> GetPerPageDistributionDictionary(DateTime forDate) {
			return _perDateAndPageDuration.GetOrAdd(forDate,new Dictionary<string,List<int>>());
		}

		private void ProcessPackets(HttpRequestData[] toProcessPackets) {
			lock(_readerLock) {
				foreach(var perDate in toProcessPackets.GroupBy(p => p.Timestamp.Date)) {
					var durationsPerDate = _perDateDuration.GetOrAdd(perDate.Key,new List<int>());
					durationsPerDate.AddRange(perDate.Select(t => t.Duration));

					var pagesPerDate = _perDateAndPageDuration.GetOrAdd(perDate.Key,new Dictionary<string,List<int>>());

					foreach(var perPage in perDate.GroupBy(p => p.Page)) {
						var durationsPerPageAndDate = pagesPerDate.GetOrAdd(perPage.Key,new List<int>());
						durationsPerPageAndDate.AddRange(perPage.Select(t => t.Duration));
					}
				}
			}
		}

		private void FlushNotProcessedPackets() {
			HttpRequestData[] toProcessPackets;

			lock(_notProcessedPackets) {
				toProcessPackets= _notProcessedPackets.ToArray();
				_notProcessedPackets.Clear();
			}

			ProcessPackets(toProcessPackets);
		}

		


		public void NewRequestDataArrived(CommBus.HttpRequestData[] res) {
			lock(_notProcessedPackets) {
				_notProcessedPackets.AddRange(res);
			}
		}



		public PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			FlushNotProcessedPackets();
			lock(_readerLock) {
				List<int> distribution;
				if(forPage == null) {
					distribution = GetAllPagesDistribution(forDate);
				}
				else {
					distribution = GetPerPageDistributionDictionary(forDate)[forPage];
				}
				return PageDurationDistributionHistogram.FromDistribution(distribution.ToArray(),100);
			}
		}
	}
}
