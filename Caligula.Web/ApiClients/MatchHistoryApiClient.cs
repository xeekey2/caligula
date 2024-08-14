using Caligula.Service;
using Caligula.Model.Caligula;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Caligula.Model.SC2Pulse;
using Newtonsoft.Json;
using System.Net.Http;
using Caligula.Service.Entity;

namespace Caligula.Web.ApiClients
{
    public class MatchHistoryApiClient
    {
        private readonly DataCollectionService _dataCollectionService;
        private readonly PlayerComparisonService _playerComparisonService;
        private readonly ILogger<MatchHistoryApiClient> _logger;
        private readonly HttpClient _httpClient;

        public MatchHistoryApiClient(DataCollectionService dataCollectionService, PlayerComparisonService playerComparisonService, ILogger<MatchHistoryApiClient> logger, HttpClient httpClient)
        {
            _dataCollectionService = dataCollectionService;
            _playerComparisonService = playerComparisonService;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task RunDailyMatchHistoryUpdateAsync()
        {
            try
            {
                await _dataCollectionService.RunDailyMatchHistoryUpdateAsync();
                var t = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during match history update: {ex.Message}");
            }
        }

        public async Task<MatchHistory> GetMatchHistoryForTwoPlayersAsync(string player1Name, string player2Name)
        {
            try
            {
                return await _playerComparisonService.GetMatchHistoryForTwoPlayersAsync(player1Name, player2Name);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during player comparison: {ex.Message}");
                return null;
            }
        }

        public async Task<MatchHistory> GetMatchHistoryForOnePlayerAsync(string player1Name)
        {
            try
            {
                return await _playerComparisonService.GetMatchHistoryForSinglePlayerAsync(player1Name);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during player comparison: {ex.Message}");
                return null;
            }
        }


        public async Task<string> GetProPlayerNameAsync(int playerCharacterId)
        {
            var response = await _httpClient.GetAsync($"/proplayername/{playerCharacterId}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var playerData = JsonConvert.DeserializeObject<GroupResponse>(jsonString);
                return playerData?.characters.FirstOrDefault()?.members.proNickname;
            }

            response = await _httpClient.GetAsync($"/playername/{playerCharacterId}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var playerData = JsonConvert.DeserializeObject<List<PlayerDataResponse>>(jsonString);
                return playerData?.FirstOrDefault()?.Name;
            }

            return null;
        }
    }
}











//https://sc2pulse.nephest.com/sc2/api/ladder/stats?season=60&queue=LOTV_1V1&team-type=ARRANGED&us=true&eu=true&kr=true&cn=true&bro=true&sil=true&gol=true&pla=true&dia=true&mas=true&gra=true&page=0
