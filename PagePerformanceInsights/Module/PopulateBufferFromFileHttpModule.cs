using PagePerformanceInsights.Handler.PerformanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace PagePerformanceInsights.Module {
	public class PopulateBufferFromFileHttpModule : IHttpModule{
		static PopulateBufferFromFileHttpModule() {
			new Thread(StartPopulating).Start();
		}

		private static void StartPopulating() {
			var data = StaticPerformanceDataProvider._data[DateTime.Now];
			
			var ct = 0;
			var dateCount = 0;

			var rnd = new Random();

			while(data.Any()) {
				var date = DateTime.Today.AddDays(-dateCount);
				var total= data.Count();

				foreach(var rec in data) {
					CommBus.Buffer.EnqueueRequest(new CommBus.HttpRequestData {
						Duration = (int)(rec.Duration),
						Page = rec.Page,
						Timestamp = date.Add(rec.DateTime.TimeOfDay)
					});	

					if(ct++%1000==0) {
						Thread.Sleep(20);
					}
				}

				data = data.Take(total-dateCount*200000).ToArray();

				dateCount++;
			}
		}

		public void Dispose() {
			//throw new NotImplementedException();
		}

		public void Init(HttpApplication context) {
			//throw new NotImplementedException();
		}
	}
}