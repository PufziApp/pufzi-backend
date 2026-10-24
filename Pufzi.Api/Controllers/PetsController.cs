using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Pets;
using Pufzi.Services.Pets;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pets")]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;

    public PetsController(IPetService petService)
    {
        _petService = petService;
    }

    /// <summary>
    /// Returnează animalele utilizatorului autentificat.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] bool archived = false,
        CancellationToken cancellationToken = default)
    {
        var response = await _petService.GetMineAsync(
            GetUserId(),
            pagination,
            archived,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează profilul unui animal.
    /// </summary>
    [HttpGet("{petId:guid}")]
    public async Task<IActionResult> GetById(
        Guid petId,
        CancellationToken cancellationToken)
    {
        var response = await _petService.GetByIdAsync(
            GetUserId(),
            petId,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Creează un animal pentru utilizatorul autentificat.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] SavePetRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _petService.CreateAsync(
            GetUserId(),
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { petId = response.Id },
            response);
    }

    /// <summary>
    /// Actualizează informațiile unui animal.
    /// </summary>
    [HttpPut("{petId:guid}")]
    public async Task<IActionResult> Update(
        Guid petId,
        [FromBody] SavePetRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _petService.UpdateAsync(
            GetUserId(),
            petId,
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Arhivează animalul fără ștergerea istoricului.
    /// </summary>
    [HttpDelete("{petId:guid}")]
    public async Task<IActionResult> Archive(
        Guid petId,
        CancellationToken cancellationToken)
    {
        await _petService.ArchiveAsync(
            GetUserId(),
            petId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Reactivează un animal arhivat.
    /// </summary>
    [HttpPost("{petId:guid}/restore")]
    public async Task<IActionResult> Restore(
        Guid petId,
        CancellationToken cancellationToken)
    {
        await _petService.RestoreAsync(
            GetUserId(),
            petId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Încarcă sau înlocuiește fotografia de profil a animalului.
    /// Acceptă JPEG, PNG și WEBP, maximum 5 MB.
    /// </summary>
    [HttpPost("{petId:guid}/photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfileImage(
        Guid petId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ValidateImage(file, 5 * 1024 * 1024);

        await using var stream = file.OpenReadStream();

        var response = await _petService.UploadProfileImageAsync(
            GetUserId(),
            petId,
            stream,
            file.ContentType,
            GetExtension(file),
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Șterge fotografia de profil a animalului.
    /// </summary>
    [HttpDelete("{petId:guid}/photo")]
    public async Task<IActionResult> DeleteProfileImage(
        Guid petId,
        CancellationToken cancellationToken)
    {
        await _petService.DeleteProfileImageAsync(
            GetUserId(),
            petId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Adaugă o fotografie în galeria animalului.
    /// Acceptă JPEG, PNG și WEBP, maximum 10 MB.
    /// </summary>
    [HttpPost("{petId:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> AddImage(
        Guid petId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ValidateImage(file, 10 * 1024 * 1024);

        await using var stream = file.OpenReadStream();

        var response = await _petService.AddImageAsync(
            GetUserId(),
            petId,
            stream,
            file.ContentType,
            GetExtension(file),
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Șterge o fotografie din galeria animalului.
    /// </summary>
    [HttpDelete("{petId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(
        Guid petId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await _petService.DeleteImageAsync(
            GetUserId(),
            petId,
            imageId,
            cancellationToken);

        return NoContent();
    }

    private static void ValidateImage(IFormFile file, long maxBytes)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException(
                "Fișierul este gol.");
        }

        if (file.Length > maxBytes)
        {
            throw new InvalidOperationException(
                "Imaginea depășește dimensiunea maximă permisă.");
        }

        if (file.ContentType.ToLowerInvariant() is not
            ("image/jpeg" or "image/png" or "image/webp"))
        {
            throw new InvalidOperationException(
                "Sunt permise doar imagini JPEG, PNG și WEBP.");
        }
    }

    private static string GetExtension(IFormFile file)
    {
        return file.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException(
                "Tip de imagine invalid.")
        };
    }

    private Guid GetUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid authenticated user.");
        }

        return userId;
    }
}