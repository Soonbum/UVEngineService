using UVEngineService.Controllers;

namespace UVEngineService;

public class EngineManager
{
    /// <summary>
    /// ID별로 컨트롤러를 저장 (Key: 엔진 ID, Value: 컨트롤러 인스턴스)
    /// </summary>
    private readonly Dictionary<int, ILightEngineController> Engines = [];

    public void AddEngine(int id, ILightEngineController controller)
    {
        if (Engines.ContainsKey(id)) Engines.Remove(id);
        Engines.Add(id, controller);
    }

    public ILightEngineController? GetEngine(int id)
    {
        Engines.TryGetValue(id, out var controller);
        return controller;
    }

    public IEnumerable<ILightEngineController> GetAllEngines() => Engines.Values;
}