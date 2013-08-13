using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PagePerformanceInsights.Events;

namespace PagePerformanceInsights.NLogLogger {
	public class RegisterLoggerModule : IHttpModule {
		static RegisterLoggerModule() {
			PPIEvents.Register(new LoggerHandler());
		}
		public void Dispose() {
			
		}

		public void Init(HttpApplication context) {
			
		}
	}
}
