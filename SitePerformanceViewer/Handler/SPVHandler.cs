using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SitePerformanceViewer.Handler.Helpers;
using SitePerformanceViewer.Handler.ViewModels;
using SitePerformanceViewer.Handler.Views;

namespace SitePerformanceViewer {
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

			context.Response.Write(new Home { }.TransformText());
		}

		private void Data(HttpContext context, string data) {
			var elements = data.Split(new [] { "/" }, StringSplitOptions.RemoveEmptyEntries);

			switch(elements[0]) {
				case "pages":
					context.Response.Write(new PagesTable { PagePerformanceData = GetPerformanceData() }.TransformText());
					break;
				case "distribution":
					context.Response.Write(new ResponseDistribution { Buckets = GetResponseDistribution() }.TransformText() );
					break;
				case "trend":
					context.Response.Write(new PerformanceTrends { Trend = GetTrends() }.TransformText());
					break;
				default:
					throw new HttpException(404,"not found");		
			}

			
		}

		readonly static DateTime Ref = new DateTime(2012,1,1);

		private TrendViewModel GetTrends() {
			Func<DateTime,DateTime> snap = d=>Ref.AddHours((int)(d-Ref).TotalHours);

			var data = _data.Value.GroupBy(d=>snap(d.DateTime)).Select(d=>{ 
			
				var sorted = d.Select(r=>r.Duration).OrderBy(r=>r).ToArray();

				return new TrendViewModel.TrendData {
					TimeStamp = d.Key,
					_90PCT = sorted[(int)(sorted.Length*0.9)],
					Median = sorted[(int)(sorted.Length*0.5)],
					Mean = (int)sorted.Average(),
					Count = sorted.Count()
				};
			}).OrderBy(td=>td.TimeStamp).ToArray();

			return new TrendViewModel { Partitioned = data };
		}



		class DistributionRow {
			public DateTime DateTime{get;set;}
			public int Duration{get;set;}
			public string Page{get;set;}
		}

		

		const int BucketCount = 100;

		readonly static Lazy<DistributionRow[]> _data =new Lazy<DistributionRow[]>(()=> {
			return  File.ReadAllLines(@"c:\tmp\filter\rapp_26sum").Select(l => {
				var spl = l.Split(' ');
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
		});

		private ResponseDistributionViewModel GetResponseDistribution() {
			//return new ResponseDistributionViewModel { Buckets = new ResponseDistributionViewModel.Bucket[0]};
			
			var data = _data.Value;

			var _99 = data[(int)(data.Length*0.99)].Duration;

			var bucketSize = (int)Math.Ceiling(_99/(double)BucketCount);

			var buckets = Enumerable.Range(0,BucketCount).Select(i => new ResponseDistributionViewModel.Bucket { Count = 0,MinIncl = i*bucketSize,MaxExcl = (i+1)*bucketSize }).ToArray();
			Func<int,int> getBucketIndex = d=>(int)(d/(double)bucketSize);

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

				ct++;
			}

			return new ResponseDistributionViewModel { Buckets = buckets, MedianBucketIndex = medianBucketIndex??0, _90PctBucketIndex = _90pctBucketIndex??0 };
			//var sorted = data.ToArray();
		}

		readonly static string TempData = File.ReadAllText(@"c:\tmp\filter\page_data");

		private ICollection<Handler.ViewModels.PagePerformanceDataViewModel> GetPerformanceData() {
			var res = new List<PagePerformanceDataViewModel>();
			using(var str = new StringReader(TempData)) {
				while(true) {
					var line = str.ReadLine();

					if(line==null) {
						break;
					}

					var spl = line.Split('\t');
					res.Add(new PagePerformanceDataViewModel { PageName = spl[0], Count = int.Parse(spl[1]), Mean = int.Parse(spl[2]), Median = int.Parse(spl[2]), Sum = int.Parse(spl[3]) });
				}
			}
			return res;
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
			}
			return "application/octet-stream";
		}

		private string GetResourceName(string resourceValue) {
			return "SitePerformanceViewer.Handler.Assets"+resourceValue.Replace('/', '.');
		}
	}
}
