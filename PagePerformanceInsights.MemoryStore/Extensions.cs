using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	static class Extensions {
		public static T2 GetOrAdd<T1,T2>(this Dictionary<T1,T2> input,T1 key,T2 add) {
			if(!input.ContainsKey(key)) {
				input[key] = add;
			}
			return input[key];
		}
	}
}
