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
    public partial class Home : RazorGenerator.Templating.RazorTemplateBase
    {
#line hidden

        #line 8 "..\..\Handler\Views\Home.cshtml"

	public DateTime ActiveDate{get;set;}

        #line default
        #line hidden

        public override void Execute()
        {


WriteLiteral("\r\n\r\n\r\n");



WriteLiteral("\r\n\r\n");


WriteLiteral(@"

<!doctype html>
<html>
<head>
<title>ASP.NET Site Performance Viewer</title>

<script type=""text/javascript"" src=""https://www.google.com/jsapi""></script>
<script type=""text/javascript"">
	google.load(""visualization"", ""1"", { packages: [""corechart""] });
</script>




");


            
            #line 25 "..\..\Handler\Views\Home.cshtml"
Write(PagePerformanceInsights.Handler.Helpers.HandlerHelpers.IncludeStyleSheet("/CSS/jquery.dataTables.css"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 26 "..\..\Handler\Views\Home.cshtml"
Write(PagePerformanceInsights.Handler.Helpers.HandlerHelpers.IncludeStyleSheet("/CSS/Main.css"));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");


            
            #line 28 "..\..\Handler\Views\Home.cshtml"
Write(PagePerformanceInsights.Handler.Helpers.HandlerHelpers.IncludeJavaScript("/Scripts/jquery-2.0.3.js"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 29 "..\..\Handler\Views\Home.cshtml"
Write(PagePerformanceInsights.Handler.Helpers.HandlerHelpers.IncludeJavaScript("/Scripts/jquery.dataTables.js"));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");


            
            #line 31 "..\..\Handler\Views\Home.cshtml"
Write(PagePerformanceInsights.Handler.Helpers.HandlerHelpers.IncludeJavaScript("/Scripts/Main.js"));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");



WriteLiteral(@"
<script type=""text/javascript"" src=""http://code.jquery.com/ui/1.10.3/jquery-ui.js""></script>
<link rel=""stylesheet"" href=""http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css"" />

<script type=""text/javascript"">
	(function () {
		var handlerUrl = '");


            
            #line 39 "..\..\Handler\Views\Home.cshtml"
               Write(PagePerformanceInsights.Handler.Helpers.HandlerHelpers.HandlerPath);

            
            #line default
            #line hidden
WriteLiteral(@"';
		window.GetRelativeUrl =	function (url) {
			return handlerUrl + '?'+url;
		}
		window.GetDataUrl = function (url) {
			return handlerUrl + '?data=' + encodeURIComponent(url);
		}
	})();
</script>

</head>
<body>


<div class=""body_wrapper"">
	<div class=""date_selector"">
	<form>
	<div class=""pages_date_selector"">
		<input class=""active_date"" name=""active_date"" type=""hidden"" value=""");


            
            #line 57 "..\..\Handler\Views\Home.cshtml"
                                                                Write(ActiveDate.Date.ToString("yyyy-MM-dd"));

            
            #line default
            #line hidden
WriteLiteral(@""" />
	<label>Insights for
		<input class=""select_day"" type=""text"" value=""today"" />
	</label>
	</div>
	</form>
	</div>
	<div class=""pages_table_pane pane col2 loading_data"">
		
	</div>
	<div class=""pane col2 chart_col"">
		<div class=""distribution_pane pane loading_data"">
		</div>
		<div class=""trend_pane pane loading_data"">
		</div>
	</div>
</div>
</body>
</html>");


        }
    }
}
#pragma warning restore 1591
