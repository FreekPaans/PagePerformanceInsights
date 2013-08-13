using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.Events {
	public interface IHandlePPIEvents<T> : IHandlePPIEvents where T : IPPIEvent {
		void Handle(T @event);
	}

	public interface IHandlePPIEvents {}
}
