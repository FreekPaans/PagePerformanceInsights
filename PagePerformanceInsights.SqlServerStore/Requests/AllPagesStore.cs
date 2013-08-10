using PagePerformanceInsights.CommBus;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using PagePerformanceInsights.Helpers;
using PagePerformanceInsights.SqlServerStore.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class AllPagesStore : INeedToBeWokenUp {
		readonly string _connectionString;
		readonly IProvidePageIds _pageIdProvider;
		readonly RealTimeAllPagesReadStrategy _realTimeReadStrategy;
		readonly PreCalculatedAllPagesStrategy _preCalculatedReadStrategy;
		readonly RequestsReader _requestsReader;
		
		public AllPagesStore(string connectionString, IProvidePageIds pageIdProvider) {
			_connectionString = connectionString;
			_pageIdProvider  = pageIdProvider;

			_realTimeReadStrategy = new RealTimeAllPagesReadStrategy(_connectionString,_pageIdProvider);
			_preCalculatedReadStrategy = new PreCalculatedAllPagesStrategy(_connectionString,_realTimeReadStrategy,_pageIdProvider);
			_requestsReader = new RequestsReader(_connectionString);
		}

		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			forDate = forDate.Date;

			if(!UseRealtimeData(forDate) &&  _preCalculatedReadStrategy.HasPreCalculatedData(forDate)) {
				return _preCalculatedReadStrategy.GetRequestStatisticsForAllPages(forDate);
			}
			return _realTimeReadStrategy.GetRequestStatisticsForAllPages(forDate);
		}


		

		private bool UseRealtimeData(DateTime forDate) {
			return forDate >= DateContext.Now.Add(TimeSkewHelper.MaxTimeSkewWindow).Date;
		}

		DateTime _lastWakeup= DateTime.MinValue;
		readonly static TimeSpan _timeBetweenChecks = TimeSpan.FromMinutes(5);
		

		public void Wakeup() {
			if((DateContext.Now - _lastWakeup) < _timeBetweenChecks) {
				return;
			} 
			_lastWakeup = DateContext.Now;
						

			foreach(var date in _requestsReader.GetDatesInRequestTable().Where(d=>!UseRealtimeData(d))) {
				if(!_preCalculatedReadStrategy.HasPreCalculatedData(date)) {
					_preCalculatedReadStrategy.PreCalculateData(date);
				}
				else {
					//this means we have already aggregated data for this day, in theory this shouldn't happen (we account for clock skew with MaxTimeSkewWindow)
					//todo: log this
				}
			}
			
		}
	}
}
