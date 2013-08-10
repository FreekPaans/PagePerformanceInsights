using PagePerformanceInsights.SqlServerStore.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore {
	class SqlPageIdProvider : IProvidePageIds{
		readonly string _connectionString;

		public SqlPageIdProvider(string connectionString) {
			_connectionString = connectionString;
		}

		const string TempInsertTableName = "#InsertPageTable";

		public Dictionary<string,int> GetPageIds(ICollection<string> pageNames) {
			using(var connection = new SqlConnection(_connectionString)) {
				var pageNameSHA1Table = CreateDataTableWithNameAndSha1Columns(pageNames);

				connection.Open();

				CreateTempTable(connection,TempInsertTableName);
				
				InsertNotKnownPagesFromTempTable(pageNameSHA1Table,connection, TempInsertTableName);
				
				return GetPageNameToIdFromTempTable(connection,TempInsertTableName);
			}
		}

		private DataTable CreateDataTableWithNameAndSha1Columns(ICollection<string> pageNames) {
			var pages = pageNames.Distinct().Select(p => new { Name = p,SHA1 = SHA1Helper.GetSHA1String(p) }).ToArray();

			var tbl = new DataTable();
			tbl.Columns.Add("PageName");
			tbl.Columns.Add("PageSHA1");
			foreach(var page in pages) {
				tbl.Rows.Add(new[] { page.Name,page.SHA1 });
			}
			return tbl;
		}

		private static SqlCommand InsertNotKnownPagesFromTempTable(DataTable tbl,SqlConnection connection, string fromTableName) {
			using(var copy = new SqlBulkCopy(connection)) {
				copy.ColumnMappings.Add("PageName","PageName");
				copy.ColumnMappings.Add("PageSHA1","PageSHA1");
				copy.DestinationTableName = TempInsertTableName;
				copy.WriteToServer(tbl);
			}

			var cmd = connection.CreateCommand();
			cmd.CommandText = string.Format(
@"merge into PageIds pids using {0} toinsert on pids.PageSHA1=toinsert.PageSHA1 
when not matched then insert (PageSHA1,PageName) values(toinsert.PageSHA1,toinsert.PageName)
;",fromTableName);

			cmd.ExecuteNonQuery();
			return cmd;
		}

		private void CreateTempTable(SqlConnection connection,string tableName) {
			//var tsql = 
			var cmd = connection.CreateCommand();
			cmd.CommandText = string.Format(@"create table {0} (PageName nvarchar(max), PageSHA1 char(40))",tableName);
			cmd.ExecuteNonQuery();
		}


		
		

		private Dictionary<string,int> GetPageNameToIdFromTempTable(SqlConnection connection, string fromTable) {
			var pages = connection.CreateCommand();
			pages.CommandText = string.Format(@"select pids.PageName, pids.Id PageId from {0} temp join PageIds pids on pids.PageSHA1=temp.PageSHA1",fromTable);
			var pageSha1ToIdMap = new Dictionary<string,int>();
			using(var rdr = pages.ExecuteReader()) {
				while(rdr.Read()) {
					pageSha1ToIdMap[(string)rdr["PageName"]]=  (int)rdr["PageId"];
				}
				return pageSha1ToIdMap;
			}
		}



		public string GetPageHash(string pageName) {
			return SHA1Helper.GetSHA1String(pageName);
		}


		public Dictionary<int,string> GetPageNames(ICollection<int> pageIds) {
			if(!pageIds.Any()) {
				return new Dictionary<int,string>();
			}
			using(var connection = new SqlConnection(_connectionString)) {
				var pageQuery = connection.CreateCommand();
				pageQuery.CommandText = string.Format("select PageName,Id from PageIds where Id in ({0})",string.Join(",",pageIds));
				var pageIdToNameMap = new Dictionary<int,string>();

				connection.Open();
				using(var rdr = pageQuery.ExecuteReader()) {
					while(rdr.Read()) {
						pageIdToNameMap[(int)rdr["Id"]] = (string)rdr["PageName"];
					}
				}
				return pageIdToNameMap;
			}
		}



		public int GetPageId(string forPage) {
			return GetPageIds(new [] { forPage})[forPage];
		}
	}
}
