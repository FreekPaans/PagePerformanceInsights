using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.Helpers {
	public static class HandlerHelpers {
		public static string IncludeStyleSheet(string path) {
			return string.Format(@"<link rel=""stylesheet"" href=""{0}"" />", GetResourcePath(path));
		}

		public static string IncludeJavaScript(string path) {
			return string.Format(@"<script type=""text/javascript"" src=""{0}""></script>",GetResourcePath(path));
		}

		private static string GetResourcePath(string resourcePath) {
			//var handlerPath = 
			return string.Format("{0}?resource={1}",HandlerPath,HttpUtility.UrlPathEncode(resourcePath));
		}

		public static string HandlerPath {
			get {
				return HttpContext.Current.Request.Url.LocalPath;
			}
		}
	}
}