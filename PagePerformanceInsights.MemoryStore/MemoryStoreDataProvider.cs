using PagePerformanceInsights.Handler.PerformanceData;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	public class MemoryStoreDataProvider : IProvidePerformanceData, IStorePerformanceData{
		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			//throw new NotImplementedException();
				return PerformanceStatisticsForPageCollection.Empty;
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

			return;
		}
	}
}
