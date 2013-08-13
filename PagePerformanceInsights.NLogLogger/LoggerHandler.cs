using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using PagePerformanceInsights.Events;
using PagePerformanceInsights.Events.LogEvents;

namespace PagePerformanceInsights.NLogLogger {
	class LoggerHandler : IHandlePPIEvents<InfoEvent>, IHandlePPIEvents<WarnEvent>, IHandlePPIEvents<ExceptionEvent> {
		public void Handle(InfoEvent @event) {
			GetLogger(@event.Source).Info(()=>@event.Message);
		}

		public void Handle(ExceptionEvent @event) {
			GetLogger(@event.Source).LogException(LogLevel.Error,@event.Message,@event.Exception);
		}

		public void Handle(WarnEvent @event) {
			GetLogger(@event.Source).Warn(()=>@event.Message);
		}

		private static Logger GetLogger(Type source) {
			return LogManager.GetLogger(source.FullName);
		}

	}
}
