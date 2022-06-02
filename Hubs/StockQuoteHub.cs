using Microsoft.AspNetCore.SignalR;
using StockQuoteMonitor.Services;

namespace StockMonitor.Hubs;

public class StockQuoteHub : Hub
{
    readonly IStockMonitor _monitor;

    public StockQuoteHub(IStockMonitor monitor)
    {
        _monitor = monitor;
    }

    public Task Subscribe(string symbol) => _monitor.Subscribe(Context.ConnectionId, symbol);

    public Task Unsubscribe(string symbol) => _monitor.Unsubscribe(Context.ConnectionId, symbol);
}
