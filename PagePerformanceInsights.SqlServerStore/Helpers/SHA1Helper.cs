using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PagePerformanceInsights.SqlServerStore.Helpers {
	static class SHA1Helper {
		readonly static Dictionary<string,string> _SHA1Cache = new Dictionary<string,string>();

		readonly static SHA1 _sha1 = SHA1.Create();

		public static string GetSHA1String(string input) {
			if(!_SHA1Cache.ContainsKey(input)) {
				var bytes = _sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
				lock(_SHA1Cache) {
					_SHA1Cache[input] = BitConverter.ToString(bytes).Replace("-","");
				}
			}
			return _SHA1Cache[input];
		}
	}
}
