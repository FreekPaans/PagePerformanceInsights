using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class RealTimeDistributionStrategy:IDistributionReadStrategy {
		readonly string _connectionString;
		readonly IProvidePageIds _pageIdProvider;

		public RealTimeDistributionStrategy(string connectionString, IProvidePageIds pageIdProvider) {
			_connectionString = connectionString;
			_pageIdProvider = pageIdProvider;
		}
		public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			using(var connection = new SqlConnection(_connectionString)) {
				var cmd = connection.CreateCommand();
				cmd.CommandText = "select Duration from Requests where Timestamp>=@From and Timestamp<@Till";

				if(forPage!=null) {
					cmd.CommandText+=" and PageId=@PageId";
					cmd.Parameters.Add(new SqlParameter("PageId", _pageIdProvider.GetPageId(forPage)));
				}

				cmd.Parameters.Add(new SqlParameter("From", forDate));
				cmd.Parameters.Add(new SqlParameter("Till",forDate.AddDays(1)));

				var durations = new List<int>();

				connection.Open();
				using(var rdr = cmd.ExecuteReader()) {
					while(rdr.Read()) {
						//var pageId = (int)rdr["PageId"];
						//if(!perPage.ContainsKey(pageId)) {
						//	perPage[pageId]= new List<int>();
						//}
						durations.Add((int)rdr["Duration"]);
					}
				}

				return PageDurationDistributionHistogram.FromDistribution(durations.ToArray());
			}
		}
	}
}
