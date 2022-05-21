using Microsoft.EntityFrameworkCore;
using Quartz;
using StockMonitor.Hubs;
using StockMonitor.Infrastructure;
using StockQuoteMonitor.Data;
using StockQuoteMonitor.Jobs;
using StockQuoteMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<StockMonitorContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("(default)"))
    );

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IStockQuoteClient, NasdaqClient>();
builder.Services.AddSingleton<IStockMonitor, StockQuoteMonitor.Services.StockMonitor>();

AddQuartz(builder);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<StockQuoteHub>("/quoteHub");

await Init();

app.Run();

void AddQuartz(WebApplicationBuilder builder)
{
    var services = builder.Services;
    var configuration = builder.Configuration;

    services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();

        // Si attribuisce un nome al job.
        var jobKey = new JobKey(nameof(StockQuoteQueryJob));
        q.AddJob<StockQuoteQueryJob>(options => options.WithIdentity(jobKey));

        // Si definisce quali sono i trigger che avviano il job.
        _ = q.AddTrigger(options => options
            .ForJob(jobKey)
            .WithIdentity($"{jobKey.Name} Trigger")
            .StartNow()

            // https://crontab.guru/
            .WithCronSchedule(configuration.GetValue($"Quartz:{nameof(StockQuoteQueryJob)}", "0/5 * * * * ?"))
        );
    });

    services.AddQuartzServer(options =>
    {
        // when shutting down we want jobs to complete gracefully
        options.WaitForJobsToComplete = true;
    });
}

async Task Init()
{
    using var scope = app.Services.CreateScope();
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StockMonitorContext>>();
    using var context = await contextFactory.CreateDbContextAsync();

    var subscriptions = await context.Subscriptions.ToListAsync();
    context.Subscriptions.RemoveRange(subscriptions);
    await context.SaveChangesAsync();
}