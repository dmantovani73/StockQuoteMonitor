#nullable disable

namespace StockQuoteMonitor.Models;

public class Quote
{
    public long Id { get; set; }

    public string Symbol { get; set; }

    public DateTime Timestamp { get; set; }

    public string Currency { get; set; }

    public decimal LastSalePrice { get; set; }

    public decimal NetChange { get; set; }

    public decimal PercentageChange { get; set; }

    public string DeltaIndicator { get; set; }
}
