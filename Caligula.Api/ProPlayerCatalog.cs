using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Caligula.Model.Caligula;
using Newtonsoft.Json;
using Caligula.Model.SC2Pulse;

namespace Caligula.Service
{
    /// <summary>
    /// Discovers pro players from SC2Pulse by scanning sequential proPlayerId values.
    /// </summary>
    public static class ProPlayerCatalog
    {
        public const int DefaultMaxIdToScan = 650;
        public const int DefaultConsecutiveMissLimit = 40;

        public static Task<Player?> GetByProPlayerIdAsync(HttpClient httpClient, int proPlayerId) =>
            TryGetProPlayerAsync(httpClient, proPlayerId);

        public static async Task<IReadOnlyList<Player>> DiscoverAsync(
            HttpClient httpClient,
            int maxIdToScan = DefaultMaxIdToScan,
            int consecutiveMissLimit = DefaultConsecutiveMissLimit)
        {
            var byProId = new Dictionary<int, Player>();
            var consecutiveMisses = 0;

            for (var proPlayerId = 1; proPlayerId <= maxIdToScan && consecutiveMisses < consecutiveMissLimit; proPlayerId++)
            {
                var player = await TryGetProPlayerAsync(httpClient, proPlayerId);
                if (player == null)
                {
                    consecutiveMisses++;
                    continue;
                }

                consecutiveMisses = 0;
                byProId[player.ProPlayerId] = player;
            }

            return byProId.Values
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static async Task<Player?> TryGetProPlayerAsync(HttpClient httpClient, int proPlayerId)
        {
            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync($"/proplayer/ids/{proPlayerId}");
            }
            catch (HttpRequestException)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            List<SearchResponse>? accounts;
            try
            {
                accounts = JsonConvert.DeserializeObject<List<SearchResponse>>(json);
            }
            catch (JsonException)
            {
                return null;
            }

            var member = accounts?
                .Select(a => a.members)
                .FirstOrDefault(m => m != null && !string.IsNullOrWhiteSpace(m.proNickname));

            if (member == null)
                return null;

            return new Player
            {
                Id = member.character.id,
                ProPlayerId = proPlayerId,
                Name = member.proNickname,
                Ids = accounts
                    .Where(a => a.members?.account != null)
                    .Select(a => a.members.account.id)
                    .Distinct()
                    .ToList()
            };
        }
    }
}
