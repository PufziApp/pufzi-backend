using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.Businesses;
using Pufzi.Services.Businesses;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/businesses")]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessesController(
        IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessService.CreateAsync(
                GetUserId(),
                request,
                cancellationToken);

        return Ok(response);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(
        CancellationToken cancellationToken)
    {
        var response =
            await _businessService.GetMineAsync(
                GetUserId(),
                cancellationToken);

        return Ok(response);
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetById(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessService.GetByIdAsync(
                GetUserId(),
                businessId,
                cancellationToken);

        return Ok(response);
    }

    [HttpPut("{businessId:guid}")]
    public async Task<IActionResult> Update(
        Guid businessId,
        [FromBody] UpdateBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessService.UpdateAsync(
                GetUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    [HttpPost("{businessId:guid}/logo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadLogo(
        Guid businessId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ValidateImage(file);

        await using var stream =
            file.OpenReadStream();

        var response =
            await _businessService.UploadLogoAsync(
                GetUserId(),
                businessId,
                stream,
                file.ContentType,
                GetExtension(file),
                cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{businessId:guid}/logo")]
    public async Task<IActionResult> DeleteLogo(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        await _businessService.DeleteLogoAsync(
            GetUserId(),
            businessId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{businessId:guid}/cover")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadCover(
        Guid businessId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ValidateImage(file);

        await using var stream =
            file.OpenReadStream();

        var response =
            await _businessService.UploadCoverAsync(
                GetUserId(),
                businessId,
                stream,
                file.ContentType,
                GetExtension(file),
                cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{businessId:guid}/cover")]
    public async Task<IActionResult> DeleteCover(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        await _businessService.DeleteCoverAsync(
            GetUserId(),
            businessId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{businessId:guid}/images")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> AddImage(
        Guid businessId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ValidateImage(file);

        await using var stream =
            file.OpenReadStream();

        var response =
            await _businessService.AddImageAsync(
                GetUserId(),
                businessId,
                stream,
                file.ContentType,
                GetExtension(file),
                cancellationToken);

        return Ok(response);
    }

    [HttpDelete(
        "{businessId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(
        Guid businessId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await _businessService.DeleteImageAsync(
            GetUserId(),
            businessId,
            imageId,
            cancellationToken);

        return NoContent();
    }

    private Guid GetUserId()
    {
        var value =
            User.FindFirst(
                JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Utilizator neautentificat.");
        }

        return userId;
    }

    private static void ValidateImage(
        IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException(
                "Fișierul este gol.");
        }

        var allowedContentTypes =
            new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

        if (!allowedContentTypes.Contains(
                file.ContentType.ToLowerInvariant()))
        {
            throw new InvalidOperationException(
                "Sunt permise doar imagini JPEG, PNG sau WEBP.");
        }
    }

    private static string GetExtension(
        IFormFile file)
    {
        return file.ContentType.ToLowerInvariant()
            switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",

            _ => throw new InvalidOperationException(
                "Tip de imagine invalid.")
        };
    }
}