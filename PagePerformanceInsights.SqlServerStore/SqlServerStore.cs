﻿using PagePerformanceInsights.Handler.PerformanceData;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using PagePerformanceInsights.Helpers;
using PagePerformanceInsights.SqlServerStore.Requests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PagePerformanceInsights.Configuration;
using System.Data.SqlClient;
using PagePerformanceInsights.SqlServerStore.Helpers;
//using PagePerformanceInsights.Events;

namespace PagePerformanceInsights.SqlServerStore {
	public class SqlServerStore:IProvidePerformanceData,IStorePerformanceData, INeedToBeWokenUp {
		readonly string _connectionString;
		readonly AllPagesStore _allPagesStore;
		readonly IProvidePageIds _pageIdProvider;
		readonly Scheduler _scheduler;
		readonly RequestsWriter _requestsWriter;
		readonly RequestsReader _requestsReader;
		readonly DistributionStore _distributionStore;
		readonly TrendStore _trendStore;
		//readonly static EventLogHelper _eventLogger = new EventLogHelper();
		

		public SqlServerStore() : this(_configConnectionString) {
		}

		public SqlServerStore(string connectionStringOrConnectionStringName) {
			var fromConfig = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName];
			if(fromConfig!=null) {
				_connectionString = fromConfig.ConnectionString;
			}
			else {
				_connectionString = connectionStringOrConnectionStringName;
			}


			Exception outConn;
			if(!ConnectionHelper.TestConnection(_connectionString,out outConn)) {
				throw new ArgumentNullException("connection",outConn);
			}
			
			_pageIdProvider = new CachedPageIdProvider(new SqlPageIdProvider(_connectionString));

			_requestsWriter = new RequestsWriter(_connectionString,_pageIdProvider);
			_requestsReader = new RequestsReader(_connectionString);

			_allPagesStore=  new AllPagesStore(_connectionString,_pageIdProvider);
			_distributionStore = new DistributionStore(_connectionString,_pageIdProvider,_requestsReader);
			_trendStore = new TrendStore(_connectionString,_pageIdProvider,_requestsReader);

			_scheduler = new Scheduler(_allPagesStore,_distributionStore,_trendStore,this);
		}

		

		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			return _allPagesStore.GetStatisticsForAllPages(forDate);
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			return _distributionStore.GetPageDistribution(forDate,forPage);
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetAllPagesDistribution(DateTime forDate) {
			return _distributionStore.GetPageDistribution(forDate,null);
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			return _trendStore.GetHourlyTrend(forDate, forPage);
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate) {
			return _trendStore.GetHourlyTrend(forDate,null);
		}

		public void Store(CommBus.HttpRequestData[] res) {
			_requestsWriter.Store(res);
		}


		readonly static TimeSpan? _requestsDataRetentionTime;
		readonly static string _configConnectionString;

		static SqlServerStore () {
			var settings = SqlServerStoreSection.Get();

			_configConnectionString = settings.ConnectionStringOrName;
			_requestsDataRetentionTime = settings.RequestsRetention;
		}

		public void Wakeup() {
			var dates = _requestsReader.GetDatesInRequestTable();

			foreach(var date in dates) {
				if(_requestsDataRetentionTime!=null && date < DateContext.Now.Add(_requestsDataRetentionTime.Value.Negate()).Date) {
					_requestsWriter.DeleteRealtimeData(date);
				}
			}
		}
	}
}
