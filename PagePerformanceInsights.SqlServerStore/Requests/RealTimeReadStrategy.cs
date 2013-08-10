using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class RealTimeReadStrategy : IRequestStatsStrategy {
		readonly string _connectionString;
		readonly IProvidePageIds _pageIdProvider;
		public RealTimeReadStrategy(string connectionString, IProvidePageIds pageIdProvider) {
			_connectionString = connectionString;
			_pageIdProvider = pageIdProvider;
		}

		public Handler.PerformanceData.DataTypes.PerformanceStatisticsForPageCollection GetRequestStatisticsForAllPages(DateTime forDate) {
			return GetRequestsForDate(forDate);
		}

		private PerformanceStatisticsForPageCollection GetRequestsForDate(DateTime forDate) {
			using(var connection = new SqlConnection(_connectionString)) {
				var q = connection.CreateCommand();
				q.CommandText = "select Duration,PageId from Requests where Timestamp>=@From and Timestamp<@Till";
				q.Parameters.Add(new SqlParameter("From",forDate));
				q.Parameters.Add(new SqlParameter("Till",forDate.AddDays(1)));

				connection.Open();

				var overall = new List<int>();
				var pages = new Dictionary<int,List<int>>();

				using(var rdr = q.ExecuteReader()) {
					while(rdr.Read()) {
						var page = (int)rdr[1];

						var duration = (int)rdr[0];

						overall.Add(duration);

						if(!pages.ContainsKey(page)) {
							pages[page] = new List<int>();
						}

						pages[page].Add(duration);
					}
				}

				var pageIdToNameMap = _pageIdProvider.GetPageNames(pages.Keys);

				return new PerformanceStatisticsForPageCollection(
					pages.Keys.Select(p => PerformanceStatisticsForPage.Calculate(pages[p].ToArray(),pageIdToNameMap[p])).ToArray(),
					//new PerformanceStatisticsForPage[0],
					PerformanceStatisticsForPage.Calculate(overall.ToArray(),"All pages")
				);
			}
		}
	}
}
