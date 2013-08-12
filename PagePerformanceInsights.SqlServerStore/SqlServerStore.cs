using PagePerformanceInsights.Handler.PerformanceData;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using PagePerformanceInsights.Helpers;
using PagePerformanceInsights.SqlServerStore.Requests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

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

		public SqlServerStore() : this(ConfigurationManager.ConnectionStrings["PPI.SqlServerStore"].ConnectionString) {
		}

		public SqlServerStore(string connectionStringOrConnectionStringName) {
			var fromConfig = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName];
			if(fromConfig!=null) {
				_connectionString = fromConfig.ConnectionString;
			}
			_connectionString = connectionStringOrConnectionStringName;

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
		
		static SqlServerStore () {
			TimeSpan retention;
			if(TimeSpan.TryParse(ConfigurationManager.AppSettings["PPI.SqlServerStore.RequestsRetention"],out retention)) {
				_requestsDataRetentionTime = retention;
			}
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
