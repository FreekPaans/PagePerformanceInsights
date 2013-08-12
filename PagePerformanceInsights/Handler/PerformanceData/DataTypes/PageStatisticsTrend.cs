using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagePerformanceInsights.Handler.Algorithms.Median;

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

			public static _TrendData FromDistribution(DateTime timestamp, ICollection<int> list) {
				var arr = list.ToArray();
				return new _TrendData{
					_90PCT = new QuickSelect().Select(arr,0.9),
					Count = arr.Length,
					Mean = (int)arr.Average(),
					Median = new QuickSelect().Select(arr,0.5),
					TimeStamp = timestamp
				};
			}
		}

		public PageStatisticsTrend (ICollection<_TrendData> trendData) {
			_trendData = trendData;
		}

		public static PageStatisticsTrend Empty {
			get {
				return new PageStatisticsTrend(new _TrendData[0]);
			}
		}

		public static PageStatisticsTrend HourlyFromHourDataDictionary(DateTime date, Dictionary<int,List<int>> input) {
			return new  PageStatisticsTrend(Enumerable.Range(0,24).Select(i=>
				input.ContainsKey(i)?
					_TrendData.FromDistribution(date.AddHours(i), (ICollection<int>)input[i]):
					_TrendData.Empty(date)
				).ToArray());
		}
	}
}