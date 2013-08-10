using PagePerformanceInsights.CommBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace PagePerformanceInsights.Module {
	public interface IFilterPagesToAnalyze {
		//IFilterPagesToAnalyze Chain(IFilterPagesToAnalyze filter);
		string Filter(HttpContext context, string currentPageName);
	}
}
