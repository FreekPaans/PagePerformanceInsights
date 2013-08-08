using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	interface INeedNewRequestData {
		void NewRequestDataArrived(CommBus.HttpRequestData[] res);
	}
}
