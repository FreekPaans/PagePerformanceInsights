using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Helpers {
	static class TimeSkewHelper {
		public readonly static TimeSpan MaxTimeSkewWindow = TimeSpan.FromMinutes(5).Negate();
	}
}
