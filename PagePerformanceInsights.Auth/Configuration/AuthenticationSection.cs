using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PagePerformanceInsights.Configuration {
	public class AuthenticationSection : ConfigurationSection {
		[ConfigurationProperty("PPIPath",IsRequired=true)]
		public string PPIPath{
			get {
				return (string)this["PPIPath"];
			}
			set {
				this["PPIPath"] = value;
			}
		}

		[ConfigurationProperty("principalUsername",IsRequired=true)]
		public string PrincipalUsername {
			get {
				return (string)this["principalUsername"];
			}
			set {
				this["principalUsername"] = value;
			}
		}

		[ConfigurationProperty("",IsDefaultCollection=true)]
		public AllowCollection IPs {
			get {
				return (AllowCollection)base[""];
			}
			set {
				this[""]  = value;
			}
		}

		public static AuthenticationSection Get() {
			return (AuthenticationSection)(ConfigurationManager.GetSection("ppi/auth")??new AuthenticationSection());
		}
	}

	[ConfigurationCollection(typeof(AllowElement))]
	public class AllowCollection:ConfigurationElementCollection {
		protected override ConfigurationElement CreateNewElement() {
			return new AllowElement();
		}
		protected override object GetElementKey(ConfigurationElement element) {
			return ((AllowElement)element).Name;
		}
	}

	public class AllowElement:ConfigurationElement {
		[ConfigurationProperty("name", IsRequired=true)]
		public string Name {
			get {
				return (string)this["name"];
			}
			set {
				this["name"]=value;
			}
		}

		[ConfigurationProperty("ip",IsRequired=true)]
		//todo: stringvalidator
		public string IP {
			get {
				return (string)this["ip"];
			}
			set {
				this["ip"] = value;
			}
		}
	}
}
