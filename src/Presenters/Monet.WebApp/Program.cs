var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "ClientApp/dist/ClientApp";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseSpaStaticFiles();
}

app.UseStaticFiles();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        timestamp = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/version", (IConfiguration configuration) =>
{
    var version = configuration["APP_VERSION"];
    if (string.IsNullOrWhiteSpace(version))
    {
        version = "Unknown";
    }

    return Results.Ok(new
    {
        version
    });
});

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
