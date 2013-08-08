using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.CommBus {
	public class HttpRequestData {
		public int Duration{get;set;}
		public string Page{get;set;}
		public DateTime Timestamp{get;set;}
	}
}