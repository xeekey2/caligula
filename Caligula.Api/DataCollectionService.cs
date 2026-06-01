using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

        private const int MatchHistoryPageSize = 20;

        public Task RunDailyMatchHistoryUpdateAsync() =>
            RunFullProMatchImportAsync();

        /// <summary>
        /// Imports all available 1v1 ladder matches for every pro in the SC2Pulse pro roster.
        /// </summary>
        public async Task RunFullProMatchImportAsync(
            CancellationToken cancellationToken = default,
            int? maxPros = null,
            int? onlyProPlayerId = null)
        {
            IReadOnlyList<Player> pros;
            if (onlyProPlayerId is > 0)
            {
                var single = await ProPlayerCatalog.GetByProPlayerIdAsync(_httpClient, onlyProPlayerId.Value);
                pros = single != null
                    ? new List<Player> { single }
                    : new List<Player>();
            }
            else
            {
                Console.WriteLine("Discovering pro players from SC2Pulse...");
                pros = await ProPlayerCatalog.DiscoverAsync(_httpClient);
                if (maxPros is > 0)
                    pros = pros.Take(maxPros.Value).ToList();
            }

            Console.WriteLine($"Found {pros.Count} pro players. Starting full match history import.");

            var totalSaved = 0;
            var totalSkipped = 0;

            foreach (var player in pros)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Console.WriteLine($"Collecting matches for {player.Name} (proPlayerId={player.ProPlayerId})...");

                var matchHistories = await GetAllMatchHistoriesAsync(player.ProPlayerId, cancellationToken);
                Console.WriteLine($"  Fetched {matchHistories.Count} unique matches from SC2Pulse.");

                var savedForPlayer = 0;
                var skippedForPlayer = 0;

                foreach (var match in matchHistories)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (await _dbContext.MatchExistsAsync(match.match.Id))
                    {
                        skippedForPlayer++;
                        continue;
                    }

                    try
                    {
                        if (await TryPersistMatchAsync(match, cancellationToken))
                            savedForPlayer++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Failed match {match.match.Id}: {ex.Message}");
                    }
                }

                totalSaved += savedForPlayer;
                totalSkipped += skippedForPlayer;
                Console.WriteLine($"  {player.Name}: saved {savedForPlayer} new, skipped {skippedForPlayer} existing.");
            }

            Console.WriteLine($"Import complete. Saved {totalSaved} new matches ({totalSkipped} already in DB).");
        }

        private async Task<bool> TryPersistMatchAsync(Result match, CancellationToken cancellationToken)
        {
            var winnerParticipant = match.participants?.FirstOrDefault(p => p.participant.decision == "WIN");
            var loserParticipant = match.participants?.FirstOrDefault(p => p.participant.decision != "WIN");
            if (winnerParticipant == null || loserParticipant == null)
                return false;

            var map = await _dbContext.EnsureMapExistsAsync(match.map.name);

            var participantPlayers = match.participants.ToPlayerList();
            if (participantPlayers.Count < 2)
            {
                Console.WriteLine($"  Skipping match {match.match.Id}: missing participant names in API payload.");
                return false;
            }
            foreach (var participantPlayer in participantPlayers)
            {
                if (participantPlayer == null)
                    continue;
                await _dbContext.EnsurePlayerExistsAsync(participantPlayer);
            }

            var participants = await match.participants.ToDbParticipantsAsync(participantPlayers, match);
            if (participants.Count == 0)
                return false;

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
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error saving match {match.match.Id}: {ex.Message}");
                return false;
            }
        }

        private async Task<List<Result>> GetAllMatchHistoriesAsync(
            int proPlayerId,
            CancellationToken cancellationToken)
        {
            var matchesById = new Dictionary<int, Result>();
            var cursor = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture);
            var page = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                page++;

                var encodedCursor = Uri.EscapeDataString(cursor);
                var response = await _httpClient.GetAsync(
                    $"/proplayer/matchhistory/{proPlayerId}/{encodedCursor}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    break;

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                List<Result>? batch;
                try
                {
                    batch = JsonConvert.DeserializeObject<List<Result>>(json);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"  Failed to parse match page {page}: {ex.Message}");
                    break;
                }

                if (batch == null || batch.Count == 0)
                    break;

                foreach (var result in batch)
                    matchesById[result.match.Id] = result;

                if (batch.Count < MatchHistoryPageSize)
                    break;

                var oldest = batch[^1].match.Date;
                if (oldest == default)
                    break;

                cursor = FormatMatchCursor(oldest);
            }

            return matchesById.Values.ToList();
        }

        private static string FormatMatchCursor(DateTime matchDate)
        {
            var utc = matchDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(matchDate, DateTimeKind.Utc)
                : matchDate.ToUniversalTime();

            return utc.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture);
        }
    }
}
