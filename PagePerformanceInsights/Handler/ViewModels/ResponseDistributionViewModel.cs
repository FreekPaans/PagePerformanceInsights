using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.ViewModels {
	public class ResponseDistributionViewModel {

		public PerformanceData.DataTypes.PageDurationDistributionHistogram Data { get; set; }
	}
}