using PagePerformanceInsights.Handler.Algorithms.Median;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData.DataTypes {
	public class PerformanceStatisticsForPage {
		public string PageName { get; set; }
		public int Count { get; set; }
		public int Median { get; set; }
		public int Mean { get; set; }
		public int Sum { get; set; }

		public static PerformanceStatisticsForPage Calculate(int[] distribution,string pageName) {
			var res=  new PerformanceStatisticsForPage {
					Count = distribution.Length,
					Mean = (int)distribution.Average(),
					Median = new QuickSelect().Select(distribution,(int)(0.5*distribution.Length)),
					PageName = pageName,
					
				};

			res.Sum = res.Count * res.Mean;

			return res;
		}

		public static PerformanceStatisticsForPage Empty {
			get {
				return new PerformanceStatisticsForPage{};
			}
		}
	}

	//from: http://en.wikipedia.org/wiki/Selection_algorithm
	//	function partition(list, left, right, pivotIndex)
	// pivotValue := list[pivotIndex]
	// swap list[pivotIndex] and list[right]  // Move pivot to end
	// storeIndex := left
	// for i from left to right-1
	//	 if list[i] < pivotValue
	//		 swap list[storeIndex] and list[i]
	//		 increment storeIndex
	// swap list[right] and list[storeIndex]  // Move pivot to its final place
	// return storeIndex
	//}

	//  function select(list, left, right, k)
	// if left = right        // If the list contains only one element
	//	  return list[left]  // Return that element
	// pivotIndex  := ...     // select a pivotIndex between left and right
	// pivotNewIndex  := partition(list, left, right, pivotIndex)
	// pivotDist  := pivotNewIndex - left + 1
	// // The pivot is in its final sorted position,
	//  // so pivotDist reflects its 1-based position if list were sorted
	//  if pivotDist = k
	//	 return list[pivotNewIndex]
	// else if k < pivotDist
	//	 return select(list, left, pivotNewIndex - 1, k)
	// else
	//	  return select(list, pivotNewIndex + 1, right, k - pivotDist)
	
}