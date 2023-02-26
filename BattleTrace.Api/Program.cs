using BattleTrace.Api.Initializers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Teraa.Extensions.AspNetCore;
using Teraa.Extensions.Configuration;
using BattleTrace.Api.Options;
using BattleTrace.Api.Features.Players;
using BattleTrace.Api.Features.Servers;
using BattleTrace.Data;
using Teraa.Extensions.Configuration.Vault.Options;
using Teraa.Extensions.Serilog.Systemd;
using Teraa.Extensions.Serilog.Seq;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddVault();

builder.Host
    .UseDefaultServiceProvider(options =>
    {
        options.ValidateOnBuild = true;
        options.ValidateScopes = true;
    })
    .UseSystemd()
    .UseSerilog((hostContext, options) =>
    {
        options
            .ReadFrom.Configuration(hostContext.Configuration)
            .ConfigureSystemdConsole()
            .ConfigureSeq(hostContext);
    });

builder.Services
    .AddAsyncInitialization()
    .AddAsyncInitializer<MigrationInitializer>()
    .AddControllers(options =>
    {
        options.ModelValidatorProviders.Clear();
    })
    .Services
    .AddDbContext<AppDbContext>((services, options) =>
    {
        using var scope = services.CreateScope();
        var dbOptions = scope.ServiceProvider
            .GetRequiredService<IOptionsMonitor<DbOptions>>()
            .CurrentValue;

        options.UseSqlite(dbOptions.ConnectionString, contextOptions =>
        {
            contextOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
            contextOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

#if DEBUG
        options.EnableSensitiveDataLogging();
#endif
    })
    .AddMediatR(typeof(Program))
    .AddRequestValidationBehaviour()
    .AddValidatorsFromAssemblyContaining<Program>()
    .AddMemoryCache()
    .AddHttpClient()
    .AddHttpContextAccessor()
    .AddOptionsWithValidation<DbOptions>()
    .AddOptionsWithValidation<ServerFetcherOptions>()
    .AddHostedService<ServerFetcherService>()
    .AddSingleton<PlayerFetcherService>()
    .AddSingleton<IHostedService>(x => x.GetRequiredService<PlayerFetcherService>())
    ;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.InitAsync();
await app.RunAsync();
