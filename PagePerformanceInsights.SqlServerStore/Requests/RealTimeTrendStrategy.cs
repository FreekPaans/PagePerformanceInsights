using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class RealTimeTrendStrategy : ITrendReadStrategy{
		readonly string _connectionString;
		readonly IProvidePageIds _pageIdProvider;

		public RealTimeTrendStrategy(string connectionString, IProvidePageIds pageIdProvider) {
			_connectionString = connectionString;
			_pageIdProvider = pageIdProvider;
		}
		
		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "select DATEPART(hh,Timestamp) Hour,Duration from Requests where Timestamp>=@From and Timestamp<@Till";

				cmd.Parameters.Add(new SqlParameter("From", forDate));
				cmd.Parameters.Add(new SqlParameter("Till",forDate.AddDays(1)));

				if(forPage!=null) {
					cmd.CommandText+=" and PageId=@PageId";
					cmd.Parameters.Add(new SqlParameter("PageId", _pageIdProvider.GetPageId(forPage)));
				}

				var res = new Dictionary<int,List<int>>();

				conn.Open();
				using(var reader = cmd.ExecuteReader()) {
					while(reader.Read()) {
						var hour = (int)reader["Hour"];
						var duration = (int)reader["Duration"];
						if(!res.ContainsKey(hour)) {
							res[hour] = new List<int>();
						}
						res[hour].Add(duration);
					}


					return PageStatisticsTrend.HourlyFromHourDataDictionary(forDate,res);
				}
			}
		}
	}
}
