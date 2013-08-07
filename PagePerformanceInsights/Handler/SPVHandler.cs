using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PagePerformanceInsights.Handler.Helpers;
using PagePerformanceInsights.Handler.ViewModels;
using PagePerformanceInsights.Helpers;
using System.Collections.Specialized;
using PagePerformanceInsights.Handler.Views;

namespace PagePerformanceInsights {
	public class SPVHandler : IHttpHandler{
		public bool IsReusable {
			get { return false; }
		}

		public void ProcessRequest(HttpContext context) {
			if(!string.IsNullOrWhiteSpace(context.Request["resource"])) {
				Resource(context, context.Request["resource"]);
				return;
			}

			if(!string.IsNullOrWhiteSpace(context.Request["data"])) {
				Data(context, context.Request["data"]);
				return;
			}

			var activeDate = DateContext.Now.Date;
			if(!string.IsNullOrEmpty(context.Request["date"])) {
				activeDate = DateTime.Parse(context.Request["date"]);
			}

			context.Response.Write(new Home { ActiveDate = activeDate }.TransformText());
		}

		private void Data(HttpContext context, string data) {
			var elements = data.Substring(1) .Split(new [] { "?" }, StringSplitOptions.RemoveEmptyEntries);

			string qs = "?";

			if(elements.Length>1) {
				qs = elements[1];
			}
			var parsedQueryString = HttpUtility.ParseQueryString(qs);
			

			switch(elements[0]) {
				case "pages":
					context.Response.Write(new PagesTable { PagePerformanceData = GetPerformanceData(parsedQueryString) }.TransformText());
					break;
				case "distribution":
					context.Response.Write(new ResponseDistribution { Buckets = GetResponseDistribution(parsedQueryString) }.TransformText());
					break;
				case "trend":
					context.Response.Write(new PerformanceTrends { Trend = GetTrends(parsedQueryString) }.TransformText());
					break;
				default:
					throw new HttpException(404,"not found");		
			}

			
		}

		readonly static DateTime Ref = new DateTime(2012,1,1);

		private TrendViewModel GetTrends(NameValueCollection queryString) {
			Func<DateTime,DateTime> snap = d=>Ref.AddHours((int)(d-Ref).TotalHours);

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]).Date;
			}

			var pageRows = _data[dt];


			if(queryString["page"]!=null) {
				pageRows= pageRows.Where(d => d.Page == queryString["page"]).ToArray();
			}


			var data = pageRows.GroupBy(d=>snap(d.DateTime)).Select(d=>{ 
			
				var sorted = d.Select(r=>r.Duration).OrderBy(r=>r).ToArray();

				return new TrendViewModel.TrendData {
					TimeStamp = d.Key,
					_90PCT = sorted[(int)(sorted.Length*0.9)],
					Median = sorted[(int)(sorted.Length*0.5)],
					Mean = (int)sorted.Average(),
					Count = sorted.Count()
				};
			}).ToDictionary(t=>t.TimeStamp, t=>t);

			var min  = data.Keys.OrderBy(k=>k).FirstOrDefault();

			//var min = snap(DateContext.Now.AddDays(-1).AddHours(1));
			return new TrendViewModel { Partitioned = Enumerable.Range(0,24).Select(i=>min.AddHours(i)).Select(ts=>data.ContainsKey(ts)?data[ts]:TrendViewModel.TrendData.Empty(ts)).ToArray() };
		}



		class DistributionRow {
			public DateTime DateTime{get;set;}
			public int Duration{get;set;}
			public string Page{get;set;}
		}

		

		const int BucketCount = 100;

		readonly static Lazy<DistributionRow[]> _data02 =new Lazy<DistributionRow[]>(()=>LoadData(@"c:\tmp\filter\rapp_02sum"));
		readonly static Lazy<DistributionRow[]> _data26 =new Lazy<DistributionRow[]>(() => LoadData(@"c:\tmp\filter\rapp_26sum"));

		class DataWrapper {
			public DistributionRow[] this[DateTime dt]  {
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
				var spl = l.Split(new []  { ' '} ,StringSplitOptions.RemoveEmptyEntries);
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

		private ResponseDistributionViewModel GetResponseDistribution(NameValueCollection queryString) {
			//return new ResponseDistributionViewModel { Buckets = new ResponseDistributionViewModel.Bucket[0]};

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]).Date;
			}

			var data = _data[dt];

			if(queryString["page"]!=null) {
				data = data.Where(d=>d.Page == queryString["page"]).ToArray();
			}

			

			if(data.Length == 0) {
				return new ResponseDistributionViewModel { Buckets  =new ResponseDistributionViewModel.Bucket[0], _90PctBucketIndex = 0, MedianBucketIndex = 0};
			}

			var _99 = data[(int)(data.Length*0.99)].Duration;

			var bucketSize = (int)Math.Ceiling(_99/(double)BucketCount);

			var buckets = Enumerable.Range(0,BucketCount).Select(i => new ResponseDistributionViewModel.Bucket { Count = 0,MinIncl = i*bucketSize,MaxExcl = (i+1)*bucketSize }).ToArray();
			Func<int,int> getBucketIndex = d=>(int)(d/(double)bucketSize);

			var avg = (int)data.Average(d=>d.Duration);
			
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

			return new ResponseDistributionViewModel { Buckets = buckets, MedianBucketIndex = medianBucketIndex??0, _90PctBucketIndex = _90pctBucketIndex??0, MeanBucketIndex = meanBucketIndex??0 };
			//var sorted = data.ToArray();
		}

		//readonly static string TempData = File.ReadAllText(@"c:\tmp\filter\page_data");

		private Handler.ViewModels.PagePerformanceDataViewModel GetPerformanceData(NameValueCollection queryString) {

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]);
			}

			var res = _data[dt].GroupBy(v=>v.Page).Select(pg=>new PagePerformanceDataViewModel.PagePerformanceDataRow {
				Count = pg.Count(),
				Mean = (int)pg.Average(p=>p.Duration),
				Sum = (int)(pg.Average(p => p.Duration)*pg.Count()),
				PageName = pg.Key,
				Median = GetMedian(pg)
			}).ToArray();
			return new PagePerformanceDataViewModel { Pages  = res, AllPages = new PagePerformanceDataViewModel.PagePerformanceDataRow {	
				Count = res.Sum(r=>r.Count),
				Mean = res.Sum(r=>r.Mean * r.Count)/res.Sum(r=>r.Count),
				Median = (int)res.Average(r=>r.Median),
				PageName = "All Pages",
				Sum = res.Sum(r=>r.Sum)
			}
			};
		}

		private int GetMedian(IEnumerable<DistributionRow> pg) {
			var sorted = pg.OrderBy(r=>r.Duration).ToArray();
			if(sorted.Length==0) {
				return 0;
			}
			return sorted[sorted.Length/2].Duration;
		}

		readonly static Assembly _selfAssembly = Assembly.GetAssembly(typeof(SPVHandler));

		private void Resource(HttpContext context, string resourceValue) {
			
			
			using(var resourceStream = _selfAssembly.GetManifestResourceStream(GetResourceName(resourceValue))) {
				if(resourceStream==null) {
					throw new HttpException(404,"Resource not found");
				}

				if(IsCss(resourceValue)) {
					CssResource(context,resourceValue,resourceStream);
					return;
				}

				OutputStream(context,resourceValue,resourceStream);
				//context.Response.Headers["Content-Type"] = mime;

			}

			//var names = _assembly.GetManifestResourceNames();

			return;
		}

		private void CssResource(HttpContext context, string resourceValue, System.IO.Stream cssStream) {
			var str = new StreamReader(cssStream).ReadToEnd().Replace("{baseDir}", string.Format("{0}?resource=", HandlerHelpers.HandlerPath));
			using(var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(str))) {
				//outputStream.Position = 0;
				OutputStream(context,resourceValue,outputStream);
			}
		}

		private void OutputStream(HttpContext context,string resourceValue,System.IO.Stream resource) {
			

			var mime = GetMimeType(resourceValue);

			resource.CopyTo(context.Response.OutputStream);

			context.Response.AddHeader("Content-Type",mime);
		}

	
		private bool IsCss(string resourceValue) {
			return resourceValue.EndsWith(".css");
		}

		readonly static Dictionary<string,string> ExtensionToMimeMap = new Dictionary<string,string> { {"js", "application/x-javascript" }, {"css", "text/css"}};

		private string GetMimeType(string resourceValue) {
			var ext = System.IO.Path.GetExtension(resourceValue);
			switch(ext.Substring(1)) {
				case "js":
					return "application/x-javascript";
				case "css":
					return "text/css";
				case "png":
					return "image/png";
				case "gif":
					return "image/gif";
			}
			return "application/octet-stream";
		}

		private string GetResourceName(string resourceValue) {
			return "PagePerformanceInsights.Handler.Assets"+resourceValue.Replace('/', '.');
		}
	}
}
