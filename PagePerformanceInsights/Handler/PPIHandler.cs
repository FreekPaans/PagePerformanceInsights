using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PagePerformanceInsights.Handler.Helpers;
using PagePerformanceInsights.Handler.ViewModels;
using PagePerformanceInsights.Helpers;
using System.Collections.Specialized;
using PagePerformanceInsights.Handler.Views;
using PagePerformanceInsights.Handler.RequestHandling;
using PagePerformanceInsights.Configuration;

namespace PagePerformanceInsights {
	public class PPIHandler : IHttpHandler{
		public bool IsReusable {
			get { return false; }
		}

		readonly static bool _allowRemote;

		static PPIHandler() {
			_allowRemote = SecuritySection.Get().AllowRemote;
			
		}

		public void ProcessRequest(HttpContext context) {
			if(!context.Request.IsLocal && !_allowRemote) {
				throw new HttpException(403, "Not allowed");
			}
			var handler = new RequestRouter().GetHandler(context);

			handler.Run(context);
		}
	}
}
