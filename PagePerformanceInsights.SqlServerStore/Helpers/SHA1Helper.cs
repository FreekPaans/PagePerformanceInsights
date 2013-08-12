using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Helpers {
	static class SHA1Helper {
		readonly static ConcurrentDictionary<string,string> _SHA1Cache = new ConcurrentDictionary<string,string>();

		//readonly static SHA1 _sha1 = SHA1.Create();

		public static string GetSHA1String(string input) {
			if(!_SHA1Cache.ContainsKey(input)) {
				var bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
				var value = BitConverter.ToString(bytes).Replace("-","");

				_SHA1Cache[input] = value;
			}
			return _SHA1Cache[input];
		}
	}
}
