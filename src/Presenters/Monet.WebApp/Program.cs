using Monet.Application;
using Monet.Application.BankAccounts;
using Monet.Application.ImportTransactions;
using Monet.Application.Ports;
using Monet.Application.Transactions;
using Monet.Infrastructure.Projections.InMemory;
using Monet.WebApp.BankAccounts;
using Monet.WebApp.ImportTransactions;
using Monet.WebApp.Infrastructure;
using Monet.WebApp.Transactions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<InMemoryEventStore>();
builder.Services.AddSingleton<AccountBalanceProjection>();
builder.Services.AddSingleton<TransactionProjection>();
builder.Services.AddSingleton<IProjection>(serviceProvider => serviceProvider.GetRequiredService<AccountBalanceProjection>());
builder.Services.AddSingleton<IProjection>(serviceProvider => serviceProvider.GetRequiredService<TransactionProjection>());
builder.Services.AddSingleton<IProvideAccountBalances>(serviceProvider => serviceProvider.GetRequiredService<AccountBalanceProjection>());
builder.Services.AddSingleton<IProvideTransactions>(serviceProvider => serviceProvider.GetRequiredService<TransactionProjection>());
builder.Services.AddSingleton<IStoreEvent>(serviceProvider => new ProjectingEventStore(
    serviceProvider.GetRequiredService<InMemoryEventStore>(),
    [.. serviceProvider.GetServices<IProjection>()]));
builder.Services.AddSingleton<IAuthenticationGateway, FixedAuthenticationGateway>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IGenerateGuid, GuidGenerator>();
builder.Services.AddScoped<TransactionImportHandler>();
builder.Services.AddScoped<GetAccountBalancesQueryHandler>();
builder.Services.AddScoped<GetTransactionsQueryHandler>();
builder.Services.AddSingleton<ITransactionImportFileParser, OfxTransactionImportFileParser>();
builder.Services.AddSingleton<ITransactionImportFileParserSelector, TransactionImportFileParserSelector>();

builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "wwwroot";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseSpaStaticFiles();
}

app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/version", (IConfiguration configuration) =>
{
    var version = configuration["Application:Version"];
    if (string.IsNullOrWhiteSpace(version))
    {
        version = "Unknown";
    }

    return Results.Ok(new
    {
        version
    });
});

app.MapTransactionImportEndpoint();
app.MapAccountBalanceEndpoint();
app.MapTransactionsEndpoint();

if (app.Environment.IsDevelopment())
{
    app.MapWhen(
        context => !context.Request.Path.StartsWithSegments("/api"),
        spaApp =>
        {
            spaApp.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            });
        }
    );
}
else
{
    app.UseSpaStaticFiles();
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";
    });
}

app.Run();
