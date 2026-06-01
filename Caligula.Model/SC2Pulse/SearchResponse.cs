namespace Caligula.Model.SC2Pulse
{

    public class SearchResponse
    {
        public int? leagueMax { get; set; }
        public int? ratingMax { get; set; }
        public int? totalGamesPlayed { get; set; }
        public Previousstats previousStats { get; set; }
        public Currentstats currentStats { get; set; }
        public Members members { get; set; }
    }

    public class Previousstats
    {
        public object rating { get; set; }
        public object gamesPlayed { get; set; }
        public object rank { get; set; }
    }

    public class Currentstats
    {
        public object rating { get; set; }
        public object gamesPlayed { get; set; }
        public object rank { get; set; }
    }

    public class Members
    {
        public int terranGamesPlayed { get; set; }
        public Character character { get; set; }
        public Account account { get; set; }
        public int zergGamesPlayed { get; set; }
        public int proId { get; set; }
        public string proNickname { get; set; }
        public string proTeam { get; set; }
    }

    public class Character
    {
        public int realm { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public int accountId { get; set; }
        public string region { get; set; }
        public int battlenetId { get; set; }
    }

    public class Account
    {
        public string battleTag { get; set; }
        public int id { get; set; }
        public string partition { get; set; }
        public object hidden { get; set; }
    }

}