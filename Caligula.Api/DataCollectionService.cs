using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Caligula.Model.Caligula;
using Caligula.Model.DBModels;
using Caligula.Model.SC2Pulse;
using Caligula.Service.Entity;
using Caligula.Service.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Caligula.Service
{
    public class DataCollectionService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _dbContext;

        public DataCollectionService(ApplicationDbContext dbContext, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
        }

        public async Task RunDailyMatchHistoryUpdateAsync()
        {
            List<string> sc2ProPlayers = new List<string>
            {
                //"Rotterdam",
                //"Iba",
                //"Fjant",
                //"Maru",
                //"Serral",
                //"Rogue",
                //"Reynor",
                //"Zest",
                //"Dark",
                //"Trap",
                //"TY",
                //"Stats",
                //"Bunny",
                //"Solar",
                //"Heromarine",
                //"ShowTime",
                //"Scarlett",
                //"Lambo",
                //"Elazer",
                //"Special",
                //"uThermal",
                //"Astrea",
                //"ByuN",
                //"Creator",
                //"DRG",
                //"Cure",
                //"Harstem",
                //"MaxPax",
                //"Cham",
                //"Nice",
                //"Has",
                //"Kelazhur",
                //"Cyan",
                //"Soo",
                //"Classic",
                //"Oliveira",
                //"Gumiho",
                //"Coffee",
                //"Spirit",
                //"Clem",
                "hero"
            };

            foreach (var playerName in sc2ProPlayers)
            {
                var player = await GetPlayerInfoAsync(playerName);
                if (player == null) continue;

                var matchHistories = await GetMatchHistoriesDailyAsync(player.ProPlayerId, DateTime.Now.AddMonths(-3), DateTime.Now);

                foreach (var match in matchHistories)
                {
                    if (await _dbContext.MatchExistsAsync(match.match.Id)) continue;

                    var winnerParticipant = match.participants.FirstOrDefault(p => p.participant.decision == "WIN");
                    var loserParticipant = match.participants.FirstOrDefault(p => p.participant.decision != "WIN");

                    if (winnerParticipant == null || loserParticipant == null) continue;

                    var map = await _dbContext.EnsureMapExistsAsync(match.map.name);

                    // Fetch and ensure all players exist using the extension method
                    var participantPlayers = await match.participants.ToPlayerListAsync();
                    foreach (var participantPlayer in participantPlayers)
                    {
                        if (participantPlayer == null)
                            continue;
                        await _dbContext.EnsurePlayerExistsAsync(participantPlayer);
                    }

                    List<DbParticipant> participants = await match.participants.ToDbParticipantsAsync(participantPlayers, match);
                    if (participants.Count == 0) continue;

                    var dbMatch = new DbMatch
                    {
                        MatchId = match.match.Id,
                        Date = match.match.Date,
                        WinnerId = winnerParticipant.participant.playerCharacterId,
                        LoserId = loserParticipant.participant.playerCharacterId,
                        Duration = match.match.Duration,
                        MapId = map.Id,
                        Participants = participants
                    };

                    _dbContext.Matches.Add(dbMatch);

                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"Match {match.match.Id} saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving match: {ex.Message}");
                        continue;
                    }
                }
            }
        }

        private async Task<Player> GetPlayerInfoAsync(string playerName)
        {
            var response = await _httpClient.GetAsync($"/playerid/{playerName}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var playerStats = JsonConvert.DeserializeObject<List<SearchResponse>>(jsonString);
                var selectedPlayer = playerStats?
                    .Where(p => p.currentStats != null && p.currentStats.rating != null)
                    .OrderByDescending(p => p.currentStats.rating)
                    .FirstOrDefault();

                if (selectedPlayer?.members != null)
                {
                    var member = selectedPlayer.members;
                    return new Player
                    {
                        Id = member.character.id,
                        Ids = await GetProPlayerIdsAsync(member.proId),
                        ProPlayerId = member.proId,
                        Name = member.proNickname
                    };
                }
            }

            return null;
        }

        private async Task<List<int>> GetProPlayerIdsAsync(int proPlayerId)
        {
            var response = await _httpClient.GetAsync($"/proplayer/ids/{proPlayerId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var accounts = JsonConvert.DeserializeObject<List<SearchResponse>>(json);
                return accounts.Select(x => x.members.account.id).ToList();
            }

            return new List<int>();
        }

        private async Task<List<Result>> GetMatchHistoriesDailyAsync(int playerId, DateTime endDate, DateTime startDate)
        {
            List<Result> matchHistories = new List<Result>();

            for (DateTime date = startDate; date >= endDate; date = date.AddDays(-1))
            {
                string dateString = date.ToString("yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture);
                string requestUrl = $"/proplayer/matchhistory/{playerId}/{dateString}";

                var response = await _httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<List<Result>>(json);

                    if (results?.Count > 0)
                    {
                        matchHistories.AddRange(results);
                    }
                }

            }

            return matchHistories;
        }
    }
}
