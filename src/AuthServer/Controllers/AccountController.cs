using System.Security.Claims;
using AuthServer.Db;
using AuthServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Controllers;

[Route("account")]
public class AccountController : Controller
{
    // GET /account/login pour afficher la page de login
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // filtre ValidateAntiForgeryToken pour valider côté serveur le token anti forgery injecté dans une razor view
    // cf https://learn.microsoft.com/fr-fr/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0#antiforgery-in-aspnet-core
    [HttpPost("login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        TestUser? user = TestUsers.Get(model.Email, model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "identifiants invalides");
            return View(model);
        }

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, Claims.Name, Claims.Role);
        identity.AddClaim(new Claim(Claims.Subject, user.Id));
        identity.AddClaim(new Claim(Claims.Name, user.Name));
        identity.AddClaim(new Claim(Claims.Email, user.Email));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Url.IsLocalUrl(model.ReturnUrl)
            ? Redirect(model.ReturnUrl)
            : Content("connecté, youpi !");
    }
}