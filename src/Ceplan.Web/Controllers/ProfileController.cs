using System.Security.Claims;
using Ceplan.Application.Abstractions;
using Ceplan.Application.Profile;
using Ceplan.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ceplan.Web.Controllers;

[Authorize]
public sealed class ProfileController : Controller
{
    private readonly IUserRepository _users;
    private readonly IProfileImageService _profileImage;

    public ProfileController(IUserRepository users, IProfileImageService profileImage)
    {
        _users = users;
        _profileImage = profileImage;
    }

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

    /// <summary>Carga o actualiza la foto de perfil del usuario autenticado (extra).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile? avatar, CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var id))
        {
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        if (avatar is null || avatar.Length == 0)
        {
            TempData["Flash.Error"] = "Seleccione una imagen.";
            return RedirectToAction(nameof(Index));
        }

        await using var stream = avatar.OpenReadStream();
        var upload = new AvatarUpload(stream, avatar.FileName, avatar.Length, avatar.ContentType);
        var result = await _profileImage.UpdateAsync(id, upload, cancellationToken);

        if (result.IsSuccess)
        {
            TempData["Flash.Success"] = "Foto de perfil actualizada.";
        }
        else
        {
            TempData["Flash.Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }
}
