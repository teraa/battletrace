using BattleTrace.Api.Features.Servers;
using BattleTrace.Api.Initializers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Teraa.Extensions.AspNetCore;
using Teraa.Extensions.Configuration;
using BattleTrace.Api.Options;
using BattleTrace.Data;
using Microsoft.Extensions.Hosting.Systemd;
using Teraa.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

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
            .Enrich.FromLogContext();

        var seqOptions = hostContext.Configuration.GetOptions(new[] {new SeqOptions.Validator()});
        if (seqOptions is { })
        {
            options.WriteTo.Seq(seqOptions.ServerUrl.ToString(), apiKey: seqOptions.ApiKey);
        }

        if (SystemdHelpers.IsSystemdService())
        {
            Serilog.Debugging.SelfLog
                .Enable(x => Console.WriteLine($"<4>SERILOG: {x}"));

            options
                .Enrich.With(new SyslogSeverityEnricher())
                .WriteTo.Console(outputTemplate: "<{SyslogSeverity}>{SourceContext}: {Message:j}{NewLine}");
        }
        else
        {
            Serilog.Debugging.SelfLog
                .Enable(x => Console.WriteLine($"SERILOG: {x}"));

            options.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
        }
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
    .AddOptionsWithValidation<FetcherOptions>()
    .AddHostedService<FetcherService>()
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
