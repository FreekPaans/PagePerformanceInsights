(function () {
	//alert('test');

	var pad = function(inp) {
		var inpStr = inp.toString();
		if(inpStr.length == 1) {
			inpStr = '0'+inpStr;
		}
		return inpStr;
	}

	Date.prototype.FormatDateTime = function() {
		date = this;
		return date.getFullYear()+'-'+pad(date.getMonth()+1)+'-'+pad(date.getDate())+' '+pad(date.getHours())+':'+pad(date.getMinutes())+':'+pad(date.getSeconds());
	}
})();

$(function () {
	var $pagesContainer = $('.pages_table_container');

	var loadPages = function (date) {
		if(date===undefined) {
			date = "today";
		}

		$.ajax({
			url: GetDataUrl('/pages/' + encodeURIComponent(date)),
			success: function (data) {
				$pagesContainer.html(data);

				$pagesContainer.find('table').dataTable({
					'bPaginate': false,
					'bLengthChange': false,
					'bFilter': true,
					'bInfo': false,
					'fnInitComplete': function () {
						var $pds = $('.pages_date_selector').clone();
						var $select = $pds.find('select');
						$select.val(date);

						$select.bind('change', function () {
							loadPages($(this).val());
						});
						$('.dataTables_filter').prepend($pds);
					}
					//'bAutoWidth': false
				});
			},
			error: function (err) {
				$pagesContainer.html("Error loading page data: " + err.statusText + ' (' + err.status + ')');
			}
		});
	}
	
	var $distributionPane = $('.distribution_pane');

	var loadDistribution = function () {
		$.ajax({
			url: GetDataUrl('/distribution'),
			success: function (data) {
				$distributionPane.html(data);
			},
			error: function (err) {
				$distributionPane.html('Error getting distribution: ' + err.statusText + ' (' + err.status + ')');
			}
		});

	}

	$trendPane = $('.trend_pane');

	var loadTrend = function () {
		$.ajax({
			url: GetDataUrl('/trend'),
			success: function (data) {
				$trendPane.html(data);
			},
			error: function (err) {
				$trendPane.html('Error getting trend: ' + err.statusText + ' (' + err.status + ')');
			}
		});
	}
	
	loadPages();
	loadDistribution();
	loadTrend();
});

