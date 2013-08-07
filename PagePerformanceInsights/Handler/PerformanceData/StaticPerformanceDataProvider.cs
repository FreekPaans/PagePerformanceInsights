using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData {
	class StaticPerformanceDataProvider  : IProvidePerformanceData{
		public DataTypes.PerformanceStatisticsForPageCollection GetStatisticsForAllPages(DateTime forDate) {
			var res = _data[forDate].GroupBy(v => v.Page).Select(pg => new PerformanceStatisticsForPage{
				Count = pg.Count(),
				Mean = (int)pg.Average(p => p.Duration),
				Sum = (int)(pg.Average(p => p.Duration)*pg.Count()),
				PageName = pg.Key,
				Median = GetMedian(pg)
			}).ToArray();
			return new PerformanceStatisticsForPageCollection(res,
				new PerformanceStatisticsForPage  {
					Count = res.Sum(r => r.Count),
					Mean = res.Sum(r => r.Mean * r.Count)/res.Sum(r => r.Count),
					Median = (int)res.Average(r => r.Median),
					PageName = "All Pages",
					Sum = res.Sum(r => r.Sum)
				}
			);
		}

		

		readonly static Lazy<DistributionRow[]> _data02 =new Lazy<DistributionRow[]>(() => LoadData(@"c:\tmp\filter\rapp_02sum"));
		readonly static Lazy<DistributionRow[]> _data26 =new Lazy<DistributionRow[]>(() => LoadData(@"c:\tmp\filter\rapp_26sum"));

		public class DistributionRow {
			public DateTime DateTime { get; set; }
			public int Duration { get; set; }
			public string Page { get; set; }
		}


		private int GetMedian(IEnumerable<DistributionRow> pg) {
			var sorted = pg.OrderBy(r => r.Duration).ToArray();
			if(sorted.Length==0) {
				return 0;
			}
			return sorted[sorted.Length/2].Duration;
		}


		public class DataWrapper {
			public DistributionRow[] this[DateTime dt] {
				get {
					if(dt.Day%2==0) {
						return _data02.Value;
					}
					return _data26.Value;
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
			}).Where(r => r!=null).OrderBy(r => r.Duration).ToArray();
		}


		public PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
			var data = _data[forDate];

			if(forPage!=null) {
				data = data.Where(d=>d.Page == forPage).ToArray();
			}

			if(data.Length == 0) {
				return PageDurationDistributionHistogram.Empty;
				//return new ResponseDistributionViewModel { Buckets  =new ResponseDistributionViewModel.Bucket[0],_90PctBucketIndex = 0,MedianBucketIndex = 0 };
			}

			var _99 = data[(int)(data.Length*0.99)].Duration;

			var bucketSize = (int)Math.Ceiling(_99/(double)BucketCount);

			var buckets = Enumerable.Range(0,BucketCount).Select(i => new PageDurationDistributionHistogram.Bucket { Count = 0,MinIncl = i*bucketSize,MaxExcl = (i+1)*bucketSize }).ToArray();
			Func<int,int> getBucketIndex = d => (int)(d/(double)bucketSize);

			var avg = (int)data.Average(d => d.Duration);

			foreach(var row in data) {
				var idx = getBucketIndex(row.Duration);
				if(idx>=buckets.Length) {
					continue;
				}
				buckets[idx].Count++;
			}

			var total = data.Count();
			var sum = 0;

			int? medianBucketIndex = null;
			int? _90pctBucketIndex=  null;
			int? meanBucketIndex = null;

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


		public PageStatisticsTrend GetTrend(DateTime forDate,string forPage) {
			var pageRows =  _data[forDate];
			
			if(forPage!=null) {
				pageRows = pageRows.Where(d => d.Page == forPage).ToArray();
			}

			var data = pageRows.GroupBy(d => SnapHour(d.DateTime)).Select(d => {

				var sorted = d.Select(r => r.Duration).OrderBy(r => r).ToArray();

				return new PageStatisticsTrend._TrendData {
					TimeStamp = d.Key,
					_90PCT = sorted[(int)(sorted.Length*0.9)],
					Median = sorted[(int)(sorted.Length*0.5)],
					Mean = (int)sorted.Average(),
					Count = sorted.Count()
				};
			}).ToDictionary(t => t.TimeStamp,t => t);

			var min  = data.Keys.First().Date;

			//var min = snap(DateContext.Now.AddDays(-1).AddHours(1));
			return new PageStatisticsTrend(Enumerable.Range(0,24).Select(i => min.AddHours(i)).Select(ts => data.ContainsKey(ts)?data[ts]:PageStatisticsTrend._TrendData.Empty(ts)).ToArray());
		}

		public PageStatisticsTrend GetTrend(DateTime forDate) {
			return GetTrend(forDate,null);
		}

		readonly static DateTime Ref = new DateTime(2012,1,1);

		static DateTime SnapHour(DateTime dt) {
			return Ref.AddHours((int)(dt-Ref).TotalHours);
		}
	}
}