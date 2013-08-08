using PagePerformanceInsights.Handler.PerformanceData;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	public class MemoryStoreDataProvider : IProvidePerformanceData, IStorePerformanceData{
		
		readonly INeedNewRequestData[] _interestedParties;
		readonly PageStatistics _pageStatistics;

		public MemoryStoreDataProvider() {
			_pageStatistics = new PageStatistics();

			_interestedParties = new INeedNewRequestData [] {
				_pageStatistics
			};			
		}

		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			//throw new NotImplementedException();
			return _pageStatistics.GetStatisticsForAllPages(forDate);
				//return PerformanceStatisticsForPageCollection.Empty;
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			return PageDurationDistributionHistogram.Empty;
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetAllPagesDistribution(DateTime forDate) {
			return PageDurationDistributionHistogram.Empty;
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
