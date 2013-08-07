using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitePerformanceViewer.Handler.ViewModels {
	public class TrendViewModel {
		public class TrendData {	
			public DateTime TimeStamp{get;set;}
			public int _90PCT {get;set;}
			public int Median{get;set;}
			public int Count{get;set;}
			public int Mean{get;set;}

			public static TrendData Empty(DateTime ts) {
				return new TrendData { TimeStamp = ts};
			}
		}

		public TrendData[] Partitioned { get; set; }
	}
}