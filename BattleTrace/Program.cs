using BattleTrace;
using FluentValidation;
using Serilog;
using Teraa.Extensions.AspNetCore;
using BattleTrace.Features.Players;
using BattleTrace.Features.Servers;
using BattleTrace.Data;
using MediatR;
using Teraa.Extensions.Configuration.Vault.Options;
using Teraa.Extensions.Serilog.Systemd;
using Teraa.Extensions.Serilog.Seq;
using IndexPlayers = BattleTrace.Features.Servers.Actions.Index;
using IndexServers = BattleTrace.Features.Servers.Actions.Index;

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
    .AddControllers(options => { options.ModelValidatorProviders.Clear(); })
    .Services
    .AddDb()
    .AddMediatR(config => { config.RegisterServicesFromAssemblyContaining<Program>(); })
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour2<,>))
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

app.MapGet(
    "/players",
    async ([AsParameters] IndexPlayers.Query query, ISender sender, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken)
);

app.MapGet(
    "/servers",
    async ([AsParameters] IndexServers.Query query, ISender sender, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken)
);

app.MapHangfire();

await app.InitAsync();
await app.RunAsync();


// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;
