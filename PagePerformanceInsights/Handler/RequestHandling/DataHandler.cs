using PagePerformanceInsights.Handler.ViewModels;
using PagePerformanceInsights.Handler.Views;
using PagePerformanceInsights.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace PagePerformanceInsights.Handler.RequestHandling {
	class DataHandler : IHandleRoutes{
		readonly string _localPath;
		public DataHandler(string localPath) {
			_localPath = localPath;
		}

		public void Run(System.Web.HttpContext context) {
			var internalUrl = InternalUrl.Parse(_localPath);

			switch(internalUrl.LocalPath) {
				case "pages":
					context.Response.Write(new PagesTable { PagePerformanceData = GetPerformanceData(internalUrl.QueryString) }.TransformText());
					break;
				case "distribution":
					context.Response.Write(new ResponseDistribution { Buckets = GetResponseDistribution(internalUrl.QueryString) }.TransformText());
					break;
				case "trend":
					context.Response.Write(new PerformanceTrends { Trend = GetTrends(internalUrl.QueryString) }.TransformText());
					break;
				default:
					throw new HttpException(404,"not found");		
			}

		}

		private Handler.ViewModels.PagePerformanceDataViewModel GetPerformanceData(NameValueCollection queryString) {

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]);
			}

			var res = _data[dt].GroupBy(v => v.Page).Select(pg => new PagePerformanceDataViewModel.PagePerformanceDataRow {
				Count = pg.Count(),
				Mean = (int)pg.Average(p => p.Duration),
				Sum = (int)(pg.Average(p => p.Duration)*pg.Count()),
				PageName = pg.Key,
				Median = GetMedian(pg)
			}).ToArray();
			return new PagePerformanceDataViewModel {
				Pages  = res, AllPages = new PagePerformanceDataViewModel.PagePerformanceDataRow {
					Count = res.Sum(r => r.Count),
					Mean = res.Sum(r => r.Mean * r.Count)/res.Sum(r => r.Count),
					Median = (int)res.Average(r => r.Median),
					PageName = "All Pages",
					Sum = res.Sum(r => r.Sum)
				}
			};
		}

		readonly static DateTime Ref = new DateTime(2012,1,1);

		private TrendViewModel GetTrends(NameValueCollection queryString) {
			Func<DateTime,DateTime> snap = d => Ref.AddHours((int)(d-Ref).TotalHours);

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]).Date;
			}

			var pageRows = _data[dt];


			if(queryString["page"]!=null) {
				pageRows= pageRows.Where(d => d.Page == queryString["page"]).ToArray();
			}


			var data = pageRows.GroupBy(d => snap(d.DateTime)).Select(d => {

				var sorted = d.Select(r => r.Duration).OrderBy(r => r).ToArray();

				return new TrendViewModel.TrendData {
					TimeStamp = d.Key,
					_90PCT = sorted[(int)(sorted.Length*0.9)],
					Median = sorted[(int)(sorted.Length*0.5)],
					Mean = (int)sorted.Average(),
					Count = sorted.Count()
				};
			}).ToDictionary(t => t.TimeStamp,t => t);

			var min  = data.Keys.First().Date;

			//var min = snap(DateContext.Now.AddDays(-1).AddHours(1));
			return new TrendViewModel { Partitioned = Enumerable.Range(0,24).Select(i => min.AddHours(i)).Select(ts => data.ContainsKey(ts)?data[ts]:TrendViewModel.TrendData.Empty(ts)).ToArray() };
		}

		private ResponseDistributionViewModel GetResponseDistribution(NameValueCollection queryString) {
			//return new ResponseDistributionViewModel { Buckets = new ResponseDistributionViewModel.Bucket[0]};

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]).Date;
			}

			var data = _data[dt];

			if(queryString["page"]!=null) {
				data = data.Where(d => d.Page == queryString["page"]).ToArray();
			}



			if(data.Length == 0) {
				return new ResponseDistributionViewModel { Buckets  =new ResponseDistributionViewModel.Bucket[0],_90PctBucketIndex = 0,MedianBucketIndex = 0 };
			}

			var _99 = data[(int)(data.Length*0.99)].Duration;

			var bucketSize = (int)Math.Ceiling(_99/(double)BucketCount);

			var buckets = Enumerable.Range(0,BucketCount).Select(i => new ResponseDistributionViewModel.Bucket { Count = 0,MinIncl = i*bucketSize,MaxExcl = (i+1)*bucketSize }).ToArray();
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

			return new ResponseDistributionViewModel { Buckets = buckets,MedianBucketIndex = medianBucketIndex??0,_90PctBucketIndex = _90pctBucketIndex??0,MeanBucketIndex = meanBucketIndex??0 };
			//var sorted = data.ToArray();
		}

		const int BucketCount = 100;

		readonly static Lazy<DistributionRow[]> _data02 =new Lazy<DistributionRow[]>(() => LoadData(@"c:\tmp\filter\rapp_02sum"));
		readonly static Lazy<DistributionRow[]> _data26 =new Lazy<DistributionRow[]>(() => LoadData(@"c:\tmp\filter\rapp_26sum"));

		class DistributionRow {
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


		class DataWrapper {
			public DistributionRow[] this[DateTime dt] {
				get {
					if(dt.Day%2==0) {
						return _data02.Value;
					}
					return _data26.Value;
				}
			}
		}

		static DataWrapper _data = new DataWrapper();


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

	}
}
