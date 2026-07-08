using System.Security.Claims;
using Ceplan.Application.Authentication;
using Ceplan.Application.Options;
using Ceplan.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using IAuthService = Ceplan.Application.Authentication.IAuthenticationService;

namespace Ceplan.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthService _auth;
    private readonly LockoutOptions _lockout;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService auth, LockoutOptions lockout, ILogger<AccountController> logger)
    {
        _auth = auth;
        _lockout = lockout;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Activation() => View();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(ProfileController.Index), "Profile");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new LoginRequest(model.DocumentType, model.DocumentNumber, model.Password);
        var result = await _auth.LoginAsync(request, cancellationToken);

        switch (result.Status)
        {
            case LoginStatus.Success:
                await SignInAsync(result.User!);
                _logger.LogInformation("Login exitoso para el documento {Doc}.", model.DocumentNumber);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(ProfileController.Index), "Profile");

            case LoginStatus.AccountLocked:
                _logger.LogWarning("Intento de login sobre cuenta bloqueada ({Doc}).", model.DocumentNumber);
                return View("Blocked", new BlockedViewModel(_lockout.MaxFailedAttempts, _lockout.LockoutMinutes));

            case LoginStatus.ValidationError:
                ModelState.AddModelError(string.Empty, result.Message ?? "Datos inválidos.");
                return View(model);

            case LoginStatus.InvalidCredentials:
            default:
                // Mensaje genérico: no revela si el usuario existe.
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Flash.Info"] = "Ha cerrado sesión correctamente.";
        return RedirectToAction(nameof(Login));
    }

    /// <summary>Refresca la sesión (cookie deslizante) al "Extender sesión". Llamado por el cliente.</summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public IActionResult KeepAlive() => NoContent();

    /// <summary>Expira la sesión por inactividad: cierra sesión y vuelve al login con aviso.</summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public async Task<IActionResult> SessionExpired()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Flash.Info"] = "Su sesión ha expirado debido a inactividad. Por favor, inicie sesión nuevamente.";
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("DocumentNumber", user.DocumentNumber)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });
    }
}
