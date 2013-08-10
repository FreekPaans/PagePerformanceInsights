using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Module.Filters {
	class RemovePPIHandlerFilter : IFilterPagesToAnalyze{
		public string Filter(HttpContext context,string currentPageName) {
			if(context.CurrentHandler is PPIHandler) {
				return null;
			}
			return currentPageName;
		}
	}
}