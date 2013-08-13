using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.Configuration {
	public class SqlServerStoreSection : ConfigurationSection {
		[ConfigurationProperty("connectionStringOrName", IsRequired=false)]
		public string ConnectionStringOrName{
			get {
				return (string)this["connectionStringOrName"];
			}
			set {
				this["connectionStringOrName"] = value;
			}
		}

		[ConfigurationProperty("requestsDataRetentionAfterDate",IsRequired=false)]
		public TimeSpan? RequestsRetention {
			get {
				var ts = this["requestsDataRetentionAfterDate"] as TimeSpan?;

				if(ts==null) {
					return ts;
				}

				if(ts < TimeSpan.Zero) {
					throw new ArgumentException("requestsDataRetentionAfterDate cannot be negative");
				}
				return ts;

				
				//return TimeSpan.Parse(strValue);
			}
			set {
				if(value==null) {
					this["individualRequestsDataRetention"] =null;
				}
				else {
					this["individualRequestsDataRetention"] = value.Value.ToString();
				}
			}
		}

		
		public static SqlServerStoreSection Get() {
			return (SqlServerStoreSection)(ConfigurationManager.GetSection("ppi/sqlServerStore")??new SqlServerStoreSection());
		}
	}
}
