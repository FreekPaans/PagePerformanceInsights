using PagePerformanceInsights.Module.Filters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Module {
	static class PageNameFilter {
		readonly static IFilterPagesToAnalyze[] _filters;
		static PageNameFilter() {
			var filters = new List<IFilterPagesToAnalyze> { new RemovePPIHandlerFilter() };

			foreach(var toloadFilter in ConfigurationManager.AppSettings["PagePerformanceInsights.Filters"].Split(';')) {
				filters.Add((IFilterPagesToAnalyze)Activator.CreateInstance(Type.GetType(toloadFilter)));
			}
			_filters = filters.ToArray();
		}

		public static string Filter(HttpContext httpContext) {
			var result  = httpContext.Request.Url.LocalPath;
			foreach(var filter in _filters) {
				result = filter.Filter(httpContext,result);
				if(result == null) {
					return null;
				}
			}

			return result;
		}

		
	}
}