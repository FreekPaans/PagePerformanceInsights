using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using PagePerformanceInsights.Helpers;
using PagePerformanceInsights.SqlServerStore.Helpers;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class TrendStore:INeedToBeWokenUp  {
		readonly RealTimeTrendStrategy _realTimeTrendStrategy;
		readonly PreCalculatedTrendStrategy _preCalculatedTrendStrategy;
		readonly RequestsReader _requestsReader;
		//readonly RequestsReader _requestsReader;

		public TrendStore(string connectionString, IProvidePageIds pageIdProvider, RequestsReader requestsReader)  {
			_realTimeTrendStrategy = new RealTimeTrendStrategy(connectionString,pageIdProvider);
			_preCalculatedTrendStrategy = new PreCalculatedTrendStrategy(connectionString,requestsReader,pageIdProvider,_realTimeTrendStrategy);
			_requestsReader = requestsReader;
			//_requestsReader = requestsReader;
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			if(UseRealTimeData(forDate) || !_preCalculatedTrendStrategy.HasPrecalculatedHourlyTrend(forDate,forPage)) {
				return _realTimeTrendStrategy.GetHourlyTrend(forDate,forPage);
			}
			return _preCalculatedTrendStrategy.GetHourlyTrend(forDate,forPage);
		}

		private bool UseRealTimeData(DateTime forDate) {
			return forDate >= DateContext.Now.Add(TimeSkewHelper.MaxTimeSkewWindow).Date;
		}

		static DateTime _lastRunTime = DateTime.MinValue;
		readonly static TimeSpan _wakeUpInterval = TimeSpan.FromMinutes(5);

		public void Wakeup() {
			if((DateContext.Now - _lastRunTime)<_wakeUpInterval) {
				return;
			}
			_lastRunTime = DateContext.Now;
			
			foreach(var date in _requestsReader.GetDatesInRequestTable()) {
				if(UseRealTimeData(date)) {	
					continue;
				}

				_preCalculatedTrendStrategy.PreCalculate(date);
			}
		}
	}
}
