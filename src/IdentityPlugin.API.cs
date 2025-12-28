/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace IdentityPlugin;

public partial class IdentityPlugin
{
    public async Task<User?> FetchUser(ulong steamId)
    {
        try
        {
            using HttpClient client = new();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await client.GetAsync(
                Url.Value.Replace("{userId}", steamId.ToString()),
                cts.Token
            );
            response.EnsureSuccessStatusCode();
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            User? data = JsonSerializer.Deserialize<User>(jsonContent);
            return data;
        }
        catch (Exception error)
        {
            Core.Logger.LogError(
                "Failed to fetch user (id: {Id}): {Message}",
                steamId,
                error.Message
            );
            return default;
        }
    }
}
