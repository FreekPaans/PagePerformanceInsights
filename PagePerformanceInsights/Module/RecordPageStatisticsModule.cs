using PagePerformanceInsights.CommBus;
using PagePerformanceInsights.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using NLog;

namespace PagePerformanceInsights.Module {
	public class RecordPageStatisticsModule : IHttpModule{
		public void Dispose() {
			//throw new NotImplementedException();
		}

		const string PPI_Guid = "1DC6F835-7688-4372-9156-373CFD7BCA67";
		readonly static string PPI_Stopwatch_Key = PPI_Guid+"-SW";
		readonly static string PPI_StartDateTime_Key = PPI_Guid+"-TS";

		//TODO: eat exceptions
		public void Init(HttpApplication context) {
			context.BeginRequest+=(s,e) => LogExceptions("BeginRequest", ()=>{
				var sw=  new Stopwatch();
				HttpContext.Current.Items[PPI_Stopwatch_Key] = sw;
				HttpContext.Current.Items[PPI_StartDateTime_Key] = DateContext.Now;
				sw.Start();
			});
			context.EndRequest+=(s,e) =>LogExceptions("EndRequest", ()=>{
				if(HttpContext.Current.Items[PPI_Stopwatch_Key]==null) {
					//this happens when we have a rewritten url
					return;
				}
				
				var sw = (Stopwatch)HttpContext.Current.Items[PPI_Stopwatch_Key];
				
				sw.Stop();
				CommBus.Buffer.EnqueueRequest(new HttpRequestData {
					Duration = (int)sw.ElapsedMilliseconds,
					Timestamp = (DateTime)HttpContext.Current.Items[PPI_StartDateTime_Key],
					Page = HttpContext.Current.Request.Url.LocalPath
				});
			});


			//throw new NotImplementedException();
		}

		readonly static Logger _logger = LogManager.GetCurrentClassLogger();

		static void LogExceptions(string @event, Action code) {
			try {
				code();
			}
			catch(Exception e) {
				_logger.LogException(LogLevel.Error, string.Format("Error raising {0}", @event), e);
			}
		}
	}
}