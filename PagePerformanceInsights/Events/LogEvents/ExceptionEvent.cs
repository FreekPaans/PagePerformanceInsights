using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Events.LogEvents {
	public class ExceptionEvent : IPPIEvent{
		readonly Func<string> _msgGenerator;
		readonly Type _source;
		readonly Exception _exception;

		public Exception Exception {
			get { return _exception; }
		}

		public Type Source {
			get { return _source; }
		} 


		public string Message {
			get { return _msgGenerator(); }
		}

		public ExceptionEvent(Type source,Exception e, Func<string> msgGenerator) {
			_msgGenerator = msgGenerator;
			_exception = e;
			_source = source;

		}
	}
}