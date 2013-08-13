using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PagePerformanceInsights.Configuration {
	public class BufferSection :ConfigurationSection {
		[ConfigurationProperty("maxBufferSize", IsRequired=false,DefaultValue=-1)]
		[IntegerValidator(MinValue=-1)]
		public int MaxBufferSize {
			get {
				return (int)this["maxBufferSize"];
			}
			set {
				this["maxBufferSize"] = value;
			}
		}

		[ConfigurationProperty("writeInterval", IsRequired=false,DefaultValue="00:00:01")]
		[TimeSpanValidator(MinValueString="00:00:01")]
		public TimeSpan WriteInterval {
			get {
				return (TimeSpan)this["writeInterval"];
			}
			set {
				this["writeInterval"] = value.ToString();
			}
		}


		public  static BufferSection Get() {
			return (BufferSection)(ConfigurationManager.GetSection("ppi/buffer")??new BufferSection());
		}
	}
}