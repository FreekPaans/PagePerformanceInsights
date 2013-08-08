using PagePerformanceInsights.Handler.PerformanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace PagePerformanceInsights.Modules {
	public class PopulateBufferFromFileHttpModule : IHttpModule{
		static PopulateBufferFromFileHttpModule() {
			new Thread(StartPopulating).Start();
		}

		private static void StartPopulating() {
			var data = StaticPerformanceDataProvider._data[DateTime.Now];
			
			var ct = 0;

			foreach(var rec in data) {
				CommBus.Buffer.EnqueueRequest(new CommBus.HttpRequestData {
					Duration = rec.Duration,
					Page = rec.Page,
					Timestamp = rec.DateTime
				});	

				if(ct++%1000==0) {
					Thread.Sleep(100);
				}
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