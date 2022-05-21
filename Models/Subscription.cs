#nullable disable

using Microsoft.EntityFrameworkCore;

namespace StockQuoteMonitor.Models;

[Index(nameof(Symbol), nameof(ConnectionId))]
public class Subscription
{
    public int Id { get; set; }

    public string Symbol { get; set; }

    public string ConnectionId { get; set; }
}
