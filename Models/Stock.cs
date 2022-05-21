#nullable disable

using System.ComponentModel.DataAnnotations;

namespace StockQuoteMonitor.Models;

public class Stock
{
    [Key]
    public string Symbol { get; set; }

    public string CompanyName { get; set; }

    public string StockType { get; set; }

    public string Exchange { get; set; }

    public bool IsNasdaqListed { get; set; }

    public bool IsNasdaq100 { get; set; }
}
