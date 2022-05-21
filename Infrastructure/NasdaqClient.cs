#nullable disable 

using System.Net;
using System.Text.Json.Serialization;

namespace StockMonitor.Infrastructure;

public class NasdaqClient : IStockQuoteClient
{
    readonly HttpClient _client;

    public NasdaqClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _client = new HttpClient(handler);
        _client.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
        _client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
        _client.DefaultRequestHeaders.Add("accept-language", "en,it-IT;q=0.9,it;q=0.8,en-US;q=0.7");
        _client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.54 Safari/537.36");
    }

    public Task<QueryResponse> Query(string symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        var url = $"https://api.nasdaq.com/api/quote/{symbol}/info?assetclass=stocks";
        return _client.GetFromJsonAsync<QueryResponse>(url);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

public class Data
{
	[JsonPropertyName("symbol")]
	public string Symbol { get; set; }

	[JsonPropertyName("companyName")]
	public string CompanyName { get; set; }

	[JsonPropertyName("stockType")]
	public string StockType { get; set; }

	[JsonPropertyName("exchange")]
	public string Exchange { get; set; }

	[JsonPropertyName("isNasdaqListed")]
	public bool IsNasdaqListed { get; set; }

	[JsonPropertyName("isNasdaq100")]
	public bool IsNasdaq100 { get; set; }

	[JsonPropertyName("isHeld")]
	public bool IsHeld { get; set; }

	[JsonPropertyName("primaryData")]
	public PrimaryData PrimaryData { get; set; }

	[JsonPropertyName("secondaryData")]
	public SecondaryData SecondaryData { get; set; }

	[JsonPropertyName("marketStatus")]
	public string MarketStatus { get; set; }

	[JsonPropertyName("assetClass")]
	public string AssetClass { get; set; }

	[JsonPropertyName("tradingHeld")]
	public object TradingHeld { get; set; }

	[JsonPropertyName("complianceStatus")]
	public object ComplianceStatus { get; set; }
}

public class PrimaryData
{
	[JsonPropertyName("lastSalePrice")]
	public string LastSalePrice { get; set; }

	[JsonPropertyName("netChange")]
	public string NetChange { get; set; }

	[JsonPropertyName("percentageChange")]
	public string PercentageChange { get; set; }

	[JsonPropertyName("deltaIndicator")]
	public string DeltaIndicator { get; set; }

	[JsonPropertyName("lastTradeTimestamp")]
	public string LastTradeTimestamp { get; set; }

	[JsonPropertyName("isRealTime")]
	public bool IsRealTime { get; set; }
}

public class QueryResponse
{
	[JsonPropertyName("data")]
	public Data Data { get; set; }

	[JsonPropertyName("message")]
	public object Message { get; set; }

	[JsonPropertyName("status")]
	public Status Status { get; set; }
}

public class SecondaryData
{
	[JsonPropertyName("lastSalePrice")]
	public string LastSalePrice { get; set; }

	[JsonPropertyName("netChange")]
	public string NetChange { get; set; }

	[JsonPropertyName("percentageChange")]
	public string PercentageChange { get; set; }

	[JsonPropertyName("deltaIndicator")]
	public string DeltaIndicator { get; set; }

	[JsonPropertyName("lastTradeTimestamp")]
	public string LastTradeTimestamp { get; set; }

	[JsonPropertyName("isRealTime")]
	public bool IsRealTime { get; set; }
}

public class Status
{
	[JsonPropertyName("rCode")]
	public int RCode { get; set; }

	[JsonPropertyName("bCodeMessage")]
	public object BCodeMessage { get; set; }

	[JsonPropertyName("developerMessage")]
	public object DeveloperMessage { get; set; }
}