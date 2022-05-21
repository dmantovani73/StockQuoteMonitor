namespace StockMonitor.Infrastructure;

public interface IStockQuoteClient : IDisposable
{
    Task<QueryResponse> Query(string symbol);
}