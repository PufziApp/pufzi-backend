
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.BusinessReviews;
using Pufzi.Services.BusinessReviews;

namespace Pufzi.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class BusinessReviewsController : ControllerBase
{
    private readonly IBusinessReviewService _businessReviewService;

    public BusinessReviewsController(
        IBusinessReviewService businessReviewService)
    {
        _businessReviewService = businessReviewService;
    }

    /// <summary>
    /// Creează o recenzie pentru o rezervare finalizată.
    /// Doar clientul care a efectuat rezervarea poate publica recenzia.
    /// Fiecare rezervare poate avea o singură recenzie.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBusinessReviewRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessReviewService.CreateReviewAsync(
                GetUserId(),
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Modifică ratingul și comentariul unei recenzii existente.
    /// Operația este disponibilă oricând, doar autorului recenziei.
    /// </summary>
    [Authorize]
    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> Update(
        Guid reviewId,
        [FromBody] UpdateBusinessReviewRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessReviewService.UpdateReviewAsync(
                GetUserId(),
                reviewId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Șterge definitiv o recenzie publicată de utilizatorul autentificat.
    /// Autorul poate șterge recenzia oricând.
    /// </summary>
    [Authorize]
    [HttpDelete("{reviewId:guid}")]
    public async Task<IActionResult> Delete(
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _businessReviewService.DeleteReviewAsync(
            GetUserId(),
            reviewId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returnează recenziile publicate de utilizatorul autentificat.
    /// Permite filtrarea după rating și existența unui răspuns,
    /// precum și sortarea și paginarea rezultatelor.
    /// Recenziile proprii sunt accesibile indiferent dacă salonul este publicat.
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews(
        [FromQuery] BusinessReviewQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessReviewService.GetMyReviewsAsync(
                GetUserId(),
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează recenziile unui salon, cu filtrare, sortare și paginare.
    /// Recenziile saloanelor publicate sunt accesibile tuturor.
    /// Pentru saloanele nepublicate, accesul este permis doar
    /// proprietarului sau recepționerului activ al salonului.
    /// Accesul neautorizat nu dezvăluie existența salonului.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetBusinessReviews(
        Guid businessId,
        [FromQuery] BusinessReviewQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessReviewService.GetBusinessReviewsAsync(
                GetOptionalUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează ratingul mediu și distribuția recenziilor unui salon.
    /// Include numărul total de recenzii și numărul evaluărilor
    /// pentru fiecare categorie de la 1 la 5 stele.
    /// Statisticile saloanelor publicate sunt accesibile tuturor.
    /// Pentru saloanele nepublicate, accesul este permis doar
    /// proprietarului sau recepționerului activ al salonului.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("business/{businessId:guid}/summary")]
    public async Task<IActionResult> GetBusinessReviewSummary(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessReviewService.GetBusinessReviewSummaryAsync(
                GetOptionalUserId(),
                businessId,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Adaugă sau modifică răspunsul salonului la o recenzie.
    /// Operația este permisă doar proprietarului sau recepționerului
    /// activ al salonului.
    /// Fiecare recenzie poate avea un singur răspuns din partea salonului.
    /// </summary>
    [Authorize]
    [HttpPut("{reviewId:guid}/reply")]
    public async Task<IActionResult> Reply(
        Guid reviewId,
        [FromBody] BusinessReviewReplyRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _businessReviewService.ReplyToReviewAsync(
                GetUserId(),
                reviewId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Șterge răspunsul salonului la o recenzie.
    /// Operația este permisă doar proprietarului sau recepționerului
    /// activ al salonului.
    /// Recenzia clientului rămâne neschimbată.
    /// </summary>
    [Authorize]
    [HttpDelete("{reviewId:guid}/reply")]
    public async Task<IActionResult> DeleteReply(
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _businessReviewService.DeleteReviewReplyAsync(
            GetUserId(),
            reviewId,
            cancellationToken);

        return NoContent();
    }

    private Guid GetUserId()
    {
        var value =
            User.FindFirst(
                JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(value, out var userId) ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Utilizator neautentificat.");
        }

        return userId;
    }

    private Guid? GetOptionalUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        var value =
            User.FindFirst(
                JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(value, out var userId) ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Utilizator neautentificat.");
        }

        return userId;
    }
}
