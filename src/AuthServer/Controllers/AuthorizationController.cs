using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Controllers;

public class AuthorizationController(IOpenIddictScopeManager scopeManager): Controller
{
    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("La requête OpenID Connect est introuvable");
        
        if (request.IsClientCredentialsGrantType())
        {
            // en mode client credentials, le subject est directement le client (pas de user à identifier)
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            // claim audience
            identity
                .SetClaim(Claims.Subject, request.ClientId)
                .SetClaim(Claims.Name, request.ClientId);
            
            identity.SetScopes(request.GetScopes());

            // claim subject
            identity.SetResources(await GetResourcesAsync(request.GetScopes()));

            identity.SetDestinations(static _ => [Destinations.AccessToken]);
            
            // déclencher la génération du jwt token
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("Ce type de grant n'est pas encore géré");
    }

    private async Task<List<string>> GetResourcesAsync(IEnumerable<string> scopes)
    {
        var resources = new List<string>();

        await foreach (var resource in scopeManager.ListResourcesAsync([..scopes]))
        {
            resources.Add(resource);
        }

        return resources;
    }    
}