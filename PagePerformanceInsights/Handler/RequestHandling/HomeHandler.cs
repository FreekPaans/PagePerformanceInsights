using PagePerformanceInsights.Handler.Views;
using PagePerformanceInsights.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;

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

			context.Response.Write(new Home { 
				ActiveDate = activeDate,
				OperationalInfo = GetOperationalInfo()
			}.TransformText());
		}

		private ViewModels.PPIOperationalInfoViewModel GetOperationalInfo() {
			return new ViewModels.PPIOperationalInfoViewModel {
				PrivateBytesMB = Process.GetCurrentProcess().PrivateMemorySize64 / (1024*1024),
				QueueSize = CommBus.Buffer.GetBacklogSize(),
				AnalyzedRequestsPerSecond  = CommBus.Buffer.GetAnalyzedRequestsPerSecond(),
				BufferFlushFrequency =  CommBus.Buffer.GetBufferFlushFrequency()
			};
		}
	}
}
