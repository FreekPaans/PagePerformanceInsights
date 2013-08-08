using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData.DataTypes {
	public class PerformanceStatisticsForPageCollection {
		readonly PerformanceStatisticsForPage[] _pageStatistics;

		public PerformanceStatisticsForPage[] PageStatistics {
			get { return _pageStatistics; }
		}

		readonly PerformanceStatisticsForPage _statisticsForAllPages;

		public PerformanceStatisticsForPage StatisticsForAllPages {
			get { return _statisticsForAllPages; }
		} 


		public PerformanceStatisticsForPageCollection(PerformanceStatisticsForPage[] pageStatistics,PerformanceStatisticsForPage preCalculatedStatisticsForAllPages) {
			_pageStatistics = pageStatistics;
			_statisticsForAllPages = preCalculatedStatisticsForAllPages;
		}


		public static PerformanceStatisticsForPageCollection Empty {
			get {
				return new PerformanceStatisticsForPageCollection(new PerformanceStatisticsForPage[0], PerformanceStatisticsForPage.Empty);
			}
		}
	}
}