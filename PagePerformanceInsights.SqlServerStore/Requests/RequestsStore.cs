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
	class RequestsStore : INeedToBeWokenUp {
		readonly string _connectionString;
		readonly StoreRequests _storeRequests;
		readonly IProvidePageIds _pageIdProvider;
		readonly RealTimeReadStrategy _realTimeReadStrategy;
		readonly PreCalculatedReadStrategy _preCalculatedReadStrategy;
		
		public RequestsStore(string connectionString, IProvidePageIds pageIdProvider) {
			_connectionString = connectionString;
			_pageIdProvider  = pageIdProvider;
			_storeRequests = new StoreRequests(_connectionString,_pageIdProvider);
			_preCalculatedReadStrategy = new PreCalculatedReadStrategy(_connectionString);
			_realTimeReadStrategy = new RealTimeReadStrategy(_connectionString,_pageIdProvider);
		}

		
		public void Store(CommBus.HttpRequestData[] res) {
			_storeRequests.Store(res);
		}

		//readonly Dictionary<DateTime,Dictionary<string,List<int>>> _dateToDurationCache = new Dictionary<DateTime,Dictionary<string,List<int>>>();
		
		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			forDate = forDate.Date;

			if(!UseRealtimeData(forDate)) {
				return _preCalculatedReadStrategy.GetRequestStatisticsForAllPages(forDate);
			}
			return _realTimeReadStrategy.GetRequestStatisticsForAllPages(forDate);
		}

		//private void AssertCacheAvailable(DateTime forDate) {
		//	if(HasCacheForDate(forDate)) {	
		//		return;
		//	}
		//	var requests = GetRequestsForDate(forDate);
		//	StoreInCacheTable(forDate,requests);
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

		readonly static TimeSpan MaxTimeSkewWindow = TimeSpan.FromMinutes(5).Negate();

		private bool UseRealtimeData(DateTime forDate) {
			return forDate >= DateContext.Now.Add(MaxTimeSkewWindow).Date;
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

		DateTime _lastWakeup= DateTime.MinValue;
		readonly static TimeSpan _timeBetweenChecks = TimeSpan.FromMinutes(5);

		public void Wakeup() {
			if((DateContext.Now - _lastWakeup) < _timeBetweenChecks) {
				return;
			} 
			foreach(var date in GetDatesInRequestTable()) {
				if(UseRealtimeData(date)) {
					continue;
				}
				if(!HasPreCalculatedData(date)) {
					PreCalculateData(date);
				}
				else {
					//this means we have already aggregated data for this day, in theory this shouldn't happen (we account for clock skew with MaxTimeSkewWindow)
					//todo: log this
				}

				DeleteRealtimeData(date);
			}
		}

		private void DeleteRealtimeData(DateTime date) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "delete from Requests where Timestamp >=@From and Timestamp< @Till";
				cmd.Parameters.Add(new SqlParameter("From", date));
				cmd.Parameters.Add(new SqlParameter("Till",date.AddDays(1)));
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		private bool HasPreCalculatedData(DateTime date) {
			using(var conn = new SqlConnection(_connectionString)) {
				var q = conn.CreateCommand();
				q.CommandText = "select count(*) from PreCalculatedPagesStatistics where Date=@Date";
				q.Parameters.Add(new SqlParameter("Date",date));
				conn.Open();
				return (int)q.ExecuteScalar()!=0;
			}
		}

		private void PreCalculateData(DateTime date) {
			var requests = _realTimeReadStrategy.GetRequestStatisticsForAllPages(date);

			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();

				cmd.CommandText = "insert into PreCalculatedPagesStatistics(Date,PageHash,Count,Median,Mean,Sum) values(@Date,@PageHash,@Count,@Median,@Mean,@Sum)";
				cmd.Parameters.Add("Date",SqlDbType.Date);
				cmd.Parameters.Add("PageHash",SqlDbType.Char);
				cmd.Parameters.Add("Count",SqlDbType.Int);
				cmd.Parameters.Add("Median",SqlDbType.Int);
				cmd.Parameters.Add("Mean",SqlDbType.Int);
				cmd.Parameters.Add("Sum",SqlDbType.Int);

				cmd.Parameters["Date"].Value = date;

				conn.Open();
				InsertPageInCache(cmd,"all",requests.StatisticsForAllPages.Count,requests.StatisticsForAllPages.Median,requests.StatisticsForAllPages.Mean,requests.StatisticsForAllPages.Sum);
				foreach(var page in requests.PageStatistics) {
					InsertPageInCache(cmd,_pageIdProvider.GetPageHash(page.PageName),page.Count,page.Median,page.Mean,page.Sum); ;
				}
			}
		}

		private void InsertPageInCache(SqlCommand cmd,string pageHash,int count,int median,int mean,int sum) {
			cmd.Parameters["PageHash"].Value = pageHash;
			cmd.Parameters["Count"].Value = count;
			cmd.Parameters["Median"].Value = median;
			cmd.Parameters["Mean"].Value = mean;
			cmd.Parameters["Sum"].Value = sum;
			cmd.ExecuteNonQuery();
		}



		private ICollection<DateTime> GetDatesInRequestTable() {
			using(var conn = new SqlConnection(_connectionString)) {
				var q = conn.CreateCommand();
				q.CommandText = "SELECT Convert(date,timestamp) Date from Requests group by (convert(date,timestamp))";

				var res = new List<DateTime>();
				conn.Open();
				using(var rdr = q.ExecuteReader()) {
					while(rdr.Read()) {
						res.Add((DateTime)rdr["Date"]);
					}
				}

				return res;
			}
		}
	}
}
