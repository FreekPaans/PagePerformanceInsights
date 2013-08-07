using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData.DataTypes {
	public class PageStatisticsTrend {
		readonly ICollection<_TrendData> _trendData;

		public ICollection<_TrendData> TrendData {
			get { return _trendData; }
		} 

		public class _TrendData {
			public DateTime TimeStamp { get; set; }
			public int _90PCT { get; set; }
			public int Median { get; set; }
			public int Count { get; set; }
			public int Mean { get; set; }

			public static _TrendData Empty(DateTime ts) {
				return new _TrendData { TimeStamp = ts };
			}
		}

		public PageStatisticsTrend (ICollection<_TrendData> trendData) {
			_trendData = trendData;
		}
	}
}