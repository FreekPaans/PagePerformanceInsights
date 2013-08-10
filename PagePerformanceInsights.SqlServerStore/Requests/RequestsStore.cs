using PagePerformanceInsights.CommBus;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using PagePerformanceInsights.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class RequestsStore {
		readonly string _connectionString;
		readonly StoreRequests _storeRequests;
		readonly IProvidePageIds _pageIdProvider;
		readonly RequestTableReadStrategy _requestTableReadStrategy;
		readonly PreCalculatedReadStrategy _preCalculatedReadStrategy;
		
		public RequestsStore(string connectionString, IProvidePageIds pageIdProvider) {
			_connectionString = connectionString;
			_pageIdProvider  = pageIdProvider;
			_storeRequests = new StoreRequests(_connectionString,_pageIdProvider);
			_preCalculatedReadStrategy = new PreCalculatedReadStrategy(_connectionString);
			_requestTableReadStrategy = new RequestTableReadStrategy(_connectionString,_pageIdProvider);
		}

		
		public void Store(CommBus.HttpRequestData[] res) {
			_storeRequests.Store(res);
		}

		readonly Dictionary<DateTime,Dictionary<string,List<int>>> _dateToDurationCache = new Dictionary<DateTime,Dictionary<string,List<int>>>();
		
		
		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			forDate = forDate.Date;

			if(!IsToday(forDate)) {
				return _preCalculatedReadStrategy.GetRequestStatisticsForAllPages(forDate);
			}
			return _requestTableReadStrategy.GetRequestStatisticsForAllPages(forDate);
		}

		//private void AssertCacheAvailable(DateTime forDate) {
		//	if(HasCacheForDate(forDate)) {	
		//		return;
		//	}
		//	var requests = GetRequestsForDate(forDate);
		//	StoreInCacheTable(forDate,requests);
		//}

		//private void StoreInCacheTable(DateTime date, PerformanceStatisticsForPageCollection requests) {
		//	using(var conn = new SqlConnection()) {
		//		var cmd = conn.CreateCommand();

		//		cmd.CommandText = "insert into RequestsCache(Date,PageHash,Count,Median,Mean,Sum) values(@Date,@PageHash,@Count,@Median,@Mean,@Sum)";
		//		cmd.Parameters.Add("Date",SqlDbType.Date);
		//		cmd.Parameters.Add("PageHash",SqlDbType.Char);
		//		cmd.Parameters.Add("Count",SqlDbType.Int);
		//		cmd.Parameters.Add("Median",SqlDbType.Int);
		//		cmd.Parameters.Add("Mean",SqlDbType.Int);
		//		cmd.Parameters.Add("Sum",SqlDbType.Int);

		//		cmd.Parameters["Date"].Value = date;
				
		//		conn.Open();
		//		InsertPageInCache(cmd,"all", requests.StatisticsForAllPages.Count,requests.StatisticsForAllPages.Median, requests.StatisticsForAllPages.Mean,requests.StatisticsForAllPages.Sum);
		//		foreach(var page in requests.PageStatistics) {
		//			InsertPageInCache(cmd, _pageIdProvider.GetPageHash(page.PageName),page.Count,page.Median,page.Mean,page.Sum);;
		//		}
		//	}
		//}

		//private void InsertPageInCache(SqlCommand cmd,string pageHash, int count, int median, int mean, int sum) {
		//	cmd.Parameters["PageHash"].Value = pageHash;
		//	cmd.Parameters["Count"].Value = count;
		//	cmd.Parameters["Median"].Value = median;
		//	cmd.Parameters["Mean"].Value = mean;
		//	cmd.Parameters["Sum"].Value = sum;
		//	cmd.ExecuteNonQuery();
		//}

		//private bool HasCacheForDate(DateTime forDate) {
		//	using(var conn = new SqlConnection(_connectionString)) {
		//		var q = conn.CreateCommand();
		//		q.CommandText = "select count(*) from RequestStatsCache where Date=@Date";
		//		q.Parameters.Add(new SqlParameter("Date",forDate));
		//		conn.Open();
		//		return (int)q.ExecuteScalar()!=0;
		//	}
		//}

		private bool IsToday(DateTime forDate) {
			return forDate == DateContext.Today;
		}

		//private static bool IsToday(DateTime forDate) {
		//	return forDate == 
		//}

		//private void InvalidateCache() {
		//	//remove cache from previous days
		//	var dates = _dateToDurationCache.Keys;
		//	_dateToDurationCache.Clear();

		//	foreach(var date in dates) {
		//		BuildCacheDurationTable(date);
		//	}
		//}

		//private void BuildCacheDurationTable(DateTime date) {
		//	throw new NotImplementedException();
		//}
	}
}
