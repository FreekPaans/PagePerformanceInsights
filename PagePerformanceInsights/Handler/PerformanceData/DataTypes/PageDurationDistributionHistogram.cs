using PagePerformanceInsights.Handler.Algorithms.Median;
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

		//TODO: make configable
		const int BucketCount = 100;

		public static PageDurationDistributionHistogram FromDistribution(int[] distribution) {
			if(distribution.Length==0) {
				return PageDurationDistributionHistogram.Empty;
			}
			var _99 = new QuickSelect().Select(distribution,0.99);

			//var _99 = data[(int)(data.Length*0.99)].Duration;

			var bucketSize = (int)Math.Ceiling(_99/(double)BucketCount);

			var buckets = Enumerable.Range(0,BucketCount).Select(i => new PageDurationDistributionHistogram.Bucket { Count = 0,MinIncl = i*bucketSize,MaxExcl = (i+1)*bucketSize }).ToArray();

			Func<int,int> getBucketIndex = d => (int)(d/(double)bucketSize);

			foreach(var row in distribution) {
				var idx = getBucketIndex(row);
				if(idx>=buckets.Length) {
					continue;
				}
				buckets[idx].Count++;
			}

			var total = distribution.Length;
			var sum = 0;

			int? medianBucketIndex = null;
			int? _90pctBucketIndex=  null;
			int? meanBucketIndex = null;

			var avg = (int)distribution.Average();
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

			return new PageDurationDistributionHistogram(buckets, meanBucketIndex??0,medianBucketIndex??0, _90pctBucketIndex??0);
		}
	}
}