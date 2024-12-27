using FluentValidation;
using Serilog;
using Teraa.Shared.AspNetCore;
using BattleTrace.Features.Players;
using BattleTrace.Features.Servers;
using BattleTrace.Data;
using BattleTrace.Hangfire;
using Teraa.Shared.Configuration.Vault;
using Teraa.Shared.Serilog.Systemd;
using Teraa.Shared.Serilog.Seq;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddVault();

builder.Host
    .UseDefaultServiceProvider(
        options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        }
    )
    .UseSystemd();


builder.Logging
    .ClearProviders()
    .AddSerilog(
        new LoggerConfiguration()
            .ConfigureDefaultLoggerConfiguration(builder.Configuration)
            .ConfigureSeq(builder.Configuration)
            .CreateLogger()
    );


builder.Services
    .AddAuthentication()
    .Services
    .AddAuthorization()
    .AddCors()
    .AddDb()
    .AddMediatR(
        config =>
        {
            config.RegisterServicesFromAssemblyContaining<Program>();
        }
    )
    .AddRequestValidationBehaviour()
    .AddValidatorsFromAssemblyContaining<Program>()
    .AddMemoryCache()
    .AddHttpClient()
    .AddHttpContextAccessor()
    .AddPlayerFetcher()
    .AddServerFetcher()
    .AddHangfire()
    .AddSingleton(TimeProvider.System)
    ;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
        }
    );
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPlayers();
app.MapServers();
app.MapHangfire();

await app.InitAsync();
await app.RunAsync();


// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;
