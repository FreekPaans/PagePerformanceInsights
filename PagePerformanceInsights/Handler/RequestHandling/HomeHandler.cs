using PagePerformanceInsights.Handler.Views;
using PagePerformanceInsights.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace PagePerformanceInsights.Handler.RequestHandling {
	class HomeHandler :IHandleRoutes{
		//private HttpContext _context;

		public HomeHandler() {
			//_context = context;
		}
		public void Run(System.Web.HttpContext context) {
			var activeDate = DateContext.Now.Date;
			if(!string.IsNullOrEmpty(context.Request["date"])) {
				activeDate = DateTime.Parse(context.Request["date"]);
			}

			context.Response.Write(new Home { ActiveDate = activeDate }.TransformText());
		}
	}
}
