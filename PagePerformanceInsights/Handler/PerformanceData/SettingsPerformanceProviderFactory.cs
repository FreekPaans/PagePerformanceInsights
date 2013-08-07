using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData {
	class SettingsPerformanceProviderFactory {
		public static IProvidePerformanceData GetDataProvider() {
			return new StaticPerformanceDataProvider();
		}
	}
}