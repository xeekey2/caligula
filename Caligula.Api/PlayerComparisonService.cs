using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caligula.Model.Caligula;
using Caligula.Model.DBModels;
using Caligula.Model.SC2Pulse;
using Caligula.Service.Entity;
using Caligula.Service.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Caligula.Service
{
    public class PlayerComparisonService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlayerComparisonService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MatchHistory> GetMatchHistoryForTwoPlayersAsync(string player1Name, string player2Name)
        {
            var player1Ids = await GetPlayerIdsAsync(player1Name);
            var player2Ids = await GetPlayerIdsAsync(player2Name);

            if (!player1Ids.Any() || !player2Ids.Any())
            {
                throw new Exception("One or both players not found.");
            }

            var commonMatches = await GetMatchesForPlayersAsync(player1Ids, player2Ids);
            var caligulaMatches = await commonMatches.ToCaliMatchesAsync();

            var history = new MatchHistory
            {
                CommonMatches = caligulaMatches.OrderByDescending(x => x.Date).ToList(),
                PlayerOne = new Player { Name = player1Name, Ids = player1Ids },
                PlayerTwo = new Player { Name = player2Name, Ids = player2Ids },
                PlayerOneTotalWins = caligulaMatches.Count(x => player1Ids.Contains(x.WinnerId)),
                PlayerTwoTotalWins = caligulaMatches.Count(x => player2Ids.Contains(x.WinnerId)),
                PlayerOneTotalLosses = caligulaMatches.Count(x => player1Ids.Contains(x.LoserId)),
                PlayerTwoTotalLosses = caligulaMatches.Count(x => player2Ids.Contains(x.LoserId)),
            };

            return history;
        }

        public async Task<MatchHistory> GetMatchHistoryForSinglePlayerAsync(string playerName)
        {
            var playerIds = await GetPlayerIdsAsync(playerName);

            if (!playerIds.Any())
            {
                throw new Exception("Player not found.");
            }

            var matches = await GetMatchesForPlayerAsync(playerIds);
            var caligulaMatches = await matches.ToCaliMatchesAsync();

            var history = new MatchHistory
            {
                CommonMatches = caligulaMatches.OrderByDescending(x => x.Date).ToList(),
                PlayerOne = new Player { Name = playerName, Ids = playerIds },
                PlayerOneTotalWins = caligulaMatches.Count(x => playerIds.Contains(x.WinnerId)),
                PlayerOneTotalLosses = caligulaMatches.Count(x => playerIds.Contains(x.LoserId)),
                TotalRatingChange = caligulaMatches.Sum(x => x.Participants
                    .Where(p => playerIds.Contains(p.participant.playerCharacterId))
                    .Sum(p => p.participant.ratingChange) ?? 0),
            };

            return history;
        }


        private async Task<List<int>> GetPlayerIdsAsync(string playerName)
        {
            return await _dbContext.Players
                .Where(p => p.Name == playerName)
                .Select(p => p.PlayerId)
                .ToListAsync();
        }

        private async Task<List<DbMatch>> GetMatchesForPlayerAsync(List<int> playerIds)
        {
            var matches = await _dbContext.Matches
                .Include(m => m.Participants)
                    .ThenInclude(p => p.DbPlayer)
                .Include(m => m.Map)
                .Where(m =>
                    m.Participants.Any(p => playerIds.Contains(p.PlayerId)) &&
                    m.Participants.All(p => !p.DbPlayer.Name.Contains("#") &&
                    m.Participants.Count == 2)
                )
                .ToListAsync();


            return matches;
       }


        private async Task<List<DbMatch>> GetMatchesForPlayersAsync(List<int> player1Ids, List<int> player2Ids)
        {
            // Fetch all matches that involve any of the player IDs
            var matches = await _dbContext.Matches
                .Include(m => m.Participants)
                    .ThenInclude(p => p.DbPlayer)
                .Include(m => m.Map)
                .Where(m => m.Participants.Any(p => player1Ids.Contains(p.PlayerId)) ||
                            m.Participants.Any(p => player2Ids.Contains(p.PlayerId)))
                .ToListAsync();

            // Filter matches to ensure both player1 and player2 are participants
            var commonMatches = matches
                .Where(m => m.Participants.Any(p => player1Ids.Contains(p.PlayerId)) &&
                            m.Participants.Any(p => player2Ids.Contains(p.PlayerId)) &&
                            m.Participants.All(p => !p.DbPlayer.Name.Contains("#") &&
                            m.Participants.Count == 2))
                .ToList();

            return commonMatches;
        }
    }
}
