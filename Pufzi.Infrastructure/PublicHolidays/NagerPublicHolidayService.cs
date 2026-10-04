using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Pufzi.Contracts.Requests.PublicHolidays;
using Pufzi.Contracts.Responses.PublicHolidays;

namespace Pufzi.Infrastructure.PublicHolidays;

public class NagerPublicHolidayService : IPublicHolidayService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public NagerPublicHolidayService(
        HttpClient httpClient,
        IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
    }

    public async Task<IReadOnlyCollection<PublicHolidayResponse>>
        GetPublicHolidaysAsync(
            PublicHolidaysFilterRequest request,
            CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var countryCode =
            request.CountryCode
                .Trim()
                .ToUpperInvariant();

        var cacheKey =
            $"public-holidays:{countryCode}:{request.Year}";

        if (_memoryCache.TryGetValue(
                cacheKey,
                out IReadOnlyCollection<PublicHolidayResponse>? cachedHolidays) &&
            cachedHolidays is not null)
        {
            return cachedHolidays;
        }

        using var response =
            await _httpClient.GetAsync(
                $"api/v4/Holidays/{countryCode}/{request.Year}",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"Nu au fost găsite sărbători legale pentru țara {countryCode} și anul {request.Year}.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Serviciul extern pentru sărbători legale nu este disponibil momentan.");
        }

        var externalHolidays =
            await response.Content
                .ReadFromJsonAsync<List<NagerPublicHolidayDto>>(
                    cancellationToken: cancellationToken);

        if (externalHolidays is null)
        {
            throw new InvalidOperationException(
                "Răspunsul primit de la serviciul de sărbători legale nu este valid.");
        }

        var holidays =
            externalHolidays
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Name) &&
                    !string.IsNullOrWhiteSpace(x.CountryCode))
                .OrderBy(x => x.Date)
                .Select(x =>
                    new PublicHolidayResponse
                    {
                        Date = x.Date,
                        Name = x.Name!.Trim(),
                        CountryCode =
                            x.CountryCode!
                                .Trim()
                                .ToUpperInvariant()
                    })
                .ToList();

        _memoryCache.Set(
            cacheKey,
            holidays,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromHours(24)
            });

        return holidays;
    }

    private static void ValidateRequest(
        PublicHolidaysFilterRequest request)
    {
        if (request.Year < 2000 ||
            request.Year > 2100)
        {
            throw new InvalidOperationException(
                "Anul trebuie să fie între 2000 și 2100.");
        }

        if (string.IsNullOrWhiteSpace(
                request.CountryCode))
        {
            throw new InvalidOperationException(
                "Codul țării este obligatoriu.");
        }

        var countryCode =
            request.CountryCode.Trim();

        if (countryCode.Length != 2 ||
            !countryCode.All(char.IsLetter))
        {
            throw new InvalidOperationException(
                "Codul țării trebuie să fie un cod ISO format din două litere.");
        }
    }
}