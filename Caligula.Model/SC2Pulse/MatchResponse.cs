using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caligula.Model.SC2Pulse
{

    public class MatchHistoryResponse
    {
        public Meta meta { get; set; }
        public Result[] result { get; set; }
    }

    public class Meta
    {
        public object totalCount { get; set; }
        public int perPage { get; set; }
        public object pageCount { get; set; }
        public int page { get; set; }
    }

    public class Result
    {
        public Match match { get; set; }
        public Map map { get; set; }
        public Participant[] participants { get; set; }
    }

    public class Match
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public int Id { get; set; }
        public int MapId { get; set; }
        public string Region { get; set; }
        public DateTime Updated { get; set; }
        public int? Duration { get; set; }
    }


    public class Map
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Participant
    {
        public Participant1 participant { get; set; }
        public MatchTeam team { get; set; }
        public string twitchVodUrl { get; set; }
        public bool? subOnlyTwitchVod { get; set; }
    }

    public class MatchTeam
    {
        public List<Member> members { get; set; }
    }

    public class Participant1
    {
        public int matchId { get; set; }
        public int playerCharacterId { get; set; }
        public string decision { get; set; }
        public int? ratingChange { get; set; }
    }

    public class Member
    {
        public int zergGamesPlayed { get; set; }
        public Character0 character { get; set; }
        public Account0 account { get; set; }
        public int protossGamesPlayed { get; set; }
        public int proId { get; set; }
        public string proNickname { get; set; }
        public string proTeam { get; set; }
        public int terranGamesPlayed { get; set; }
    }

    public class Character0
    {
        public int realm { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public int accountId { get; set; }
        public string region { get; set; }
        public int battlenetId { get; set; }
        public string tag { get; set; }
    }

    public class Account0
    {
        public string battleTag { get; set; }
        public int id { get; set; }
        public string partition { get; set; }
        public object hidden { get; set; }
    }

}




