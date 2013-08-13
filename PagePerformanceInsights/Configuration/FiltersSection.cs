using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Configuration {
	public class FiltersSection : ConfigurationSection{
		public static FiltersSection Get() {
			return (FiltersSection)(ConfigurationManager.GetSection("ppi/filters")??new FiltersSection());
		}

		[ConfigurationProperty("",IsDefaultCollection=true)]
		public FiltersCollection Filters {
			get {
				return (FiltersCollection)base[""];
			}
			set {
				this[""]  = value;
			}
		}
	}

	[ConfigurationCollection(typeof(FilterElement))]
	public class FiltersCollection: ConfigurationElementCollection {
		protected override ConfigurationElement CreateNewElement() {
			return new FilterElement();
		}
		protected override object GetElementKey(ConfigurationElement element) {
			return ((FilterElement)element).NameOrType;
		}

		

	}

	public class FilterElement : ConfigurationElement{
		[ConfigurationProperty("nameOrType", IsRequired=true)]
		//todo: stringvalidator
		public string NameOrType{
			get {
				return (string)this["nameOrType"];
			}
			set {
				this["nameOrType"] = value;
			}
		}
	}
}