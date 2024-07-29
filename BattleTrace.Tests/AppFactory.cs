﻿using System.Text.RegularExpressions;
using BattleTrace.Data;
using BattleTrace.Features.Players;
using BattleTrace.Features.Servers;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace BattleTrace.Tests;

[Collection(AppFactoryFixture.CollectionName)]
public abstract class AppFactoryTests(AppFactory appFactory) : IAsyncLifetime
{
    protected readonly AppFactory _appFactory = appFactory;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _appFactory.ResetDatabaseAsync();
}

[CollectionDefinition(CollectionName)]
public class AppFactoryFixture : ICollectionFixture<AppFactory>
{
    public const string CollectionName = nameof(AppFactoryFixture);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class AppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly Regex s_allowedConnectionString =
        new(@"\bDatabase=\w+_tests\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Respawner? _respawner;

    public Mock<IBattlelogApi> BattlelogApiMock { get; } = new();
    public Mock<IKeeperBattlelogApi> KeeperBattlelogApiMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Test");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveService<PlayerFetcherJobInitializer>();
            services.RemoveService<ServerFetcherJobInitializer>();
            services.RemoveService<BackgroundJobServerHostedService>();
            services.RemoveService<IGlobalConfiguration>();
            services.RemoveService<IBattlelogApi>();
            services.AddTransient<IBattlelogApi>(_ => BattlelogApiMock.Object);
            services.RemoveService<IKeeperBattlelogApi>();
            services.AddTransient<IKeeperBattlelogApi>(_ => KeeperBattlelogApiMock.Object);
        });
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var options = Services.GetRequiredService<IOptions<DbOptions>>().Value;

        if (!s_allowedConnectionString.IsMatch(options.ConnectionString))
        {
            throw new InvalidOperationException(
                """Tests can only run on databases with a name ending with "_tests", please check your appsettings file."""
            );
        }

        await using var connection = new NpgsqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        _respawner ??= await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = [new Table("__EFMigrationsHistory")],
                SchemasToInclude = ["public"],
            }
        );

        await _respawner.ResetAsync(connection);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        // In case something else forgot to reset DB, this will clean after all tests are done.
        await ResetDatabaseAsync();
    }
}
