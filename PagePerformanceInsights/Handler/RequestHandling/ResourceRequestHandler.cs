using PagePerformanceInsights.Handler.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace PagePerformanceInsights.Handler.RequestHandling {
	class ResourceRequestHandler : IHandleRoutes {
		//readonly System.Web.HttpContext _context;
		readonly string _resourceValue;

		public ResourceRequestHandler(string resourceValue) {
			//_context = context;
			_resourceValue = resourceValue;
		}

		readonly static Assembly _selfAssembly = Assembly.GetAssembly(typeof(ResourceRequestHandler));

		private string GetResourceName(string resourceValue) {
			return "PagePerformanceInsights.Handler.Assets"+resourceValue.Replace('/','.');
		}

		private static bool IsCss(string resourceValue) {
			return resourceValue.EndsWith(".css");
		}


		public void Run(HttpContext context) {
			using(var resourceStream = _selfAssembly.GetManifestResourceStream(GetResourceName(_resourceValue))) {
				if(resourceStream==null) {
					throw new HttpException(404,"Resource not found");
				}

				if(IsCss(_resourceValue)) {
					CssResource(context,_resourceValue,resourceStream);
					return;
				}

				OutputStream(context,_resourceValue,resourceStream);
			}
		}

		private void CssResource(HttpContext context,string resourceValue,System.IO.Stream cssStream) {
			var str = new StreamReader(cssStream).ReadToEnd().Replace("{baseDir}",string.Format("{0}?resource=",HandlerHelpers.HandlerPath));
			using(var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(str))) {
				//outputStream.Position = 0;
				OutputStream(context,resourceValue,outputStream);
			}
		}

		private void OutputStream(HttpContext context,string resourceValue,System.IO.Stream resource) {
			var mime = MimeHelpers.GetMimeType(resourceValue);

			resource.CopyTo(context.Response.OutputStream);

			context.Response.AddHeader("Content-Type",mime);
		}

	
	}
}
