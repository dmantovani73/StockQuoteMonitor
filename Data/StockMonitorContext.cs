using Microsoft.EntityFrameworkCore;
using StockQuoteMonitor.Models;

namespace StockQuoteMonitor.Data;

public class StockMonitorContext : DbContext
{
    public StockMonitorContext(DbContextOptions<StockMonitorContext> options)
        : base(options)
    { }

    public DbSet<Stock> Stocks => Set<Stock>();

    public DbSet<Quote> Quotes => Set<Quote>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
}
