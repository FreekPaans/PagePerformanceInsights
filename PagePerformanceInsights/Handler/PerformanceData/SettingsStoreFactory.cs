using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using PagePerformanceInsights.Configuration;
using PagePerformanceInsights.Events;

namespace PagePerformanceInsights.Handler.PerformanceData {
	class SettingsStoreFactory {
		readonly static object _store;

		//const string DefaultStore = ;
		readonly static string[] DefaultStores = new string[] { 
			"PagePerformanceInsights.MemoryStore.MemoryStoreDataProvider",
			"PagePerformanceInsights.MemoryStore.MemoryStoreDataProvider,PagePerformanceInsights.MemoryStore"
		};

		readonly static EventLogHelper _eventLogger = new EventLogHelper(typeof(SettingsStoreFactory));

		static SettingsStoreFactory() {
			//var configStore = ConfigurationManager.AppSettings["PPI.Store"];

			var configStore = StoreSection.Get().StoreNameOrType;
			
			Type storeImplementation=null;

			if(!string.IsNullOrEmpty(configStore)) {
				storeImplementation= Type.GetType(configStore);
			}
			else {
				foreach(var store in DefaultStores) {
					storeImplementation = Type.GetType(store);
					if(storeImplementation!=null) {
						break;
					}
				}
			}

			//var storeImplementation= ??DefaultStore;

			try {
				_store = Activator.CreateInstance(storeImplementation);
			}
			catch(Exception e) {
				_eventLogger.LogException("Failed creating data store, using NullStore", e);
				_store =new NullStore();
			}
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