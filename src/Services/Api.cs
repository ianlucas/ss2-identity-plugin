/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;

namespace Identity;

public static class Api
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    private const int MaxRetries = 3;

    private const int RetryDelayMs = 100;

    public static bool IsActive() => ConVars.Url.Value.Contains("{userId}");

    public static async Task<User?> FetchUser(ulong steamId)
    {
        var url = ConVars.Url.Value.Replace("{userId}", steamId.ToString());
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
                Swiftly.Core.Logger.LogError(
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
