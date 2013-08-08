using PagePerformanceInsights.CommBus;
using PagePerformanceInsights.Handler.PerformanceData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.MemoryStore {
	//class PageDistribution:INeedNewRequestData {
	//	readonly List<HttpRequestData> _notProcessed;
	//	readonly object _readerLock = new object();

	//	public PageDistribution() {
	//		_notProcessed = new List<HttpRequestData>();
	//	}

	//	public void NewRequestDataArrived(CommBus.HttpRequestData[] res) {
	//		lock(_notProcessed) {
	//			_notProcessed.AddRange(res);
	//		}
	//	}

		

	//	public Handler.PerformanceData.DataTypes.PageDurationDistributionHistogram GetPageDistribution(DateTime forDate,string forPage) {
	//		HttpRequestData[] toProcess;
	//		lock(_notProcessed) {
	//			toProcess = _notProcessed.ToArray();
	//			_notProcessed.Clear();
	//		}

	//		lock(_readerLock) {
	//		}
	//	}
	//}
}
