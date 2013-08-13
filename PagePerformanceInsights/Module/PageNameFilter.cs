using PagePerformanceInsights.Module.Filters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using PagePerformanceInsights.Configuration;
using PagePerformanceInsights.Events;

namespace PagePerformanceInsights.Module {
	static class PageNameFilter {
		readonly static IFilterPagesToAnalyze[] _filters;
		readonly static EventLogHelper _logger;
		static PageNameFilter() {
			_logger = new EventLogHelper(typeof(PageNameFilter));

			var filters = new List<IFilterPagesToAnalyze> { new RemovePPIHandlerFilter() };

			var config = FiltersSection.Get();

			foreach(var filter in config.Filters.Cast<FilterElement>()) {
				var filterType = Type.GetType(filter.NameOrType);
				if(filterType==null) {
					_logger.Warn(() => string.Format("Couldn't load type for filter: {0}",filter.NameOrType));
					continue;
				}

				filters.Add((IFilterPagesToAnalyze)Activator.CreateInstance(filterType));
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