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
    public partial class ResponseDistribution : RazorGenerator.Templating.RazorTemplateBase
    {
#line hidden

        #line 3 "..\..\Handler\Views\ResponseDistribution.cshtml"

	internal PagePerformanceInsights.Handler.ViewModels.ResponseDistributionViewModel ViewModel{get;set;}

        #line default
        #line hidden

        public override void Execute()
        {


WriteLiteral("\r\n\r\n");


WriteLiteral(@"


<script type=""text/javascript"">
	(function () {
		//google.load(""visualization"", ""1"", { packages: [""corechart""] });
		
		//google.setOnLoadCallback(drawPingChart);

		var drawNoData = function() {
			var $container = $('.distribution_chart_container');
			$container.html('<div class=""no_data"">No data available</div>');
		}

		//drawPingChart();
		function drawPingChart() {
			var tbl = [[""Duration"", ""Count"", ""Cumulative %""]];

			var _tblData = ");


            
            #line 23 "..\..\Handler\Views\ResponseDistribution.cshtml"
              Write(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(ViewModel.Data.Buckets.Select(b=>new [] { b.MaxExcl,b.Count,b.CumulativePercentage}).ToArray()));

            
            #line default
            #line hidden
WriteLiteral(@";

			if(_tblData.length==0) {
				return drawNoData();
			}

			tbl = tbl.concat(_tblData);

			

			//console.log(tbl);

			var data = google.visualization.arrayToDataTable(tbl);

			data.addColumn({ type: 'string', role: 'annotation' })

			data.setValue(");


            
            #line 39 "..\..\Handler\Views\ResponseDistribution.cshtml"
            Write(ViewModel.Data.MeanBucketIndex);

            
            #line default
            #line hidden
WriteLiteral(", tbl[0].length, \"Mean\");\r\n\t\t\tdata.setValue(");


            
            #line 40 "..\..\Handler\Views\ResponseDistribution.cshtml"
            Write(ViewModel.Data.MedianBucketIndex);

            
            #line default
            #line hidden
WriteLiteral(", tbl[0].length, \"50%\");\r\n\t\t\tdata.setValue(");


            
            #line 41 "..\..\Handler\Views\ResponseDistribution.cshtml"
            Write(ViewModel.Data._90PercentileBucketIndex);

            
            #line default
            #line hidden
WriteLiteral(@", tbl[0].length, ""90%"");

			var options = {
				title: 'Distribution',
				series: { 1: { targetAxisIndex: 1 } }
				//vAxis: { logScale: true },
				//legend: { position: ""right"" },
				//chartArea: {
				//	width: ""70%"",
				//	left: 50
				//}
			};

			var chart = new google.visualization.LineChart(document.getElementsByClassName('distribution_chart_container')[0]);
			chart.draw(data, options);
		}
		drawPingChart();
	})();
</script>
<div class=""distribution_chart_container"">

</div>");


        }
    }
}
#pragma warning restore 1591
