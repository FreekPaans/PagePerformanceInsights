using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Configuration {
	public class SecuritySection : ConfigurationSection{
		[ConfigurationProperty("allowRemote",DefaultValue=false)]
		public bool AllowRemote {
			get {
				return (bool)this["allowRemote"];
			}
			set {
				this["allowRemote"] = value;
			}
		}

		public static SecuritySection Get() {

			return (SecuritySection)(ConfigurationManager.GetSection("ppi/security")??new SecuritySection());
		}
	}
}