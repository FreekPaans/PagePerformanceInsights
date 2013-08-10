using PagePerformanceInsights.Helpers;
using PagePerformanceInsights.SqlServerStore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class DistributionStore : INeedToBeWokenUp{
		readonly RealTimeDistributionStrategy _realTimeDistributionStrategy;
		readonly PreCalculatedDistributionStrategy _preCalculatedDistributionStrategy;
		readonly RequestsReader _requestsReader;
		public DistributionStore(string connectionString, IProvidePageIds pageIdProvider,RequestsReader requestsReader) {
			_realTimeDistributionStrategy = new RealTimeDistributionStrategy(connectionString,pageIdProvider);
			_preCalculatedDistributionStrategy = new PreCalculatedDistributionStrategy(connectionString,pageIdProvider,requestsReader,_realTimeDistributionStrategy);
			_requestsReader = requestsReader;

		}
		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			if(!UseRealtimeData(forDate) && _preCalculatedDistributionStrategy.HasPreCalculatedData(forDate,forPage)) {
				return _preCalculatedDistributionStrategy.GetPageDistribution(forDate,forPage);
			}
			return _realTimeDistributionStrategy.GetPageDistribution(forDate,forPage);
		}

		private bool UseRealtimeData(DateTime forDate) {
			return forDate >= DateContext.Now.Add(TimeSkewHelper.MaxTimeSkewWindow).Date;
		}

		DateTime _lastRun = DateTime.MinValue;
		readonly static TimeSpan _runInterval = TimeSpan.FromMinutes(5);
		public void Wakeup() {
			if((DateContext.Now - _lastRun) < _runInterval) {
				return;
			}
			_lastRun = DateContext.Now;


			foreach(var date in _requestsReader.GetDatesInRequestTable()) {
				if(UseRealtimeData(date)) {
					continue;
				}

				_preCalculatedDistributionStrategy.PreCalculateForDate(date);
				
			}
		}
	}
}
