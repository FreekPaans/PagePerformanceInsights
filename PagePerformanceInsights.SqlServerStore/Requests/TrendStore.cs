using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class TrendStore {
		readonly RealTimeTrendStrategy _realTimeTrendStrategy;
		//readonly RequestsReader _requestsReader;

		public TrendStore(string connectionString, IProvidePageIds pageIdProvider) {
			_realTimeTrendStrategy = new RealTimeTrendStrategy(connectionString,pageIdProvider);
			//_requestsReader = requestsReader;
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			return _realTimeTrendStrategy.GetHourlyTrend(forDate,forPage);
		}
	}
}
