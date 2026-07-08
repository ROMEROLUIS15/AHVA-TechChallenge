using System.Security.Claims;
using Ceplan.Application.Abstractions;
using Ceplan.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ceplan.Web.Controllers;

[Authorize]
public sealed class ProfileController : Controller
{
    private readonly IUserRepository _users;

    public ProfileController(IUserRepository users) => _users = users;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var id))
        {
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        return View(ProfileViewModel.FromUser(user));
    }
}
