$(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/quoteHub").build();

    connection.on("ReceiveQuote", function (stock, quote) {
        console.log(stock);
        console.log(quote);

        $("#company-name").val(stock.companyName);
        $("#stock-type").val(stock.stockType);
        $("#exchange").val(stock.exchange);
        $("#last-sale-price").val(quote.currency + quote.lastSalePrice);
        $("#net-change").val(quote.netChange);
        $("#percentage-change").val(quote.percentageChange + '%');
        $("#delta-indicator").attr("src", "images/arrow_" + quote.deltaIndicator + ".png");
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
    }).catch(function (err) {
        console.error(err);
    });
})