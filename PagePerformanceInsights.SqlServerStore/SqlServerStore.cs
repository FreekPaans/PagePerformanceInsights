using PagePerformanceInsights.Handler.PerformanceData;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using PagePerformanceInsights.SqlServerStore.Requests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore {
	public class SqlServerStore:IProvidePerformanceData,IStorePerformanceData {
		readonly string _connectionString;
		readonly RequestsStore _requestsStore;
		readonly SqlPageIdProvider _pageIdProvider;
		readonly Scheduler _scheduler;


		public SqlServerStore() : this(ConfigurationManager.ConnectionStrings["PPI.SqlServerStore"].ConnectionString) {
		}

		public SqlServerStore(string connectionStringOrConnectionStringName) {
			var fromConfig = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName];
			if(fromConfig!=null) {
				_connectionString = fromConfig.ConnectionString;
			}
			_connectionString = connectionStringOrConnectionStringName;

			_pageIdProvider = new SqlPageIdProvider(_connectionString);

			_requestsStore=  new RequestsStore(_connectionString,_pageIdProvider);
			_scheduler = new Scheduler(_requestsStore);
		}

		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			return _requestsStore.GetStatisticsForAllPages(forDate);
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			return PageDurationDistributionHistogram.Empty;
		}

		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetAllPagesDistribution(DateTime forDate) {
			return PageDurationDistributionHistogram.Empty;
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			return PageStatisticsTrend.Empty;
		}

		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate) {
			return PageStatisticsTrend.Empty;
		}

		public void Store(CommBus.HttpRequestData[] res) {
			_requestsStore.Store(res);
		}
	}
}
