using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Events {
	public static class PPIEvents {
		readonly static ConcurrentBag<IHandlePPIEvents> _handlers = new ConcurrentBag<IHandlePPIEvents>();

		public static void Register(IHandlePPIEvents handler) {
			_handlers.Add(handler);
		}

		public static void Raise<T>(T @event) where T : IPPIEvent {
			foreach(var handler in _handlers) {
				if(handler is IHandlePPIEvents<T>) {
					((IHandlePPIEvents<T>)handler).Handle(@event);
				}
			}
		}
	}
}