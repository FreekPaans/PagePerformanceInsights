using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.Algorithms.Median {
	public class QuickSelect {
		public int Select(int[] list,double percentile) {
			return Select(list,(int)(percentile*(list.Length-1)));
		}
		public int Select(int[] list,int k) {
			return Select(list,0,list.Length-1,k);
		}
		int Select(int[] list,int left,int right,int k) {
			if(left==right) {
				return list[left];
			}
			var pivotIndex = rnd.Next(left,right);
			var pivotNewIndex = Partition(list,left,right,pivotIndex);
			var pivotDist = pivotNewIndex-left+1;

			if(pivotDist == k) {
				return list[pivotNewIndex];
			}
			if(k<pivotDist) {
				return Select(list,left,pivotNewIndex-1,k);
			}
			return Select(list,pivotNewIndex+1,right,k-pivotDist);
		}

		Random rnd = new Random();

		int Partition(int[] items,int left,int right,int pivotIndex) {
			var pivotValue = items[pivotIndex];
			Swap(items,pivotIndex,right);
			var storeIndex = left;

			for(var i=left;i<right;i++) {
				if(items[i]>=pivotValue) {
					continue;
				}
				Swap(items,i,storeIndex);
				storeIndex++;
			}
			Swap(items,right,storeIndex);
			return storeIndex;
		}

		private void Swap(int[] items,int idx1,int idx2) {
			var tmp = items[idx1];
			items[idx1] = items[idx2];
			items[idx2] = tmp;
		}
	}
}