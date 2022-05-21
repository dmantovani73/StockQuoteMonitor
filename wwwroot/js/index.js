$(function () {
    function updateUI(stock, quote) {
        $("#company-name").val(stock == null ? null : stock.companyName);
        $("#stock-type").val(stock == null ? null : stock.stockType);
        $("#exchange").val(stock == null ? null : stock.exchange);
        $("#last-sale-price").val(quote == null ? null : quote.currency + quote.lastSalePrice);
        $("#net-change").val(quote == null ? null : quote.netChange);
        $("#percentage-change").val(quote == null ? null : quote.percentageChange + '%');
        $("#delta-indicator").attr("src", quote == null ? null : "images/arrow_" + quote.deltaIndicator + ".png");
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
})