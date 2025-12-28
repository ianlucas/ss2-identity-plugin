/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Convars;

namespace IdentityPlugin;

public class Api
{
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 100;

    private static readonly HttpClient _httpClient = new();
    private static ILogger? _logger;
    private static IConVar<string>? _url;

    public static void Initialize(ISwiftlyCore core, IConVar<string> url)
    {
        _logger = core.Logger;
        _url = url;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public static string GetUrl(string pathname = "")
    {
        if (_url == null)
            throw new InvalidOperationException("API not initialized. Call Initialize() first.");
        return $"{_url.Value}{pathname}";
    }

    public static async Task<User?> FetchUser(ulong steamId)
    {
        var url = GetUrl().Replace("{userId}", steamId.ToString());
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(jsonContent);
            }
            catch (Exception error)
            {
                _logger?.LogError(
                    "GET {Url} failed (attempt {Attempt}/{MaxRetries}): {Message}",
                    url,
                    attempt,
                    MaxRetries,
                    error.Message
                );
                if (attempt == MaxRetries)
                    return null;
                await Task.Delay(TimeSpan.FromMilliseconds(RetryDelayMs * attempt));
            }
        return null;
    }
}
