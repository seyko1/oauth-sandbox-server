using AuthServer.Db;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
   options.UseInMemoryDatabase("auth-server");
   options.UseOpenIddict(); 
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();        
    })
    .AddServer(options =>
    {
        options
            .SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("connect/token")
            .SetUserInfoEndpointUris("connect/userinfo")
            .SetEndSessionEndpointUris("connect/endsession");
        
        options
            .AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow()
            .AllowClientCredentialsFlow();

        options.RequireProofKeyForCodeExchange();

        options.RegisterScopes(
            Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.OfflineAccess,
            "orders.read", "stock.read"
        );

        // certificats générés localement pour le dev
        options
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        
        // access token en jwt signé non chiffré pour pouvoir le décoder en dev
        options.DisableAccessTokenEncryption();

        // laisser openiddict valider les requêtes et laisser la main au controller pour le reste
        options
            .UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough();
    });

builder.Services.AddHostedService<OpenIddictSeeder>();

builder.Services.AddControllers();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/account/login";
        options.ExpireTimeSpan    = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
