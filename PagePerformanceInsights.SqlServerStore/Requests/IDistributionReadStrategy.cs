using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	interface IDistributionReadStrategy {
		Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage);
	}
}
