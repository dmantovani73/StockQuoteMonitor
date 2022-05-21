using Microsoft.EntityFrameworkCore;
using StockMonitor.Infrastructure;
using StockQuoteMonitor.Data;
using StockQuoteMonitor.Jobs;
using StockQuoteMonitor.Models;
using System.Globalization;

namespace StockQuoteMonitor.Services;

public class StockMonitor : IStockMonitor
{
    readonly ILogger<StockQuoteQueryJob> _logger;
    readonly IDbContextFactory<StockMonitorContext> _contextFactory;
    readonly IStockQuoteClient _client;

    static readonly IFormatProvider NumberFormat = new NumberFormatInfo
    {
        NumberDecimalSeparator = ".",
        NumberGroupSeparator = "",
    };
    public StockMonitor(ILogger<StockQuoteQueryJob> logger, IDbContextFactory<StockMonitorContext> contextFactory, IStockQuoteClient client)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _client = client;
    }

    public async Task Subscribe(string connectionId, string symbol, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Per ogni connessione può esistere una sola sottoscrizione attiva. 
        // Cancello l'eventuale sottoscrizione attiva per sostituirla.
        var subscriptions = await context.Subscriptions.Where(s => s.ConnectionId == connectionId).ToListAsync(cancellationToken);
        context.Subscriptions.RemoveRange(subscriptions);

        // Aggiungo la nuova sottoscrizione.
        context.Subscriptions.Add(new Subscription
        {
            Symbol = symbol,
            ConnectionId = connectionId,
        });

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{ConnectionId} subscribes {Symbol}", connectionId, symbol);
    }

    public async Task Notify(Func<Stock, Quote, IEnumerable<string>, CancellationToken, Task> notifier, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var subscriptions =
            from s in await context.Subscriptions.ToListAsync(cancellationToken)
            group s.ConnectionId by s.Symbol;

        await Parallel.ForEachAsync(subscriptions, cancellationToken, async (s, cancellationToken) =>
        {
            var symbol = s.Key;

            _logger.LogWarning("Get quotes for {Symbol}", symbol);

            var response = await _client.Query(symbol);
            var (stock, quote) = await UpdateDatabase(symbol, response, cancellationToken);
            if (stock != null && quote != null) await notifier(stock, quote, s, cancellationToken);
        });
    }

    async Task<(Stock? Stock, Quote? Quote)> UpdateDatabase(string symbol, QueryResponse response, CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;
        var data = response?.Data;
        var rawQuote = data?.PrimaryData;
        if (data == null || rawQuote == null)
        {
            _logger.LogWarning("Missing data for {Symbol}", symbol);
            return (null, null);
        }

        using var localContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var stock = await localContext.Stocks.FindAsync(symbol);
        if (stock == null)
        {
            stock = new Stock
            {
                Symbol = symbol,
                CompanyName = data.CompanyName,
                Exchange = data.Exchange,
                IsNasdaq100 = data.IsNasdaq100,
                IsNasdaqListed = data.IsNasdaqListed,
                StockType = data.StockType,
            };

            localContext.Stocks.Add(stock);
        }

        var currency = rawQuote.LastSalePrice[0].ToString();
        var lastSalePrice = ParseDecimal(rawQuote.LastSalePrice[1..]);
        var netChange = ParseDecimal(rawQuote.NetChange);
        var percentageChange = ParseDecimal(rawQuote.PercentageChange);

        var quote = new Quote
        {
            Timestamp = timestamp,
            Symbol = symbol,
            Currency = currency,
            LastSalePrice = lastSalePrice,
            NetChange = netChange,
            PercentageChange = percentageChange,
            DeltaIndicator = rawQuote.DeltaIndicator,
        };

        localContext.Quotes.Add(quote);

        await localContext.SaveChangesAsync(cancellationToken);

        return (stock, quote);
    }

    static decimal ParseDecimal(string value) => decimal.Parse(value.Replace("+", "").Replace("%", ""), NumberFormat);
}
