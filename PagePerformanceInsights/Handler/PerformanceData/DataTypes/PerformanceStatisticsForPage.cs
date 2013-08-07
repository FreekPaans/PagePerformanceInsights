using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData.DataTypes {
	public class PerformanceStatisticsForPage {
		public string PageName { get; set; }
		public int Count { get; set; }
		public int Median { get; set; }
		public int Mean { get; set; }
		public int Sum { get; set; }

		public static PerformanceStatisticsForPage Calculate(int[] distribution,string pageName) {
			
			var res=  new PerformanceStatisticsForPage {
					Count = distribution.Length,
					Mean = (int)distribution.Average(),
					Median = distribution[distribution.Length/2],
					PageName = pageName,
					
				};

			res.Sum = res.Count * res.Mean;

			return res;
		}
	}
}