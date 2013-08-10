using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	class StoreRequests {
		readonly string _connectionString;
		readonly IProvidePageIds _pageProvider;

		public StoreRequests(string connectionString, IProvidePageIds pageProvider) {
			_pageProvider = pageProvider;
			_connectionString = connectionString;
		}
		public void Store(CommBus.HttpRequestData[] res) {
			if(!res.Any()) {
				return;
			}

			//var pageIds = _pageProvider.GetPageIds(res.Select(p=>p.Page).ToArray());

			//_pageProvider.AssertPageIdsAvailable(res.Select(p=>p.Page).ToArray());
			//var tbl = GetPageNameDataTable(res);


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
	}
}
