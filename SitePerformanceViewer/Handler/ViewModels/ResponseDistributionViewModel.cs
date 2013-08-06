﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitePerformanceViewer.Handler.ViewModels {
	public class ResponseDistributionViewModel {
		public class Bucket {
			public int MinIncl { get; set; }
			public int MaxExcl { get; set; }
			public int Count { get; set; }
			public int CumulativePercentage{get;set;}
		}

		public Bucket[] Buckets { get; set; }

		public int _90PctBucketIndex { get; set; }

		public int MedianBucketIndex { get; set; }
	}
}