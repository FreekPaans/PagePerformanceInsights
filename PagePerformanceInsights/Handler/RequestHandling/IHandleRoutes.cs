using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace PagePerformanceInsights.Handler.RequestHandling {
	public interface IHandleRoutes {
		void Run(HttpContext context);
	}
}
