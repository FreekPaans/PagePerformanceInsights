using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.Handler.PerformanceData {
	public interface IStorePerformanceData {
		void Store(CommBus.HttpRequestData[] res);
	}
}
