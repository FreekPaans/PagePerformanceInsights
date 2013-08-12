using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class PreCalculatedTrendStrategy : ITrendReadStrategy{
		readonly RequestsReader _reader;
		readonly IProvidePageIds _pageIdProvider;
		readonly string _connectionString;
		readonly ITrendReadStrategy _backend;

		const int AllPagesPageId= 0;

		public PreCalculatedTrendStrategy(string connectionString,RequestsReader reader, IProvidePageIds pageIdProvider, ITrendReadStrategy backend) {
			_reader = reader;
			_pageIdProvider = pageIdProvider;
			_connectionString = connectionString;
			_backend = backend;
		}
		public Handler.PerformanceData.DataTypes.PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "select Hour,Count,Median,Mean,_90Percentile from PreCalculatedHourlyTrend where Date=@Date and PageId=@PageId";

				
				cmd.Parameters.Add(new SqlParameter("PageId",GetPageId(forPage)));
				cmd.Parameters.Add(new SqlParameter("Date", forDate));
				var res = Enumerable.Range(0,24).ToDictionary(k=>k, k=>PageStatisticsTrend._TrendData.Empty(forDate.AddHours(k)));
				conn.Open();
				using(var reader = cmd.ExecuteReader()) {
					while(reader.Read()) {
						var hour = (int)reader["Hour"];
						res[hour] = new PageStatisticsTrend._TrendData{ 
							_90PCT = (int)reader["_90Percentile"], 
							Median = (int)reader["Median"],
							Count = (int)reader["Count"],
							Mean = (int)reader["Mean"],
							TimeStamp = forDate.AddHours(hour)
						};
					}
					return new PageStatisticsTrend(res.Values.OrderBy(v=>v.TimeStamp).ToArray());
				}
			}	
		}

		public bool HasPrecalculatedHourlyTrend(DateTime forDate, string forPage) {
			return HasPrecalculatedHourlyTrend(forDate,GetPageId(forPage));
		}

		private int GetPageId(string forPage) {
			if(forPage==null) {
				return AllPagesPageId;
			}
			return _pageIdProvider.GetPageId(forPage);
		}

		private bool HasPrecalculatedHourlyTrend(DateTime forDate,int pageId) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "select count(*) from PreCalculatedHourlyTrend where Date=@Date and PageId=@PageId";
				cmd.Parameters.Add(new SqlParameter("Date",forDate));
				cmd.Parameters.Add(new SqlParameter("PageId", pageId));
				conn.Open();
				return (int)cmd.ExecuteScalar()>0;
			}
		}
		
		public void PreCalculate(DateTime date) {
			if(!HasPrecalculatedHourlyTrend(date, null)) {
				PreCalculateHourlyTrend(date,AllPagesPageId, null);
			}

			var pages = _reader.ListAllPages(date);

			var pageNames = _pageIdProvider.GetPageNames(pages);

			foreach(var pageId in pages) {
				var pageName = pageNames[pageId];
				if(HasPrecalculatedHourlyTrend(date,pageName)) {
					//shouldn't happen.
					continue;
				}
				PreCalculateHourlyTrend(date,pageId,pageName);
			}
		}

		private void PreCalculateHourlyTrend(DateTime date,int pageId, string pageName) {
			var calc = _backend.GetHourlyTrend(date,pageName);

			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = @"insert into PreCalculatedHourlyTrend (Date,PageId,Hour,Count,Median,Mean,_90Percentile) 
					select @Date,@PageId,@hour,@Count,@Median,@Mean,@_90Percentile where not exists (
						select * from PreCalculatedHourlyTrend where Date=@Date and PageId=@PageId and Hour=@hour
					)";
				cmd.Parameters.Add(new SqlParameter("Date", date));
				cmd.Parameters.Add(new SqlParameter("PageId",pageId));
				cmd.Parameters.Add(new SqlParameter("Hour",SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("Count",SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("Mean",SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("Median",SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("_90Percentile",SqlDbType.Int));

				conn.Open();

				foreach(var hourData in calc.TrendData) {
					cmd.Parameters["Hour"].Value = hourData.TimeStamp.Hour;
					cmd.Parameters["Count"].Value = hourData.Count;
					cmd.Parameters["Mean"].Value = hourData.Mean;
					cmd.Parameters["Median"].Value = hourData.Median;
					cmd.Parameters["_90Percentile"].Value = hourData._90PCT;
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
