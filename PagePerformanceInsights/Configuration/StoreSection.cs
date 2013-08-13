using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Configuration {
	public class StoreSection : ConfigurationSection{
		[ConfigurationProperty("nameOrType",IsRequired=true)]
		//[StringValidator(MinLength=1,MaxLength=int.MaxValue)]
		public string StoreNameOrType {
			get {
				return (string)this["nameOrType"];
			}
			set {
				this["nameOrType"] = value;
			}
		}

		public static StoreSection Get() {
			return (StoreSection)(ConfigurationManager.GetSection("ppi/store")??new StoreSection());
		}
	}
}