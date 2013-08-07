using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Handler.RequestHandling {
	public static class MimeHelpers {
		public static string GetMimeType(string path) {
			var ext = System.IO.Path.GetExtension(path);
			switch(ext.Substring(1)) {
				case "js":
					return "application/x-javascript";
				case "css":
					return "text/css";
				case "png":
					return "image/png";
				case "gif":
					return "image/gif";
			}
			return "application/octet-stream";
		}
	}
}