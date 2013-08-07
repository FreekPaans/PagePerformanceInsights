using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.RequestHandling {
	public class RequestRouter {
		private HttpContext _context;

		public IHandleRoutes GetHandler(HttpContext context) {
			_context = context;
			if(!string.IsNullOrWhiteSpace(context.Request["resource"])) {
				return Resource(context.Request["resource"]);
			}

			if(!string.IsNullOrWhiteSpace(context.Request["data"])) {
				return Data(context.Request["data"]);
			}

			return Home();

			
		}

		private IHandleRoutes Data(string data) {
			return new DataHandler(data);
		}

		private IHandleRoutes Home() {
			return new HomeHandler();
		}

		private IHandleRoutes Resource(string resourceValue) {
			return new ResourceRequestHandler(resourceValue);
		}
	}
}