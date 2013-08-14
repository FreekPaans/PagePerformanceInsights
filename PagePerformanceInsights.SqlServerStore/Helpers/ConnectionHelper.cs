using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Helpers {
	static class ConnectionHelper {
		public static bool TestConnection(string connectionString, out Exception e) {
			try {
				e=null;
				using(var connection = new SqlConnection(connectionString)) {
					var cmd =connection.CreateCommand();
					cmd.CommandText = "select @@version";
					connection.Open();
					cmd.ExecuteNonQuery();
					return true;
				}
			}
			catch(Exception _e) {
				e = _e;
				return false;
			}

		}
	}
}
