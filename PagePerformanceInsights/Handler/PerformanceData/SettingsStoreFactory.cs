using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.PerformanceData {
	class SettingsStoreFactory {
		readonly static object _store;

		static SettingsStoreFactory() {
			_store = Activator.CreateInstance(Type.GetType("PagePerformanceInsights.MemoryStore.MemoryStoreDataProvider,PagePerformanceInsights.MemoryStore"));
			//Bus.Buffer.SetPerformanceDataStore((IStorePerformanceData)_store);
		}

		public static IProvidePerformanceData GetDataProvider() {
			return (IProvidePerformanceData)_store;
		}

		public static IStorePerformanceData GetDataStorer() {
			return (IStorePerformanceData)_store;
		}
	}
}