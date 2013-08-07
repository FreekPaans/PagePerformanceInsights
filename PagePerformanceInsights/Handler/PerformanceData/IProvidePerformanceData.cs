using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData {
	public interface IProvidePerformanceData {
		PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate);


		PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage);

		PageDurationDistributionHistogram GetAllPagesDistribution(DateTime forDate);

		PageStatisticsTrend GetTrend(DateTime forDate,string forPage);

		PageStatisticsTrend GetTrend(DateTime forDate);
	}
}