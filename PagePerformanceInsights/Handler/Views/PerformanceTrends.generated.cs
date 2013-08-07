﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PagePerformanceInsights.Handler.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.5.4.0")]
    public partial class PerformanceTrends : RazorGenerator.Templating.RazorTemplateBase
    {
#line hidden

        #line 3 "..\..\Handler\Views\PerformanceTrends.cshtml"

	public PagePerformanceInsights.Handler.ViewModels.TrendViewModel ViewModel{get;set;}

        #line default
        #line hidden

        public override void Execute()
        {


WriteLiteral("\r\n\r\n");


WriteLiteral("\r\n\r\n<script type=\"text/javascript\">\r\n\t//google.setOnLoadCallback();\r\n\t(function (" +
") {\r\n\t\tfunction drawChart() {\r\n\t\t\tvar tbl = [\r\n\t\t\t\t[\'Time\', \'Count\', \'Mean\', \'Me" +
"dian\', \'90%\']\r\n\t\t\t];\r\n\r\n\t\t\ttbl = tbl.concat(");


            
            #line 15 "..\..\Handler\Views\PerformanceTrends.cshtml"
                Write(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(ViewModel.Data.TrendData.Select(t=>new object[] { t.TimeStamp.TimeOfDay.ToString("hh\\:mm"), t.Count, t.Mean,t.Median, t._90PCT}).ToArray()));

            
            #line default
            #line hidden
WriteLiteral(@");

			var data = google.visualization.arrayToDataTable(tbl);

			var options = {
				title: 'Trend',
				series: { 0: { targetAxisIndex: 1 } }
				//hAxis: { title: 'Year', titleTextStyle: { color: 'red' } }
			};

			//var chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
			var chart = new google.visualization.LineChart(document.getElementsByClassName('trend_chart_container')[0]);
			chart.draw(data, options);
		}

		drawChart();

	})();
</script>

<div class=""trend_chart_container"">

</div>");


        }
    }
}
#pragma warning restore 1591
