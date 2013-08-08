using PagePerformanceInsights.Handler.PerformanceData;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	public class MemoryStoreDataProvider : IProvidePerformanceData, IStorePerformanceData{
		
		readonly INeedNewRequestData[] _interestedParties;
		readonly DurationStatistics _durationStatistics;
		//readonly PageDistribution _pageDistribution;

		public MemoryStoreDataProvider() {
			_durationStatistics = new DurationStatistics();
			//_pageDistribution = new PageDistribution();
			_interestedParties = new INeedNewRequestData [] {
				_durationStatistics,
				//_pageDistribution
			};			
		}

		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			//throw new NotImplementedException();
			return _durationStatistics.GetStatisticsForAllPages(forDate);
				//return PerformanceStatisticsForPageCollection.Empty;
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			return _durationStatistics.GetPageDistribution(forDate,forPage);
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetAllPagesDistribution(DateTime forDate) {
			return _durationStatistics.GetPageDistribution(forDate,null);
			//throw new NotImplementedException();
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			return PageStatisticsTrend.Empty;
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate) {
			return PageStatisticsTrend.Empty;
		}

		public void Store(CommBus.HttpRequestData[] res) {
			foreach(var party in _interestedParties) {
				party.NewRequestDataArrived(res);
			}
		}
	}
}
