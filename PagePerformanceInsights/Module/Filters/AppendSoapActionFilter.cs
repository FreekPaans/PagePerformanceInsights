using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Module.Filters {
	public class AppendSoapActionFilter  : IFilterPagesToAnalyze{
		public string Filter(HttpContext context,string currentPageName) {
			if(string.IsNullOrWhiteSpace(context.Request.Headers["SOAPAction"])) {
				return currentPageName;
			}

			return string.Format("{0}|{1}", currentPageName,context.Request.Headers["SOAPAction"].Trim(new [] { '"' }));
		}
	}
}