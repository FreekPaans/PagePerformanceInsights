using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Requests {
	public class RequestsReader {
		readonly string _connectionString;
		public RequestsReader(string connectionString) {
			_connectionString = connectionString;
		}

		public ICollection<DateTime> GetDatesInRequestTable() {
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

		public ICollection<int> ListAllPages(DateTime date) {
			using(var conn = new SqlConnection(_connectionString)) {
				var q = conn.CreateCommand();
				q.CommandText = "select Distinct PageId from Requests where Timestamp>=@From and Timestamp<@Till";
				q.Parameters.Add(new SqlParameter("From", date));
				q.Parameters.Add(new SqlParameter("Till",date.AddDays(1)));

				var res = new List<int>();

				conn.Open();
				using(var rdr=  q.ExecuteReader()) {
					while(rdr.Read()) {
						res.Add((int)rdr["PageId"]);
					}
					return res;
				}
			}
		}
	}
}
