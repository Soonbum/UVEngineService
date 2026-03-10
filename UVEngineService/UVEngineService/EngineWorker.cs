using UVEngineService.Controllers;

namespace UVEngineService;

public class EngineWorker(EngineManager manager, ILogger<EngineWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("엔진 관리 서비스가 구동되었습니다. API를 통해 엔진을 연결해주세요.");

        // 특정 엔진을 자동으로 시작하고 싶지 않다면 여기서는 아무것도 하지 않고 대기합니다.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("엔진 서비스를 중지하고 장치를 해제합니다.");

        // EngineManager에 등록된 모든 엔진을 순회하며 안전하게 끕니다.
        foreach (var engine in manager.GetAllEngines())
        {
            try
            {
                logger.LogInformation($"엔진 ID {engine.GetEngineID()} 종료 중...");
                engine.Disconnect(); //
            }
            catch (Exception ex)
            {
                logger.LogError($"엔진 종료 중 오류 발생: {ex.Message}");
            }
        }

        await base.StopAsync(cancellationToken);
    }
}