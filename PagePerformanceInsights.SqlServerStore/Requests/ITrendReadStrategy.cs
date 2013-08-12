using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	interface ITrendReadStrategy {
		Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate, string forPage);
	}
}
