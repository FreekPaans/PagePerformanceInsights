using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData.DataTypes {
	public class PageDurationDistributionHistogram {
		readonly Bucket[] _buckets;

		public Bucket[] Buckets {
			get { return _buckets; }
		}

		readonly int _preCalculatedMean;

		public int MeanBucketIndex {
			get { return _preCalculatedMean; }
		}

		readonly int _preCalculatedMedian;

		public int MedianBucketIndex {
			get { return _preCalculatedMedian; }
		}

		readonly int _preCalculate90Percentile;

		public int _90PercentileBucketIndex {
			get { return _preCalculate90Percentile; }
		} 


		public class Bucket {
			public int MinIncl { get; set; }
			public int MaxExcl { get; set; }
			public int Count { get; set; }
			public int CumulativePercentage { get; set; }
		}

		public PageDurationDistributionHistogram(Bucket[] buckets, int preCalculatedMeanIndex, int preCalculatedMedianIndex, int preCalculated90PercentileIndex) {
			_buckets = buckets;
			_preCalculatedMean = preCalculatedMeanIndex;
			_preCalculatedMedian = preCalculatedMedianIndex;
			_preCalculate90Percentile = preCalculated90PercentileIndex;
		}

		//public Bucket[] Buckets { get; set; }
		//public int _90PctBucketIndex { get; set; }
		//public int MedianBucketIndex { get; set; }
		//public int MeanBucketIndex { get; set; }

		static PageDurationDistributionHistogram _empty =new PageDurationDistributionHistogram(new Bucket[0],0,0,0);

		public static PageDurationDistributionHistogram Empty {
			get {
				return _empty;
			}
		}
	}
}