using PagePerformanceInsights.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace PagePerformanceInsights.Auth.Modules {
	public class PPIAuthModule : IHttpModule{
		public void Dispose() {
			//throw new NotImplementedException();
		}

		readonly static string _principalUsername;
		readonly static HashSet<string> _IPs;
		readonly static string _PPIBasePath;
		readonly static bool _customAuthEnabled;

		static PPIAuthModule() {
			var settings = PagePerformanceInsights.Configuration.AuthenticationSection.Get();

			if(string.IsNullOrWhiteSpace(settings.PrincipalUsername) || string.IsNullOrWhiteSpace(settings.PPIPath)) {
				_customAuthEnabled = false;
				return;
			}

			_principalUsername = settings.PrincipalUsername;
			_PPIBasePath = settings.PPIPath;
			_IPs = new HashSet<string>(settings.IPs.Cast<AllowElement>().Where(a=>!string.IsNullOrWhiteSpace(a.IP)).Select(a=>a.IP));
			_customAuthEnabled = true;

		}

		public void Init(HttpApplication context) {
			if(!_customAuthEnabled) {
				return;
			}

			context.AuthenticateRequest+=(s,e)=> {
				if(!HttpContext.Current.Request.Url.LocalPath.StartsWith(_PPIBasePath)) {
					return;
				}

				if(HttpContext.Current.Request.IsAuthenticated) {
					return;
				}

				if(!_IPs.Contains(HttpContext.Current.Request.UserHostAddress)) {
					return;
				}

				var principal = new GenericPrincipal(new GenericIdentity(_principalUsername),null);
				Thread.CurrentPrincipal = principal;
				HttpContext.Current.User = principal;
				return;
			};
			//throw new NotImplementedException();
		}
	}
}