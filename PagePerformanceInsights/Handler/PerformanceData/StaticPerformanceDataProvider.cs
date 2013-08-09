using PagePerformanceInsights.Handler.Algorithms.Median;
using PagePerformanceInsights.Handler.DataStructures;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData {
	class StaticPerformanceDataProvider  : IProvidePerformanceData{
		static ConcurrentDictionary<DateTime, PerformanceStatisticsForPageCollection>  _statisticsForAllPages = new ConcurrentDictionary<DateTime,PerformanceStatisticsForPageCollection>();


		readonly static ConcurrentDictionary<DateTime,int[]> _getStatisticsForAllPagesCache = new ConcurrentDictionary<DateTime,int[]>();
		readonly static ConcurrentDictionary<DateTime,Dictionary<string,int[]>> _perPageCache = new ConcurrentDictionary<DateTime,Dictionary<string,int[]>>();

		public DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			if(!_getStatisticsForAllPagesCache.ContainsKey(forDate)) {
				FillCache(forDate);
			}

			var allStats = _getStatisticsForAllPagesCache[forDate];

			return new PerformanceStatisticsForPageCollection(
				_perPageCache[forDate].Keys.Select(pp=>PerformanceStatisticsForPage.Calculate(_perPageCache[forDate][pp],pp)).ToArray(),
				PerformanceStatisticsForPage.Calculate(allStats,"All Pages")
			);

			
		}

		private void FillCache(DateTime forDate) {
			_getStatisticsForAllPagesCache[forDate] = _data[forDate].Select(f=>f.Duration).ToArray();

			_perPageCache[forDate] = new Dictionary<string,int[]>();
			foreach(var page in _data[forDate].GroupBy(v => v.Page)) {
				_perPageCache[forDate][page.Key] =page.Select(p=>p.Duration).ToArray();
			}
			
		}

		

		readonly static Lazy<DistributionRow[]> _data02 =new Lazy<DistributionRow[]>(() => LoadData(@"c:\tmp\filter\rapp_09sum"));
		readonly static Lazy<DistributionRow[]> _data26 =new Lazy<DistributionRow[]>(() => LoadData(@"c:\tmp\filter\rapp_26sum"));

		public class DistributionRow {
			public DateTime DateTime { get; set; }
			public int Duration { get; set; }
			public string Page { get; set; }
		}


		//private int GetMedian(IEnumerable<DistributionRow> pg) {
		//	var sorted = pg.OrderBy(r => r.Duration).ToArray();
		//	if(sorted.Length==0) {
		//		return 0;
		//	}
		//	return sorted[sorted.Length/2].Duration;
		//}


		public class DataWrapper {
			public DistributionRow[] this[DateTime dt] {
				get {
					//if(dt.Day%2==0) {
						return _data02.Value;
					//}
					//return _data26.Value;
				}
			}
		}

		public static DataWrapper _data = new DataWrapper();


		static DistributionRow[] LoadData(string filename) {
			return File.ReadAllLines(filename).Select(l => {
				var spl = l.Split(new[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
				try {
					var res = new DistributionRow {
						DateTime = DateTime.Parse(spl[0]).Add(TimeSpan.Parse(spl[1])),
						Duration = int.Parse(spl[2]),
						Page = spl[3]
					};
					return res;
				}
				catch {
					return null;
				}
			}).Where(r => r!=null && r.DateTime.TimeOfDay<TimeSpan.Parse("15:00")).ToArray();
		}


		ConcurrentDictionary<DateTime,int[]> CachedAllPageDuration =new ConcurrentDictionary<DateTime,int[]>();
		ConcurrentDictionary<DateTime,Dictionary<string,int[]>> CachedPageDuration =new ConcurrentDictionary<DateTime,Dictionary<string,int[]>>();



		public PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			//var data = _data[forDate];


			//if(forPage!=null) {
			//	data = data.Where(d=>d.Page == forPage).ToArray();
			//}


			int[] dataDuration;// = data.Select(d=>d.Duration).ToArray();

			if(forPage==null) {
				if(!CachedAllPageDuration.ContainsKey(forDate)) {
					CachedAllPageDuration[forDate] = _data[forDate].Select(f=>f.Duration).ToArray();
				}
				dataDuration = CachedAllPageDuration[forDate];
			}
			else {
				if(!CachedPageDuration.ContainsKey(forDate)) {
					CachedPageDuration[forDate] = new Dictionary<string,int[]>();
				}
				if(!CachedPageDuration[forDate].ContainsKey(forPage)) {
					CachedPageDuration[forDate][forPage] = _data[forDate].Where(d=>d.Page == forPage).Select(d=>d.Duration).ToArray();
				}
				dataDuration = CachedPageDuration[forDate][forPage];
			}



			if(dataDuration.Length == 0) {
				return PageDurationDistributionHistogram.Empty;
				//return new ResponseDistributionViewModel { Buckets  =new ResponseDistributionViewModel.Bucket[0],_90PctBucketIndex = 0,MedianBucketIndex = 0 };
			}


			var _99 = new QuickSelect().Select(dataDuration,0.99);

			//var _99 = data[(int)(data.Length*0.99)].Duration;

			var bucketSize = (int)Math.Ceiling(_99/(double)BucketCount);

			var buckets = Enumerable.Range(0,BucketCount).Select(i => new PageDurationDistributionHistogram.Bucket { Count = 0,MinIncl = i*bucketSize,MaxExcl = (i+1)*bucketSize }).ToArray();

			Func<int,int> getBucketIndex = d => (int)(d/(double)bucketSize);

			foreach(var row in dataDuration) {
				var idx = getBucketIndex(row);
				if(idx>=buckets.Length) {
					continue;
				}
				buckets[idx].Count++;
			}

			var total = dataDuration.Length;
			var sum = 0;

			int? medianBucketIndex = null;
			int? _90pctBucketIndex=  null;
			int? meanBucketIndex = null;

			var avg = (int)dataDuration.Average();
			var ct = 0;
			foreach(var bucket in buckets) {
				sum+=bucket.Count;
				bucket.CumulativePercentage = (int)(100.0*sum/(double)total);
				if(medianBucketIndex==null &&  bucket.CumulativePercentage>=50) {
					medianBucketIndex = ct;
				}

				if(_90pctBucketIndex == null && bucket.CumulativePercentage>=90) {
					_90pctBucketIndex = ct;
				}

				if(meanBucketIndex==null && bucket.MinIncl>=avg) {
					meanBucketIndex = ct;
				}

				ct++;
			}

			return new PageDurationDistributionHistogram(buckets,meanBucketIndex??0,medianBucketIndex??0,_90pctBucketIndex??0);
		}

		const int BucketCount = 100;

		public PageDurationDistributionHistogram GetAllPagesDistribution(DateTime forDate) {
			return GetPageDistribution(forDate,null);
		}


		public PageStatisticsTrend GetHourlyTrend(DateTime forDate,string forPage) {
			var pageRows =  _data[forDate];
			
			if(forPage!=null) {
				pageRows = pageRows.Where(d => d.Page == forPage).ToArray();
			}

			var data = pageRows.GroupBy(d => SnapHour(d.DateTime)).Select(d => {

				var duration = d.Select(r=>r.Duration).ToArray();
				//var sorted = d.Select(r => r.Duration).OrderBy(r => r).ToArray();

				return new PageStatisticsTrend._TrendData {
					TimeStamp = d.Key,
					_90PCT = new QuickSelect().Select(duration, 0.9),
					Median = new QuickSelect().Select(duration,0.5),
					Mean = (int)duration.Average(),
					Count = duration.Length
				};
			}).ToDictionary(t => t.TimeStamp,t => t);

			var min  = data.Keys.First().Date;

			//var min = snap(DateContext.Now.AddDays(-1).AddHours(1));
			return new PageStatisticsTrend(Enumerable.Range(0,24).Select(i => min.AddHours(i)).Select(ts => data.ContainsKey(ts)?data[ts]:PageStatisticsTrend._TrendData.Empty(ts)).ToArray());
		}

		public PageStatisticsTrend GetHourlyTrend(DateTime forDate) {
			return GetHourlyTrend(forDate,null);
		}

		readonly static DateTime Ref = new DateTime(2012,1,1);

		static DateTime SnapHour(DateTime dt) {
			return Ref.AddHours((int)(dt-Ref).TotalHours);
		}
	}
}