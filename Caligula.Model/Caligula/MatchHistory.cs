using Caligula.Model.SC2Pulse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caligula.Model.Caligula
{
    public class MatchHistory
    {
        public List<MatchObject> CommonMatches { get; set; }
        public Player PlayerOne { get; set; }
        public Player PlayerTwo { get; set; }
        public int PlayerOneTotalWins { get; set; }
        public int PlayerTwoTotalWins { get; set; }
        public int PlayerOneTotalLosses { get; set; }
        public int PlayerTwoTotalLosses { get; set; }
        public List<int> LastFiveResults { get; set; }
        public int TotalRatingChange { get; set; }  
    }


    public class MatchObject
    {
        public int Id { get; set; }
        public List<Player> Players { get; set; }
        public Map Map { get; set; }
        public Participant[] Participants { get; set; }
        public string Winner { get; set; }
        public string Loser { get; set; }
        public int WinnerId { get; set; }  // Add WinnerId
        public int LoserId { get; set; }   // Add LoserId
        public int? Duration { get; set; }
        public DateTime Date { get; set; }
    }


    public class Player
    {
        public List<int> Ids { get; set; }
        public int Id { get; set; }
        public int ProPlayerId { get; set; }
        public string Name { get; set; }
    }

    public class Map
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
