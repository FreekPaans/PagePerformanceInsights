using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Module.Filters {
	class IdentityFilter:IFilterPagesToAnalyze {
		public string Filter(HttpContext context,string currentPageName) {
			return currentPageName;
		}
	}
}