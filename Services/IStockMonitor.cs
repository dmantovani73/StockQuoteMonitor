using StockQuoteMonitor.Models;

namespace StockQuoteMonitor.Services;
public interface IStockMonitor
{
    Task Notify(Func<Stock, Quote, IEnumerable<string>, CancellationToken, Task> notifier, CancellationToken cancellationToken = default);
    Task Subscribe(string connectionId, string symbol, CancellationToken cancellationToken = default);
}