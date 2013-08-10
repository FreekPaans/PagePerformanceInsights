using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	interface IAllPagesReadStrategy {
		PerformanceStatisticsForPageCollection GetRequestStatisticsForAllPages(DateTime forDate);
	}
}
