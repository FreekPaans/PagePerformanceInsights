using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Helpers {
	public static class DateContext {
		public static DateTime Today {
			get {
				return Now.Date;
			}
		}
		public static DateTime Now {
			get {
				return DateTime.Now;
			}
		}
	}
}