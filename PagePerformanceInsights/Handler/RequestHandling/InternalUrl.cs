using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace PagePerformanceInsights.Handler.RequestHandling {
	public class InternalUrl {
		readonly string _localPath;

		public string LocalPath {
			get { return _localPath; }
		}

		readonly System.Collections.Specialized.NameValueCollection _queryString;

		public System.Collections.Specialized.NameValueCollection QueryString {
			get { return _queryString; }
		} 


		InternalUrl(string localPath,System.Collections.Specialized.NameValueCollection queryString) {
			_localPath = localPath;
			_queryString = queryString;
		}

		public static InternalUrl Parse(string path) {
			var elements = path.Substring(1).Split(new[] { "?" },StringSplitOptions.RemoveEmptyEntries);

			var localPath = elements[0];

			string qs = "?";

			if(elements.Length>1) {
				qs = elements[1];
			}

			var queryString = HttpUtility.ParseQueryString(qs);

			return new InternalUrl(localPath,queryString);
		}
	}
}
