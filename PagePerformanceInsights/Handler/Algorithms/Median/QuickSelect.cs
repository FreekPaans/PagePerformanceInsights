using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.Algorithms.Median {
	//from: http://blog.teamleadnet.com/2012/07/quick-select-algorithm-find-kth-element.html
	//tried to implement a recursion version from wikipedia first, got stackoverflow exceptions and this one seems to work..
	class QuickSelect2 {
		public static int selectKth(int[] arr,int k) {
			if(arr == null || arr.Length <= k)
				throw new InvalidOperationException();

			int from = 0,to = arr.Length - 1;

			// if from == to we reached the kth element
			while(from < to) {
				int r = from,w = to;
				int mid = arr[(r + w) / 2];

				// stop if the reader and writer meets
				while(r < w) {

					if(arr[r] >= mid) { // put the large values at the end
						int tmp = arr[w];
						arr[w] = arr[r];
						arr[r] = tmp;
						w--;
					}
					else { // the value is smaller than the pivot, skip
						r++;
					}
				}

				// if we stepped up (r++) we need to step one down
				if(arr[r] > mid)
					r--;

				// the r pointer is on the end of the first k elements
				if(k <= r) {
					to = r;
				}
				else {
					from = r + 1;
				}
			}

			return arr[k];
		}
	}
	public class QuickSelect {
		public int Select(int[] list,double percentile) {
			return Select(list,(int)(percentile*(list.Length-1)));
		}
		public int Select(int[] list,int k) {
			if(list.Length==0) {
				return 0;
			}
			if(k==0) {
				return list[0];
			}

			return QuickSelect2.selectKth(list,k);

			//return Select(list,0,list.Length-1,k);
		}
		//int Select(int[] list,int left,int right,int k) {
			
		//	if(left==right) {
		//		return list[left];
		//	}
		//	var pivotIndex = rnd.Next(left,right);
		//	var pivotNewIndex = Partition(list,left,right,pivotIndex);
		//	var pivotDist = pivotNewIndex-left+1;

		//	if(pivotDist == k) {
		//		return list[pivotNewIndex];
		//	}
		//	if(k<pivotDist) {
		//		return Select(list,left,pivotNewIndex-1,k);
		//	}
		//	return Select(list,pivotNewIndex+1,right,k-pivotDist);
		//}

		//Random rnd = new Random();

		//int Partition(int[] items,int left,int right,int pivotIndex) {
		//	var pivotValue = items[pivotIndex];
		//	Swap(items,pivotIndex,right);
		//	var storeIndex = left;

		//	for(var i=left;i<right;i++) {
		//		if(items[i]>=pivotValue) {
		//			continue;
		//		}
		//		Swap(items,i,storeIndex);
		//		storeIndex++;
		//	}
		//	Swap(items,right,storeIndex);
		//	return storeIndex;
		//}

		//private void Swap(int[] items,int idx1,int idx2) {
		//	var tmp = items[idx1];
		//	items[idx1] = items[idx2];
		//	items[idx2] = tmp;
		//}

		//public static int Median(List<int> list) {
		//	return new QuickSelect().Select(list.ToArray(), 0.5);
		//}
	}
}