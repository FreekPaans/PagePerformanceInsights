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

			var filterConfig = ConfigurationManager.AppSettings["PPI.Filters"];
			if(!string.IsNullOrWhiteSpace(filterConfig)) {
				LoadFilters(filters,filterConfig);
			}

			
			_filters = filters.ToArray();
		}

		private static void LoadFilters(List<IFilterPagesToAnalyze> filters,string filterConfig) {
			foreach(var toloadFilter in filterConfig.Split(';')) {
				filters.Add((IFilterPagesToAnalyze)Activator.CreateInstance(Type.GetType(toloadFilter)));
			}
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