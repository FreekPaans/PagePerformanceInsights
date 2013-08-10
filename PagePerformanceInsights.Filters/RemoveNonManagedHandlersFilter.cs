using PagePerformanceInsights.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.Filters {
	public class RemoveNonManagedHandlersFilter : IFilterPagesToAnalyze{
		public string Filter(System.Web.HttpContext context,string currentPageName) {
			if(context.CurrentHandler == null) {
				return null;
			}
			return currentPageName;
		}
	}
}
