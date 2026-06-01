using Caligula.Model.Caligula;
using Caligula.Model.SC2Pulse;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caligula.Service.Extensions
{
    public static class ParticipantExtensions
    {
        public static Task<List<Player>> ToPlayerListAsync(this IEnumerable<Participant> participants) =>
            Task.FromResult(participants.ToPlayerList());

        public static List<Player> ToPlayerList(this IEnumerable<Participant> participants) =>
            participants.Select(ToPlayer).Where(p => p != null).ToList()!;

        public static Player? ToPlayer(this Participant entry)
        {
            var participant = entry.participant;
            if (participant == null)
                return null;

            var name = entry.ResolveDisplayName();
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return new Player
            {
                Id = participant.playerCharacterId,
                Name = name
            };
        }

        public static string? ResolveDisplayName(this Participant entry)
        {
            var member = entry.team?.members?
                .FirstOrDefault(m => m.character?.id == entry.participant.playerCharacterId)
                ?? entry.team?.members?.FirstOrDefault();

            if (member != null)
            {
                if (!string.IsNullOrWhiteSpace(member.proNickname))
                    return member.proNickname;

                if (!string.IsNullOrWhiteSpace(member.character?.name))
                    return member.character.name;

                if (!string.IsNullOrWhiteSpace(member.character?.tag))
                    return member.character.tag;
            }

            return null;
        }

        public static async Task<string> ToPlayerName(this int participantId)
        {
            SC2PulseWrapper sc2PulseWrapper = new SC2PulseWrapper("https://sc2pulse.nephest.com/sc2/");
            var response = await sc2PulseWrapper.GetProPlayerName(participantId);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var playerData = JsonConvert.DeserializeObject<GroupResponse>(json);
                var proPlayerName = playerData?.characters?.FirstOrDefault()?.members?.proNickname;

                if (!string.IsNullOrEmpty(proPlayerName))
                    return proPlayerName;
            }

            response = await sc2PulseWrapper.GetNameFromId(participantId);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var playerData = JsonConvert.DeserializeObject<List<PlayerDataResponse>>(json);
                return playerData?.FirstOrDefault()?.Name;
            }

            return null;
        }
    }
}
