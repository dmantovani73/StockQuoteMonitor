using Microsoft.AspNetCore.SignalR;
using Quartz;
using StockMonitor.Hubs;
using StockQuoteMonitor.Models;
using StockQuoteMonitor.Services;

namespace StockQuoteMonitor.Jobs;

[DisallowConcurrentExecution]
public class StockQuoteQueryJob : IJob
{
    readonly IHubContext<StockQuoteHub> _hubContext;
    readonly IStockMonitor _monitor;

    public StockQuoteQueryJob(IStockMonitor monitor, IHubContext<StockQuoteHub> hubContext)
    {
        _monitor = monitor;
        _hubContext = hubContext;
    }

    public async Task Execute(IJobExecutionContext jobContext)
    {
        var cancellationToken = jobContext.CancellationToken;
        await _monitor.Notify(SendQuote, cancellationToken);
    }

    Task SendQuote(Stock stock, Quote quote, IEnumerable<string> connectionIds, CancellationToken cancellationToken) =>
        _hubContext.Clients.Clients(connectionIds).SendAsync("ReceiveQuote", stock, quote, cancellationToken);
}
