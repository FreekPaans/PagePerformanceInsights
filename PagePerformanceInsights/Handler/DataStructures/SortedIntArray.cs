using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.DataStructures {
	class SortedIntArray {
		readonly SortedList<int,int> _sortedList;

		public SortedIntArray(): this(new int[0]) {
		}

		public SortedIntArray(ICollection<int> @base) {
			_sortedList = new SortedList<int,int>();
			foreach(var grp in @base.GroupBy(g => g)) {
				_sortedList.Add(grp.Key,grp.Count());
			}
		}


		public void Insert(int value) {

			if(_sortedList.ContainsKey(value)) {
				_sortedList[value]++;
				return;
			}
			_sortedList[value] = 1;
		}

		public int[] ToArray() {
			var res = new List<int>();
			foreach(var key in _sortedList.Keys) {
				res.AddRange(Enumerable.Range(0,_sortedList[key]).Select(i => key));
			}
			return res.ToArray();
		}
	}
}