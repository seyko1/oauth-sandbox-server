using AuthServer.Db;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Services;

public class OpenIddictSeeder(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        var appManager   = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Config scopes custom

        if (await scopeManager.FindByNameAsync("orders.read", cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "orders.read",
                DisplayName = "Consulter les commandes",
                Resources = { "drive-api" }
            }, cancellationToken);
        }

        if (await scopeManager.FindByNameAsync("stock.read", cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "stock.read",
                DisplayName = "Consulter le stock",
                Resources = { "stock-api" }
            }, cancellationToken);
        }

        // Config client public

        if (await appManager.FindByClientIdAsync("drive-spa", cancellationToken) is null)
        {
            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "drive-spa",
                ClientType = ClientTypes.Public,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "Drive Spa",
                RedirectUris =
                {
                    new Uri("http://localhost:3000/callback"),
                    new Uri("https://oauth.pstmn.io/v1/callback")
                },
                PostLogoutRedirectUris = { new Uri("http://localhost:3000") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Email,
                    Permissions.Prefixes.Scope + Scopes.OfflineAccess,
                    Permissions.Prefixes.Scope + "orders.read"
                },
                Requirements = { Requirements.Features.ProofKeyForCodeExchange }
            }, cancellationToken);
        }

        // config client privé

        if (await appManager.FindByClientIdAsync("service-stock", cancellationToken) is null)
        {
            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "service-stock",
                ClientSecret = "pwet",
                ClientType = ClientTypes.Confidential,
                DisplayName = "Service Stock",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "stock.read"
                }
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}