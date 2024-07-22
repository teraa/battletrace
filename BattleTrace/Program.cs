using FluentValidation;
using Serilog;
using Teraa.Extensions.AspNetCore;
using BattleTrace.Features.Players;
using BattleTrace.Features.Servers;
using BattleTrace.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;
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
    .AddControllers(options =>
    {
        options.ModelValidatorProviders.Clear();
    })
    .Services
    .AddDb()
    .AddMediatR(config =>
    {
        config.RegisterServicesFromAssemblyContaining<Program>();
    })
    .AddRequestValidationBehaviour()
    .AddValidatorsFromAssemblyContaining<Program>()
    .AddMemoryCache()
    .AddHttpClient()
    .AddHttpContextAccessor()
    .AddPlayerFetcher()
    .AddServerFetcher()
    .AddHangfire((services, config) =>
    {
        config.UsePostgreSqlStorage(options =>
        {
            var dbOptions = services.GetRequiredService<IOptions<DbOptions>>().Value;
            options.UseNpgsqlConnection(dbOptions.ConnectionString);
        });
    })
    .AddHangfireServer();
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

app.MapHangfireDashboard(options: new DashboardOptions
{
    Authorization = [],
});

await app.InitAsync();
await app.RunAsync();
