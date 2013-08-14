using PagePerformanceInsights.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PagePerformanceInsights.Filters.MVC{
	public class GroupMvcControllerActionFilter : IFilterPagesToAnalyze {
		public string Filter(System.Web.HttpContext context,string currentPageName) {
			var mvcHandler = context.Handler as MvcHandler;
			if(mvcHandler==null) {
				return currentPageName;
			}

			try {
				return string.Format("/{0}/{1}",mvcHandler.RequestContext.RouteData.Values["controller"].ToString().ToLower(),mvcHandler.RequestContext.RouteData.Values["action"].ToString().ToLower());
			}
			catch {
				//not to sure whether ["controller"] and ["action"] are always available, so use this
				return currentPageName;
			}
		}
	}
}
