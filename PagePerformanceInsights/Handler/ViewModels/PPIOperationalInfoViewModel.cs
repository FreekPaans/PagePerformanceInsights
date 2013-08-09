using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.ViewModels {
	public class PPIOperationalInfoViewModel {
		public long PrivateBytesMB { get; set; }
		public int  QueueSize { get; set; }
		public double AnalyzedRequestsPerSecond { get; set; }

		public double BufferFlushFrequency { get; set; }
	}
}