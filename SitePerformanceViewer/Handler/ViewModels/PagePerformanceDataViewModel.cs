using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitePerformanceViewer.Handler.ViewModels {
	public class PagePerformanceDataViewModel {
		public class PagePerformanceDataRow {
			public string PageName { get; set; }
			public int Count { get; set; }
			public int Median { get; set; }
			public int Mean { get; set; }

			public int Sum { get; set; }
		}
		

		public ICollection<PagePerformanceDataRow> Pages{get;set;}
		public PagePerformanceDataRow AllPages {get;set;}

	}
}