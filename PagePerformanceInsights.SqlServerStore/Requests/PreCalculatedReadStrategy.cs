using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class PreCalculatedReadStrategy : IRequestStatsStrategy{
		readonly string _connectionString;
		public PreCalculatedReadStrategy(string connectionString) {
			
			_connectionString = connectionString;
		}
		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetRequestStatisticsForAllPages(DateTime forDate) {
			return GetFromCacheTable(forDate);
		}

		private PerformanceStatisticsForPageCollection GetFromCacheTable(DateTime forDate) {
			//AssertCacheAvailable(forDate);

			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = @"select * from PreCalculatedPagesStatistics rc left join PageIds pid on pid.PageSHA1 = rc.PageHash where Date=@Date";
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
	}
}
