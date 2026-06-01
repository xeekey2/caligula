using Caligula.Service;
using Quartz;

namespace Caligula.DataCollector;

public class MatchHistoryJob : IJob
{
    private readonly DataCollectionService _dataCollectionService;

    public MatchHistoryJob(DataCollectionService dataCollectionService)
    {
        _dataCollectionService = dataCollectionService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _dataCollectionService.RunDailyMatchHistoryUpdateAsync();
    }
}
