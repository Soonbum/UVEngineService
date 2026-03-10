using UVEngineService;
using UVEngineService.Controllers;

var builder = WebApplication.CreateBuilder(args);

// 싱글톤 매니저 등록
builder.Services.AddSingleton<EngineManager>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 서비스 등록 섹션
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // JSON 문자열을 Enum으로, Enum을 문자열로 상호 변환하도록 설정
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Swagger에서도 문자열 Enum이 잘 표시되도록 아래 설정도 추가하면 좋습니다.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var engineApi = app.MapGroup("/api/engine");

// 1. 엔진 연결 API (엔진 종류 및 ID 지정)
engineApi.MapPost("/connect", async (ConnectionRequest req, EngineManager manager, CancellationToken ct) =>
{
    ILightEngineController controller = req.Type switch
    {
        EngineType.NQM => new NQMEngineController(), //
        EngineType.Anhua => new AnhuaEngineController(req.Ip ?? "127.0.0.1", req.Port), //
        EngineType.Visitech => new VisitechWQXGAEngineController(req.Ip ?? "127.0.0.1", req.Port), //
        _ => throw new ArgumentException("잘못된 엔진 타입입니다.")
    };

    // 장치 연결 및 백그라운드 루프 시작
    controller.Connect(req.Id); //
    await controller.StartAsync(ct);

    // 매니저에 등록
    manager.AddEngine(req.Id, controller);

    return Results.Ok(new { Message = $"{req.Type} 엔진(ID:{req.Id}) 연결 성공" });
});

// 2. 특정 ID의 엔진 상태 조회
engineApi.MapGet("/{id}/status", (int id, EngineManager manager) =>
{
    var ctrl = manager.GetEngine(id);
    if (ctrl == null) return Results.NotFound("엔진을 찾을 수 없습니다.");

    return Results.Ok(new
    {
        Id = ctrl.GetEngineID(), //
        Connected = ctrl.GetDeviceConnected(), //
        LedOn = ctrl.GetDeviceLEDOn(), //
        Temperature = ctrl.GetTemperatureSensorValue(0) //
    });
});

// 3. 특정 ID의 엔진 LED 제어
engineApi.MapPost("/{id}/led", (int id, bool isOn, EngineManager manager) =>
{
    var ctrl = manager.GetEngine(id);
    if (ctrl == null) return Results.NotFound();

    ctrl.SetDeviceLEDOn(isOn); //
    return Results.Ok($"{id}번 엔진 LED {(isOn ? "ON" : "OFF")}");
});

app.Run();