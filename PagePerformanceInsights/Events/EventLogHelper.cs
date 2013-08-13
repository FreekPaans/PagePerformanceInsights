using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagePerformanceInsights.Events.LogEvents;

namespace PagePerformanceInsights.Events {
	public class EventLogHelper {
		readonly Type _source;
		public EventLogHelper(Type source) {
			_source=  source;
		}		

		public void Warn(Func<string> msgGenerator) {
			PPIEvents.Raise(new WarnEvent(_source,msgGenerator));
		}

		public void Info(Func<string> msgGenerator) {
			PPIEvents.Raise(new InfoEvent(_source,msgGenerator));
		}

		public void LogException(string msg,Exception e) {
			PPIEvents.Raise(new ExceptionEvent(_source,e, ()=>msg));
		}
	}
}