$(function () {
    function updateUI(stock, quote) {
        $("#company-name").val(stock == null ? null : stock.companyName);
        $("#stock-type").val(stock == null ? null : stock.stockType);
        $("#exchange").val(stock == null ? null : stock.exchange);
        $("#last-sale-price").val(quote == null ? null : quote.currency + quote.lastSalePrice);
        $("#net-change").val(quote == null ? null : quote.netChange);
        $("#percentage-change").val(quote == null ? null : quote.percentageChange + '%');
		$("#delta-indicator").attr("src", quote == null ? null : "images/arrow_" + quote.deltaIndicator + ".png");

		window.lastSalePrice = quote == null ? null : quote.lastSalePrice; + Utils.rand(-20, 20);
    }

    var connection = new signalR.HubConnectionBuilder().withUrl("/quoteHub").build();

    connection.on("ReceiveQuote", function (stock, quote) {
        console.log(stock);
        console.log(quote);

        updateUI(stock, quote);
    });

    connection.start().then(function () {
        var button = $("#subscribe");
        button.click(function () {
            var symbol = $("#symbol").val();

            connection.invoke("Subscribe", symbol).catch(function (err) {
                return console.error(err.toString());
            });
        });

        // La connessione è attiva, posso attivare il pulsante.
        button.prop("disabled", false);

        var button = $("#unsubscribe");
        button.click(function () {
            var symbol = $("#symbol").val();

            connection.invoke("Unsubscribe", symbol)
                .catch(function (err) {
                    return console.error(err.toString());
                });

            updateUI();
        });

        // La connessione è attiva, posso attivare il pulsante.
        button.prop("disabled", false);
    }).catch(function (err) {
        console.error(err);
    });

	// Chart.
	var chartColors = {
		red: 'rgb(255, 99, 132)',
		orange: 'rgb(255, 159, 64)',
		yellow: 'rgb(255, 205, 86)',
		green: 'rgb(75, 192, 192)',
		blue: 'rgb(54, 162, 235)',
		purple: 'rgb(153, 102, 255)',
		grey: 'rgb(201, 203, 207)'
	};

	function randomScalingFactor() {
		return (Math.random() > 0.5 ? 1.0 : -1.0) * Math.round(Math.random() * 100);
	}

	function onRefresh(chart) {
		if (!window.lastSalePrice) return;

        chart.config.data.datasets.forEach(function (dataset) {
            dataset.data.push({
                x: Date.now(),
                y: window.lastSalePrice + randomScalingFactor()
            });
        });
    }

	var color = Chart.helpers.color;
	var config = {
		type: 'line',
		data: {
			datasets: [{
				label: 'Last Sale Price',
				backgroundColor: color(chartColors.red).alpha(0.5).rgbString(),
				borderColor: chartColors.red,
				fill: false,
				lineTension: 0,
				borderDash: [8, 4],
				data: []
			}]
		},
		options: {
			title: {
				display: true,
				text: 'Line chart (horizontal scroll) sample'
			},
			scales: {
				xAxes: [{
					type: 'realtime',
					realtime: {
						duration: 20000,
						refresh: 1000,
						delay: 2000,
						onRefresh: onRefresh
					}
				}],
				yAxes: [{
					scaleLabel: {
						display: true,
						labelString: 'value'
					},
					ticks: {
						//max: 1000,
						min: 0
					}
				}]
			},
			tooltips: {
				mode: 'nearest',
				intersect: false
			},
			hover: {
				mode: 'nearest',
				intersect: false
			}
		}
	};

	var ctx = document.getElementById('myChart').getContext('2d');
	window.myChart = new Chart(ctx, config);
})