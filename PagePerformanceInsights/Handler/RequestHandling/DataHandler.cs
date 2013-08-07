using PagePerformanceInsights.Handler.PerformanceData;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
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
		readonly IProvidePerformanceData _performanceDataProvider; 

		public DataHandler(string localPath) {
			_localPath = localPath;
			_performanceDataProvider  = SettingsPerformanceProviderFactory.GetDataProvider();
		}

		public void Run(System.Web.HttpContext context) {
			var internalUrl = InternalUrl.Parse(_localPath);

			switch(internalUrl.LocalPath) {
				case "pages":
					context.Response.Write(new PagesTable { ViewModel = GetPerformanceData(internalUrl.QueryString) }.TransformText());
					break;
				case "distribution":
					context.Response.Write(new ResponseDistribution { ViewModel = GetResponseDistribution(internalUrl.QueryString) }.TransformText());
					break;
				case "trend":
					context.Response.Write(new PerformanceTrends { ViewModel = GetTrends(internalUrl.QueryString) }.TransformText());
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

			return new PagePerformanceDataViewModel { Data = _performanceDataProvider.GetStatisticsForAllPages(dt) };

			
		}

		PagePerformanceInsights.Handler.PerformanceData.StaticPerformanceDataProvider.DataWrapper _data = StaticPerformanceDataProvider._data;


		

		private TrendViewModel GetTrends(NameValueCollection queryString) {
			

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]).Date;
			}

			PageStatisticsTrend trend;

			if(queryString["page"]!=null) {
				trend = _performanceDataProvider.GetTrend(dt,queryString["page"]);
				
			}
			else {
				trend = _performanceDataProvider.GetTrend(dt);
			}

			return new TrendViewModel { Data = trend };
		}

		

		private ResponseDistributionViewModel GetResponseDistribution(NameValueCollection queryString) {
			//return new ResponseDistributionViewModel { Buckets = new ResponseDistributionViewModel.Bucket[0]};

			var dt = DateContext.Now.Date;

			if(queryString["date"]!=null) {
				dt = DateTime.Parse(queryString["date"]).Date;
			}

			
			PageDurationDistributionHistogram distribution;
			
			//var data = _data[dt];

			if(queryString["page"]!=null) {
				distribution = _performanceDataProvider.GetPageDistribution(dt,queryString["page"]);
				
			}
			else {
				distribution = _performanceDataProvider.GetAllPagesDistribution(dt);
			}



			

			return new ResponseDistributionViewModel { Data = distribution };
			//var sorted = data.ToArray();
		}

		

	}
}
