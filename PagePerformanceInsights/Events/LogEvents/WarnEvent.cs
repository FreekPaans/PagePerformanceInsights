using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Events.LogEvents {
	public class WarnEvent : IPPIEvent{
		readonly Func<string> _msgGenerator;
		readonly Type _source;

		public Type Source {
			get { return _source; }
		} 


		public string Message {
			get { return _msgGenerator(); }
		}

		public WarnEvent(Type source,Func<string> msgGenerator) {
			_msgGenerator = msgGenerator;
			
			_source = source;

		}
	}
}