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
	var $pagesContainer = $('.pages_table_pane');

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
					"aaSorting": [[1, "desc"]],
					'fnInitComplete': function () {
						//var $pds = $('.pages_date_selector');//.clone();

						//var $select = $pds.find('select');
						//$select.val(date);

						//$select.bind('change', function () {
						//	loadPages($(this).val());
						//});
						//$('.dataTables_filter').prepend($pds);

						$pagesContainer.find('tbody tr').bind('click', function () {
							$(this).siblings('tr').removeClass('selected');
							$(this).addClass('selected');
							var $pageCell = $(this).find('td').first();
							var page = null;

							if(!$pageCell.hasClass('total')) {
								page = $pageCell.text();
							}
							updatePageCharts(page);
						});

						$('.dataTables_filter input').appendTo('.dataTable thead th:first-child').bind('click', function(ev) { ev.stopPropagation() ;});
						$('.dataTables_filter').remove();
					}
					//'bAutoWidth': false
				});
			},
			error: function (err) {
				$pagesContainer.html("Error loading page data: " + err.statusText + ' (' + err.status + ')');
			},
			complete: function () {
				$pagesContainer.removeClass('loading_data');
			}
		});
	}
	
	var $distributionPane = $('.distribution_pane');

	var loadDistribution = function (page) {
		var url = '/distribution';
		if (page) {
			url += '/' + encodeURIComponent(page);
		}

		if (!$distributionPane.hasClass("loading_data")) {
			var $overlay = $('<div class="overlay_loading">').css({ width: $distributionPane.outerWidth(), height: $distributionPane.outerHeight() });
			$distributionPane.append($overlay);
		}

		$.ajax({
			url: GetDataUrl(url),
			success: function (data) {
				$distributionPane.html(data);
			},
			error: function (err) {
				$distributionPane.html('Error getting distribution: ' + err.statusText + ' (' + err.status + ')');
			},
			complete: function () {
				$distributionPane.removeClass('loading_data');
			}
		});

	}

	$trendPane = $('.trend_pane');

	var loadTrend = function (page) {
		var url = '/trend';
		if (page) {
			url += '/' + encodeURIComponent(page);
		}

		if (!$trendPane.hasClass("loading_data")) {
			var $overlay = $('<div class="overlay_loading">').css({ width: $trendPane.outerWidth(), height: $trendPane.outerHeight() });
			$trendPane.append($overlay);
		}

		$.ajax({
			url: GetDataUrl(url),
			success: function (data) {
				$trendPane.html(data);
			},
			error: function (err) {
				$trendPane.html('Error getting trend: ' + err.statusText + ' (' + err.status + ')');
			},
			complete: function () {
				$trendPane.removeClass('loading_data');
			}
		});
	}
	

	var updatePageCharts = function (page) {
		loadDistribution(page);
		loadTrend(page);
	}

	loadPages();
	loadDistribution();
	loadTrend();
});

