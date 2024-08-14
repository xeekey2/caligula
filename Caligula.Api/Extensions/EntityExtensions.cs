//using Caligula.Model.Caligula;
//using Caligula.Model.DBModels;
//using Caligula.Model.SC2Pulse;
//using Caligula.Service.Entity;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Map = Caligula.Model.Caligula.Map;

//namespace Caligula.Service.Extensions
//{
//    public static class EntityExtensions
//    {
//        public static async Task EnsurePlayerExistsAsync(this ApplicationDbContext context, Player player)
//        {
//            if (!await context.Players.AnyAsync(p => p.PlayerId == player.Id))
//            {
//                var playerName = !string.IsNullOrEmpty(player.Name) ? player.Name : "unknown";
//                if (playerName != null)
//                {
//                    var dbPlayer = new DbPlayer
//                    {
//                        PlayerId = player.Id,
//                        Name = playerName
//                    };

//                    try
//                    {
//                        context.Players.Add(dbPlayer);
//                        await context.SaveChangesAsync();
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"Error saving player: {ex.Message}");
//                        throw; // Rethrow to handle or log the error outside
//                    }
//                }
//            }
//        }

//        public static async Task<DbMap> EnsureMapExistsAsync(this ApplicationDbContext context, string mapName)
//        {
//            var map = await context.Maps.FirstOrDefaultAsync(m => m.Name == mapName);
//            if (map == null)
//            {
//                map = new DbMap { Name = mapName };
//                context.Maps.Add(map);
//                await context.SaveChangesAsync();
//            }
//            return map;
//        }

//        public static async Task<bool> MatchExistsAsync(this ApplicationDbContext context, int matchId)
//        {
//            return await context.Matches.AnyAsync(m => m.MatchId == matchId);
//        }

//        public static async Task<List<DbParticipant>> ToDbParticipantsAsync(this IEnumerable<Participant> participants, List<Player> participantPlayers)
//        {
//            var dbParticipants = new List<DbParticipant>();

//            foreach (var participant in participants)
//            {
//                var playerInfo = participantPlayers.FirstOrDefault(p => p.Id == participant.participant.playerCharacterId);
//                if (playerInfo == null)
//                {
//                    Console.WriteLine($"Player info not found for participant ID: {participant.participant.playerCharacterId}");
//                    continue;
//                }

//                dbParticipants.Add(new DbParticipant
//                {
//                    PlayerId = playerInfo.Id,
//                    Decision = participant.participant.decision,
//                    RatingChange = participant.participant.ratingChange
//                });
//                Console.WriteLine($"Participant added: PlayerId={playerInfo.Id}, Decision={participant.participant.decision}, RatingChange={participant.participant.ratingChange}");
//            }

//            return dbParticipants;
//        }

//        public static async Task<List<MatchObject>> ToCaliMatchesAsync(this IEnumerable<DbMatch> matches)
//        {
//            var tasks = matches.Select(match => match.ToCaliMatchAsync());
//            var matchObjects = await Task.WhenAll(tasks);
//            return matchObjects.ToList();
//        }

//        public static async Task<MatchObject> ToCaliMatchAsync(this DbMatch match)
//        {
//            var players = await match.Participants.ToPlayerListEntAsync();
//            var winnerName = await match.Participants.FirstOrDefault(x => x.Decision == "WIN")?.PlayerId.ToPlayerNameEnt();

//            return new MatchObject
//            {
//                Players = players,
//                Map = match.Map.ToMapEnt(),
//                Winner = winnerName,
//                Duration = match.Duration,
//                Date = match.Date,
//                Participants = match.Participants.Select(p => p.ToParticipant()).ToArray(),
//            };
//        }

//        public static async Task<List<Player>> ToPlayerListEntAsync(this IEnumerable<DbParticipant> participants)
//        {
//            var tasks = participants.Select(p => ToPlayerEntAsync(p)).ToList();
//            var players = await Task.WhenAll(tasks);
//            return players.ToList();
//        }

//        private static async Task<Player> ToPlayerEntAsync(DbParticipant participant)
//        {
//            var name = await participant.PlayerId.ToPlayerNameEnt();
//            return new Player
//            {
//                Id = participant.PlayerId,
//                Name = name
//            };
//        }

//        public static async Task<List<Player>> ToPlayerListEntAsync(this IEnumerable<Participant> participants)
//        {
//            var tasks = participants.Select(p => ToPlayerEntAsync(p.participant)).ToList();
//            var players = await Task.WhenAll(tasks);
//            return players.ToList();
//        }

//        private static async Task<Player> ToPlayerEntAsync(Participant1 participant)
//        {
//            var name = await participant.playerCharacterId.ToPlayerNameEnt();
//            return new Player
//            {
//                Id = participant.playerCharacterId,
//                Name = name
//            };
//        }

//        public static async Task<string> ToPlayerNameEnt(this int participantId)
//        {
//            SC2PulseWrapper sc2PulseWrapper = new SC2PulseWrapper("https://sc2pulse.nephest.com/sc2/");

//            var response = await sc2PulseWrapper.GetProPlayerName(participantId);
//            if (response.IsSuccessStatusCode)
//            {
//                var json = await response.Content.ReadAsStringAsync();
//                var playerData = JsonConvert.DeserializeObject<GroupResponse>(json);
//                var proPlayerName = playerData.characters.FirstOrDefault()?.members.proNickname;

//                if (!string.IsNullOrEmpty(proPlayerName))
//                {
//                    return proPlayerName;
//                }
//            }

//            // If the pro player name is empty or null, or if the request failed, try to get the normal player name
//            response = await sc2PulseWrapper.GetNameFromId(participantId);
//            if (response.IsSuccessStatusCode)
//            {
//                var json = await response.Content.ReadAsStringAsync();
//                var playerData = JsonConvert.DeserializeObject<List<PlayerDataResponse>>(json);
//                return playerData.FirstOrDefault()?.Name;
//            }

//            return null;
//        }

//        private static Map ToMapEnt(this DbMap dbMap)
//        {
//            return new Map
//            {
//                Id = dbMap.Id,
//                Name = dbMap.Name
//            };
//        }

//        private static Participant ToParticipant(this DbParticipant dbParticipant)
//        {
//            return new Participant
//            {
//                participant = new Participant1
//                {
//                    playerCharacterId = dbParticipant.PlayerId,
//                    decision = dbParticipant.Decision,
//                    ratingChange = dbParticipant.RatingChange ?? 0
//                }
//            };
//        }
//    }
//}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caligula.Model.Caligula;
using Caligula.Model.DBModels;
using Caligula.Model.SC2Pulse;
using Caligula.Service.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Map = Caligula.Model.Caligula.Map;

namespace Caligula.Service.Extensions
{
    public static class EntityExtensions
    {
        public static async Task EnsurePlayerExistsAsync(this ApplicationDbContext context, Player player)
        {
            if (!await context.Players.AnyAsync(p => p.PlayerId == player.Id))
            {
                var playerName = !string.IsNullOrEmpty(player.Name) ? player.Name : "unknown";
                var dbPlayer = new DbPlayer
                {
                    PlayerId = player.Id,
                    Name = playerName
                };

                try
                {
                    context.Players.Add(dbPlayer);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving player: {ex.Message}");
                    throw; // Rethrow to handle or log the error outside
                }
            }
        }

        public static async Task<DbMap> EnsureMapExistsAsync(this ApplicationDbContext context, string mapName)
        {
            var map = await context.Maps.FirstOrDefaultAsync(m => m.Name == mapName);
            if (map == null)
            {
                map = new DbMap { Name = mapName };
                context.Maps.Add(map);
                await context.SaveChangesAsync();
            }
            return map;
        }

        public static async Task<bool> MatchExistsAsync(this ApplicationDbContext context, int matchId)
        {
            return await context.Matches.AnyAsync(m => m.MatchId == matchId);
        }

        public static async Task<List<DbParticipant>> ToDbParticipantsAsync(this IEnumerable<Participant> participants, List<Player> participantPlayers, Result match)
        {
            var dbParticipants = new List<DbParticipant>();

            foreach (var participant in participants)
            {
                var playerInfo = participantPlayers.FirstOrDefault(p => p.Id == participant.participant.playerCharacterId);
                if (playerInfo == null)
                {
                    Console.WriteLine($"Player info not found for participant ID: {participant.participant.playerCharacterId}");
                    continue;
                }

                dbParticipants.Add(new DbParticipant
                {
                    PlayerId = playerInfo.Id,
                    Decision = participant.participant.decision,
                    RatingChange = participant.participant.ratingChange,
                    DbMatchId = match.match.Id
                });
                Console.WriteLine($"Participant added: PlayerId={playerInfo.Id}, Decision={participant.participant.decision}, RatingChange={participant.participant.ratingChange}");
            }

            return dbParticipants;
        }

        public static async Task<List<MatchObject>> ToCaliMatchesAsync(this IEnumerable<DbMatch> matches)
        {
            var tasks = matches.Select(match => match.ToCaliMatchAsync());
            var matchObjects = await Task.WhenAll(tasks);
            return matchObjects.ToList();
        }

        public static async Task<MatchObject> ToCaliMatchAsync(this DbMatch match)
        {
            var players = await match.Participants.ToPlayerListEntAsync();
            var winner = match.Participants.FirstOrDefault(x => x.Decision == "WIN");
            var loser = match.Participants.FirstOrDefault(x => x.Decision != "WIN");

            return new MatchObject
            {
                Players = players,
                Map = match.Map.ToMapEnt(),
                Winner = winner.DbPlayer.Name,
                Loser = loser.DbPlayer.Name,
                WinnerId = winner.DbPlayer.PlayerId,
                LoserId = loser.DbPlayer.PlayerId,
                Duration = match.Duration,
                Date = match.Date,
                Participants = match.Participants.Select(p => p.ToParticipant()).ToArray(),
            };
        }


        public static async Task<List<Player>> ToPlayerListEntAsync(this IEnumerable<DbParticipant> participants)
        {
            var tasks = participants.Select(p => ToPlayerEntAsync(p)).ToList();
            var players = await Task.WhenAll(tasks);
            return players.ToList();
        }

        private static async Task<Player> ToPlayerEntAsync(DbParticipant participant)
        {
            //var name = await participant.PlayerId.ToPlayerNameEnt();
            return new Player
            {
                Id = participant.PlayerId,
                Name = participant.DbPlayer.Name
            };
        }

        public static async Task<List<Player>> ToPlayerListEntAsync(this IEnumerable<Participant> participants)
        {
            var tasks = participants.Select(p => ToPlayerEntAsync(p.participant)).ToList();
            var players = await Task.WhenAll(tasks);
            return players.ToList();
        }

        private static async Task<Player> ToPlayerEntAsync(Participant1 participant)
        {
            var name = await participant.playerCharacterId.ToPlayerNameEnt();
            return new Player
            {
                Id = participant.playerCharacterId,
                Name = name
            };
        }

        public static async Task<string> ToPlayerNameEnt(this int participantId)
        {
            SC2PulseWrapper sc2PulseWrapper = new SC2PulseWrapper("https://sc2pulse.nephest.com/sc2/");

            var response = await sc2PulseWrapper.GetProPlayerName(participantId);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var playerData = JsonConvert.DeserializeObject<GroupResponse>(json);
                var proPlayerName = playerData.characters.FirstOrDefault()?.members.proNickname;

                if (!string.IsNullOrEmpty(proPlayerName))
                {
                    return proPlayerName;
                }
            }

            // If the pro player name is empty or null, or if the request failed, try to get the normal player name
            response = await sc2PulseWrapper.GetNameFromId(participantId);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var playerData = JsonConvert.DeserializeObject<List<PlayerDataResponse>>(json);
                return playerData.FirstOrDefault()?.Name;
            }

            return null;
        }

        private static Map ToMapEnt(this DbMap dbMap)
        {
            return new Map
            {
                Id = dbMap.Id,
                Name = dbMap.Name
            };
        }

        private static Participant ToParticipant(this DbParticipant dbParticipant)
        {
            return new Participant
            {
                participant = new Participant1
                {
                    playerCharacterId = dbParticipant.PlayerId,
                    decision = dbParticipant.Decision,
                    ratingChange = dbParticipant.RatingChange ?? 0
                }
            };
        }
    }
}
