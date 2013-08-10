using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class RequestsWriter  {
		readonly string _connectionString;
		readonly IProvidePageIds _pageProvider;

		public RequestsWriter(string connectionString, IProvidePageIds pageProvider) {
			_pageProvider = pageProvider;
			_connectionString = connectionString;
		}
		public void Store(CommBus.HttpRequestData[] res) {
			if(!res.Any()) {
				return;
			}

			var durationTable = GetPageDurationTable(res);
			using(var connection = new SqlConnection(_connectionString)) {
				connection.Open();

				InsertPageDuration(connection,durationTable);
			}
		}

		private void InsertPageDuration(SqlConnection connection,DataTable durationTable) {
			using(var bcp = new SqlBulkCopy(connection)) {
				bcp.DestinationTableName = "Requests";
				bcp.ColumnMappings.Add("PageId","PageId");
				bcp.ColumnMappings.Add("Duration","Duration");
				bcp.ColumnMappings.Add("Timestamp","Timestamp");
				bcp.WriteToServer(durationTable);
			}
		}

		private DataTable GetPageDurationTable(CommBus.HttpRequestData[] res) {
			var pageNameToIdMap= _pageProvider.GetPageIds(res.Select(r=>r.Page).ToArray());

			var pages = res.Select(p => new object[] { pageNameToIdMap[p.Page],p.Duration,p.Timestamp }).ToArray();

			var tbl = new DataTable();
			tbl.Columns.Add("PageId");
			tbl.Columns.Add("Duration");
			tbl.Columns.Add("Timestamp");
			foreach(var page in pages) {
				tbl.Rows.Add(page);
			}
			return tbl;
		}

		public void DeleteRealtimeData(DateTime date) {
			using(var conn = new SqlConnection(_connectionString)) {
				var cmd = conn.CreateCommand();
				cmd.CommandText = "delete from Requests where Timestamp >=@From and Timestamp< @Till";
				cmd.Parameters.Add(new SqlParameter("From",date));
				cmd.Parameters.Add(new SqlParameter("Till",date.AddDays(1)));
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}




		//static StoreRequests() {
		//}

		//public void Wakeup() {
		//	if(date < DateContext.Now.Add(_requestsRetentionTime.Negate()).Date) {
		//		DeleteRealtimeData(date);
		//	}
		//}
	}
}
