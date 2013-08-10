using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore {
	interface IProvidePageIds {
		Dictionary<string,int> GetPageIds(ICollection<string> pageNames);
		string GetPageHash(string pageName);

		Dictionary<int,string> GetPageNames(ICollection<int> pageIds);
	}
}
