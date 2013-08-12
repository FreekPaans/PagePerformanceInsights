using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class PreCalculatedAllPagesStrategy : IAllPagesReadStrategy{
		readonly string _connectionString;
		readonly IAllPagesReadStrategy _backend;
		readonly IProvidePageIds _pageIdProvider;

		public PreCalculatedAllPagesStrategy(string connectionString, IAllPagesReadStrategy backend, IProvidePageIds pageIdProvider) {
			_backend = backend;
			_connectionString = connectionString;
			_pageIdProvider=  pageIdProvider;
		}
		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetRequestStatisticsForAllPages(DateTime forDate) {
			return GetFromCacheTable(forDate);
		}

		private PerformanceStatisticsForPageCollection GetFromCacheTable(DateTime forDate) {
			//AssertCacheAvailable(forDate);

			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = @"select * from PreCalculatedPagesStatistics rc left join PageIds pid on pid.Id = rc.PageId where Date=@Date";
				cmd.Parameters.Add(new SqlParameter("Date",forDate));
				conn.Open();

				var pageList = new List<PerformanceStatisticsForPage>();
				var allPages = PerformanceStatisticsForPage.Empty;
				using(var rdr = cmd.ExecuteReader()) {
					while(rdr.Read()) {
						var item = new PerformanceStatisticsForPage {
							Count = (int)rdr["Count"],
							Mean = (int)rdr["Mean"],
							Median = (int)rdr["Median"],
							Sum = (int)rdr["Sum"]
						};

						if(rdr["PageName"]==DBNull.Value) {
							item.PageName = "All Pages";
							allPages = item;
						}
						else {
							item.PageName=  (string)rdr["PageName"];
							pageList.Add(item);
							
						}
					}

					return new PerformanceStatisticsForPageCollection(pageList.ToArray(),allPages);
				}
			}
		}

		public bool HasPreCalculatedData(DateTime date) {
			using(var conn = new SqlConnection(_connectionString)) {
				var q = conn.CreateCommand();
				q.CommandText = "select count(*) from PreCalculatedPagesStatistics where Date=@Date";
				q.Parameters.Add(new SqlParameter("Date",date));
				conn.Open();
				return (int)q.ExecuteScalar()!=0;
			}
		}

		public void PreCalculateData(DateTime date) {
			var requests = _backend.GetRequestStatisticsForAllPages(date);

			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();

				cmd.CommandText = "insert into PreCalculatedPagesStatistics(Date,PageId,Count,Median,Mean,Sum) select Convert(Date,@Date),@PageId,@Count,@Median,@Mean,@Sum where not exists(select * from PreCalculatedPagesStatistics pcps where pcps.Date=@Date and pcps.PageId=@PageId)"; //where not exists for optimistic concurrency
				cmd.Parameters.Add("Date",SqlDbType.Date);
				cmd.Parameters.Add("PageId",SqlDbType.Int);
				cmd.Parameters.Add("Count",SqlDbType.Int);
				cmd.Parameters.Add("Median",SqlDbType.Int);
				cmd.Parameters.Add("Mean",SqlDbType.Int);
				cmd.Parameters.Add("Sum",SqlDbType.Int);

				cmd.Parameters["Date"].Value = date;

				conn.Open();
				InsertPageInCache(cmd,AllPagesPageId,requests.StatisticsForAllPages.Count,requests.StatisticsForAllPages.Median,requests.StatisticsForAllPages.Mean,requests.StatisticsForAllPages.Sum);
				foreach(var page in requests.PageStatistics) {
					InsertPageInCache(cmd,_pageIdProvider.GetPageId(page.PageName),page.Count,page.Median,page.Mean,page.Sum); ;
				}
			}
		}

		const int AllPagesPageId = 0;

		private void InsertPageInCache(SqlCommand cmd,int  pageId,int count,int median,int mean,int sum) {
			cmd.Parameters["PageId"].Value = pageId;
			cmd.Parameters["Count"].Value = count;
			cmd.Parameters["Median"].Value = median;
			cmd.Parameters["Mean"].Value = mean;
			cmd.Parameters["Sum"].Value = sum;
			cmd.ExecuteNonQuery();
		}
	}
}
