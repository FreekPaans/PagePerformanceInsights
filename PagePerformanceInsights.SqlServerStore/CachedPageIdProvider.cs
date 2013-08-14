using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore {
	class CachedPageIdProvider : IProvidePageIds{
		readonly static ConcurrentDictionary<int,string> _pageIdToNameMap = new ConcurrentDictionary<int,string>();
		readonly static ConcurrentDictionary<string,int> _pageNameToIdMap= new ConcurrentDictionary<string,int>();
		readonly static object _writeLock = new object();
		readonly IProvidePageIds _backend;

		public CachedPageIdProvider(IProvidePageIds backend) {
			_backend = backend;
		}

		public Dictionary<string,int> GetPageIds(ICollection<string> pageNames) {
			pageNames = pageNames.Distinct().ToArray();
			var notCached=  pageNames.Except(_pageNameToIdMap.Keys).ToArray();
			//var cached = pageNames.Intersect(notCached);

			AddPageIdsToCache(notCached);

			return pageNames.ToDictionary(p=>p, p=>_pageNameToIdMap[p]);
		}

		private void AddPageIdsToCache(ICollection<string> notCached) {
			if(!notCached.Any()) {
				return;
			}
			var pageIds=  _backend.GetPageIds(notCached.ToArray());

			lock(_writeLock) {
				foreach(var pageIdNamePair in pageIds) {
					_pageIdToNameMap[pageIdNamePair.Value] = pageIdNamePair.Key;
					_pageNameToIdMap[pageIdNamePair.Key] = pageIdNamePair.Value;
				}
			}
		}

		public string GetPageHash(string pageName) {
			return _backend.GetPageHash(pageName);
		}

		public Dictionary<int,string> GetPageNames(ICollection<int> pageIds) {
			pageIds = pageIds.Distinct().ToArray();
			var notCached = pageIds.Except(_pageIdToNameMap.Keys);
			AddPageIdsToCache(notCached.ToArray());
			return pageIds.ToDictionary(p=>p, p=>_pageIdToNameMap[p]);
		}

		private void AddPageIdsToCache(ICollection<int> pageIds) {
			if(!pageIds.Any()) {
				return;
			}
			
			var pairs = _backend.GetPageNames(pageIds);
			lock(_writeLock) {
				foreach(var idNamePair in pairs) {
					_pageIdToNameMap[idNamePair.Key] = idNamePair.Value;
					_pageNameToIdMap[idNamePair.Value] = idNamePair.Key;
				}
			}
		}

		public int GetPageId(string forPage) {
			return GetPageIds(new [] {forPage})[forPage];
		}
	}
}
